using System;
using Microsoft.Extensions.Configuration;
using Vostok.Hosting;

namespace Vostok.Instrumentation.AspNetCore
{
    internal class VostokConfigurationSource : IConfigurationSource
    {
        private readonly IVostokHostingEnvironment vostokHostingEnvironment;

        public VostokConfigurationSource(IVostokHostingEnvironment vostokHostingEnvironment)
        {
            this.vostokHostingEnvironment = vostokHostingEnvironment ?? throw new ArgumentNullException(nameof(vostokHostingEnvironment));
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new VostokConfigurationProvider(vostokHostingEnvironment.Configuration);
        }
    }
}