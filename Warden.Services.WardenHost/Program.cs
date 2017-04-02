using System;
using System.Net;
using System.Threading.Tasks;
using Warden.Core;
using Warden.Watchers.Web;
using Warden.Services.WardenHost.Services;

namespace Warden.Services.WardenHost
{
    class Program
    {
        public static void Main(string[] args)
        {
            Task.WaitAll(StartAsync());
        }

        private static async Task StartAsync()
        {
            var userId = "user";
            var wardenHost = new WardenHostService();
            await wardenHost.AddWardenAsync(userId, CreateWarden("Warden #1", 1));
            await wardenHost.AddWardenAsync(userId, CreateWarden("Warden #2", 3));
            await wardenHost.AddWardenAsync(userId, CreateWarden("Warden #3"));
            await wardenHost.AddWardenAsync(userId, CreateWarden("Warden #4", 10));
            await wardenHost.AddWardenAsync(userId, CreateWarden("Warden #5", 2));
            await wardenHost.StartAllWardensAsync();
        }

        private static IWarden CreateWarden(string name, int interval = 5)
        {
            var wardenConfiguration = WardenConfiguration
                .Create()
                .AddWebWatcher("http://httpstat.us/200", cfg =>
                {
                    cfg.EnsureThat(response => response.StatusCode == HttpStatusCode.Accepted);
                })  
                .SetGlobalWatcherHooks((hooks, integrations) =>
                {
                })
                .WithInterval(TimeSpan.FromSeconds(interval))
                .WithConsoleLogger()
                .Build();

            return WardenInstance.Create(name,wardenConfiguration);
        }
    }
}
