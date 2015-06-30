using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NoCompile.NoCompile_Vsix
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false), ComVisible(true)]
    public class SettingsView : DialogPage
    {
        private string serverUrl;
        private string signKeyPath;
        private string assemblyName;

        [Category("NoCompile")]
        [DisplayName("Server url")]
        [Description("The Server url that will be used")]
        public string ServerUrl
        {
            get 
            {
                if (string.IsNullOrEmpty(this.serverUrl))
                    this.serverUrl = "http://localhost";
                
                return this.serverUrl; 
            }
            
            set 
            { 
                this.serverUrl = value; 
            }
        }

        [Category("NoCompile")]
        [DisplayName("Sign key path")]
        [Description("The path to the key file used for signing the assembly.")]
        public string SignKeyPath
        {
            get
            {
                return this.signKeyPath;
            }

            set
            {
                this.signKeyPath = value;
            }
        }

        [Category("NoCompile")]
        [DisplayName("Assembly name")]
        [Description("The name of the assembly that will be generated.")]
        public string AssemblyName
        {
            get
            {
                if (string.IsNullOrEmpty(this.assemblyName))
                    this.assemblyName = "DynammicLib";

                return this.assemblyName; 
            }

            set
            {
                this.assemblyName = value;
            }
        }

        public override void ResetSettings()
        {
            this.AssemblyName = "DynammicLib";
            this.ServerUrl = "http://localhost";
            base.ResetSettings();
        }
    }
}
