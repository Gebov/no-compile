using System;
using System.Runtime.Serialization;

namespace NoCompile.Web.Services
{
    [DataContract]
    public class JsonError
    {
        public JsonError(Exception err)
        {
            this.Message = err.Message;
            this.StackTrace = err.StackTrace;
        }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string StackTrace { get; set; }
    }
}
