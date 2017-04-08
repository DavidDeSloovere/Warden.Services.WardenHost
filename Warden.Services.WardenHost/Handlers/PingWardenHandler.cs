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
    public class PingWardenHandler : ICommandHandler<PingWarden>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IWardenHostService _wardenHostService;

        public PingWardenHandler(IHandler handler, IBusClient bus,
            IWardenHostService wardenHostService)
        {
            _handler = handler;
            _bus = bus;
            _wardenHostService = wardenHostService;
        }

        public async Task HandleAsync(PingWarden command)
        {
            var name = string.Empty;
            await _handler
                .Run(async () => 
                {
                    var warden = await _wardenHostService.GetWardenAsync(command.WardenId);
                    name = warden.Name;
                    await _wardenHostService.PingWardenAsync(command.WardenId);
                })
                .OnSuccess(async () =>
                {
                    await _bus.PublishAsync(new WardenPinged(command.Request.Id, command.UserId,
                        command.OrganizationId, command.WardenId, name, DateTime.UtcNow));
                })
                .OnCustomError(async ex => await _bus.PublishAsync(new PingWardenRejected(command.Request.Id,
                    command.UserId, ex.Code, ex.Message)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error(ex, "Error occured while pinging a warden.");
                    await _bus.PublishAsync(new PingWardenRejected(command.Request.Id,
                        command.UserId, OperationCodes.Error, ex.Message));
                })
                .ExecuteAsync();
        }        
    }
}