using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Subtext.Scripting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace NBB.Tools.SqlMigrations
{
    public class DatabaseMigrator
    {
        private readonly string _connectionString;
        private readonly Scripts _scripts;
        private readonly Internal.Scripts _internalScripts;
        private readonly IConfiguration _configuration;
        private readonly ScriptSubstitutor _scriptSubstitutor;
        private readonly ILogger<DatabaseMigrator> _logger;

        public DatabaseMigrator(IConfiguration configuration, ScriptSubstitutor scriptSubstitutor, Scripts scripts, Internal.Scripts internalScripts, ILogger<DatabaseMigrator> logger)
        {
            _configuration = configuration;
            _scriptSubstitutor = scriptSubstitutor;
            _scripts = scripts;
            _internalScripts = internalScripts;
            _connectionString = _configuration.GetConnectionString("Target_Database");
            _logger = logger;
        }

        public async Task MigrateToLatestVersion()
        {
            _logger.LogInformation("Migrating Database");

            using (var cnx = new SqlConnection(_connectionString))
            {
                await cnx.OpenAsync();

                await Initialize(cnx);
                await ExecuteScripts(cnx);
            }

            _logger.LogInformation("Database Migration completed");
        }

        private async Task Initialize(SqlConnection cnx)
        {
            await EnsureVersionTable(cnx);
            await EnsureFilesTable(cnx);

            var scriptsVersion = await GetScriptsVersion(cnx);
            _logger.LogInformation($"Current scripts version: {scriptsVersion}");
        }

        private async Task ExecuteScripts(SqlConnection cnx)
        {
            _logger.LogInformation("Migrating database");
            var scriptsToExecute = _scripts.ToList();
            var alreadyExecutedScripts = await GetMigrationFiles(cnx);

            foreach (var script in scriptsToExecute)
            {
                if (alreadyExecutedScripts.Contains(script.RelativePath)){
                    _logger.LogInformation($" Skipping {script.ScriptFileName}");
                    continue;
                }
                _logger.LogInformation($" * {script.ScriptFileName}");
                await ExecuteScript(cnx, script);
            }
        }
       

        private async Task ExecuteScript(SqlConnection cnx, Script script)
        {
            var scriptContent = _scripts.GetScriptContent(script.ScriptFileName);
            scriptContent = _scriptSubstitutor.SubstituteString(scriptContent);

            var updateVersionScriptContent = _internalScripts.UpdateScriptsVersion.Replace("@NewScriptsVersion", $"{script.ScriptNumber}");

            await ExecuteNonQuery(cnx, $"{scriptContent}{Environment.NewLine}{updateVersionScriptContent}");
            var insertScript = _internalScripts.InsertMigrationFile.Replace("@FileName", $"{script.RelativePath}");
            await ExecuteNonQuery(cnx, $"{insertScript}");
        }

        private Task EnsureVersionTable(SqlConnection cnx)
        {
            var sql = _internalScripts.EnsureMigrationsTable;
            return ExecuteNonQuery(cnx, sql);
        }

        private Task EnsureFilesTable(SqlConnection cnx)
        {
            var sql = _internalScripts.EnsureMigrationFilesTable;
            return ExecuteNonQuery(cnx, sql);
        }

        private async Task<int> GetScriptsVersion(SqlConnection cnx)
        {
            var sql = _internalScripts.GetScriptsVersion;

            using (var command = new SqlCommand(sql, cnx))
            {
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        private async Task<List<string>> GetMigrationFiles(SqlConnection cnx)
        {
            var sql = _internalScripts.GetMigrationFiles;
                var list = new List<string>();

            using (var command = new SqlCommand(sql, cnx))
            {
                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    list.Add(reader.GetString(0));
                }
            }
            return list;
        }

        private static Task ExecuteNonQuery(SqlConnection cnx, string sql)
        {
            var runner = new SqlScriptRunner(sql);
            using (var transaction = cnx.BeginTransaction())
            {
                runner.Execute(transaction);
                transaction.Commit();
            }

            return Task.CompletedTask;
        }
    }
}