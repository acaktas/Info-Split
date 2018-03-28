using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls.Expressions;
using InfoWebApp.Models;

namespace InfoWebApp.Scraper
{
    public class Scraper
    {
        private static readonly string[] Search = { "Sućidar", "Ivaniševićeva", "Drage Ivaniševića" };

        public async Task<List<Article>> Scrape()
        {
            var tasks = new List<Task<List<Article>>>();

            var scrapers = new List<IScraper>();
            scrapers.Add(new NzjzScraper(Search));
            scrapers.Add(new VikScraper(Search));
            scrapers.Add(new HepScraper(Search));

            foreach (var scraper in scrapers)
            {
                tasks.AddRange(scraper.Scrape());
            }
            
            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch
            {
                // ignored
            }

            var articles = new List<Article>();
            foreach (var task in tasks)
            {
                if (task.Exception != null) continue;

                foreach (var article in task.Result)
                {
                    if (!articles.Any(a=>a.Equals(article)))
                    {
                        articles.Add(article);
                    }
                }

            }

            return articles;
        }
    }
}
