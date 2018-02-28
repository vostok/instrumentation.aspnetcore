using System;
using Microsoft.Extensions.Configuration;
using Vstk.Hosting;

namespace Vstk.Instrumentation.AspNetCore
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