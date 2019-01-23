using ConduitApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConduitApi
{
    public static class MvcOptionsExtensions
    {
        public static void UseRoutePrefix(this MvcOptions opts, IRouteTemplateProvider routeAttribute)
        {
            opts.Conventions.Add(new RoutePrefixConvention(routeAttribute));
        }

        public static void UseRoutePrefix(this MvcOptions opts, string prefix)
        {
            opts.UseRoutePrefix(new RouteAttribute(prefix));
        }
    }
}
