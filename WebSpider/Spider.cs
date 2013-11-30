using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Threading;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Text;

namespace myLegis.Spider
{
    public class Spider
    {
       // private const string dataDir = @"C:\Users\ragingsmurf\Documents\Spider\";
        private static bool finished;

        //private List<String> SessionActivity(DateTime Start, DateTime End)
        //{

        //    WebSiteDownloaderOptions options =
        //        new WebSiteDownloaderOptions();
        //    options.DestinationFolderPath =
        //        new DirectoryInfo(dataDir);
        //    options.DestinationFileName = String.Format("Session-Activity[{0}][{1}].state",
        //                                                Start.Date.ToShortDateString().Replace("/", "-"),
        //                                                End.Date.ToShortDateString().Replace("/", "-"));
        //    options.MaximumLinkDepth = 0;
        //    options.TargetSession = 28;
        //    options.DownloadUri =
        //        new Uri(String.Format(@"http://www.legis.state.ak.us/basis/range_multi.asp?session={0}&Date1={1}&Date2={2}",
        //            options.TargetSession,
        //            Start.Date.ToShortDateString(),
        //            End.Date.ToShortDateString()));

        //    //Get all bill links.
        //    options.GitCollectionRequest.Add(new DocumentHrefList()
        //    {
        //        pageName = "range_multi.asp",
        //        pageType = UriType.Content,
        //        pattern = new Regex(@"(?<=[=])[H|R|S][B|C|R|J]{0,3}[0-9]{1,4}", RegexOptions.IgnoreCase)
        //    });

        //    //Download que engine.
        //    WebSiteDownloader downloader = new WebSiteDownloader(options);

        //    downloader.ProcessingUrl +=
        //       new WebSiteDownloader.ProcessingUrlEventHandler(
        //       downloader_ProcessingUrl);

        //    downloader.ProcessCompleted +=
        //        new WebSiteDownloader.ProcessCompletedEventHandler(
        //        downloader_ProcessCompleted);

        //    downloader.ProcessAsync();

        //    while (true)
        //    {
        //        Thread.Sleep(1000);
        //        Console.WriteLine(@".");

        //        lock (typeof(Spider))
        //        {
        //            if (finished)
        //            {
        //                break;
        //            }
        //        }
        //    }

        //    Console.WriteLine(@"finished processing.");

        //    foreach (iCollector col in downloader.Parsings)
        //        Console.WriteLine(String.Format("Rule found for {0}", col.pageName));

        //    //Reset the exit.
        //    finished = false;

        //    //Grab saved targets.
        //    return ((DocumentHrefList)downloader.Parsings[0]).matches;

        //}

        public static WebSiteDownloader DownloadingProcessor(WebSiteDownloaderOptions options)
        {
            //Download que engine.
            WebSiteDownloader downloader = new WebSiteDownloader(options);

            downloader.ProcessingUrl +=
               new WebSiteDownloader.ProcessingUrlEventHandler(
               downloader_ProcessingUrl);

            downloader.ProcessCompleted +=
                new WebSiteDownloader.ProcessCompletedEventHandler(
                downloader_ProcessCompleted);

            downloader.ProcessAsync();

            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine(@".");

                lock (typeof(Spider))
                {
                    if (finished)
                    {
                        break;
                    }
                }
            }

            Console.WriteLine(@"finished processing.");

            foreach (iCollector col in downloader.Parsings)
                Console.WriteLine(String.Format("Rule found for {0}", col.pageName));

            finished = false;

            return downloader;

        }

        private static void downloader_ProcessingUrl(
            object sender,
            WebSiteDownloader.ProcessingUrlEventArgs e)
        {
            Console.WriteLine(
                string.Format(
                @"Processing URL '{0}'.", e.UriInfo.AbsoluteUri));
        }

        private static void downloader_ProcessCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Console.WriteLine(@"Error: " + e.Error.Message);
            }
            else if (e.Cancelled)
            {
                Console.WriteLine(@"Canceled");
            }
            else
            {
                Console.WriteLine(@"Success");
            }

            lock (typeof(Spider))
            {
                finished = true;
            }
        }

    }
}
