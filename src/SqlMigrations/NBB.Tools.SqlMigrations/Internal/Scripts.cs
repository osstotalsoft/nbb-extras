using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

namespace NBB.Tools.SqlMigrations.Internal
{
    public class Scripts
    {
        private readonly ScriptSubstitutor _scriptSubstitutor;

        public Scripts(ScriptSubstitutor scriptSubstitutor)
        {
            _scriptSubstitutor = scriptSubstitutor;
        }

        private const string scriptExtension = ".sql";
        private readonly ConcurrentDictionary<string, string> _scripts
            = new ConcurrentDictionary<string, string>();

        public string EnsureMigrationsTable => GetInternalScript(nameof(EnsureMigrationsTable));
        public string GetScriptsVersion => GetInternalScript(nameof(GetScriptsVersion));
        public string UpdateScriptsVersion => GetInternalScript(nameof(UpdateScriptsVersion));
        public string EnsureMigrationFilesTable => GetInternalScript(nameof(EnsureMigrationFilesTable));
        public string GetMigrationFiles => GetInternalScript(nameof(GetMigrationFiles));
        public string InsertMigrationFile => GetInternalScript(nameof(InsertMigrationFile));

        private string GetInternalScript(string name)
        {
            return _scripts.GetOrAdd(name,
                key =>
                {
                    var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
                    var path = Path.Combine(fi.Directory.FullName, "Internal", "SqlScripts", $"{name}{scriptExtension}");
                    var fiPath = new FileInfo(path);

                    if (!fiPath.Exists)
                    {
                        throw new Exception($"{path} does not exist");
                    }

                    var content = File.ReadAllText(path);
                    content = _scriptSubstitutor.SubstituteString(content);
                    return content;
                });
        }
    }
}