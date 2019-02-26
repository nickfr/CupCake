using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;

namespace cupcake_aspnet3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        protected async Task<NpgsqlConnection> CreateConnectionAsync(string connectionString)
        {
            var connection = new NpgsqlConnection { ConnectionString = connectionString };

            if (connection.State == ConnectionState.Closed)
            {
                return await connection.OpenAsync().ContinueWith(t =>
               {
                   if (t.Exception == null)
                   {
                       return connection;
                   }

                   if (t.Exception.InnerExceptions.Count == 1)
                   {
                       throw t.Exception.InnerExceptions[0];
                   }

                   throw t.Exception;
               });
            }

            return connection;
        }

        [HttpGet]
        [Produces("application/json")]
        public async IAsyncEnumerable<string> GetAsync()
        {
            var pid = System.Diagnostics.Process.GetCurrentProcess().Id;
            Console.WriteLine($"Get {pid}");
            using (var connection = await CreateConnectionAsync("Server=localhost;Port=5432;Database=postgres;Persist Security Info=False;User ID=postgres;Password=JKQQRLrAprfvJvix4LdkN[;Timeout=30;"))
            {
                using (var command = new Npgsql.NpgsqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "public.get_data";
                    command.CommandType = CommandType.StoredProcedure;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var result = new List<string>();

                        while (await reader.ReadAsync())
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
