using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Configuration;

using LibGit2Sharp;
using myLegis.Spider.Models;

namespace myLegis.Spider
{
    public class GitRepository
    {

        string dir = ConfigurationManager.AppSettings["appData"];

        public GitRepository()
        {
            //Github repo object.
            //Repository.Init(dir);

            //RepoManager(dir);
        }

        public void RepoManager()
        {
            using (var repo = new Repository(dir))
            {
                // CREATE a new file
                const string oldName = "polite.txt";
                string oldPath = Path.Combine(repo.Info.WorkingDirectory, oldName);

                File.WriteAllText(oldPath, "hello test file\n", Encoding.ASCII);
                repo.Index.Stage(oldName);

                Signature who = new Signature("James Davis", "ragingsmurf@gmail.com", new DateTimeOffset(System.DateTime.Now));   //This is a test helper that returns a dummy signature
                repo.Commit("Initial Commit", who, who, false);

                // RENAME a file
                const string newName = "being.frakking.polite.txt";
                string newPath = Path.Combine(repo.Info.WorkingDirectory, newName);

                repo.Index.Unstage(oldName);
                File.Move(oldPath, newPath);
                repo.Index.Stage(newName);

                //who = who.TimeShift(TimeSpan.FromMinutes(1));    //Also a test helper extension method
                Commit commit = repo.Commit("Fix file name", who, who);

                ObjectId blobId = commit.Tree[newName].Target.Id; //For future usage below

                // UPDATE a file
                File.AppendAllText(newPath, "Hey! I said 'hello'. Why don't you answer me\n", Encoding.ASCII);
                repo.Index.Stage(newName);


                //who = who.TimeShift(TimeSpan.FromMinutes(1));
                repo.Commit("Update the content", who, who);

                // DELETE a file
                repo.Index.Unstage(newName);

                //who = who.TimeShift(TimeSpan.FromMinutes(2));
                repo.Commit("Remove the file as it's not that polite", who, who);

                // RETRIEVE a specific version of a file
                Blob file = repo.Lookup<Blob>(blobId);
            }
        }

        public List<IVersionHistory> ViewHistory(String loc, String filename)
        {

            List<IVersionHistory> list = new List<IVersionHistory>();

            using (var repo = new Repository(dir))
            {
                string path = loc + filename;
                foreach (Commit commit in repo.Head.Commits)
                {
                    if (this.TreeContainsFile(commit.Tree, path))
                    {
                        list.Add(new GitCommit()
                        {
                            Author = commit.Author.Name,
                            When = commit.Author.When,
                            Sha = commit.Sha,
                            Message = commit.MessageShort
                        } as IVersionHistory);
                    }
                }

            }

            return list;

        }

