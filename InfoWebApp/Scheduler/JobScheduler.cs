using Quartz;
using Quartz.Impl;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace InfoWebApp.Scheduler
{
    public static class JobScheduler
    {
        public static async Task ScheduleAsync()
        {
            var schedFact = new StdSchedulerFactory();

            var scheduler = await schedFact.GetScheduler();
            await scheduler.Start();
            
            var job = JobBuilder.Create<ScraperJob>().Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartNow()
                .WithSimpleSchedule(s => s
                    .WithIntervalInMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["schedulerIntervalMinutes"]))
                    .RepeatForever()
                )
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}