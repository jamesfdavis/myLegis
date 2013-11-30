using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Web;
using System.Web.Mvc;
using System.Text;

using myLegis.Spider.Models;
using www.Models;
using myLegis.Spider;
using System.Text.RegularExpressions;
using System.Configuration;

namespace www.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        [HttpGet]
        public ActionResult Index()
        {

            return View();

        }

        [HttpGet]
        public ViewResult Senate()
        {

            List<SessionList> lst = new List<SessionList>();

            lst.Add(this.CollectBills("SB*", "SB"));
            lst.Add(this.CollectBills("SR*", "SR"));
            lst.Add(this.CollectBills("SJR*", "SJR"));
            lst.Add(this.CollectBills("SCR*", "SCR"));

            return View(lst);

        }

        [HttpGet]
        public ViewResult House()
        {

            List<SessionList> lst = new List<SessionList>();

            lst.Add(this.CollectBills("HB*", "HB"));
            lst.Add(this.CollectBills("HR*", "HR"));
            lst.Add(this.CollectBills("HJR*", "HJR"));
            lst.Add(this.CollectBills("HCR*", "HCR"));

            return View(lst);

        }

        [HttpGet]
        public ActionResult Bill(String id)
        {
            string residence = String.Empty;

            Regex rgxSenate = new Regex("S[A-Z]", RegexOptions.IgnoreCase);
            Regex rgxHouse = new Regex("H[A-Z]", RegexOptions.IgnoreCase);

            if (rgxSenate.IsMatch(id))
                residence = "Senate";

            if (rgxHouse.IsMatch(id))
                residence = "House";

            ViewData["Residence"] = residence;

            //Locate the bill
            String dataDir = String.Format(@"{0}{1}\{2}",
                                    ConfigurationManager.AppSettings["appData"],
                                    28, id);

            GitBill gb = null;

            //File path.
            StringBuilder objectPath = new StringBuilder();
            objectPath.Append(String.Format(@"{0}\{1}", dataDir, "bill.xml"));

            //Local file info
            FileInfo fi = new FileInfo(objectPath.ToString());
            //GitBill gb = null;

            //Serialize
            XmlSerializer serializer = new XmlSerializer(typeof(GitBill));

            FileStream fs = new FileStream(objectPath.ToString(), FileMode.Open, FileAccess.Read);
            gb = (GitBill)serializer.Deserialize(fs);

            fs.Close();
            fs.Dispose();
            fs = null;

           // GitRepository gr = new GitRepository();

            //TODO - Fix, this takes too long, kills server cpu.
            // List<IVersionHistory> commits = gr.ViewHistory(String.Format(@"28\{0}\", id.ToUpper()), @"content.txt");

            //Build out Bill Metadata
            Legislation ls = new Legislation
            {
                Copy = gb.Copy,
                CurrentStatus = gb.CurrentStatus,
                Name = gb.Name,
                ShortTitle = gb.ShortTitle,
                Title = gb.Title,
                Sponsors = gb.Sponsors,
                StatusDate = gb.StatusDate
            };

            //Bill Revisions
            ls.Revisions = new List<BillRevision>();
            ls.Revisions.AddRange((from r in gb.Revisions
                                   select new BillRevision
                                   {
                                       Date = r.Date,
                                       Offer = r.Offer,
                                       Version = r.Version,
                                       Sha = r.BlobId
                                   }).ToList());

            //Bill History
            ls.History = new List<BillHistory>();
            ls.History.AddRange((from h in gb.History
                                 select new BillHistory
                                 {
                                     Title = h.Title,
                                     Version = h.Version,
                                     OfferDate = h.OfferDate,
                                     Offered = h.Offered,
                                     PassedHouse = h.PassedHouse,
                                     PassedSenate = h.PassedSenate
                                 }).ToList());

            ls.Minutes = new List<BillMinutes>();
            ls.Minutes.AddRange((from a in gb.Minutes
                                 select new BillMinutes
                                 {
                                     DateTime = a.DateTime,
                                     Audio_Url = a.Audio_Url,
                                     Begin_Line = a.Begin_Line,
                                     End_Line = a.End_Line,
                                     Committee = a.Committee,
                                     Committee_Name = a.Committee_Name,
                                     Copy = a.NoteData.Value,
                                     Minutes_Url = a.Minutes_Url,
                                     Time = a.Time,
                                     LineItems = new List<LineItem>()
                                 }).ToList());

            //Regex lines = new Regex(@"(?<time>[0-9]{1,2}[:][0-9]{1,2}[:][0-9]{1,2}\s[A|P][M]\n)(?<notes>.*[\n])", RegexOptions.IgnoreCase);
            Regex lines = new Regex(@"(?<time>[0-9]{1,2}[:][0-9]{1,2}[:][0-9]{1,2}\s[A|P][M][\s,\n])(?<notes>.*[\s,\n])", RegexOptions.IgnoreCase);

            foreach (var min in ls.Minutes)
            {
                if (Regex.IsMatch(min.Copy, @"(?<title>.*[\s,\n])"))
                {
                    //Grab the title (first line)
                    MatchCollection mc = Regex.Matches(min.Copy, @"(?<title>.*[\s,\n])");
                    min.Title = mc[0].Groups["title"].Value;
                }

                foreach (Match m in lines.Matches(min.Copy))
                    min.LineItems.Add(new LineItem
                    {
                        Copy = m.Groups["notes"].Value,
                        Time = m.Groups["time"].Value
                    });
            }


            ls.Activity = new List<BillActivity>();
            ls.Activity.AddRange((from a in gb.Activity
                                  select new BillActivity
                                  {
                                      Journal = a.Journal,
                                      Sha = String.Empty,
                                      Date = a.Date,
                                      Description = a.Description
                                  }).ToList());

            //For each item in the History, go check to see what it's Sha is.
            //foreach (BillRevision r in ls.Revisions)
            //{

            //    //Add the GitHub revisions
            //    var a = (from c in ls.Activity
            //             where c.Description.Contains(r.Version)
            //             orderby c.Date ascending
            //             select new BillActivity
            //             {
            //                 Description = c.Description,
            //                 Sha = c.Sha,
            //                 Date = r.Date
            //             }).ToList().FirstOrDefault();

            //    if (a != null)
            //        ls.Activity.Add(a);
            //}

            //Add all the revisions
            ls.Activity.AddRange(from r in ls.Revisions
                            select new BillActivity
                            {
                                Description = String.Format("Legislation updated to revision: {0}.", r.Version),
                                Date = r.Date,
                                Sha = r.Sha
                            });

            return View(ls);

        }

        private SessionList CollectBills(string fileMap, string listName)
        {

            //Locate the bill
            String dataDir = String.Format(@"{0}{1}",
                                    ConfigurationManager.AppSettings["appData"],
                                    28);

            GitBill gb = null;
            DirectoryInfo di = new DirectoryInfo(dataDir);

            SessionList sl = new SessionList { Name = listName };
            sl.Bills = new List<ItemOverview>();

            foreach (var d in di.GetDirectories(fileMap, SearchOption.TopDirectoryOnly))
            {

                var s = d;
                //File path.
                StringBuilder objectPath = new StringBuilder();
                objectPath.Append(String.Format(@"{0}\{1}\{2}", dataDir, d.Name, "bill.xml"));

                //Local file info
                FileInfo fi = new FileInfo(objectPath.ToString());

                //Serialize
                XmlSerializer serializer = new XmlSerializer(typeof(GitBill));
                FileStream fs;

                try
                {
                    fs = new FileStream(objectPath.ToString(), FileMode.Open, FileAccess.Read);
                    gb = (GitBill)serializer.Deserialize(fs);
                    sl.Bills.Add(new ItemOverview { Location = d.Name, Name = gb.Name, Title = gb.ShortTitle });
                    fs.Close();
                    fs.Dispose();
                    fs = null;
                }
                catch (FileNotFoundException)
                {
                    //No file found. :(
                }

            }

            return sl;

        }

    }

}