using System;
using System.Threading.Tasks;

namespace Warden.Services.WardenHost.Services
{
    public interface IWardenHostService
    {
         Task AddWardenAsync(Guid wardenId, IWarden warden);
         Task RemoveWardenAsync(Guid wardenId);
         Task StartWardenAsync(Guid wardenId);
         Task StopWardenAsync(Guid wardenId);
         Task PauseWardenAsync(Guid wardenId);
    }
}