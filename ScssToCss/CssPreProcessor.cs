using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace ClassLibrary1
{
    public class CssPreProcessor
    {
        private readonly IFileSystem _fileSystem;
        private readonly ScssCompiler _compiler;

        public CssPreProcessor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _compiler = new ScssCompiler();
        }

        public Dictionary<string, string> BundleAndCompile(string file)
        {
            if (file.IsScssPartialFileName())
                return new Dictionary<string, string>();

            if (!_fileSystem.File.Exists(file))
            {
                throw new FileNotFoundException(string.Format("'{0}' not found.", file), file);
            }

            var contents = _fileSystem.File.ReadAllText(file);
            var imports = ScanForImportedFiles(contents);


            var compiledContents = _compiler.Compile(contents);
            var outName = file.Replace(".scss", ".css");

            return new Dictionary<string, string>
            {
                {outName, compiledContents}
            };
        }

        private Dictionary<string, string> ScanForImportedFiles(string contents)
        {
            var importDeclaration = new Regex(@"@import '(?<importFileName>[\w\d\-\.]+)';");
            var match = importDeclaration.Matches(contents);

            var dictionary = new Dictionary<string, string>();
            foreach (Match m in match)
            {
                var importedFilename = m.Groups["importFileName"].Value;
                
                try
                {
                    _fileSystem.File.ReadAllText(importedFilename + ".scss");
                }
                catch (FileNotFoundException ex)
                {
                    throw new FileNotFoundException(string.Format("File to import not found: '{0}'.", importedFilename), ex);
                }
            }

            return dictionary;
        }
    }
}