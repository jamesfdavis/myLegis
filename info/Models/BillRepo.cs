using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using myLegis.Spider.Models;

namespace info.Models
{
    public static class BillRepo
    {

        /// <summary>
        /// Single Bill DTO
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Legislation LoadBill(String id)
        {

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
            //Serialize
            XmlSerializer serializer = new XmlSerializer(typeof(GitBill));

            FileStream fs = new FileStream(objectPath.ToString(), FileMode.Open, FileAccess.Read);
            gb = (GitBill)serializer.Deserialize(fs);

            fs.Close();
            fs.Dispose();
            fs = null;

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

            //Bill Minutes
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

            //Bill Minutes Content
            Regex lines = new Regex(@"(?<time>[0-9]{1,2}[:][0-9]{1,2}[:][0-9]{1,2}\s[A|P][M][\s,\n])(?<notes>.*[\s,\n])", RegexOptions.IgnoreCase);

            foreach (var min in ls.Minutes)
            {
                //Title
                if (Regex.IsMatch(min.Copy, @"(?<title>.*[\s,\n])"))
                {
                    //Grab the title (first line)
                    MatchCollection mc = Regex.Matches(min.Copy, @"(?<title>.*[\s,\n])");
                    min.Title = mc[0].Groups["title"].Value;
                }

                //Line Item
                foreach (Match m in lines.Matches(min.Copy))
                    min.LineItems.Add(new LineItem
                    {
                        Copy = m.Groups["notes"].Value,
                        Time = m.Groups["time"].Value
                    });
            }

            //Bill Activity
            ls.Activity = new List<BillActivity>();
            ls.Activity.AddRange((from a in gb.Activity
                                  select new BillActivity
                                  {
                                      Journal = a.Journal,
                                      Sha = String.Empty,
                                      Date = a.Date,
                                      Description = a.Description
                                  }).ToList());

            //Bill GitHub revisions
            ls.Activity.AddRange(from r in ls.Revisions
                                 select new BillActivity
                                 {
                                     Description = String.Format("Legislation updated to revision: {0}.", r.Version),
                                     Date = r.Date,
                                     Sha = r.Sha
                                 });
            return ls;

        }

        /// <summary>
        /// List of All Bills (Directory Scan)
        /// </summary>
        /// <param name="fileMap"></param>
        /// <param name="listName"></param>
        /// <returns></returns>
        public static SessionList CollectBills(string fileMap, string listName)
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