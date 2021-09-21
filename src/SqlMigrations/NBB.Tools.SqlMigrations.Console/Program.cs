using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NBB.Tools.SqlMigrations
{
    class Program
    {
        public static IConfiguration Configuration;
        static async Task Main(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = configurationBuilder.Build();

            var services = new ServiceCollection()
                .AddLogging(configure => configure.AddConsole());

            services.AddSingleton(Configuration);
            services.AddSingleton<Internal.Scripts>();
            services.AddSingleton<Scripts>();
            services.AddSingleton<DatabaseMigrator>();
            services.AddSingleton<ScriptSubstitutor>();
            var serviceProvider = services.BuildServiceProvider();
            var migrator = serviceProvider.GetRequiredService<DatabaseMigrator>();
            await migrator.MigrateToLatestVersion();
            Console.ReadLine();
        }
    }
}