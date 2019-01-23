using ConduitApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ConduitApi.Infrastructure
{
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }

    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);

                if (context.Response.StatusCode >= 400 && !context.Response.HasStarted)
                {
                    await HandleExceptionAsync(context, null);
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            List<string> errors = new List<string>();
            HttpStatusCode code = (HttpStatusCode)context.Response.StatusCode;
            if (code == HttpStatusCode.OK) code = HttpStatusCode.InternalServerError;

            if (exception != null)
            {
                if (exception is StatusCodeException see)
                {
                    code = see.Code;
                }

                if (exception is BadRequestException bre)
                {
                    errors.AddRange(bre.ModelState
                        .Where(m => m.Value.ValidationState == ModelValidationState.Invalid && m.Value.Errors.Count > 0)
                            .SelectMany(m => m.Value.Errors
                                .Select(e => string.IsNullOrEmpty(m.Key) ? e.ErrorMessage : $"{m.Key}: {e.ErrorMessage}")));
                }
            }

            if (errors.Count == 0)
            {
                switch (code)
                {
                    case HttpStatusCode.NotFound:
                        errors.Add("Not Found.");
                        break;
                    case HttpStatusCode.Forbidden:
                        errors.Add("You are not allowed to perform this action.");
                        break;
                    case HttpStatusCode.Unauthorized:
                        errors.Add("This request needs authentication information.");
                        break;
                    case HttpStatusCode.BadRequest:
                        errors.Add("There is an unknown problem with the request.");
                        break;
                    default:
                        errors.Add("An unknown error has ocurred.");
                        break;
                }
            }

            var result = JsonConvert.SerializeObject(new Envelope<Errors>
            {
                EnvelopePropertyName = "errors",
                Content = new Errors
                {
                    Body = errors.ToArray()
                }
            });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }
    }
}
