using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CupCake
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server...");
            const int Port = 50052;

            Server server = new Server
            {
                Services = { CupcakeService.CupCakeService.BindService(new CupCakeImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
