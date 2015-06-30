using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoCompile
{
    internal class CompilerOptions
    {
        public string FilePath { get; set; }

        public string SignKeyPath { get; set; }

        public string[] ReferencedAssemblyPaths { get; set; }

        public string OutputDir { get; set; }

        public string OutputAssemblyName { get; set; }

        internal string OutputAssemblyPath { get; set; }
    }
}
