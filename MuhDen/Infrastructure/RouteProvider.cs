using Muh.Web.Framework.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Muh.Web.Framework.Localization;

namespace MuhDen.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(System.Web.Routing.RouteCollection routes)
        {
            //We reordered our routes so the most used ones are on top. It can improve performance.

            //home page
            routes.MapLocalizedRoute("HomePage",
                            "",
                            new { controller = "Home", action = "Index" },
                            new[] { "MuhDen.Controllers" });

            //login
            routes.MapLocalizedRoute("Login",
                            "login/",
                            new { controller = "Customer", action = "Login" },
                            new[] { "MuhDen.Controllers" });
            //register
            routes.MapLocalizedRoute("Register",
                            "register/",
                            new { controller = "Customer", action = "Register" },
                            new[] { "MuhDen.Controllers" });
            //logout
            routes.MapLocalizedRoute("Logout",
                            "logout/",
                            new { controller = "Customer", action = "Logout" },
                            new[] { "MuhDen.Controllers" });
            //register result page
            routes.MapLocalizedRoute("RegisterResult",
                            "registerresult/{resultId}",
                            new { controller = "Customer", action = "RegisterResult" },
                            new { resultId = @"\d+" },
                            new[] { "MuhDen.Controllers" });

            routes.MapLocalizedRoute("CustomerAdmin",
                           "CustomerAdmin/{action}/{id}",
                           new { controller = "CustomerAdmin", action = "List", id = "" },
                            new[] { "MuhDen.Controllers" });

       
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}