        private bool TreeContainsFile(Tree tree, string filename)
        {
            if (tree.Any(x => x.Path == filename))
            {
                return true;
            }
            else
            {
                foreach (Tree branch in tree.Where(x => x.TargetType == TreeEntryTargetType.Tree).Select(x => x.Target as Tree))
                {
                    if (this.TreeContainsFile(branch, filename))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public ObjectId ProcessContent(String content, String loc, String fileName, String comments)
        {

            StringBuilder objectPath = new StringBuilder();
            objectPath.Append(String.Format(@"{0}{1}{2}", dir, loc, fileName));

            //GitHub GUID
            ObjectId blobId = null;

            //Save the data to the github repo.
            FileInfo fi = new FileInfo(objectPath.ToString());
            if (fi.Exists)
            {
                using (var repo = new Repository(dir))
                {
                    // CREATE a new file
                    string docPath = Path.Combine(repo.Info.WorkingDirectory, loc, fileName);
                    File.WriteAllText(docPath, content.ToString(), Encoding.ASCII);

                    repo.Index.Stage(docPath);

                    Signature who = new Signature("James Davis", "ragingsmurf@gmail.com", new DateTimeOffset(System.DateTime.Now));   //This is a test helper that returns a dummy signature
                    Commit commit = repo.Commit(comments, who, who, false);

                    blobId = commit.Tree[loc + fileName].Target.Id; //For future usage below
                }
            }
            else
            {
                using (var repo = new Repository(dir))
                {
                    // CREATE a new file
                    string docPath = Path.Combine(repo.Info.WorkingDirectory, loc, fileName);
                    File.WriteAllText(docPath, content.ToString(), Encoding.ASCII);
                    repo.Index.Stage(docPath);

                    Signature who = new Signature("James Davis", "ragingsmurf@gmail.com", new DateTimeOffset(System.DateTime.Now));   //This is a test helper that returns a dummy signature
                    Commit commit = repo.Commit(comments, who, who, false);

                    blobId = commit.Tree[loc + fileName].Target.Id; //For future usage below
                }
            }

            return blobId;
        }

        public void ProcessBill(String loc, ParsedBill bill)
        {

            //File path.
            StringBuilder objectPath = new StringBuilder();
            objectPath.Append(String.Format(@"{0}{1}{2}", dir, loc, "bill.xml"));

            StringBuilder contentPath = new StringBuilder();
            contentPath.Append(String.Format(@"{0}", loc));

            //Local file info
            FileInfo fi = new FileInfo(objectPath.ToString());
            GitBill gb = null;
            //Serialize
            XmlSerializer serializer = new XmlSerializer(typeof(GitBill));

            bool processed = false;

            if (fi.Exists) //Existing object.
            {

                //Modified date.
                FileInfo s = new FileInfo(objectPath.ToString());

                var t = s.LastWriteTime;
                var ts = (DateTime.Now - s.LastWriteTime).Minutes;

                if (ts > 90)
                {
                    #region Update Bill

                    //Deserialize the Bill.
                    FileStream fs = new FileStream(objectPath.ToString(), FileMode.Open, FileAccess.ReadWrite);
                    gb = (GitBill)serializer.Deserialize(fs);

                    //Basic Bill Details
                    DocumentKVP kvp = (DocumentKVP)(from r in bill.meta
                                                    where r.pageName == "get_bill.asp"
                                                    select r).FirstOrDefault();

                    if (kvp != null)
                    {

                        //Processing file.
                        processed = true;

                        //Update basic meta data.
                        if (kvp.kvp.Where(n => n.Key == "Bill Name").Count() != 0)
                            gb.Name = kvp.kvp["Bill Name"];

                        if (kvp.kvp.Where(n => n.Key == "Title").Count() != 0)
                            gb.Title = kvp.kvp["Title"];

                        gb.ShortTitle = kvp.kvp["Short Title"];
                        gb.StatusDate = kvp.kvp["Status Date"];

                        if (kvp.kvp.Where(n => n.Key == "Current Status").Count() != 0)
                            gb.CurrentStatus = kvp.kvp["Current Status"];

                        if (kvp.kvp.Where(n => n.Key == "Current Status").Count() != 0)
                            gb.Sponsors = kvp.kvp["Sponsors"];

                        //Bill Activity
                        DocumentActivity activity = (DocumentActivity)(from r in bill.meta
                                                                       where r.pageName == "get_complete_bill.asp"
                                                                       select r).FirstOrDefault();

                        //Get activity
                        gb.Activity = (from j in activity.Journals
                                       select new Activity
                                       {
                                           Description = j.Action,
                                           Date = j.Date,
                                           Journal = j.Journal
                                       }).ToList();

                        //Bill Activity
                        DocumentHistory history = (DocumentHistory)(from r in bill.meta
                                                                    where r.pageName == "get_fulltext.asp"
                                                                    select r).FirstOrDefault();

                        //Revision history. - Not currently part of the revision index.
                        var eof = ((from h in history.Revisions
                                    where !(from d in gb.History
                                            select d.Version).Contains(h.Version)
                                    select new History
                                    {
                                        OfferDate = h.DateOffered.Value,
                                        Offered = h.Offered,
                                        Version = h.Version,
                                        PassedHouse = h.DatePassedHouse,
                                        PassedSenate = h.DatePassedSenate,
                                        Title = h.Title
                                    }).ToList());

                        //Loop through all the revisions and update the the bill content.
                        foreach (History hist in eof)
                        {

                            //Grab the revision copy.
                            iCollector iCopy = (from r in bill.revisions
                                                where r.source.AbsoluteUri.AbsoluteUri.Contains(hist.Version)
                                                select r).FirstOrDefault<iCollector>();

                            //Content
                            DocumentCopy dc = (DocumentCopy)iCopy;

                            //Document content
                            gb.Copy = Remove_Illegal_UTF8_Characters(dc.copy);

                            //Save to Git
                            ObjectId blobId = ProcessContent(
                                  (dc.copy.ToString().Substring(0, 2) == "\r\n") ? dc.copy.Remove(0, 2) : dc.copy, //Remove leading CRLF,
                                  contentPath.ToString(),
                                  @"content.txt",
                                  hist.OfferDate.HasValue ? String.Format("Legislative content updated on {0} to {1}.",
                                  hist.Offered,
                                  hist.Version) : String.Format("Legislative content updated to {0}.", hist.Version));

                            List<IVersionHistory> vl = (from v in this.ViewHistory(loc, "content.txt").ToList()
                                                        where v.Message.Contains(hist.Version)
                                                        orderby v.When descending
                                                        select v).ToList();

                            //Current revision.
                            gb.Revisions.Add(new Revision
                            {
                                Version = hist.Version,
                                Offer = hist.Offered,
                                Date = hist.OfferDate,
                                BlobId = vl.First() != null ? vl.First().Sha : ""
                            });
                        }

                        if (bill.committee.Count() > 0)
                        {

                            //Parse meeting for meeting minutes, save them off.
                            foreach (DocumentMinutes j in bill.committee.Cast<DocumentMeeting>().FirstOrDefault().Journals)
                            {

                                //Committee results placeholders.
                                String blData = @"";
                                String elData = @"";
                                String cmData = @"";
                                String dtData = @"";
                                String tmData = @"";
                                DocumentCopy iCopy = null;

                                //is there is a link to meeting notes?
                                if (!String.IsNullOrEmpty(j.UrlText))
                                {

                                    //Grab meeting minutes
                                    iCopy = (from r in bill.minutes
                                             where r.source.AbsoluteUri.AbsoluteUri.Contains(j.UrlText)
                                             select r).Cast<DocumentCopy>().FirstOrDefault();

                                    //Found meeting notes?
                                    if (iCopy != null)
                                    {
                                        //RegEx for minutes URL.
                                        Regex bl = new Regex(@"(?<=beg_line=)[0-9]{5}", RegexOptions.IgnoreCase);
                                        Regex el = new Regex(@"(?<=end_line=)[0-9]{5}", RegexOptions.IgnoreCase);
                                        Regex cm = new Regex(@"(?<=comm=)[A-Z]{0,10}", RegexOptions.IgnoreCase);
                                        Regex dt = new Regex(@"(?<=date=)[0-9]{0,10}", RegexOptions.IgnoreCase);
                                        Regex tm = new Regex(@"(?<=time=)[0-9]{0,10}", RegexOptions.IgnoreCase);

                                        //Content to match.
                                        MatchCollection mc = bl.Matches(iCopy.source.AbsoluteUri.AbsoluteUri);
                                        //Scrape the data.
                                        blData = (from m in mc.Cast<Match>()
                                                  select m.Groups[0].Value).FirstOrDefault();

                                        mc = null;
                                        //Content to match.
                                        mc = el.Matches(iCopy.source.AbsoluteUri.AbsoluteUri);
                                        //Scrape the data.
                                        elData = (from m in mc.Cast<Match>()
                                                  select m.Groups[0].Value).FirstOrDefault();

                                        mc = null;
                                        //Content to match.
                                        mc = cm.Matches(iCopy.source.AbsoluteUri.AbsoluteUri);
                                        //Scrape the data.
                                        cmData = (from m in mc.Cast<Match>()
                                                  select m.Groups[0].Value).FirstOrDefault();

                                        mc = null;
                                        //Content to match.
                                        mc = dt.Matches(iCopy.source.AbsoluteUri.AbsoluteUri);
                                        //Scrape the data.
                                        dtData = (from m in mc.Cast<Match>()
                                                  select m.Groups[0].Value).FirstOrDefault();

                                        mc = null;
                                        //Content to match.
                                        mc = tm.Matches(iCopy.source.AbsoluteUri.AbsoluteUri);
                                        //Scrape the data.
                                        tmData = (from m in mc.Cast<Match>()
                                                  select m.Groups[0].Value).FirstOrDefault();

                                    }
                                }

                                var mof = (from m in gb.Minutes
                                           where
                                               m.Begin_Line == elData
                                           where !(from d in gb.Minutes
                                                   select d.Begin_Line).Contains(blData) &&
                                                 !(from d in gb.Minutes
                                                   select d.End_Line).Contains(elData)
                                           select new Minute
                                           {
                                               Begin_Line = blData,
                                               End_Line = elData,
                                               Committee = cmData,
                                               Date = dtData,
                                               Time = tmData,
                                               Committee_Name = j.Committee,
                                               Audio_Url = j.UrlAudio,
                                               Minutes_Url = j.UrlText,
                                               DateTime = j.Date,
                                               Note = iCopy != null ? Remove_Illegal_UTF8_Characters(iCopy.copy) : @""
                                           }).ToList();

                                //Add minute details.
                                gb.Minutes.AddRange(mof);

                            }
                        }

                        //Revision history - Add new stuff.
                        gb.History.AddRange(eof);
                        //Close out IO work
                        fs.Close();
                        fs.Dispose();
                        fs = null;
                    }

                    #endregion
                }
            }
            else //New object.
            {

                #region  New Bill

                gb = new GitBill(); //Clean container.
                gb.Revisions = new List<Revision>();
                gb.History = new List<History>();
                gb.Activity = new List<Activity>();
                gb.Minutes = new List<Minute>();

                //Basic Bill Details
                DocumentKVP kvp = (DocumentKVP)(from r in bill.meta
                                                where r.pageName == "get_bill.asp"
                                                select r).FirstOrDefault();

                if (kvp != null)
                {

                    //Processing file.
                    processed = true;

                    //Add basic meta data.
                    if (kvp.kvp.Where(n => n.Key == "Bill Name").Count() != 0)
                        gb.Name = kvp.kvp["Bill Name"];

                    if (kvp.kvp.Where(n => n.Key == "Title").Count() != 0)
                        gb.Title = kvp.kvp["Title"];

                    gb.ShortTitle = kvp.kvp["Short Title"];
                    gb.StatusDate = kvp.kvp["Status Date"];

                    if (kvp.kvp.Where(n => n.Key == "Current Status").Count() != 0)
                        gb.CurrentStatus = kvp.kvp["Current Status"];

                    if (kvp.kvp.Where(n => n.Key == "Current Status").Count() != 0)
                        gb.Sponsors = kvp.kvp["Sponsors"];


                    //Bill Activity
                    DocumentActivity activity = (DocumentActivity)(from r in bill.meta
                                                                   where r.pageName == "get_complete_bill.asp"
                                                                   select r).FirstOrDefault();

                    //Get activity
                    gb.Activity = (from j in activity.Journals
                                   select new Activity
                                   {
                                       Description = j.Action,
                                       Date = j.Date,
                                       Journal = j.Journal
                                   }).ToList();

                    //Bill Activity
                    DocumentHistory history = (DocumentHistory)(from r in bill.meta
                                                                where r.pageName == "get_fulltext.asp"
                                                                select r).FirstOrDefault();

                    //Revision history.
                    gb.History.AddRange((from h in history.Revisions
                                         select new History
                                         {
                                             Title = h.Title,
                                             OfferDate = h.DateOffered,
                                             Offered = h.Offered,
                                             PassedHouse = h.DatePassedHouse,
                                             PassedSenate = h.DatePassedSenate,
                                             Version = h.Version
                                         }).ToList());

                    //Loop through all the revisions and update the the bill content.
                    foreach (History hist in gb.History)
                    {

                        //Grab the revision copy.
                        iCollector iCopy = (from r in bill.revisions
                                            where r.source.AbsoluteUri.AbsoluteUri.Contains(hist.Version)
                                            select r).FirstOrDefault<iCollector>();

                        //Content
                        DocumentCopy dc = (DocumentCopy)iCopy;

                        //Document content
                        gb.Copy = Remove_Illegal_UTF8_Characters(dc.copy);

                        //Save to Git
                        ObjectId blobId = ProcessContent(
                            (dc.copy.ToString().Substring(0,2) == "\r\n") ? dc.copy.Remove(0,2) : dc.copy, //Remove leading CRLF
                              contentPath.ToString(),
                              @"content.txt",
                              hist.OfferDate.HasValue ? String.Format("Legislative content updated on {0} to {1}.",
                              hist.Offered,
                              hist.Version) : String.Format("Legislative content updated to {0}.", hist.Version));

                        List<IVersionHistory> vl = (from v in this.ViewHistory(loc, "content.txt").ToList()
                                                    where v.Message.Contains(hist.Version)
                                                    orderby v.When descending
                                                    select v).ToList();

                        //Current revision.
                        gb.Revisions.Add(new Revision
                        {
                            Version = hist.Version,
                            Offer = hist.Offered,
                            Date = hist.OfferDate,
                            BlobId = vl.First() != null ? vl.First().Sha : ""
                        });

                    }

                    if (bill.committee.Count() > 0)
                    {
                        //Parse meeting for meeting minutes, save them off.
                        foreach (DocumentMinutes j in bill.committee.Cast<DocumentMeeting>().FirstOrDefault().Journals)
                        {

                            //Committee results placeholders.
                            String blData = @"";
                            String elData = @"";
                            String cmData = @"";
                            String dtData = @"";
                            String tmData = @"";
                            DocumentCopy iCopy = null;

                            //is there is a link to meeting notes?
                            if (!String.IsNullOrEmpty(j.UrlText))
                            {

                                //Grab meeting minutes
                                iCopy = (from r in bill.minutes
                                         where r.source.AbsoluteUri.AbsoluteUri.Contains(j.UrlText)
                                         select r).Cast<DocumentCopy>().FirstOrDefault();

                                //Found meeting notes?
                                if (iCopy != null)
                                {
                                    //RegEx for minutes URL.
                                    Regex bl = new Regex(@"(?<=beg_line=)[0-9]{5}", RegexOptions.IgnoreCase);
                                    Regex el = new Regex(@"(?<=end_line=)[0-9]{5}", RegexOptions.IgnoreCase);
                                    Regex cm = new Regex(@"(?<=comm=)[A-Z]{0,10}", RegexOptions.IgnoreCase);
                                    Regex dt = new Regex(@"(?<=date=)[0-9]{0,10}", RegexOptions.IgnoreCase);
                                    Regex tm = new Regex(@"(?<=time=)[0-9]{0,10}", RegexOptions.IgnoreCase);

                                    //Content to match.
                                    MatchCollection mc = bl.Matches(iCopy.source.AbsoluteUri.AbsoluteUri);
                                    //Scrape the data.
                                    blData = (from m in mc.Cast<Match>()
                                              select m.Groups[0].Value).FirstOrDefault();

                                    mc = null;
                                    //Content to match.
                                    mc = el.Matches(iCopy.source.AbsoluteUri.AbsoluteUri);
                                    //Scrape the data.
                                    elData = (from m in mc.Cast<Match>()
                                              select m.Groups[0].Value).FirstOrDefault();

                                    mc = null;
                                    //Content to match.
                                    mc = cm.Matches(iCopy.source.AbsoluteUri.AbsoluteUri);
                                    //Scrape the data.
                                    cmData = (from m in mc.Cast<Match>()
                                              select m.Groups[0].Value).FirstOrDefault();

                                    mc = null;
                                    //Content to match.
                                    mc = dt.Matches(iCopy.source.AbsoluteUri.AbsoluteUri);
                                    //Scrape the data.
                                    dtData = (from m in mc.Cast<Match>()
                                              select m.Groups[0].Value).FirstOrDefault();

                                    mc = null;
                                    //Content to match.
                                    mc = tm.Matches(iCopy.source.AbsoluteUri.AbsoluteUri);
                                    //Scrape the data.
                                    tmData = (from m in mc.Cast<Match>()
                                              select m.Groups[0].Value).FirstOrDefault();

                                }

                            }

                            //Add minute details.
                            gb.Minutes.Add(new Minute
                            {
                                Begin_Line = blData,
                                End_Line = elData,
                                Committee = cmData,
                                Date = dtData,
                                Time = tmData,
                                Committee_Name = j.Committee,
                                Audio_Url = j.UrlAudio,
                                Minutes_Url = j.UrlText,
                                DateTime = j.Date,
                                Note = iCopy != null ? Remove_Illegal_UTF8_Characters(iCopy.copy) : @""
                            });

                            //if (iCopy != null)
                            //{

                            //Save meeting minutes locally.
                            //SaveFile(
                            //      iCopy.copy,
                            //      contentPath.ToString(),
                            //      String.Format(@"{0}-{1}-{2}-{3}-{4}.txt", blData, elData, cmData, dtData, tmData, tmData),
                            //      String.Format("Meeting notes for the {0} committee meeting on {1}.",
                            //      j.Committee,
                            //      j.Date.ToLongDateString()));
                            //}

                            //}

                        }
                    }
                }

                //var jrnl = (from n in bill.minutes.Cast<DocumentMinutes>()
                //            join m in bill.committee.Cast<DocumentMeeting>().FirstOrDefault().Journals
                //            on n.UrlText equals 

                //foreach (DocumentMeeting m in bill.committee)
                //{
                //    foreach (var d in m.Journals)
                //    {

                //    }
                //}

                #endregion

            }

            //Save the file to disk, if added or updated.
            if (processed)
            {
                //Save the file
                using (TextWriter textWrite = new StreamWriter(objectPath.ToString()))
                    serializer.Serialize(textWrite, gb);
            }
        }

        private static string Remove_Illegal_UTF8_Characters(string inString)
        {
            if (inString == null) return null;

            StringBuilder newString = new StringBuilder();
            char ch;

            for (int i = 0; i < inString.Length; i++)
            {

                ch = inString[i];
                // remove any characters outside the valid UTF-8 range as well as all control characters
                // except tabs and new lines
                if ((ch < 0x00FD && ch > 0x001F) || ch == '\t' || ch == '\n' || ch == '\r')
                {
                    newString.Append(ch);
                }
            }
            return newString.ToString();

        }

    }

    public class ParsedBill
    {
        //Results placeholders
        public List<iCollector> meta { get; set; }
        public List<iCollector> revisions { get; set; }
        public List<iCollector> minutes { get; set; }
        public List<iCollector> committee { get; set; }
        public String fileLoc { get; set; }
    }

}
