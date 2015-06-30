using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace NoCompile.Web.Services
{
    class JsonErrorHandler : IErrorHandler
    {
        public bool HandleError(Exception error)
        {
            return true;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            fault = Message.CreateMessage(version, "", new JsonError(error), new DataContractJsonSerializer(typeof(JsonError)));

            var wbf = new WebBodyFormatMessageProperty(WebContentFormat.Json);
            fault.Properties.Add(WebBodyFormatMessageProperty.Name, wbf);

            var rmp = new HttpResponseMessageProperty()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                StatusDescription = "Uknown exception…"
            };
            rmp.Headers[HttpResponseHeader.ContentType] = "application/json";

            fault.Properties.Add(HttpResponseMessageProperty.Name, rmp);
        }
    }
}
