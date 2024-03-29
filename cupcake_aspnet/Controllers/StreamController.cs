﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;

namespace cupcake_aspnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamController : ControllerBase
    {
        private readonly IConfiguration config;
        public StreamController(IConfiguration config)
        {
            this.config = config;
        }

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
            var server = config.GetValue("DBSERVER", "localhost");
            var port = config.GetValue("DBPORT", "5432");
            var dbname = config.GetValue("DBNAME", "postgres");
            var user = config.GetValue("DBUSER", "postgres");
            var password = config.GetValue("DBPASSWORD", "password1");
            using (var connection = CreateConnection($"Server={server};Port={port};Database={dbname};Persist Security Info=False;User ID={user};Password={password};Timeout=30;"))
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
