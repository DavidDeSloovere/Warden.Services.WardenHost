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
    public class StartWardenHandler : ICommandHandler<StartWarden>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IWardenHostService _wardenHostService;

        public StartWardenHandler(IHandler handler, IBusClient bus,
            IWardenHostService wardenHostService)
        {
            _handler = handler;
            _bus = bus;
            _wardenHostService = wardenHostService;
        }

        public async Task HandleAsync(StartWarden command)
        {
            var name = string.Empty;
            await _handler
                .Run(async () => 
                {
                    var warden = await _wardenHostService.GetWardenAsync(command.WardenId);
                    name = warden.Name;
                    await _wardenHostService.StartWardenAsync(command.WardenId);
                })
                .OnSuccess(async () =>
                {
                    await _bus.PublishAsync(new WardenStarted(command.Request.Id, command.UserId,
                        command.OrganizationId, command.WardenId, name, DateTime.UtcNow));
                })
                .OnCustomError(async ex => await _bus.PublishAsync(new StartWardenRejected(command.Request.Id,
                    command.UserId, ex.Code, ex.Message)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error(ex, "Error occured while starting a warden.");
                    await _bus.PublishAsync(new StartWardenRejected(command.Request.Id,
                        command.UserId, OperationCodes.Error, ex.Message));
                })
                .ExecuteAsync();
        }        
    }
}