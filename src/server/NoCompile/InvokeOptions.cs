using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoCompile
{
    internal class InvokeOptions
    {
        public string ClassName { get; set; }

        public string MethodName { get; set; }

        public bool Async { get; set; }
    }
}
