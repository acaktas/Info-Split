using Quartz;

namespace InfoWebApp.Scheduler
{
    public class ScraperJob : IJob
    {
        public void Execute(IJobExecutionContext context) => new Scraper.Scraper().Scrape().GetAwaiter();
    }
}