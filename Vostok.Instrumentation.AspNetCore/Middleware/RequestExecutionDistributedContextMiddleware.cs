using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vostok.Flow;

namespace Vostok.Instrumentation.AspNetCore.Middleware
{
    public class RequestExecutionDistributedContextMiddleware
    {
        private readonly RequestDelegate next;
        private const string XDistributedContextPrefix = "X-Distributed-Context";

        public RequestExecutionDistributedContextMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var candidates = context.Request.Headers.Where(x => x.Key.StartsWith(XDistributedContextPrefix))
                .Select(x => new KeyValuePair<string, string>(
                    Decode(x.Key.Substring(XDistributedContextPrefix.Length + 1)),
                    Decode(x.Value)));

            Context.PopulateDistributedProperties(candidates);
            await next(context).ConfigureAwait(false);
        }

        private static string Decode(string str)
        {
            return Uri.UnescapeDataString(str);
        }
    }
}