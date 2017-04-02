using System.Collections.Generic;
using System.Threading.Tasks;

namespace Warden.Services.WardenHost.Services
{
    public class WardenHostService : IWardenHostService
    {
        private readonly IDictionary<string, IWarden> _wardens = new Dictionary<string, IWarden>();

        public async Task AddWardenAsync(string userId, IWarden warden)
        {
            _wardens[warden.Name] = warden;
            await Task.CompletedTask;
        }

        public async Task RemoveWardenAsync(string userId, string name)
        {
            _wardens.Remove(name);
            await Task.CompletedTask;
        }

        public async Task StartWardenAsync(string userId, string name)
        {
            await _wardens[name].StartAsync();
        }

        public async Task StopWardenAsync(string name)
        {
            await _wardens[name].StopAsync();
        }

        public async Task PauseWardenAsync(string name)
        {
            await _wardens[name].PauseAsync();
        }

        public async Task StartAllWardensAsync()
        {
            var tasks = new List<Task>();
            foreach(var warden in _wardens)
            {
                tasks.Add(warden.Value.StartAsync());
            }
            await Task.WhenAll(tasks.ToArray());
        }
    }
}