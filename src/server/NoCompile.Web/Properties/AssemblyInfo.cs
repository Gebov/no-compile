using NoCompile.Web;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;

[assembly: AssemblyTitle("NoCompile.Web")]
[assembly: AssemblyDescription("Web interface for invoking a method.")]
[assembly: ComVisible(false)]
[assembly: Guid("5dfa7724-89a9-471f-87c3-2ab5f7461066")]

[assembly: PreApplicationStartMethod(typeof(Injector), "OnStart")]
