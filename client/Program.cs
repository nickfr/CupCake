using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using CommandLine;

namespace client
{
    // e.g. 
    // dotnet run bin/Debug/netcoreapp2.2/client.dll -s Go -n 4 -l
    // dotnet run bin/Debug/netcoreapp2.2/client.dll -s AspStream -n 4 -l
    class Program
    {
        private class Result
        {
            public long TimeToResponse { get; internal set; }
            public long TimeToFirstRead { get; internal set; }
            public long TimeToAllRead { get; internal set; }
        }

        private static string goUri = "http://localhost:8000/get";
        private static string aspStreamUri = "http://localhost:5000/api/stream";
        private static string aspUri = "http://localhost:5000/api/values";

        private static async Task<List<string>> DeserializeJsonFromStreamAsync(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return new List<String>();

            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                var a = await jtr.ReadAsync();
                var x = await jtr.ReadAsStringAsync();

                return new List<String>();
            }
        }
        private static async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
                using (var sr = new StreamReader(stream))
                    content = await sr.ReadToEndAsync();

            return content;
        }

        private enum Server
        {
            Asp,
            AspStream,
            Go
        }

        class Options
        {
            [Option('n', "num", Default = 1, Required = false, HelpText = "Number to run concurrently.")]
            public int NumConcurrent { get; set; }

            [Option('s', "server", Default = Server.AspStream, Required = false, HelpText = "Use stream server (AspStream, Asp, Go).")]
            public Server StreamServer { get; set; }

            [Option('l', "client", Default = false, Required = false, HelpText = "Use stream client.")]
            public bool StreamClient { get; set; }
        }

        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => RunProgram(opts))
                .WithNotParsed<Options>((errs) => HandleParseError(errs));
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (var err in errs)
            {
                Console.WriteLine(err);
            }
        }

        private static void RunProgram(Options opts)
        {
            Task.WaitAll(Run(opts.StreamServer, opts.StreamClient, opts.NumConcurrent));
        }

        static async Task Run(Server streamServer, bool streamClient, int num)
        {
            var uri = streamServer == Server.Asp  ? aspUri : streamServer == Server.Go ? goUri : aspStreamUri;
            Console.WriteLine($"Calling {uri} n={num} client streaming={streamClient}");
            var mem = (double)GC.GetTotalMemory(true);
            var tasks = Enumerable.Range(1, num).Select(i => streamClient ? CallAsync(uri) : CallSync(uri));
            var results = await Task.WhenAll(tasks);
            mem = (GC.GetTotalMemory(false) - mem) / (1024 * 1024);
            Console.WriteLine($"Stream server: {streamServer} Stream client: {streamClient} Response time (ms): {results.Average(r => r.TimeToResponse)} First result: {results.Average(r => r.TimeToFirstRead)} All read: {results.Average(r => r.TimeToAllRead)} Memory: {mem:N}MB");
        }

        static async Task<Result> CallAsync(string uri)
        {
            var client = new HttpClient();
            var watch = new Stopwatch();
            watch.Start();
            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
                long time2 = 0, time3 = 0, time4 = 0, time5 = 0;
                var time1 = watch.ElapsedMilliseconds;
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    time2 = watch.ElapsedMilliseconds;
                    var stream = await response.Content.ReadAsStreamAsync();

                    time3 = watch.ElapsedMilliseconds;
                    if (response.IsSuccessStatusCode)
                    {
                        using (var sr = new StreamReader(stream))
                        using (var jtr = new JsonTextReader(sr))
                        {
                            var a = await jtr.ReadAsync();
                            var x = await jtr.ReadAsStringAsync();
                            time4 = watch.ElapsedMilliseconds;
                            while (x != null)
                            {
                                x = await jtr.ReadAsStringAsync();
                            }
                            time5 = watch.ElapsedMilliseconds;
                        }
                    }
                }

                return new Result
                {
                    TimeToResponse = time3 - time1,
                    TimeToFirstRead = time4 - time1,
                    TimeToAllRead = time5 - time1
                };
            }
        }

        static async Task<Result> CallSync(string uri)
        {
            var client = new HttpClient();
            //client.BaseAddress = new Uri("http://localhost:5000/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var watch = new Stopwatch();
            watch.Start();
            var response = await client.GetAsync(uri);
            var time1 = watch.ElapsedMilliseconds;
            response.EnsureSuccessStatusCode();
            var foo = await response.Content.ReadAsStringAsync();
            var time2 = watch.ElapsedMilliseconds;
            return new Result
            {
                TimeToResponse = time1,
                TimeToFirstRead = time2,
                TimeToAllRead = time2
            };
        }
    }
}
