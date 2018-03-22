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
            var nzjzScraper = new NzjzScraper(Search);
            var vikScraper = new VikScraper(Search);
            var hepScraper = new HepScraper(Search);

            var tasks = new List<Task<List<Article>>>();
            //tasks.AddRange(vikScraper.Scrape());
            //tasks.AddRange(nzjzScraper.Scrape());
            tasks.AddRange(hepScraper.Scrape());

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
