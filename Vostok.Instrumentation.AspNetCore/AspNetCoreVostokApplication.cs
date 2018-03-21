using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Hosting;

namespace Vostok.Instrumentation.AspNetCore
{
    public abstract class AspNetCoreVostokApplication : IVostokApplication
    {
        private Task workTask;

        public async Task StartAsync(IVostokHostingEnvironment hostingEnvironment)
        {
            var webHost = BuildWebHost(hostingEnvironment);
            var applicationLifetime = webHost.Services.GetRequiredService<IApplicationLifetime>();
            var tcs = new TaskCompletionSource<int>();
            applicationLifetime.ApplicationStarted.Register(() => tcs.TrySetResult(0));
            workTask = webHost.RunAsync(hostingEnvironment.ShutdownCancellationToken);
            OnStarted(hostingEnvironment);
            if (workTask == await Task.WhenAny(tcs.Task, workTask).ConfigureAwait(false))
                await workTask.ConfigureAwait(false);
        }

        public async Task WaitForTerminationAsync()
        {
            await workTask.ConfigureAwait(false);
        }

        protected virtual void OnStarted(IVostokHostingEnvironment hostingEnvironment)
        {
        }

        protected abstract IWebHost BuildWebHost(IVostokHostingEnvironment hostingEnvironment);
    }
}