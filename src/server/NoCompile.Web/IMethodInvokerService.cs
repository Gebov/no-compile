using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

namespace NoCompile.Web
{
    [ServiceContract]
    interface IMethodInvokerService
    {
        [OperationContract]
        [WebGet(UriTemplate = "assemblies", ResponseFormat = WebMessageFormat.Json)]
        string[] GetAssemblies();

        //[OperationContract]
        //[WebGet(UriTemplate = "types?asm={assemblyName}", ResponseFormat = WebMessageFormat.Json)]
        //string[] GetTypes(string assemblyName);

        //[OperationContract]
        //[WebGet(UriTemplate = "methods?type={typeName}", ResponseFormat = WebMessageFormat.Json)]
        //string[] GetMethods(string typeName);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "invoke-json", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        void InvokeMethodJson(InvokeParams options);

        //[OperationContract]
        //[WebInvoke(Method = "POST", UriTemplate = "invoke-raw", BodyStyle = WebMessageBodyStyle.Bare)]
        //void InvokeMethodRaw(Stream input);
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
}
