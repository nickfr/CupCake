using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace cupcake_aspnet3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IConfiguration config;
        public ValuesController(IConfiguration config)
        {
            this.config = config;
        }

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
            var server = config.GetValue("DBSERVER", "localhost");
            var port = config.GetValue("DBPORT", "5432");
            var dbname = config.GetValue("DBNAME", "postgres");
            var user = config.GetValue("DBUSER", "postgres");
            var password = config.GetValue("DBPASSWORD", "password1");
            using (var connection = await CreateConnectionAsync($"Server={server};Port={port};Database={dbname};Persist Security Info=False;User ID={user};Password={password};Timeout=30;"))
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
