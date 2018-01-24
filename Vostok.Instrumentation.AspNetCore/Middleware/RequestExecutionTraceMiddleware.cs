using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Vostok.Commons;
using Vostok.Commons.Extensions.Uri;
using Vostok.Hosting;
using Vostok.Tracing;

namespace Vostok.Instrumentation.AspNetCore.Middleware
{
    public class RequestExecutionTraceMiddleware
    {
        private readonly RequestDelegate next;
        private readonly string serviceName;

        public RequestExecutionTraceMiddleware(RequestDelegate next, IVostokHostingEnvironment vostokHostingEnvironment)
            : this(next, vostokHostingEnvironment.Service)
        {
        }

        public RequestExecutionTraceMiddleware(RequestDelegate next, string serviceName)
        {
            this.next = next;
            this.serviceName = serviceName;
        }

        public async Task Invoke(HttpContext context)
        {
            using (var spanBuilder = Trace.BeginSpan())
            {
                var url = GetUrl(context.Request);
                spanBuilder.SetAnnotation(TracingAnnotationNames.Operation, GetOperationName(context.Request.Method, url));
                spanBuilder.SetAnnotation(TracingAnnotationNames.Kind, "http-server");
                spanBuilder.SetAnnotation(TracingAnnotationNames.Service, serviceName);
                spanBuilder.SetAnnotation(TracingAnnotationNames.Host, HostnameProvider.Get());
                spanBuilder.SetAnnotation(TracingAnnotationNames.HttpUrl, url.ToStringWithoutQuery());
                if (context.Request.ContentLength.HasValue)
                    spanBuilder.SetAnnotation(TracingAnnotationNames.HttpRequestContentLength, context.Request.ContentLength);

                try
                {
                    await next.Invoke(context).ConfigureAwait(false);
                    if (context.Response.ContentLength.HasValue)
                        spanBuilder.SetAnnotation(TracingAnnotationNames.HttpResponseContentLength, context.Response.ContentLength);
                    spanBuilder.SetAnnotation(TracingAnnotationNames.HttpCode, context.Response.StatusCode);
                }
                catch
                {
                    spanBuilder.SetAnnotation(TracingAnnotationNames.HttpCode, HttpStatusCode.InternalServerError);
                    throw;
                }
            }
        }

        private static string GetOperationName(string httpMethod, Uri url)
        {
            return httpMethod + " " + url.GetNormalizedPath();
        }

        private static Uri GetUrl(HttpRequest request)
        {
            var absoluteUrl = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path, request.QueryString);
            return new Uri(absoluteUrl);
        }
    }
}