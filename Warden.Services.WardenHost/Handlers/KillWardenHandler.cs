using System;
using System.Threading.Tasks;
using RawRabbit;
using Warden.Common.Handlers;
using Warden.Messages.Commands;
using Warden.Messages.Commands.Spawn;
using Warden.Messages.Events.Spawn;
using Warden.Services.WardenHost.Services;

namespace Warden.Services.WardenHost.Handlers
{
    public class KillWardenHandler : ICommandHandler<KillWarden>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IWardenHostService _wardenHostService;

        public KillWardenHandler(IHandler handler, IBusClient bus,
            IWardenHostService wardenHostService)
        {
            _handler = handler;
            _bus = bus;
            _wardenHostService = wardenHostService;
        }

        public async Task HandleAsync(KillWarden command)
        {
            var name = string.Empty;
            await _handler
                .Run(async () => 
                {
                    var warden = await _wardenHostService.GetWardenAsync(command.WardenId);
                    name = warden.Name;
                    await _wardenHostService.RemoveWardenAsync(command.WardenId);
                })
                .OnSuccess(async () =>
                {
                    await _bus.PublishAsync(new WardenKilled(command.Request.Id, command.UserId,
                        command.OrganizationId, command.WardenId, name, DateTime.UtcNow));
                })
                .OnCustomError(async ex => await _bus.PublishAsync(new KillWardenRejected(command.Request.Id,
                    command.UserId, ex.Code, ex.Message)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error(ex, "Error occured while killing a warden.");
                    await _bus.PublishAsync(new KillWardenRejected(command.Request.Id,
                        command.UserId, OperationCodes.Error, ex.Message));
                })
                .ExecuteAsync();
        }        
    }
}