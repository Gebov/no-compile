using NoCompile.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Text;
using System.Web.Routing;

namespace NoCompile.Web
{
    public class Injector
    {
        public static void OnStart()
        {
            RouteTable.Routes.Add(Constants.InvokerServiceRouteName, new ServiceRoute("invoker/api/v1", new WebServiceHostFactory(), typeof(MethodInvokerService)));

#if DEBUG
            RouteTable.Routes.Add(Constants.InvokerFormRouteName, new Route("invoker", new InvokerRouteHandler()));
#endif
        }

        public static void AddRoutes(RouteCollection routes)
        {
            if (routes[Constants.InvokerServiceRouteName] != null)
                RouteTable.Routes.Add(Constants.InvokerServiceRouteName, new ServiceRoute("invoker/api/v1", new InvokerServiceHostFactory(), typeof(MethodInvokerService)));
        }

        public class Constants
        {
            public const string InvokerServiceRouteName = "invoker-service";
            public const string InvokerFormRouteName = "invoker-form";
        }
    }
}
