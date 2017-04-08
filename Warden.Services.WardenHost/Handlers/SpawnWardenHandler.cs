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
    public class SpawnWardenHandler : ICommandHandler<SpawnWarden>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IWardenFactory _wardenFactory;
        private readonly IWardenHostService _wardenHostService;

        public SpawnWardenHandler(IHandler handler, IBusClient bus,
            IWardenFactory wardenFactory, IWardenHostService wardenHostService)
        {
            _handler = handler;
            _bus = bus;
            _wardenFactory = wardenFactory;
            _wardenHostService = wardenHostService;
        }

        public async Task HandleAsync(SpawnWarden command)
        {
            await _handler
                .Run(async () => 
                {
                    Console.WriteLine($"Spawned new warden: '{command.WardenId}' -> '{command.Name}'.");
                    var warden = _wardenFactory.Create(command.Name);
                    await _wardenHostService.AddWardenAsync(command.WardenId, warden);
                })
                .OnSuccess(async () =>
                {
                    await _bus.PublishAsync(new WardenSpawned(command.Request.Id, command.UserId,
                        command.OrganizationId, command.WardenId, command.Name, DateTime.UtcNow));
                })
                .OnCustomError(async ex => await _bus.PublishAsync(new SpawnWardenRejected(command.Request.Id,
                    command.UserId, ex.Code, ex.Message)))
                .OnError(async (ex, logger) =>
                {
                    logger.Error(ex, "Error occured while spawning a warden.");
                    await _bus.PublishAsync(new SpawnWardenRejected(command.Request.Id,
                        command.UserId, OperationCodes.Error, ex.Message));
                })
                .ExecuteAsync();
        }        
    }
}