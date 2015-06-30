using ResMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;

namespace NoCompile.Web
{
    class InvokerHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var stream = GetStream("NoCompile.Web.invoker.html", typeof(InvokerHandler).Assembly);
            using (var reader = new StreamReader(stream))
            {
                var fileContents = reader.ReadToEnd();
                context.Response.Write(fileContents);
            }
        }

        private static Stream GetStream(string resourceName, System.Reflection.Assembly containingAssembly)
        {
#if DEBUG
            string filePath = null;
            if (ResRepo.TryGetPath(containingAssembly.GetName().FullName, resourceName, out filePath))
                return new FileStream(filePath, FileMode.Open);

            return null;
#else
    return containingAssembly.GetManifestResourceStream(resourceName);
#endif
        }
    }

    class InvokerRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new InvokerHandler();
        }
    }

}
