using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ConduitApi.Infrastructure
{
    [Serializable]
    public class BadRequestException : StatusCodeException
    {
        public BadRequestException(ModelStateDictionary modelState) : base(HttpStatusCode.BadRequest)
        {
            ModelState = modelState;
        }
        public BadRequestException(HttpStatusCode statusCode, ModelStateDictionary modelState) : base(statusCode)
        {
            ModelState = modelState;
        }
        protected BadRequestException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public ModelStateDictionary ModelState { get; }
    }
}
