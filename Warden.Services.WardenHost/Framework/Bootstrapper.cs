using System.IO;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.Configuration;
using RawRabbit.Configuration;
using Warden.Common.Extensions;
using Warden.Common.Host;
using Warden.Common.RabbitMq;
using Warden.Messages.Commands;
using Warden.Messages.Commands.WardenChecks;
using Warden.Messages.Events;
using Warden.Messages.Events.Organizations;

namespace Warden.Services.WardenHost.Framework
{
    public class Bootstrapper
    {
        private static bool _configured = false;
        public static IConfiguration Configuration { get; private set; }
        public static ILifetimeScope LifetimeScope { get; private set; }

        public static void Initialize()
        {
            if(_configured)
            {
                return;
            }

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                // .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .SetBasePath(Directory.GetCurrentDirectory());
            Configuration = builder.Build();
            LifetimeScope = GetLifetimeScope();
            ServiceHost
                .Create<Program>()
                .UseAutofac(LifetimeScope)
                .UseRabbitMq(queueName: typeof(Program).Namespace)
                .SubscribeToEvent<WardenCreated>()
                .Build()
                .Run();
            _configured = true;
        }

        private static ILifetimeScope GetLifetimeScope()
        {
            var containerBuilder = new ContainerBuilder();
                var assembly = typeof(Program).GetTypeInfo().Assembly;
                containerBuilder.RegisterAssemblyTypes(assembly).AsClosedTypesOf(typeof(IEventHandler<>));
                containerBuilder.RegisterAssemblyTypes(assembly).AsClosedTypesOf(typeof(ICommandHandler<>));
                RabbitMqContainer.Register(containerBuilder, Configuration.GetSettings<RawRabbitConfiguration>());

            return containerBuilder.Build();
        }
    }
}