using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClassLibrary1
{
    public class ScssCompiler
    {
        private const string NamePattern = @"(?<name>\$[\-\w]+)";
        private readonly Regex _variableName = new Regex(NamePattern);
        private readonly Regex _variableNameAndValue = new Regex(NamePattern + @"\s*:\s*(?<value>.+);[\s]*");

        public string Compile(string scss)
        {
            var result = DebugCompile(scss);
            return result.Css;
        }

        public CompileResult DebugCompile(string scss)
        {
            var vars = GetVariables(scss);
            var assignedVars = GetAssignments(scss);
            scss = StripAssignments(scss);

            var css = assignedVars.Aggregate(scss, (current, variable) => current.Replace(variable.Key, variable.Value));
            
            var unassigned = vars.Except(assignedVars.Keys).ToList();
            var errors = string.Join(Environment.NewLine,
                unassigned.Select(x => string.Format("/* Error: {0} not defined. */", x)));

            var finalCss = string.IsNullOrWhiteSpace(errors)
                ? css
                : string.Join(Environment.NewLine, errors, css);

            return new CompileResult
            {
                Css = finalCss,
                Variables = assignedVars
            };
        }
        
        private string StripAssignments(string scssContents)
        {
            scssContents = _variableNameAndValue.Replace(scssContents, "");
            return scssContents;
        }

        private Dictionary<string, string> GetAssignments(string contents)
        {
            var match = _variableNameAndValue.Matches(contents);
            var assignments = new Dictionary<string, string>();

            foreach (Match m in match)
            {
                assignments.Add(m.Groups["name"].Value, m.Groups["value"].Value);
            }

            return assignments;
        }

        private List<string> GetVariables(string contents)
        {
            var match = _variableName.Matches(contents);
            var variables = new List<string>();

            foreach (Match m in match)
            {
                variables.Add(m.Groups["name"].Value);
            }

            return variables;
        }
    }
}