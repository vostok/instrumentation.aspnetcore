using System;
using Microsoft.Extensions.Configuration;
using Vstk.Hosting;

namespace Vstk.Instrumentation.AspNetCore
{
    internal class VostokConfigurationSource : IConfigurationSource
    {
        private readonly IVostokHostingEnvironment vstkHostingEnvironment;

        public VostokConfigurationSource(IVostokHostingEnvironment vstkHostingEnvironment)
        {
            this.vstkHostingEnvironment = vstkHostingEnvironment ?? throw new ArgumentNullException(nameof(vstkHostingEnvironment));
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new VostokConfigurationProvider(vstkHostingEnvironment.Configuration);
        }
    }
}