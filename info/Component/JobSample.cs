using Quartz;
using System;
using System.Diagnostics;

public class HelloWorld : IJob
{

    public void Execute(IJobExecutionContext context)
    {
        EventLog myLog = new EventLog();
        myLog.Source = "MYEVENTSOURCE";

        // Write an informational entry to the event log.    
        myLog.WriteEntry("Job has run!");

    }
}