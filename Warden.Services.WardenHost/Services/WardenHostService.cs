using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Warden.Services.WardenHost.Services
{
    public class WardenHostService : IWardenHostService
    {
        private readonly IDictionary<Guid, IWarden> _wardens = new Dictionary<Guid, IWarden>();

        public async Task<IWarden> GetWardenAsync(Guid wardenId)
            => await Task.FromResult(_wardens[wardenId]);

        public async Task AddWardenAsync(Guid wardenId, IWarden warden)
        {
            _wardens[wardenId] = warden;
            await Task.CompletedTask;
        }

        public async Task RemoveWardenAsync(Guid wardenId)
        {
            await StopWardenAsync(wardenId);
            _wardens.Remove(wardenId);
            await Task.CompletedTask;
        }

        public async Task StartWardenAsync(Guid wardenId)
        {
            await _wardens[wardenId].StartAsync();
        }

        public async Task StopWardenAsync(Guid wardenId)
        {
            await _wardens[wardenId].StopAsync();
        }

        public async Task PauseWardenAsync(Guid wardenId)
        {
            await _wardens[wardenId].PauseAsync();
        }

        public async Task PingWardenAsync(Guid wardenId)
        {
            var warden = _wardens[wardenId];
            await Task.CompletedTask;
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