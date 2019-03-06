using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cupcake_client.Models
{
    public class CupcakeResponse
    {
        public long TimeToResponse { get; internal set; }
        public long TimeToFirstRead { get; internal set; }
        public long TimeToAllRead { get; internal set; }
        public double Memory { get; internal set; }

        public Dictionary<char,int> Counts { get; internal set; }
    }
}
