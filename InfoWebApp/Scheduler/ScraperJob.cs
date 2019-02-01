using System.Threading.Tasks;
using Quartz;

namespace InfoWebApp.Scheduler
{
    public class ScraperJob : IJob
    {
        Task IJob.Execute(IJobExecutionContext context) => new Scraper.Scraper().Scrape();
    }
}