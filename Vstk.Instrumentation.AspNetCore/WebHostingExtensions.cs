using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Vstk.Hosting;
using Vstk.Instrumentation.AspNetCore.Middleware;

namespace Vstk.Instrumentation.AspNetCore
{
    public static class WebHostingExtensions
    {
        public static IWebHostBuilder AddVostokServices(this IWebHostBuilder webHostBuilder, IVostokHostingEnvironment vstkHostingEnvironment = null)
        {
            vstkHostingEnvironment = vstkHostingEnvironment ?? VostokHostingEnvironment.Current;
            if (vstkHostingEnvironment == null)
                throw new InvalidOperationException($"{nameof(VostokHostingEnvironment)} is not defined");
            webHostBuilder.UseSetting(WebHostDefaults.EnvironmentKey, TranslateEnvironmentName(vstkHostingEnvironment));
            webHostBuilder.ConfigureAppConfiguration((context, builder) => builder.Add(new VostokConfigurationSource(vstkHostingEnvironment)));
            return webHostBuilder.ConfigureServices(
                (webHostBuilderContext, services) =>
                {
                    services.AddSingleton(vstkHostingEnvironment);
                    services.AddSingleton(vstkHostingEnvironment.AirlockClient);
                    services.AddSingleton(vstkHostingEnvironment.MetricScope);
                });
        }

        public static IApplicationBuilder UseVstk(this IApplicationBuilder app)
        {
            return app
                .UseMiddleware<RequestExecutionDistributedContextMiddleware>()
                .UseMiddleware<RequestExecutionTraceMiddleware>()
                .UseMiddleware<RequestExecutionTimeMiddleware>();
        }

        private static string TranslateEnvironmentName(IVostokHostingEnvironment vstkHostingEnvironment)
        {
            if (vstkHostingEnvironment.IsProduction())
                return EnvironmentName.Production;
            if (vstkHostingEnvironment.IsDevelopment())
                return EnvironmentName.Development;
            if (vstkHostingEnvironment.IsStaging())
                return EnvironmentName.Staging;
            return vstkHostingEnvironment.Environment;
        }
    }
}