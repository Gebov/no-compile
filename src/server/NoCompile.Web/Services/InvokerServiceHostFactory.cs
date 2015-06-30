using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace NoCompile.Web.Services
{
    class InvokerServiceHostFactory : WebServiceHostFactory
    {
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            var serviceType = Type.GetType(constructorString, false, false);
            var sh = new WebServiceHost(serviceType, baseAddresses);

            foreach (var uri in baseAddresses)
            {
                sh.AddServiceEndpoint(typeof(IMethodInvokerService), new WebHttpBinding(), uri.AbsolutePath);
            }

            foreach (var endpoint in sh.Description.Endpoints)
            {
                endpoint.Behaviors.Add(new JsonErrorWebHttpBehaviour());
            }

            return sh;

        }
    }
}
