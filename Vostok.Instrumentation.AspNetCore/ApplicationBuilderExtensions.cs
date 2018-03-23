using Microsoft.AspNetCore.Builder;
using Vostok.Instrumentation.AspNetCore.Middleware;

namespace Vostok.Instrumentation.AspNetCore
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseVostok(this IApplicationBuilder app)
        {
            return app
                .UseMiddleware<RequestExecutionDistributedContextMiddleware>()
                .UseMiddleware<RequestExecutionTraceMiddleware>()
                .UseMiddleware<RequestExecutionTimeMiddleware>();
        }
    }
}