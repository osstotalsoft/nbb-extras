using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NBB.Mediator.OpenFaaS;
using NBB.Mediator.OpenFaaS.Extensions;

using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Contracts.Functions.CreateContract
{
    public class Function
    {
        private ServiceProvider _container;

        public void PrepareFunctionContext()
        {
            _container = BuildServiceProvider();
        }

        public async Task Invoke(string body, CancellationToken cancellationToken)
        {

                using (var scope = _container.CreateScope())
                {
                    var command = GetCommandFromBuffer(body);
                    var commandHandler = scope.ServiceProvider.GetService<IRequestHandler<Application.CreateContract.Command>>();
                    if (commandHandler != null)
                    {
                        await commandHandler.Handle(command, cancellationToken);
                    }
                }
            
            //using (CorrelationManager.NewCorrelationId(correlationId))
            //{
            //    var command = GetCommandFromBuffer(buffer);
            //    var commandHandler = GetCommandHandler();
            //    await commandHandler.Handle(command, CancellationToken.None);
            //    await commandHandler.Handle(command, CancellationToken.None);
            //}

        }

        private static ServiceProvider BuildServiceProvider()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.Equals(environment, "development", StringComparison.OrdinalIgnoreCase);

            if (isDevelopment)
            {
                configurationBuilder.AddUserSecrets(Assembly.GetCallingAssembly());
            }

            var configuration = configurationBuilder.Build();


            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging(builder =>
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.MSSqlServer(configuration.GetConnectionString("Logs"), "Logs", autoCreateSqlTable: true)
                    .CreateLogger();

                builder.AddSerilog(dispose: true);
            });

            services.AddOpenFaaSMediator();


            services.AddScoped<IRequestHandler<Application.CreateContract.Command>, Application.CreateContract.Handler>();

            var container = services.BuildServiceProvider();
            return container;
        }


        private static Application.CreateContract.Handler GetCommandHandler()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            
            var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.Equals(environment, "development", StringComparison.OrdinalIgnoreCase);

            if (isDevelopment)
            {
                configurationBuilder.AddUserSecrets(Assembly.GetCallingAssembly());
            }

            var configuration = configurationBuilder.Build();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.MSSqlServer(configuration.GetConnectionString("Logs"), "Logs", autoCreateSqlTable: true)
                .CreateLogger();

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddSerilog();

            var mediator = new OpenFaaSMediator(configuration, new Logger<OpenFaaSMediator>(loggerFactory));
            var result = new Application.CreateContract.Handler(mediator);

            return result;
        }


        private static Guid? GetCorrelationIdFromBuffer(string buffer)
        {
            return null;
        }

        private static Application.CreateContract.Command GetCommandFromBuffer(string buffer)
        {
            return new Application.CreateContract.Command(Guid.NewGuid());
        }

    }
}
