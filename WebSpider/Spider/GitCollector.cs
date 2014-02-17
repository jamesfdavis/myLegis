using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace myLegis.Spider
{

    #region iCollector

    [Serializable]
    public class DocumentHrefList : iCollector, iClone<iCollector>
    {
        public string pageName { get; set; }
        public UriType pageType { get; set; }
        public Regex pattern { get; set; }
        public UriResourceInformation source { get; set; }
        public List<String> matches { get; set; }

        public void Collect(XmlReader xml, UriResourceInformation uri)
        {

            if (pattern == null)
                throw new Exception("Pattern is required!");

            this.source = uri;

            XDocument doc = XDocument.Load(xml);
            XNamespace ns = @"http://www.w3.org/1999/xhtml";

            //Get all bill versions.
            List<String> links = (from el in doc.Descendants(ns + "a")
                                  let matches = pattern.Matches(el.Attribute("href").Value.Trim())
                                  where matches.Count != 0
                                  select el.Value.Replace(" ", "")).ToList();

            this.matches = links;

        }

        public iCollector Clone()
        {
            return new DocumentHrefList
            {
                pageName = this.pageName,
                pageType = this.pageType,
                pattern = this.pattern
            };
        }

    }

    [Serializable]
    public class DocumentKVP : iCollector, iClone<iCollector>
    {
        public string pageName { get; set; }
        public UriType pageType { get; set; }
        public UriResourceInformation source { get; set; }
        public Regex pattern { get; set; }
        public Dictionary<String, String> kvp { get; set; }
        public Dictionary<String, Regex> rvp { get; set; }

        public DocumentKVP()
        {

            this.kvp = new Dictionary<string, string>();
            this.rvp = new Dictionary<string, Regex>();

        }

        public void Collect(XmlReader xml, UriResourceInformation uri)
        {
            this.source = uri;

            XDocument doc = XDocument.Load(xml);
            XNamespace ns = @"http://www.w3.org/1999/xhtml";

            StringBuilder sb = new StringBuilder();
            sb.Append(doc.Document);

            foreach (var r in this.rvp.ToList())
            {
                //Content to match.
                MatchCollection mc = r.Value.Matches(sb.ToString());
                //Scrape the data.
                String data = (from m in mc.Cast<Match>()
                               select m.Groups[0].Value)
                               .FirstOrDefault();
                //Save the data.
                if (!String.IsNullOrEmpty(data))
                {

                    string str = r.Key.ToString();
                    if ((this.kvp.Where(n => n.Key == r.Key.ToString()).Count() == 0))
                    {
                        this.kvp.Add(r.Key.ToString(), data);
                    }
                    else
                    {
                        //Figure out why we have had a conflict
                        string stp = @"";
                    }
                }

            }

        }

        public iCollector Clone()
        {
            return new DocumentKVP
            {
                rvp = this.rvp,
                pageName = this.pageName,
                pageType = this.pageType
            };
        }

    }

    [Serializable]
    public class DocumentCopy : iCollector, iClone<iCollector>
    {
        public string pageName { get; set; }
        public UriType pageType { get; set; }
        public Regex pattern { get; set; }
        public UriResourceInformation source { get; set; }
        public String copy { get; set; }

        public void Collect(XmlReader xml, UriResourceInformation uri)
        {

            this.source = uri;

            XDocument doc = XDocument.Load(xml);
            XNamespace ns = @"http://www.w3.org/1999/xhtml";

            //Get all bill versions.
            XElement pre = (from el in doc.Descendants(ns + "pre")
                            select el).FirstOrDefault();

            StringBuilder sb = new StringBuilder();
            //Format content.
            String content = pre.Value;

            //Format minutes text.
            if (this.source.AbsoluteUri.AbsoluteUri.Contains(@"get_single_minute"))
            {
                content = Regex.Replace(pre.Value, @"\s{2,}", @" ", RegexOptions.IgnoreCase);
                content = Regex.Replace(content, @"[0-9]{1,2}:[0-9]{2}:[0-9]{2}\s(PM|AM)", m => '\r' + m.Value + '\r' + '\n');
            }

            //Save the content
            sb.Append(content);

            this.copy = sb.ToString();

        }

        public iCollector Clone()
        {
            return new DocumentCopy
            {
                pageName = this.pageName,
                pageType = this.pageType,
                pattern = this.pattern
            };
        }

    }

    [Serializable]
    public class DocumentHistory : iCollector, iClone<iCollector>
    {
        public string pageName { get; set; }
        public UriType pageType { get; set; }
        public Regex pattern { get; set; }
        public UriResourceInformation source { get; set; }
        public List<DocumentRevision> Revisions { get; set; }

        public void Collect(XmlReader xml, UriResourceInformation uri)
        {

            this.source = uri;
            this.Revisions = new List<DocumentRevision>();

            XDocument doc = XDocument.Load(xml);
            XNamespace ns = @"http://www.w3.org/1999/xhtml";

            var tblHistory = (from n in doc.Descendants(ns + @"TABLE")
                              select n).FirstOrDefault();

            if (tblHistory != null)
            {
                //Grab normal bill history.
                var trHistory = (from r in tblHistory.Descendants(ns + @"TR")
                                 where r.HasElements == true &&
                                       r.Descendants(ns + @"TD").Count() == 5
                                 //select r).ToList();
                                 select new DocumentRevision
                                 {
                                     Version = r.Descendants(ns + @"TD")
                                                .First()
                                                .Value.Trim(),
                                     Title = r.Descendants(ns + @"TD")
                                                .Skip(1)
                                                .Descendants()
                                                .First()
                                                .Value.Trim(),
                                     Offered = r.Descendants(ns + @"TD")
                                                .Skip(2)
                                                .First()
                                                .Value.Trim(),
                                     PassedHouse = r.Descendants(ns + @"TD")
                                                  .Skip(3)
                                                  .First()
                                                  .Value.Trim(),
                                     PassedSenate = r.Descendants(ns + @"TD")
                                                  .Skip(4)
                                                  .First()
                                                  .Value.Trim()
                                 }).ToList();

                //Save all rows to the revisions.
                this.Revisions.AddRange(trHistory);

                //Grab bill variance history.
                trHistory = (from r in tblHistory.Descendants(ns + @"TR")
                             where r.HasElements == true &&
                                   r.Descendants(ns + @"TD").Count() == 3
                             //select r).ToList();
                             select new DocumentRevision
                             {
                                 Version = r.Descendants(ns + @"TD")
                                            .First()
                                            .Value.Trim(),
                                 Title = r.Descendants(ns + @"TD")
                                            .Skip(1)
                                            .Descendants()
                                            .First()
                                            .Value.Trim(),
                                 TimeLine = r.Descendants(ns + @"TD")
                                          .Skip(2)
                                          .First()
                                          .Value.Trim()
                             }).ToList();

                //Save all rows to the revisions.
                this.Revisions.AddRange(trHistory);

            }

        }

        public iCollector Clone()
        {
            return new DocumentHistory
            {
                pageName = this.pageName,
                pageType = this.pageType,
                pattern = this.pattern
            };
        }

    }

    [Serializable]
    public class DocumentActivity : iCollector, iClone<iCollector>
    {
        public string pageName { get; set; }
        public UriType pageType { get; set; }
        public Regex pattern { get; set; }
        public UriResourceInformation source { get; set; }
        public List<DocumentJournal> Journals { get; set; }

        public void Collect(XmlReader xml, UriResourceInformation uri)
        {

            this.source = uri;
            this.Journals = new List<DocumentJournal>();

            XDocument doc = XDocument.Load(xml);
            XNamespace ns = @"http://www.w3.org/1999/xhtml";

            var actions = from row in doc.Descendants(ns + "html")
                                       .Descendants(ns + "table").Skip(5)
                                       .Descendants(ns + "TR")
                          select new DocumentJournal
                          {
                              Journal = row.Descendants(ns + "TD")
                                        .First()
                                        .Value,
                              Action = row.Descendants(ns + "td")
                                          .Skip(1)
                                          .Descendants(ns + "font")
                                          .First()
                                          .Value.Trim(),
                          };

            var moreactions = from row in doc.Descendants(ns + "html")
                                    .Descendants(ns + "table").Skip(5)
                                    .Descendants(ns + "tr").Skip(2)
                              select new DocumentJournal
                              {
                                  Journal = row.Descendants(ns + "td")
                                            .First()
                                            .Value,
                                  Action = row.Descendants(ns + "td")
                                              .Skip(2)
                                              .Descendants(ns + "font")
                                              .First()
                                              .Value.Trim(),
                              };

            //Combine the set of actions together.
            var result = (from r in actions.Concat(moreactions)
                          select r).OrderBy(n => n.Date);

            //Save the data.
            this.Journals.AddRange(result);

            //Save the data off?
            //GitRepository gr = new GitRepository();
        }

        public iCollector Clone()
        {
            return new DocumentActivity
            {
                pageName = this.pageName,
                pageType = this.pageType,
                pattern = this.pattern
            };
        }

    }

    [Serializable]
    public class DocumentMeeting : iCollector, iClone<iCollector>
    {
        public string pageName { get; set; }
        public UriType pageType { get; set; }
        public Regex pattern { get; set; }
        public UriResourceInformation source { get; set; }
        public List<DocumentMinutes> Journals { get; set; }

        public void Collect(XmlReader xml, UriResourceInformation uri)
        {

            this.source = uri;
            this.Journals = new List<DocumentMinutes>();

            XDocument doc = XDocument.Load(xml);
            XNamespace ns = @"http://www.w3.org/1999/xhtml";

            //Rows available in the table.
            var rows = (from row in doc.Descendants(ns + @"html")
                           .Descendants(ns + @"table")
                           .Skip(1)
                           .Descendants(ns + "tr")
                        select row).ToList();

            //Loop through each row.
            for (int i = 0; i < rows.Count(); i++)
            {

                //Grab each cell of data
                List<XElement> cells = (from r in rows[i].Descendants(ns + "td")
                                        select r).ToList();

                //Grab each cell of data
                cells.InsertRange(2, (from r in rows[i].Descendants(ns + "TD")
                                      select r).ToList());

                //Grab all available links.
                List<XElement> links = (from l in cells.Descendants(ns + @"a")
                                        select l).ToList();

                //Recreate the journal entry.
                DocumentMinutes jrn = new DocumentMinutes()
                {
                    Journal = String.Format(@"{0} {1}", cells[0].Value, cells[1].Value),
                    Committee = cells[2].Value,
                    UrlText = (from l in links
                               let h = l.Attributes().FirstOrDefault()
                               where h.Value.Contains("get_single_minute")
                               select h.Value)
                               .FirstOrDefault()
                    ,
                    UrlAudio = (from l in links
                                let h = l.Attributes().FirstOrDefault()
                                where h.Value.Contains("get_audio")
                                select h.Value)
                               .FirstOrDefault()
                };

                this.Journals.Add(jrn);

                // String minutes = (from t in t
            }


            //Add the committee meetings.
            //this.Journals.AddRange((from row in doc.Descendants(ns + @"html")
            //                              .Descendants(ns + @"table")
            //                              .Skip(1)
            //                              .Descendants(ns + "tr")
            //                        let min = row.Descendants(ns + @"td")
            //                                 .Skip(3)
            //                                 .FirstOrDefault()
            //                        let aud = row.Descendants(ns + @"td")
            //                                 .Skip(5)
            //                                 .FirstOrDefault()
            //                        select new DocumentMinutes
            //                        {
            //                            Journal = (row.Descendants(ns + @"td")
            //                                      .First()
            //                                      .Value + ' ' +
            //                                      row.Descendants(ns + @"td")
            //                                      .Skip(1)
            //                                      .First()
            //                                      .Value),
            //                            Committee = (row.Descendants(ns + @"td")
            //                                      .Skip(2)
            //                                      .First()
            //                                      .Value.Trim()),
            //                            UrlText = (min == null ? null : (from a in min.Descendants(ns + "a")
            //                                                             select a.Attribute("HREF").Value
            //                                                              ).FirstOrDefault()),
            //                            UrlAudio = (aud == null ? null : (from a in aud.Descendants(ns + "a")
            //                                                              where a.Attribute("href").Value.Contains("get_audio.asp")
            //                                                              select a.Attribute("href").Value).FirstOrDefault())
            //                        }).ToList());
        }

        public iCollector Clone()
        {
            return new DocumentMeeting
            {
                pageName = this.pageName,
                pageType = this.pageType,
                pattern = this.pattern
            };
        }

    }

    #region " Helper Classes "

    [Serializable]
    public class DocumentJournal
    {
        public string Action { get; set; }
        public string Journal { get; set; }
        public DateTime Date
        {
            get
            {
                DateTime dt;
                DateTime.TryParse(this.Journal, out dt);
                return dt;
            }
        }
    }

    [Serializable]
    public class DocumentMinutes
    {
        public string Journal { get; set; }
        public string Committee { get; set; }
        public string UrlText { get; set; }
        public string UrlAudio { get; set; }
        public DateTime Date
        {
            get
            {
                DateTime dt;
                DateTime.TryParse(this.Journal, out dt);
                return dt;
            }
        }
    }

    [Serializable]
    public class DocumentRevision
    {
        public string Version { get; set; }
        public string TimeLine { get; set; }
        public string Title { get; set; }
        public string Offered { get; set; }
        public DateTime? DateOffered
        {
            get
            {
                DateTime dt;
                DateTime? opt = null;
                bool success = DateTime.TryParse(this.Offered, out dt);
                if (success)
                    opt = dt;

                return opt;
            }
        }
        public string PassedHouse { get; set; }
        public DateTime? DatePassedHouse
        {
            get
            {
                DateTime dt;
                DateTime? opt = null;
                bool success = DateTime.TryParse(this.PassedHouse, out dt);
                if (success)
                    opt = dt;

                return opt;
            }
        }
        public string PassedSenate { get; set; }
        public DateTime? DatePassedSenate
        {
            get
            {
                DateTime dt;
                DateTime? opt = null;
                bool success = DateTime.TryParse(this.PassedSenate, out dt);
                if (success)
                    opt = dt;

                return opt;
            }
        }

    }

    #endregion

    public interface iCollector : iClone<iCollector>
    {

        string pageName { get; set; }
        UriType pageType { get; set; }
        Regex pattern { get; set; }
        UriResourceInformation source { get; set; }

        void Collect(XmlReader xml, UriResourceInformation uri);

    }

    public interface iClone<T>
    {
        T Clone();
    }

    #endregion

}
