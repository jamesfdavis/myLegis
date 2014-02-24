using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using myLegis;
using myLegis.Spider;
using myLegis.Spider.Models;
using System.Configuration;

namespace info.Tests
{
    [TestClass]
    public class SingleBill
    {

        private string dataDir = ConfigurationManager.AppSettings["appData"];

        //[TestMethod]
        public void Get_Commit_History_For_Single_File()
        {

            GitRepository gr = new GitRepository();
            List<IVersionHistory> list = gr.ViewHistory(@"28\HB129\", @"content.txt");

            List<IVersionHistory> list2 = gr.ViewHistory(@"28\HB129\", @"bill.xml");

            var stp = @"Stop";

        }

        /// <summary>
        /// Re-inflates the search results.
        /// </summary>
        //[TestMethod]
        public void Run_Spider_On_Activity_Search_Results()
        {

            DateTime Start = new DateTime(2013, 2, 15);
            DateTime End = new DateTime(2013, 2, 18);

            WebSiteDownloaderOptions options = new WebSiteDownloaderOptions();
            options.DestinationFolderPath = new DirectoryInfo(dataDir);
            options.DestinationFileName = String.Format("Session-Activity[{0}][{1}].state",
                                            Start.Date.ToShortDateString().Replace("/", "-"),
                                            End.Date.ToShortDateString().Replace("/", "-"));

            //Download que engine.
            WebSiteDownloader downloader = new WebSiteDownloader(options);

            List<iCollector> coll = (from p in downloader.Parsings
                                     select p).ToList();

            //RegEx for matching bill copy.
            Regex r = new Regex("get[_]bill[_]text[.]asp");

            //Get all matches.
            List<iCollector> refined = (from el in coll
                                        let matches = r.Matches(el.source.AbsoluteUri.AbsoluteUri)
                                        where matches.Count != 0
                                        select el).ToList();

            Assert.IsTrue(coll.Count() > 0);

        }

        /// <summary>
        /// Basic version of the spider, fewer spidering options are set.
        /// </summary>
        //[TestMethod]
        public void Run_Spider_On_Activity_Parsing_Run()
        {

            DateTime Start = new DateTime(2013, 2, 15);
            DateTime End = new DateTime(2013, 2, 18);

            WebSiteDownloaderOptions options =
               new WebSiteDownloaderOptions();
            options.DestinationFolderPath =
                new DirectoryInfo(dataDir);
            options.DestinationFileName = String.Format("Session-Activity[{0}][{1}].state",
                                            Start.Date.ToShortDateString().Replace("/", "-"),
                                            End.Date.ToShortDateString().Replace("/", "-"));
            options.MaximumLinkDepth = 1;
            options.TargetSession = 28;
            options.DownloadUri =
                new Uri(String.Format(@"http://www.legis.state.ak.us/basis/range_multi.asp?session={0}&Date1={1}&Date2={2}",
                            options.TargetSession,
                            Start.Date.ToShortDateString(),
                            End.Date.ToShortDateString()));

            //Create git repo hooks.
            options.GitCollectionRequest.Add(new DocumentHistory() { pageName = "get_fulltext.asp", pageType = UriType.Form });
            options.GitCollectionRequest.Add(new DocumentActivity() { pageName = "get_complete_bill.asp", pageType = UriType.Form });
            options.GitCollectionRequest.Add(new DocumentCopy() { pageName = "get_bill_text.asp", pageType = UriType.Content });
            options.GitCollectionRequest.Add(new DocumentCopy() { pageName = "get_single_minute.asp", pageType = UriType.Content });

            //Regular expression matches.
            var d = new Dictionary<String, Regex>();
            d.Add("Title", new Regex(@"(?<=<b>TITLE:</b>)[a-z,\s,\w,(,),"",',;,.]{5,800}", RegexOptions.IgnoreCase));
            d.Add("Short Title", new Regex(@"(?<=<b>SHORT TITLE:</b>)(.*)(?=</font>)", RegexOptions.IgnoreCase));
            d.Add("Status Date", new Regex(@"(?<=<b>STATUS DATE:</b>)[0-9,/\,\w\s]{5,50}", RegexOptions.IgnoreCase));
            d.Add("Current Status", new Regex(@"(?<=<b>CURRENT STATUS:</b>)[a-z,\s,\ ,(,)]{10,60}", RegexOptions.IgnoreCase));
            d.Add("Sponsors", new Regex(@"(?<=<b>SPONSOR[(]S[)]:</b>)[a-z,\s,\w,(,),.,;,""]{5,800}", RegexOptions.IgnoreCase));
            //Regex kvp matching container.
            options.GitCollectionRequest.Add(new DocumentKVP() { pageName = "get_bill.asp", pageType = UriType.Content, rvp = d });

            //Re-inflate the indexing.
            WebSiteDownloader rslt = Spider.DownloadingProcessor(options);

            Assert.IsTrue(rslt.Parsings.Count() > 0);

        }

        //[TestMethod]
        public void Run_Spider_On_Activity_Parsing_Using_iFollower()
        {

            DateTime Start = new DateTime(2013, 2, 16);
            DateTime End = new DateTime(2013, 2, 21);

            WebSiteDownloaderOptions options =
               new WebSiteDownloaderOptions();
            options.DestinationFolderPath =
                new DirectoryInfo(dataDir);
            options.DestinationFileName = String.Format("Session-Activity[{0}][{1}].state",
                                            Start.Date.ToShortDateString().Replace("/", "-"),
                                            End.Date.ToShortDateString().Replace("/", "-"));
            options.MaximumLinkDepth = 3;
            options.TargetSession = 28;
            options.DownloadUri =
                new Uri(String.Format(@"http://www.legis.state.ak.us/basis/range_multi.asp?session={0}&Date1={1}&Date2={2}",
                            options.TargetSession,
                            Start.Date.ToShortDateString(),
                            End.Date.ToShortDateString()));

            options.UriFollower.Add(new Follow { depth = 1, pattern = new Regex("get_bill.asp") });
            options.UriFollower.Add(new Follow { depth = 2, pattern = new Regex("get_fulltext.asp") });
            options.UriFollower.Add(new Follow { depth = 3, pattern = new Regex("get_bill_text.asp") });

            //Re-inflate the indexing.
            WebSiteDownloader rslt = Spider.DownloadingProcessor(options);

            Assert.IsTrue(rslt.Resources.Count() > 0);

        }

        /// <summary>
        /// Primary spider index and save to the result's .state file. 
        /// </summary>
        //[TestMethod]
        public void Run_Spider_On_Activity_Parsing_Using_iFollower_And_Saving_Using_GitCollector()
        {

            DateTime Start = new DateTime(2014, 2, 19);
            DateTime End = new DateTime(2015, 2, 21);

            WebSiteDownloaderOptions options =
               new WebSiteDownloaderOptions();
            options.DestinationFolderPath =
                new DirectoryInfo(dataDir);
            options.DestinationFileName = String.Format("Session-Activity[{0}][{1}].state",
                                            Start.Date.ToShortDateString().Replace("/", "-"),
                                            End.Date.ToShortDateString().Replace("/", "-"));
            options.MaximumLinkDepth = 3;
            options.TargetSession = 28;
            options.DownloadUri =
                new Uri(String.Format(@"http://www.legis.state.ak.us/basis/range_multi.asp?session={0}&Date1={1}&Date2={2}",
                            options.TargetSession,
                            Start.Date.ToShortDateString(),
                            End.Date.ToShortDateString()));

            //What pages to follow.
            options.UriFollower.Add(new Follow { depth = 1, pattern = new Regex("get_bill.asp") });
            options.UriFollower.Add(new Follow { depth = 2, pattern = new Regex("get_fulltext.asp") });
            options.UriFollower.Add(new Follow { depth = 2, pattern = new Regex("get_complete_bill.asp") });
            options.UriFollower.Add(new Follow { depth = 2, pattern = new Regex("get_minutes.asp") });
            options.UriFollower.Add(new Follow { depth = 3, pattern = new Regex("get_bill_text.asp") });
            options.UriFollower.Add(new Follow { depth = 3, pattern = new Regex("get_single_minute.asp[?]ch") });

            //What content to serialize and save off.
            options.GitCollectionRequest.Add(new DocumentHistory() { pageName = "get_fulltext.asp", pageType = UriType.Form });
            options.GitCollectionRequest.Add(new DocumentActivity() { pageName = "get_complete_bill.asp", pageType = UriType.Form });
            options.GitCollectionRequest.Add(new DocumentMeeting() { pageName = "get_minutes.asp", pageType = UriType.Form });
            options.GitCollectionRequest.Add(new DocumentCopy() { pageName = "get_bill_text.asp", pageType = UriType.Content });
            options.GitCollectionRequest.Add(new DocumentCopy() { pageName = "get_single_minute.asp", pageType = UriType.Content });

            //Specialized content filter using regular expression matches.
            var d = new Dictionary<String, Regex>();
            d.Add("Bill Name", new Regex(@"(?<=<b>BILL:</b>)[a-z,\s,\w,(,),"",',;,.]{5,800}", RegexOptions.IgnoreCase));
            d.Add("Title", new Regex(@"(?<=<b>TITLE:</b>)[a-z,\s,\w,(,),"",',\-,;,.]{5,5000}", RegexOptions.IgnoreCase));
            d.Add("Short Title", new Regex(@"(?<=<b>SHORT TITLE:</b>)(.*)(?=</font>)", RegexOptions.IgnoreCase));
            d.Add("Status Date", new Regex(@"(?<=<b>STATUS DATE:</b>)[0-9,/\,\w\s]{5,50}", RegexOptions.IgnoreCase));
            d.Add("Current Status", new Regex(@"(?<=<b>CURRENT STATUS:</b>)[a-z,\s,&,;,\ ,(,),/,0-9]{2,60}", RegexOptions.IgnoreCase));
            d.Add("Sponsors", new Regex(@"(?<=<b>SPONSOR[(]S[)]:</b>)[a-z,\s,\w,(,),.,;,""]{5,800}", RegexOptions.IgnoreCase));

            //RegEx matching container.
            options.GitCollectionRequest.Add(new DocumentKVP() { pageName = "get_bill.asp", pageType = UriType.Content, rvp = d });

            //Re-inflate the indexing.
            WebSiteDownloader rslt = Spider.DownloadingProcessor(options);

            Assert.IsTrue(rslt.Parsings.Count() > 0);

        }

        [TestMethod]
        public void Run_Spider_Inflate_And_Save_Results_To_GitHub()
        {

            DateTime Start = new DateTime(2014, 2, 19);
            DateTime End = new DateTime(2015, 2, 21);

            WebSiteDownloaderOptions options =
             new WebSiteDownloaderOptions();
            options.DestinationFolderPath =
                new DirectoryInfo(dataDir);
            options.DestinationFileName = String.Format("Session-Activity[{0}][{1}].state",
                                            Start.Date.ToShortDateString().Replace("/", "-"),
                                            End.Date.ToShortDateString().Replace("/", "-"));

            options.MaximumLinkDepth = 3;
            options.TargetSession = 28;
            options.DownloadUri =
                new Uri(String.Format(@"http://www.legis.state.ak.us/basis/range_multi.asp?session={0}&Date1={1}&Date2={2}",
                            options.TargetSession,
                            Start.Date.ToShortDateString(),
                            End.Date.ToShortDateString()));

            WebSiteDownloader rslt = Spider.DownloadingProcessor(options);

            /*
            1. Select Bill Names
            * We need to know the bill name (HB16), so we can save data in a folder of the same name.
            */

            //Static List of Bills
            var masterlist = (from r in rslt.Resources
                              where r.Index == 1 && (r.AbsoluteUri.AbsoluteUri.Contains(@"get_bill.asp"))
                              select r).ToList();

            //Match bill titles in the URI (HB16,SB12..)
            Regex billTitles = new Regex(@"(?<=[=])[H|R|S][B|C|R|J]{0,3}[0-9]{1,4}", RegexOptions.IgnoreCase);

            //Return a list of the first matches
            var bills = (from b in masterlist
                         let matches = billTitles.Matches(b.AbsoluteUri.AbsoluteUri)
                         where matches.Count > 0
                         select new
                         {
                             resource = b,
                             url = b.AbsoluteUri,
                             name = matches.Cast<Match>().FirstOrDefault()
                         }).ToList();

            /*
            2. Build out directory structure for bill data.
            * We have a list of bills, now where are we going to save the data?
            */

            DirectoryInfo session = new DirectoryInfo(String.Format(@"{0}/{1}", dataDir, 28));
            if (!session.Exists)
                session.Create();

            foreach (var item in bills)
            {
                //bill directory
                DirectoryInfo bill = new DirectoryInfo(String.Format(@"{0}/{1}/{2}", dataDir, 28, item.name));
                if (!bill.Exists)
                    bill.Create();
            }

            /*
            3. Associated bill data
            *  Grab associated bill data. Name, Title, LongTitle, 
             *  Minutes Content, Bill Revisions, Bill Activity
            */

            foreach (var bill in bills)
            {

                //Results placeholders
                List<iCollector> meta = new List<iCollector>();
                List<iCollector> revisions = new List<iCollector>();
                List<iCollector> minutes = new List<iCollector>();
                List<iCollector> committee = new List<iCollector>();

                //Document history, activity and kvp..
                meta.AddRange((from h in rslt.Parsings
                               where h.source.AbsoluteUri.AbsoluteUri == bill.url.AbsoluteUri
                                  || h.source.Parent.AbsoluteUri == bill.url.AbsoluteUri
                               select h).ToList());

                //Bill Content
                revisions.AddRange((from d in rslt.Parsings
                                    where d.source.Parent.AbsoluteUri
                                           .Contains(String.Format(@"get_fulltext.asp?session={0}&bill={1}", 28, bill.name))
                                    select d).ToList());

                //Committee Meetings
                committee.AddRange((from d in rslt.Resources
                                    join p in rslt.Parsings
                                    on d.AbsoluteUri.AbsoluteUri equals
                                              p.source.Parent.AbsoluteUri
                                    where p.source.AbsoluteUri.AbsoluteUri.Contains("get_minutes.asp")
                                    && d.AbsoluteUri.AbsoluteUri.Contains(String.Format("{0}", bill.name))
                                    select p).ToList());

                //Meeting Transcript (minutes)
                minutes.AddRange((from d in rslt.Resources
                                  join p in rslt.Parsings
                                  on d.AbsoluteUri.AbsoluteUri equals
                                     p.source.Parent.AbsoluteUri
                                  where p.source.AbsoluteUri.AbsoluteUri
                                         .Contains(@"get_single_minute.asp")
                                         && d.AbsoluteUri.AbsoluteUri
                                         .Contains(String.Format("{0}", bill.name))
                                  select p).ToList());

                /*
                4. Start saving off the data
                * We have a list of bills, now where are we going to save the data?
                */

                String fileLoc = String.Format(@"{0}\{1}\", 28, bill.name);

                GitRepository gr = new GitRepository();
                //Process bill parts
                gr.ProcessBill(fileLoc, new ParsedBill()
                {
                    meta = meta,
                    minutes = minutes,
                    revisions = revisions,
                    committee = committee
                });

            }

            Assert.IsTrue(true);

        }

        //[TestMethod]
        public void Run_Spider_Inflate_Check_Results_For_Minutes()
        {
            DateTime Start = new DateTime(2013, 2, 16);
            DateTime End = new DateTime(2013, 2, 21);

            WebSiteDownloaderOptions options =
               new WebSiteDownloaderOptions();
            options.DestinationFolderPath =
                new DirectoryInfo(dataDir);
            options.DestinationFileName = String.Format("Session-Activity[{0}][{1}].state",
                                            Start.Date.ToShortDateString().Replace("/", "-"),
                                            End.Date.ToShortDateString().Replace("/", "-"));
            options.MaximumLinkDepth = 3;
            options.TargetSession = 28;
            options.DownloadUri =
                new Uri(String.Format(@"http://www.legis.state.ak.us/basis/range_multi.asp?session={0}&Date1={1}&Date2={2}",
                            options.TargetSession,
                            Start.Date.ToShortDateString(),
                            End.Date.ToShortDateString()));

            WebSiteDownloader rslt = Spider.DownloadingProcessor(options);

            var minutes = (from m in rslt.Resources
                           where m.AbsoluteUri.AbsoluteUri.Contains(@"get_minutes.asp")
                           select m).ToList();

            string stop = @"";

        }

    }

}