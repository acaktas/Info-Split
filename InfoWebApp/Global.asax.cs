using System;
using System.Collections.Generic;
using System.Web.Caching;
using InfoWebApp.Scheduler;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using InfoWebApp.Scraper;

namespace InfoWebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            JobScheduler.Schedule();

            var cash = new Cache();
            cash.Insert("articles", new List<Article>(), null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(1));
        }
    }
}
