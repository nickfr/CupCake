using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace cupcake_client.Models
{
    public class CupcakeRequest
    {
        public RequestType RequestType { get; set; }
    }

    public enum RequestType
    {
        [Definition("ASP_URL",false)]
        Asp,
        [Definition("ASPSTREAM_URL", true)]
        AspStream,
        [Definition("GO_URL", true)]
        Go,
        Node,
        gRPC
    }

    public static class DefinitionHelper
    {
        public static (bool isSupported, DefinitionAttribute attr) GetDefinition(RequestType requestType)
        {
            var attr = requestType.GetType().GetMember(requestType.ToString())[0].GetCustomAttribute<DefinitionAttribute>();
            if (attr == null)
                return (false, attr);

            return (true, attr);
        }
    }

    public class DefinitionAttribute : Attribute
    {
        public string UrlEnvVar { get; private set; }
        public bool UseStreaming { get; private set; }
        public DefinitionAttribute(string urlEnvVar, bool useStreaming)
        {
            this.UrlEnvVar = urlEnvVar;
            this.UseStreaming = useStreaming;
        }
    }
}
