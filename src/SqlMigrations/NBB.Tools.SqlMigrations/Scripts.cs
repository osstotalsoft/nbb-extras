using Microsoft.Extensions.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NBB.Tools.SqlMigrations
{
    public class Scripts : IEnumerable<Script>
    {
        private List<string> _specifications;
        private readonly ScriptSubstitutor _scriptSubstitutor;
        public Scripts(IConfiguration configuration, ScriptSubstitutor scriptSubstitutor)
        {
            _specifications = configuration.GetSection("SqlScripts").Get<List<string>>();
            _scriptSubstitutor = scriptSubstitutor;
        }

        private IEnumerable<Script> _all
        {
            get
            {
                var list = new List<Script>();
                var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
                var path = fi.Directory.FullName;

                foreach (var spec in _specifications)
                {
                    var substituted = _scriptSubstitutor.SubstituteString(spec);

                    if (substituted.IndexOf("*") > 0)
                    {
                        var path2 = substituted.Substring(0, spec.LastIndexOf('/'));
                        var pattern = substituted.Substring(spec.LastIndexOf('/') + 1);

                        var fullPath = Path.Combine(path, path2);
                        var files = Directory.EnumerateFiles(fullPath, pattern);
                        foreach (var f in files)
                        {
                            list.Add(new Script(f));
                        }
                    }
                    else
                    {
                        var fullPath = Path.Combine(path, substituted);
                        list.Add(new Script(fullPath));
                    }
                }
                return list;
            }
        }


        public string GetScriptContent(string name)
        {
            return File.ReadAllText(name);
        }

        public IEnumerator<Script> GetEnumerator()
        {
            return _all.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _all.GetEnumerator();
        }
    }

    public class Script
    {
        public string ScriptFileName { get; }
        public string FileName => new FileInfo(ScriptFileName).Name;
        public string RelativePath
        {
            get
            {
                var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
                var path = fi.Directory.FullName;
                return ScriptFileName.Replace(path, string.Empty);
            }
        }
        public int ScriptNumber { get; }

        public Script(string scriptFileName)
        {
            var values = scriptFileName.Split(".");

            if (values.Length < 1 || !int.TryParse(values[0], out var scriptNumber))
                scriptNumber = 0;
            //throw new ApplicationException($"Script name ${scriptFileName} is invalid");
            var fi = new FileInfo(scriptFileName);

            ScriptNumber = scriptNumber;
            ScriptFileName = fi.FullName;
        }
    }
}
