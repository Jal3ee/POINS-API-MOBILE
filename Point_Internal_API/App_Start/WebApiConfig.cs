using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Routing;

namespace Point_Internal_API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            EnableCorsAttribute cors = new EnableCorsAttribute("*", "*", "GET,POST,OPTIONS,PUT,DELETE");
            config.EnableCors(cors);

            // Web API configuration and services
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            // Ignore MVC routes
            //RouteTable.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            //RouteTable.Routes.IgnoreRoute("{resource}.aspx/{*pathInfo}");
            //RouteTable.Routes.IgnoreRoute("{resource}.svc/{*pathInfo}");
        }
    }
}
