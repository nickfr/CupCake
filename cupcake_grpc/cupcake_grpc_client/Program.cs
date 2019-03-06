using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CupCake
{
    class Program
    {
        public class Client
        {
            readonly CupcakeService.CupCakeService.CupCakeServiceClient client;

            public Client(CupcakeService.CupCakeService.CupCakeServiceClient client)
            {
                this.client = client;
            }

            public async Task Get()
            {
                try
                {
                    using (var call = client.Get(new CupcakeService.EmptyRequest()))
                    {
                        var responseStream = call.ResponseStream;

                        while (await responseStream.MoveNext())
                        {
                            var result = responseStream.Current;
                            //Console.Write(result.Data);
                        }
                    }
                }
                catch (RpcException e)
                {
                    throw;
                }
            }
        }

        static void Main(string[] args)
        {
            var watch = new Stopwatch();
            watch.Start();
            var channel = new Channel("127.0.0.1:50052", ChannelCredentials.Insecure);
            var client = new Client(new CupcakeService.CupCakeService.CupCakeServiceClient(channel));
            var time1 = watch.ElapsedMilliseconds;
            client.Get().Wait();
            var time2 = watch.ElapsedMilliseconds;
            channel.ShutdownAsync().Wait();
            Console.WriteLine($"Time taken {time2-time1}ms");
            Console.ReadKey(); 
        }
    }
}
