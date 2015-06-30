using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace NoCompile
{
    internal class CompilationException : Exception
    {
        public CompilationException(string message, CompilerErrorCollection errors, StringCollection output)
            : base (message)
        {
            this.Errors = errors;
            this.Ouput = output;
        }

        public StringCollection Ouput { get; set; }

        public CompilerErrorCollection Errors { get; set; }
    }
}
