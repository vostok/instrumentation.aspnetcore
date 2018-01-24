using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Vostok.Instrumentation.AspNetCore
{
    internal class VostokConfigurationProvider : IConfigurationProvider
    {
        private readonly IConfiguration configuration;

        public VostokConfigurationProvider(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void Set(string key, string value) => configuration[key] = value;

        public IChangeToken GetReloadToken() => configuration.GetReloadToken();

        public void Load()
        {
        }

        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            var section = string.IsNullOrEmpty(parentPath) ? configuration : configuration.GetSection(parentPath);
            var allKeys = section.GetChildren().Select(x => x.Key).Concat(earlierKeys);
            return allKeys.OrderBy(x => x, ConfigurationKeyComparer.Instance);
        }

        public bool TryGet(string key, out string value)
        {
            var section = configuration.GetSection(key);
            if (section.Exists())
            {
                value = section.Value;
                return true;
            }
            value = null;
            return false;
        }
    }
}