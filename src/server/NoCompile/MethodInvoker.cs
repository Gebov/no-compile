using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace NoCompile
{
    internal class MethodInvoker
    {
        private static int counter = 0;
        private static IDictionary<string, CompilerResult> compileCache = new Dictionary<string, CompilerResult>();

        #region Compile

        private static bool TryCompile(CompilerOptions compileParams, out CompilerResult result)
        {
            bool compilationOccured = false;
            if (compileCache.TryGetValue(compileParams.FilePath, out result))
            {
                var lastWriteTime = File.GetLastAccessTimeUtc(compileParams.FilePath);
                if (lastWriteTime > result.LastCompileTime)
                {
                    lock (compileCache)
                    {
                        if (lastWriteTime > compileCache[compileParams.FilePath].LastCompileTime)
                        {
                            result = CompileInternal(compileParams);
                            compileCache[compileParams.FilePath] = result;
                            compilationOccured = true;
                        }
                    }
                }
            }
            else
            {
                lock (compileCache)
                {
                    result = CompileInternal(compileParams);
                    compileCache[compileParams.FilePath] = result;
                    compilationOccured = true;
                }
            }

            return compilationOccured;
        }

        private static CompilerResult CompileInternal(CompilerOptions compileParams)
        {
            Prepare(compileParams);

            var provider = new CSharpCodeProvider();

            var parameters = new CompilerParameters(compileParams.ReferencedAssemblyPaths, compileParams.OutputAssemblyPath, true);
            parameters.GenerateInMemory = false;
            parameters.GenerateExecutable = false;

            if (!string.IsNullOrEmpty(compileParams.SignKeyPath))
                parameters.CompilerOptions = string.Format("/keyFile:\"{0}\"", compileParams.SignKeyPath);

            var compilationResults = provider.CompileAssemblyFromFile(parameters, compileParams.FilePath);

            if (compilationResults.Errors != null && compilationResults.Errors.Count > 0)
            {
                var builder = new StringBuilder();
                var compilerErrors = compilationResults.Errors.OfType<CompilerError>().Where(x => !x.IsWarning);

                foreach (var error in compilerErrors)
                {
                    builder.AppendLine("An error occured during the compilation process.");
                    builder.AppendFormat("Error {0}: {1}. Line: {2} Column: {3}.", error.ErrorNumber, error.ErrorText, error.Line, error.Column).AppendLine();
                    builder.AppendFormat("File: {0}", error.FileName).AppendLine();
                }

                throw new CompilationException(builder.ToString(), 
                    compilationResults.Errors, compilationResults.Output);
            }

            return new CompilerResult()
            {
                LastCompileTime = DateTime.UtcNow,
                PathToAssembly = compilationResults.PathToAssembly
            };
        }

        private static void Prepare(CompilerOptions options)
        {
            if (string.IsNullOrEmpty(options.OutputAssemblyName))
                options.OutputAssemblyName = Guid.NewGuid().ToString();

            if (string.IsNullOrEmpty(options.OutputDir))
                options.OutputDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var folderName = "artefacts" + counter++;
            var folderPath = Path.Combine(options.OutputDir, folderName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var outputAssemblyPath = Path.Combine(folderPath, options.OutputAssemblyName);
            if (!outputAssemblyPath.EndsWith(".dll"))
                outputAssemblyPath = string.Concat(outputAssemblyPath, ".dll");

            options.OutputAssemblyPath = outputAssemblyPath;
        }

        #endregion

        #region Invoke

        private static Assembly LoadAssembly(string assemblyPath)
        {
            using (var stream = new FileStream(assemblyPath, FileMode.Open))
            {
                using (var memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);
                    var bytes = memStream.ToArray();
                    return Assembly.Load(bytes);
                }
            }    
        }

        private static void Invoke(Assembly asm, InvokeOptions invokeOptions)
        {
            var type = asm.GetTypes().FirstOrDefault(x => x.FullName == invokeOptions.ClassName);
            var method = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .FirstOrDefault(x => x.Name == invokeOptions.MethodName && x.GetParameters().Count() == 0);

            if (method != null)
            {
                if (invokeOptions.Async)
                {
                    ThreadPool.QueueUserWorkItem(InvokeMethod, method);
                }
                else
                {
                    MethodInvoker.InvokeMethod(method);
                }


            }
            else throw new ArgumentOutOfRangeException("No parameterless method (static or instance) found that matches the provided name.");
        }

        private static void InvokeMethod(object state)
        {
            var method = state as MethodInfo;
            if (method != null)
            {
                method.Invoke(null, null);
            }

            //else
            //{
            //    var instance = Activator.CreateInstance(type);
            //    method.Invoke(instance, null);
            //}
        }

        public static void Execute(InvokeOptions invokeOptions, CompilerOptions compilerOptions)
        {
            CompilerResult result = null;
            if (TryCompile(compilerOptions, out result))
                result.CompiledAssembly = LoadAssembly(result.PathToAssembly);

            Invoke(result.CompiledAssembly, invokeOptions);
        }

        #endregion
    }

    class CompilerResult
    {
        public DateTime LastCompileTime { get; set; }

        public string PathToAssembly { get; set; }

        public Assembly CompiledAssembly { get; set; }
    }
}