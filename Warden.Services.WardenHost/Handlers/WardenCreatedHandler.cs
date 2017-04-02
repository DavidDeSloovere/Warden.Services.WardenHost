using System;
using System.Threading.Tasks;
using Warden.Messages.Events;
using Warden.Messages.Events.Organizations;

namespace Warden.Services.WardenHost.Handlers
{
    public class WardenCreatedHandler : IEventHandler<WardenCreated>
    {
        public async Task HandleAsync(WardenCreated @event)
        {
            Console.WriteLine($"Warden '{@event.Name}' created...");
            await Task.CompletedTask;
        }
    }
}