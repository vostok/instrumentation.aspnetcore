using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Hosting;
using Vostok.Instrumentation.AspNetCore.Middleware;

namespace Vostok.Instrumentation.AspNetCore
{
    public static class WebHostingExtensions
    {
        public static IWebHostBuilder AddVostokServices(this IWebHostBuilder webHostBuilder, IVostokHostingEnvironment vostokHostingEnvironment = null)
        {
            vostokHostingEnvironment = vostokHostingEnvironment ?? VostokHostingEnvironment.Current;
            if (vostokHostingEnvironment == null)
                throw new InvalidOperationException($"{nameof(VostokHostingEnvironment)} is not defined");
            webHostBuilder.UseSetting(WebHostDefaults.EnvironmentKey, TranslateEnvironmentName(vostokHostingEnvironment));
            webHostBuilder.ConfigureAppConfiguration((context, builder) => builder.Add(new VostokConfigurationSource(vostokHostingEnvironment)));
            return webHostBuilder.ConfigureServices(
                (webHostBuilderContext, services) =>
                {
                    services.AddSingleton(vostokHostingEnvironment);
                    services.AddSingleton(vostokHostingEnvironment.AirlockClient);
                    services.AddSingleton(vostokHostingEnvironment.MetricScope);
                });
        }

        public static IApplicationBuilder UseVostok(this IApplicationBuilder app)
        {
            return app
                .UseMiddleware<RequestExecutionDistributedContextMiddleware>()
                .UseMiddleware<RequestExecutionTraceMiddleware>()
                .UseMiddleware<RequestExecutionTimeMiddleware>();
        }

        private static string TranslateEnvironmentName(IVostokHostingEnvironment vostokHostingEnvironment)
        {
            if (vostokHostingEnvironment.IsProduction())
                return EnvironmentName.Production;
            if (vostokHostingEnvironment.IsDevelopment())
                return EnvironmentName.Development;
            if (vostokHostingEnvironment.IsStaging())
                return EnvironmentName.Staging;
            return vostokHostingEnvironment.Environment;
        }
    }
}