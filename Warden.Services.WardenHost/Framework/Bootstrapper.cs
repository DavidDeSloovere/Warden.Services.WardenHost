using System.IO;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.Configuration;
using RawRabbit.Configuration;
using Warden.Common.Extensions;
using Warden.Common.Handlers;
using Warden.Common.Host;
using Warden.Common.RabbitMq;
using Warden.Messages.Commands;
using Warden.Messages.Commands.Spawn;
using Warden.Services.WardenHost.Services;

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
                .SubscribeToCommand<SpawnWarden>()
                .SubscribeToCommand<StartWarden>()
                .SubscribeToCommand<StopWarden>()
                .SubscribeToCommand<PauseWarden>()
                .SubscribeToCommand<PingWarden>()
                .SubscribeToCommand<KillWarden>()
                .Build()
                .Run();
            _configured = true;
        }

        private static ILifetimeScope GetLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Handler>().As<IHandler>();
            builder.RegisterType<WardenFactory>().As<IWardenFactory>().SingleInstance();
            builder.RegisterType<WardenHostService>().As<IWardenHostService>().SingleInstance();
            var assembly = typeof(Program).GetTypeInfo().Assembly;
            builder.RegisterAssemblyTypes(assembly).AsClosedTypesOf(typeof(ICommandHandler<>));
            RabbitMqContainer.Register(builder, Configuration.GetSettings<RawRabbitConfiguration>());

            return builder.Build();
        }
    }
}