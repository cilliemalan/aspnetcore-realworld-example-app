using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ConduitApi.Infrastructure
{
    [Serializable]
    public class StatusCodeException : Exception
    {
        public StatusCodeException(HttpStatusCode code) : base(code.ToString()) { Code = code; }
        protected StatusCodeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public HttpStatusCode Code { get; }
    }
}
