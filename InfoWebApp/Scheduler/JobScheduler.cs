using Quartz;
using Quartz.Impl;
using System;
using System.Configuration;

namespace InfoWebApp.Scheduler
{
    public class JobScheduler
    {
        public static void Schedule()
        {
            var schedFact = new StdSchedulerFactory();

            var scheduler = schedFact.GetScheduler();
            scheduler.Start();
            
            var job = JobBuilder.Create<ScraperJob>().Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartNow()
                .WithSimpleSchedule(s => s
                    .WithIntervalInMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["schedulerIntervalMinutes"]))
                    .RepeatForever()
                )
                .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}