using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace NoCompile.Web.Services
{
    class JsonErrorWebHttpBehaviour : WebHttpBehavior
    {
        protected override void AddServerErrorHandlers(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Clear();
            endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(new JsonErrorHandler());
        }
    }
}
