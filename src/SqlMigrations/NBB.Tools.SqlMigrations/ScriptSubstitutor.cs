using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace NBB.Tools.SqlMigrations
{
    public class ScriptSubstitutor
    {
        private readonly Dictionary<string, string> _replacements;

        public ScriptSubstitutor(IConfiguration configuration)
        {
            _replacements = configuration.GetSection("Parameters").GetChildren()
                  .ToDictionary(x => x.Key, x => x.Value);
        }

        public string SubstituteString(string spec)
        {
            var temp = spec;
            foreach (var key in _replacements.Keys)
            {
                var stuff = _replacements[key];
                var toReplace = "{{" + key + "}}";
                if (temp.Contains(toReplace))
                {
                    temp = temp.Replace(toReplace, stuff);
                }
            }
            return temp;
        }
    }
}