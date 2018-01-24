using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Vostok.Instrumentation.AspNetCore.Middleware
{
    public class RequestExecutionTimeMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<RequestExecutionTimeMiddleware> logger;

        public RequestExecutionTimeMiddleware(RequestDelegate next, ILogger<RequestExecutionTimeMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            logger.LogInformation(
                "Request {Method} {Scheme}://{Host}{PathBase}{Path}{QueryString} started",
                context.Request.Method,
                context.Request.Scheme,
                context.Request.Host.Value,
                context.Request.PathBase.Value,
                context.Request.Path.Value,
                context.Request.QueryString.Value);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                await next.Invoke(context).ConfigureAwait(false);
                logger.LogInformation(
                    "Request {Method} {Scheme}://{Host}{PathBase}{Path}{QueryString} finished with {StatusCode} after {ElapsedMs} ms",
                    context.Request.Method,
                    context.Request.Scheme,
                    context.Request.Host.Value,
                    context.Request.PathBase.Value,
                    context.Request.Path.Value,
                    context.Request.QueryString.Value,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                // todo (spaceorc, 15.12.2017) не выбрасывать исключение дальше - возвращать 500
                logger.LogError(
                    e,
                    "Request {Method} {Scheme}://{Host}{PathBase}{Path}{QueryString} failed after {ElapsedMs} ms",
                    context.Request.Method,
                    context.Request.Scheme,
                    context.Request.Host.Value,
                    context.Request.PathBase.Value,
                    context.Request.Path.Value,
                    context.Request.QueryString.Value,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}