using System.Collections.Generic;
using System.Threading.Tasks;
using InfoWebApp.Models;

namespace InfoWebApp.Scraper
{
    internal interface IScraper
    {
        Task<List<Article>>[] Scrape();
    }
}
