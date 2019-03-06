using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Data;
using Grpc.Core;
using Grpc.Core.Utils;

namespace CupCake
{
    public class CupCakeImpl : CupcakeService.CupCakeService.CupCakeServiceBase
    {
        public CupCakeImpl()
        {
        }

        public override async Task Get(CupcakeService.EmptyRequest request, IServerStreamWriter<CupcakeService.CupCakeResult> responseStream, ServerCallContext context)
        {
            using(var connection = new NpgsqlConnection { ConnectionString = "Server=localhost;Port=5432;Database=postgres;Persist Security Info=False;User ID=postgres;Password=JKQQRLrAprfvJvix4LdkN[;Timeout=300;Pooling=false" }) 
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
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
                            await responseStream.WriteAsync(new CupcakeService.CupCakeResult{ Data = reader.GetString(1)});
                        }
                    }
                }
            }
        }
    }
}
