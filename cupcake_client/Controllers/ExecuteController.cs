using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using cupcake_client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace cupcake_client.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ExecuteController : ControllerBase
    {
        private readonly IConfiguration config;
        public ExecuteController(IConfiguration config)
        {
            this.config = config;
        }
        // GET api/values
        [HttpGet("{requestType}")]
        public async Task<ActionResult<CupcakeResponse>> Get(RequestType requestType)
        {
            (bool supported, DefinitionAttribute attr) = DefinitionHelper.GetDefinition(requestType);
            if (!supported)
                return BadRequest($"{requestType} not supported");

            var uri = config.GetValue<string>(attr.UrlEnvVar);
            

            return attr.UseStreaming ? Ok(await CallAsync(uri)) : Ok(await CallSync(uri));

        }
        private async Task<CupcakeResponse> CallAsync(string uri)
        {
            var mem = (double)GC.GetTotalMemory(true);
            using (var client = new HttpClient())
            { 
                var watch = new Stopwatch();
                watch.Start();
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
                    long time2 = 0, time3 = 0, time4 = 0, time5 = 0;
                    var time1 = watch.ElapsedMilliseconds;
                    var counts = new Dictionary<char, int>();
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
                                //var x = await jtr.ReadAsStringAsync();
                                time4 = watch.ElapsedMilliseconds;
                                string x;
                                do
                                {
                                    x = await jtr.ReadAsStringAsync();
                                    if (x != null)
                                    {
                                        foreach (var c in x)
                                        {
                                            if (!counts.ContainsKey(c))
                                                counts.Add(c, 1);
                                            else
                                                counts[c]++;
                                        }
                                    }
                                }
                                while (x != null);
                                time5 = watch.ElapsedMilliseconds;
                            }
                        }
                    }
                    mem = (GC.GetTotalMemory(false) - mem) / (1024 * 1024);
                    return new CupcakeResponse
                    {
                        TimeToResponse = time3 - time1,
                        TimeToFirstRead = time4 - time1,
                        TimeToAllRead = time5 - time1,
                        Memory = mem,
                        Counts = counts
                    };
                }
            }
        }
        private async Task<CupcakeResponse> CallSync(string uri)
        {
            var mem = (double)GC.GetTotalMemory(true);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var watch = new Stopwatch();
                watch.Start();
                var response = await client.GetAsync(uri);
                var time1 = watch.ElapsedMilliseconds;
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var time2 = watch.ElapsedMilliseconds;

                var arr = JsonConvert.DeserializeObject<string[]>(body);
                var counts = arr.SelectMany(x => x.ToCharArray()).GroupBy(x => x).ToDictionary(k => k.Key, v => v.Count());

                mem = (GC.GetTotalMemory(false) - mem) / (1024 * 1024);
                return new CupcakeResponse
                {
                    TimeToResponse = time1,
                    TimeToFirstRead = time2,
                    TimeToAllRead = time2,
                    Memory = mem,
                    Counts = counts
                };
            }
        }
    }
}
