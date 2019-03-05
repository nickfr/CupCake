using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;

namespace cupcake_aspnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        protected async Task<NpgsqlConnection> CreateConnectionAsync(string connectionString)
        {
            var connection = new NpgsqlConnection {ConnectionString = connectionString};

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
        
        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetAsync()
        {
            var server = Environment.GetEnvironmentVariable("CUPCAKE_DB");
            if(string.IsNullOrEmpty(server))
                server = "localhost";
            using (var connection = await CreateConnectionAsync($"Server={server};Port=5432;Database=postgres;Persist Security Info=False;User ID=postgres;Password=JKQQRLrAprfvJvix4LdkN[;Timeout=30;"))
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
                            result.Add(reader.GetString(1));
                        }

                        return result;
                    }
                }
            }
        }
    }
}
