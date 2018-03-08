﻿using System.Collections.Specialized;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace InfoWebApp.Scheduler
{
    public class JobScheduler
    {
        public static void Schedule()
        {
            var props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            };
            var factory = new StdSchedulerFactory(props);
            
            var scheduler = factory.GetScheduler().Result;
            scheduler.Start().Wait();
            
            var job = JobBuilder.Create<ScraperJob>().Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartNow()
                .WithSimpleSchedule(s => s
                    .WithIntervalInMinutes(1)
                    .RepeatForever()
                )
                .Build();

            scheduler.ScheduleJob(job, trigger).Wait();
        }
    }
}