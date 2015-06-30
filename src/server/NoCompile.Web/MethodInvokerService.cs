using NoCompile.Web.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Web;
using System.Web.Compilation;

namespace NoCompile.Web
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [ErrorBehaviorAttribute(typeof(JsonErrorHandler))]
    class MethodInvokerService : IMethodInvokerService
    {
        public string[] GetAssemblies()
        {
            throw new Exception("It not works");
            var referencesAssemblies = BuildManager.GetReferencedAssemblies().OfType<Assembly>().Select(x => x.GetName().Name).ToArray();
            return referencesAssemblies;
        }

        public string[] GetTypes(string assemblyName)
        {
            var assembly = Assembly.Load(assemblyName);
            var typeNames = assembly.GetTypes().Select(x => x.FullName).ToArray();

            return typeNames;
        }

        public string[] GetMethods(string typeName)
        {
            var type = BuildManager.GetType(typeName, false);
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public).Select(x => x.Name).ToArray();
            return methods;
        }

        public void InvokeMethodJson(InvokeParams options)
        {
            this.InvokeMethodInternal(options);
        }

        public void InvokeMethodRaw(Stream input)
        {
            using (var reader = new StreamReader(input))
            {
                var body = reader.ReadToEnd();
                var inputParams = HttpUtility.ParseQueryString(body);

                var options = new InvokeParams()
                {
                    TypeName = inputParams["typeName"],
                    MethodName = inputParams["methodName"],
                    FilePath = inputParams["filePath"]
                };

                this.InvokeMethodInternal(options);
            }

            HttpContext.Current.Response.Redirect("/invoker-form", true);
        }


        private void InvokeMethodInternal(InvokeParams invokeParams)
        {
            var invokeOptions = new InvokeOptions()
            {
                ClassName = invokeParams.TypeName,
                MethodName = invokeParams.MethodName,
                Async = true
            };

            var compilerOptions = new CompilerOptions()
            {
                FilePath = invokeParams.FilePath,
                ReferencedAssemblyPaths = BuildManager.GetReferencedAssemblies().OfType<Assembly>().Select(x => x.Location).ToArray(),
                SignKeyPath = invokeParams.KeyPath,
                OutputAssemblyName = invokeParams.AsmName
            };

            MethodInvoker.Execute(invokeOptions, compilerOptions);
        }
    }
}
