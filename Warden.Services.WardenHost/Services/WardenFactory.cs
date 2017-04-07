using System.Net;
using Warden.Core;
using Warden.Watchers.Web;

namespace Warden.Services.WardenHost.Services
{
  public class WardenFactory : IWardenFactory
  {
        public IWarden Create(string name)
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
                .WithConsoleLogger()
                .Build();

            return WardenInstance.Create(name,wardenConfiguration);
        }
  }
}