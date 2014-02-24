using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

public class QuartzDatastore
{

    /// <summary>
    /// Scheduled Job List
    /// </summary>
    public Dictionary<String, Quartz.Collection.ISet<JobKey>> Jobs { get; set; }
    private IScheduler Scheduler { get; set; }

    private void GetAllJobs()
    {

        IList<string> jobGroups = this.Scheduler.GetJobGroupNames();
        IList<string> triggerGroups = this.Scheduler.GetTriggerGroupNames();

        Dictionary<String, Quartz.Collection.ISet<JobKey>> grps = new Dictionary<String, Quartz.Collection.ISet<JobKey>>();

        foreach (string group in jobGroups)
        {


            var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
            Quartz.Collection.ISet<JobKey> jobKeys = this.Scheduler.GetJobKeys(groupMatcher);

            grps.Add(group, jobKeys);

            foreach (var jobKey in jobKeys)
            {
                var detail = this.Scheduler.GetJobDetail(jobKey);
                var triggers = this.Scheduler.GetTriggersOfJob(jobKey);
                foreach (ITrigger trigger in triggers)
                {
                    Console.WriteLine(group);
                    Console.WriteLine(jobKey.Name);
                    Console.WriteLine(detail.Description);
                    Console.WriteLine(trigger.Key.Name);
                    Console.WriteLine(trigger.Key.Group);
                    Console.WriteLine(trigger.GetType().Name);
                    Console.WriteLine(this.Scheduler.GetTriggerState(trigger.Key));
                    DateTimeOffset? nextFireTime = trigger.GetNextFireTimeUtc();
                    if (nextFireTime.HasValue)
                    {
                        Console.WriteLine(nextFireTime.Value.LocalDateTime.ToString());
                    }

                    DateTimeOffset? previousFireTime = trigger.GetPreviousFireTimeUtc();
                    if (previousFireTime.HasValue)
                    {
                        Console.WriteLine(previousFireTime.Value.LocalDateTime.ToString());
                    }
                }
            }

            this.Jobs = grps;
        }
    }

    /// <summary>
    /// Quartz Job Scheduler - ServerScheduler
    /// </summary>
    public QuartzDatastore()
    {

        NameValueCollection properties = new NameValueCollection();
        properties["quartz.scheduler.instanceName"] = "ServerScheduler";

        // set thread pool info
        properties["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
        properties["quartz.threadPool.threadCount"] = "5";
        properties["quartz.threadPool.threadPriority"] = "Normal";

        // set remoting expoter
        properties["quartz.scheduler.proxy"] = "true";
        properties["quartz.scheduler.proxy.address"] = "tcp://localhost:555/QuartzScheduler";
        // First we must get a reference to a scheduler
        ISchedulerFactory sf = new StdSchedulerFactory(properties);

        //Any schedulers?
        if (sf.AllSchedulers.Count() == 0)
        {
            this.Scheduler = sf.GetScheduler();
        }
        else {
            this.Scheduler = sf.AllSchedulers.First();
        }

        //Load jobs from Scheduler
        GetAllJobs();

    }

    public void ScheduleJob()
    {

        IJobDetail iJob = JobBuilder.Create<HelloWorld>()
            .WithIdentity("myJob", "group1")
            .Build();

        JobDataMap map = new JobDataMap();
        map.Put("msg", "Your remotely added job has executed!");

        ITrigger tgr = TriggerBuilder.Create()
           .WithIdentity("myTrigger", "group1")
           .StartNow()
           .WithSimpleSchedule(x => x
               .WithIntervalInSeconds(10)
               .WithRepeatCount(3))
               .Build();

        //Add to Scheduler
        Scheduler.ScheduleJob(iJob, tgr);

    }

}