using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.CommandBars;
using EnvDTE80;
using EnvDTE;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace NoCompile.NoCompile_Vsix
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    //[ProvideToolWindow(typeof(MyToolWindow))]
    [Guid(GuidList.guidNoCompile_VsixPkgString)]
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
    [ProvideOptionPage(typeof(SettingsView), "NoCompile", "NoCompileSettings", 0, 0, true)]
    public sealed class NoCompile_VsixPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.s
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public NoCompile_VsixPackage()
        {
            //Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            //ToolWindowPane window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
            //if ((null == window) || (null == window.Frame))
            //{
            //    throw new NotSupportedException(Resources.CanNotCreateWindow);
            //}
            //IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            //Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }


        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();
            
            var mcs = this.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                var menuCommandID = new CommandID(GuidList.guidNoCompile_VsixCmdSet, (int)PkgCmdIDList.cmdidInvoke);
                var menuItem = new OleMenuCommand(OnMenuItemCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                menuItem.BeforeQueryStatus += OnMenuItemQueryStatus;
            }
        }

        private void OnMenuItemQueryStatus(object sender, EventArgs e)
        {
            var command = sender as OleMenuCommand;
            if (command != null)
            {
                var appObj = (DTE)this.GetService(typeof(DTE));
                if (appObj != null)
                    command.Enabled = appObj.Debugger.DebuggedProcesses.Count != 0;
            }
        }

        #endregion

        private void OnMenuItemCallback(object sender, EventArgs e)
        {

            var appObj = (DTE)this.GetService(typeof(DTE));
            if (appObj.Debugger.DebuggedProcesses.Count == 0)
                return;
                
            var path = appObj.ActiveDocument.FullName;
            
            var currentDocument = appObj.ActiveDocument.Object() as TextDocument;
            var currentPoint = currentDocument.Selection.ActivePoint as TextPoint;

            var codeElements = appObj.ActiveDocument.ProjectItem.FileCodeModel.CodeElements;
            foreach (CodeElement entry in codeElements)
            {
                if (entry.Kind == vsCMElement.vsCMElementImportStmt)
                    continue;

                var method = this.FindConainingMethod(currentPoint, entry);
                if (method != null)
                {
                    if (!method.IsShared || method.Parameters.Count > 0)
                    {
                        this.WriteToOutput("Only static methods with no parameters are supported.");
                        return;
                    }

                    var settings = appObj.get_Properties("NoCompile", "NoCompileSettings");
                    var serverUrl = (string)settings.Item("ServerUrl").Value;
                    if (string.IsNullOrEmpty(serverUrl))
                    {
                        this.WriteToOutput("Server url is invalid. Check the configuration under Tools/NoComile.");
                        return;
                    }

                    var serviceUrl = serverUrl.TrimEnd(new char[] { '/' }) + "/invoker/api/v1/invoke-json";
                    var request = (HttpWebRequest)WebRequest.Create(serviceUrl);
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.Timeout = 1000 * 10; // 10 secs

                    using (var stream = new MemoryStream())
                    {
                        var name = method.Name;
                        var className = (method.Parent as CodeType).FullName;

                        var invokeParmas = new InvokeParams()
                        {
                            TypeName = className,
                            MethodName = name,
                            FilePath = path,
                            AsmName = (string)settings.Item("AssemblyName").Value,
                            KeyPath = (string)settings.Item("SignKeyPath").Value,
                        };

                        var serializer = new DataContractJsonSerializer(typeof(InvokeParams));
                        serializer.WriteObject(stream, invokeParmas);

                        var json = Encoding.UTF8.GetString(stream.ToArray());
                        json = @"{""options"":" + json + "}";

                        try
                        {
                            using (var writer = new StreamWriter(request.GetRequestStream()))
                            {
                                writer.Write(json);
                            }
                        }
                        catch (Exception err)
                        {
                            this.WriteToOutput(err.Message);
                        }

                        ThreadPool.QueueUserWorkItem((x) =>
                        {
                            try
                            {
                                var response = request.GetResponse();
                            }
                            catch (WebException err)
                            {
                                if (err.Response != null)
                                {
                                    try
                                    {
                                        using (var responseStream = err.Response.GetResponseStream())
                                        {
                                            var errorSerializer = new DataContractJsonSerializer(typeof(JsonError));
                                            var jsonErr = (JsonError)errorSerializer.ReadObject(responseStream);
                                            this.WriteToOutput(jsonErr.Message);
                                        }
                                    }
                                    catch { }
                                }
                                else
                                {
                                    this.WriteToOutput(err.Message);
                                }
                            }
                            catch (Exception ex)
                            {
                                this.WriteToOutput(ex.Message);
                            }

                        }, request);
                    }

                    break;
                }    
            }
        }

        private static readonly Guid outputPane = Guid.Parse("356BDBDE-EAD4-406E-8E60-45D8D1BD9981");

        private void WriteToOutput(string message)
        {
            const int VISIBLE = 1;
            const int DO_NOT_CLEAR_WITH_SOLUTION = 0;
            
            IVsOutputWindow outputWindow;
            IVsOutputWindowPane outputWindowPane = null;
            int hr;

            // Get the output window
            outputWindow = base.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            hr = outputWindow.GetPane(outputPane, out outputWindowPane);
            if (outputWindowPane == null)
            {
                hr = outputWindow.CreatePane(outputPane, "NoCompile", VISIBLE, DO_NOT_CLEAR_WITH_SOLUTION);
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
                outputWindow.GetPane(outputPane, out outputWindowPane);
            }
            
            // Output the text
            if (outputWindowPane != null)
            {
                outputWindowPane.Activate();
                outputWindowPane.OutputString(message);
                outputWindowPane.OutputString(Environment.NewLine);
            }
        }


        private CodeFunction FindConainingMethod(TextPoint currentPoint, CodeElement element)
        {
            if (element.Kind == vsCMElement.vsCMElementFunction)
            {
                if (currentPoint.Line >= element.StartPoint.Line && currentPoint.Line <= element.EndPoint.Line)
                {
                    return (CodeFunction)element;
                }
            }

            var children = this.GetChildElements(element);
            if (children != null)
            {
                foreach (CodeElement entry in children)
                {
                    var method = this.FindConainingMethod(currentPoint, entry);
                    if (method != null)
                        return method;
                }
            }

            return null;
        }

        private CodeElements GetChildElements(CodeElement parent)
        {
            var namespaceElement = parent as CodeNamespace;
            if (namespaceElement != null)
                return namespaceElement.Members;

            var typeElement = parent as CodeType;
            if (typeElement != null)
                return typeElement.Members;

            return null;
        }
    }

    [DataContract]
    class InvokeParams
    {
        [DataMember]
        public string TypeName { get; set; }

        [DataMember]
        public string MethodName { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string KeyPath { get; set; }

        [DataMember]
        public string AsmName { get; set; }
    }

    [DataContract]
    public class JsonError
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string StackTrace { get; set; }
    }
}
