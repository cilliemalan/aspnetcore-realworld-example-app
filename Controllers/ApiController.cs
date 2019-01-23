using ConduitApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi.Controllers
{
    [ApiController]
    public abstract class ApiController : Controller
    {
        public string Username => User?.Identity?.IsAuthenticated ?? false ? User.Identity.Name : null;

        protected void EnsureModelValid()
        {
            if(!ModelState.IsValid)
            {
                throw new BadRequestException(ModelState);
            }
        }

        protected Exception NotFoundException() => new StatusCodeException(System.Net.HttpStatusCode.NotFound);
    }
}
