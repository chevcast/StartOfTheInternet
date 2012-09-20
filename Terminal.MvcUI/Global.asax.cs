using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Terminal.Domain.Settings;
using Terminal.Domain.Entities;
using System.Configuration;
using System.Data.Entity;

namespace Terminal.MvcUI
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}", // URL with parameters
                new { controller = "Terminal", action = "Index", Cli = "INITIALIZE" } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            Database.SetInitializer<EntityContainer>(new MigrateDatabaseToLatestVersion<EntityContainer, Terminal.Domain.Migrations.Configuration>());
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}