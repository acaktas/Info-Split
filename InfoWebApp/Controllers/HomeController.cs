using System;
using InfoWebApp.DAL;
using InfoWebApp.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace InfoWebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            List<Article> cashedArticles;

            using (var articleContext = new ArticleContext())
            {
                cashedArticles = articleContext.Articles.Where(a =>
                    DbFunctions.TruncateTime(a.UpdateDateTime) == DbFunctions.TruncateTime(DateTime.Today)
                    || DbFunctions.TruncateTime(a.CreatedDateTime) == DbFunctions.TruncateTime(DateTime.Today)).ToList();
            }

            return View(cashedArticles);
        }
    }
}