using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
using Microsoft.AspNetCore.Http.Features;

namespace cupcake_aspnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamController : ControllerBase
    {
        protected NpgsqlConnection CreateConnection(string connectionString)
        {
            var retries = 0;
            Exception lastException = null;

            while (retries < 50)
            {
                try
                {
                    var connection = new NpgsqlConnection { ConnectionString = connectionString };
                    if (connection.State == ConnectionState.Closed)
                    {
                        connection.Open();
                    }
                    Console.WriteLine($"success retries={retries}");
                    return connection;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    retries++;
                    Console.WriteLine($"delaying retries={retries}");
                    Task.Delay(10);
                }
            }
            throw new Exception("Failed to create connection", lastException);
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var pid = System.Diagnostics.Process.GetCurrentProcess().Id;
            Console.WriteLine($"Get {pid}");
            using (var connection = CreateConnection("Server=localhost;Port=5432;Database=postgres;Persist Security Info=False;User ID=postgres;Password=JKQQRLrAprfvJvix4LdkN[;Timeout=300;Pooling=false"))
            {
                using (var command = new Npgsql.NpgsqlCommand())
                {
                    command.CommandTimeout = 300;
                    command.Connection = connection;
                    command.CommandText = "public.get_data";
                    command.CommandType = CommandType.StoredProcedure;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reader.GetInt32(0);
                            yield return reader.GetString(1);
                        }
                    }
                }
            }
        }
    }
}
