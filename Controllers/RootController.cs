using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Controllers
{
    public class RootController : ApiController
    {
        [HttpGet]
        [Route("")]
        public object Info()
        {
            return new
            {
                Api = "Conduit API",
                Version = "1.0.0",
                Welcome = true
            };
        }
    }
}
