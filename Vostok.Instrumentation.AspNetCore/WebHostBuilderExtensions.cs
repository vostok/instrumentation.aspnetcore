using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vostok.Hosting;

namespace Vostok.Instrumentation.AspNetCore
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder AddVostokServices(
            this IWebHostBuilder webHostBuilder,
            IVostokHostingEnvironment vostokHostingEnvironment = null,
            LogLevel internalsMinLogLevel = LogLevel.Error)
        {
            vostokHostingEnvironment = vostokHostingEnvironment ?? VostokHostingEnvironment.Current;
            if (vostokHostingEnvironment == null)
                throw new InvalidOperationException($"{nameof(VostokHostingEnvironment)} is not defined");
            webHostBuilder.UseSetting(WebHostDefaults.EnvironmentKey, TranslateEnvironmentName(vostokHostingEnvironment));
            webHostBuilder.ConfigureAppConfiguration(builder => builder.Add(new VostokConfigurationSource(vostokHostingEnvironment)));
            return webHostBuilder
                .ConfigureServices(
                    services =>
                    {
                        services.AddSingleton(vostokHostingEnvironment);
                        services.AddSingleton(vostokHostingEnvironment.AirlockClient);
                        services.AddSingleton(vostokHostingEnvironment.MetricScope);
                        if (vostokHostingEnvironment.Log != null)
                            services.AddSingleton(vostokHostingEnvironment.Log);
                    })
                .ConfigureLogging(
                    builder =>
                    {
                        if (vostokHostingEnvironment.Log != null)
                        {
                            builder.AddVostok(vostokHostingEnvironment.Log);
                            builder.AddFilter("Microsoft", internalsMinLogLevel);
                            builder.AddFilter("System", internalsMinLogLevel);
                        }
                    });
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