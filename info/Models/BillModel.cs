using myLegis.Spider.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace www.Models
{

    public class SessionList
    {
        public string Name { get; set; }
        public List<ItemOverview> Bills { get; set; }
    }

    public class ItemOverview
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
    }

    /// <summary>
    /// Public view of a legislative bill.
    /// </summary>
    public class Legislation
    {

        public string Name { get; set; }
        public string Title { get; set; }
        public string ShortTitle { get; set; }
        public string StatusDate { get; set; }
        public string CurrentStatus { get; set; }
        public string Sponsors { get; set; }
        public string Copy { get; set; }

        private List<BillActivity> _activity;
        public List<BillActivity> Activity
        {
            get
            {
                if (_activity != null)
                {
                    _activity = (from a in _activity
                                 orderby a.Date descending
                                 select a).ToList();
                }
                return _activity;
            }
            set { _activity = value; }
        }
        public List<BillHistory> History { get; set; }
        public List<BillRevision> Revisions { get; set; }
        public List<BillMinutes> Minutes { get; set; }

    }

    /// <summary>
    /// Legislative bill activity
    /// </summary>
    public class BillActivity
    {

        public BillActivity()
        {
            if ((!this.Date.HasValue) && (!String.IsNullOrEmpty(this.Sha)))
            {
                DateTime result;
                bool b = DateTime.TryParse("", out result);

                if (b)
                    this.Date = new DateTime?(result);
            }
        }
        private string _Description;
        public string Description
        {
            get
            {
                //Abbreviations
                List<KeyValuePair<String, String>> abbr = new List<KeyValuePair<string, string>>();

                //Committees
                abbr.Add(new KeyValuePair<string, string>(@"CRA\b", "Community & Regional Affairs Committee"));
                abbr.Add(new KeyValuePair<string, string>(@"EDC\b", "Education Committee"));
                abbr.Add(new KeyValuePair<string, string>(@"FIN\b", "Finance Committee"));
                abbr.Add(new KeyValuePair<string, string>(@"HSS\b", "Health & Social Services Committee"));
                abbr.Add(new KeyValuePair<string, string>(@"JUD\b", "Judiciary Committee"));
                abbr.Add(new KeyValuePair<string, string>(@"L&C\b", "Labor & Commerce Committee"));
                abbr.Add(new KeyValuePair<string, string>(@"RES\b", "Resources Committee"));
                abbr.Add(new KeyValuePair<string, string>(@"RLS\b", "Rules Committee"));
                abbr.Add(new KeyValuePair<string, string>(@"STA\b", "State Affairs Committee"));
                abbr.Add(new KeyValuePair<string, string>(@"TRA\b", "Transportation Committee"));

                //Committee Votes
                abbr.Add(new KeyValuePair<string, string>(@"(?<=([0-9]{1}))DP\b", " Committee members voted to Pass"));
                abbr.Add(new KeyValuePair<string, string>(@"(?<=([0-9]{1}))DNP\b", " Committee members voted Not to Pass"));
                abbr.Add(new KeyValuePair<string, string>(@"(?<=([0-9]{1}))NR\b", " Committee members have No Recommendation"));
                abbr.Add(new KeyValuePair<string, string>(@"(?<=([0-9]{1}))AM\b", " Committee members have Amended"));

                //Where is the bill?
                abbr.Add(new KeyValuePair<string, string>(@"^[(]H[)]", "House Action, "));
                abbr.Add(new KeyValuePair<string, string>(@"[(]H[)]", "House "));
                abbr.Add(new KeyValuePair<string, string>(@"(?<![DATE|COSPONSOR|\s])[(]S[)]", "Senate Action, "));
                abbr.Add(new KeyValuePair<string, string>(@"(?<=[\s])[(]S[)]", "Senate "));

                //Bill Idenfication
                abbr.Add(new KeyValuePair<string, string>(@"FN(?=[0-9][:])", "Financial "));
                abbr.Add(new KeyValuePair<string, string>(@"ZERO(?=\([A-Z]{1,6})", "No Fiscal Impact On "));
                abbr.Add(new KeyValuePair<string, string>(@"INDETERMINATE(?=\([A-Z]{1,6})", "Unknown Fiscal Impact on "));
                abbr.Add(new KeyValuePair<string, string>(@"AM[:]", "Amended:"));
                abbr.Add(new KeyValuePair<string, string>(@"DP[:]", "Voted to Pass:"));
                abbr.Add(new KeyValuePair<string, string>(@"NR[:]", "No Recommendation:"));
                abbr.Add(new KeyValuePair<string, string>(@"AM H", "Amended House"));
                abbr.Add(new KeyValuePair<string, string>(@"RPT", "Report"));
                abbr.Add(new KeyValuePair<string, string>(@"AM NO", "Amendement Number"));
                abbr.Add(new KeyValuePair<string, string>(@"AM S", "Amended Senate"));
                abbr.Add(new KeyValuePair<string, string>(@"TITLE AM", "Title of bill was amended "));
                abbr.Add(new KeyValuePair<string, string>(@"BRF SUP MAJ FLD", "Budget Reserve Fund Super Majority Failed"));
                abbr.Add(new KeyValuePair<string, string>(@"BRF SUP MAJ PFLD", "Budget Reserve Fund Super Majority Partially Failed"));
                abbr.Add(new KeyValuePair<string, string>(@"CCS", "Conference Committee Substitute "));
                abbr.Add(new KeyValuePair<string, string>(@"^CS\b", "Committee Substitute "));
                abbr.Add(new KeyValuePair<string, string>(@"CSHB\b", "Committee Substitute House Bill "));
                abbr.Add(new KeyValuePair<string, string>(@"HB\b", "House Bill "));
                abbr.Add(new KeyValuePair<string, string>(@"CT RULE FLD", "Court Rule Failed"));
                abbr.Add(new KeyValuePair<string, string>(@"EFD ADD", "Effective Date Added"));
                abbr.Add(new KeyValuePair<string, string>(@"EFD AM", "Effective Date Amended"));
                abbr.Add(new KeyValuePair<string, string>(@"EFD DEL", "Effective Date Deleted"));
                abbr.Add(new KeyValuePair<string, string>(@"EFD FLD", "Effective Date Failed"));
                abbr.Add(new KeyValuePair<string, string>(@"FCC", "Free Conference Committee"));
                abbr.Add(new KeyValuePair<string, string>(@"FCCS", "Free Conference Committee Substitute"));
                abbr.Add(new KeyValuePair<string, string>(@"FLD H", "Failed House"));
                abbr.Add(new KeyValuePair<string, string>(@"FLD S", "Failed Senate"));
                abbr.Add(new KeyValuePair<string, string>(@"^HB\b", "House Bill "));
                abbr.Add(new KeyValuePair<string, string>(@"^HCS\b", "House Committee Substitute"));
                abbr.Add(new KeyValuePair<string, string>(@"^HCR\b", "House Concurrent Resolution"));
                abbr.Add(new KeyValuePair<string, string>(@"^HJR\b", "House Joint Resolution"));
                abbr.Add(new KeyValuePair<string, string>(@"^HR\b", " House Resolution"));
                abbr.Add(new KeyValuePair<string, string>(@"PFLD H", "Partially Failed House"));
                abbr.Add(new KeyValuePair<string, string>(@"PFLD S", "Partially Failed Senate"));
                abbr.Add(new KeyValuePair<string, string>(@"^REEN\b", "Reengrossed"));
                abbr.Add(new KeyValuePair<string, string>(@"^SB\b", "Senate Bill"));
                abbr.Add(new KeyValuePair<string, string>(@"^SR\b", "Senate Resolution "));
                abbr.Add(new KeyValuePair<string, string>(@"SCS\b", "Senate Committee Substitute"));
                abbr.Add(new KeyValuePair<string, string>(@"^SCR\b", "Senate Concurrent Resolution"));
                abbr.Add(new KeyValuePair<string, string>(@"^SJR\b", "Senate Joint Resolution"));
                abbr.Add(new KeyValuePair<string, string>(@"^SS\b", "Sponsor Substitute"));
                abbr.Add(new KeyValuePair<string, string>(@"SLA\b", "Session Laws of Alaska"));
                abbr.Add(new KeyValuePair<string, string>(@"2D\b", "Second"));
                abbr.Add(new KeyValuePair<string, string>(@"3D\b", "Third"));

                //Departments
                abbr.Add(new KeyValuePair<string, string>(@"[(]ADM[)]", "(ADM - ADMINISTRATION)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]CED[)]", "(CED - COMMERCE, COMMUNITY & ECONOMIC DEVELOPMENT)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]COR[)]", "(COR - CORRECTIONS)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]CRT[)]", "(CRT - COURT SYSTEM)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]EED[)]", "(EED - EDUCATION AND EARLY DEVELOPMENT)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]DEC[)]", "(DEC - ENVIRONMENTAL CONSERVATION)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]DFG[)]", "(DFG - FISH AND GAME)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]GOV[)]", "(GOV - GOVERNOR’S OFFICE)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]DHS[)]", "(DHS - HEALTH AND SOCIAL SERVICES)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]LWF[)]", "(LWF - LABOR AND WORFKFORCE DEVELOPMENT)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]LAW[)]", "(LAW - LAW)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]LEG[)]", "(LEG - LEGISLATIVE AGENCY)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]MVA[)]", "(MVA - MILITARY AND VETERANS’ AFFAIRS)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]DNR[)]", "(DNR - NATURAL RESOURCES)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]DPS[)]", "(DPS - PUBLIC SAFETY)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]REV[)]", "(REV - REVENUE)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]DOT[)]", "(DOT - TRANSPORTATION AND PUBLIC FACILITIES)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]UA[)]", "(UA - UNIVERSITY OF ALASKA)"));
                abbr.Add(new KeyValuePair<string, string>(@"[(]ALL[)]", "(ALL DEPARTMENTS)"));

                //Voting Nmbers
                abbr.Add(new KeyValuePair<string, string>(@"Y(?=[0-9]{1,2})", "YES: "));
                abbr.Add(new KeyValuePair<string, string>(@"N(?=[0-9]{1,2})", "NO: "));
                abbr.Add(new KeyValuePair<string, string>(@"E(?=[0-9]{1,2})", "EXCUSED: "));
                abbr.Add(new KeyValuePair<string, string>(@"A(?=[0-9]{1,2})", "ABSENT: "));

                //Zero votes
                abbr.Add(new KeyValuePair<string, string>(@"Y(?=[\-]{1})-", "YES: 0 "));
                abbr.Add(new KeyValuePair<string, string>(@"N(?=[\-]{1})-", "NO: 0 "));

                //Minute fix
                abbr.Add(new KeyValuePair<string, string>(@"(?<=MINUTE)[(]", "S ("));

                //Replace values
                foreach (KeyValuePair<string, string> r in abbr.ToList())
                    if (_Description != null)
                        _Description = Regex.Replace(_Description, r.Key, r.Value);
                
                return _Description;

                //return _Description.ToLower();
            }
            set { _Description = value; }
        }
        public DateTime? Date { get; set; }
        public string Journal { get; set; }
        public string Sha { get; set; }

    }

    /// <summary>
    /// Legislative bill history
    /// </summary>
    public class BillHistory
    {
        public string Offered { get; set; }
        public DateTime? OfferDate { get; set; }
        public DateTime? PassedHouse { get; set; }
        public DateTime? PassedSenate { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
    }

    /// <summary>
    /// Legislative revisions
    /// </summary>
    public class BillRevision
    {
        public string Version { get; set; }
        public string Offer { get; set; }
        public DateTime? Date { get; set; }
        public string Sha { get; set; }
    }

    /// <summary>
    /// Legislative meeting minutes
    /// </summary>
    public class BillMinutes
    {

        public DateTime DateTime { get; set; }
        public string Copy { get; set; }
        public string Title { get; set; }
        public List<LineItem> LineItems { get; set; }
        public string Committee_Name { get; set; }
        public string Audio_Url { get; set; }
        public string Minutes_Url { get; set; }
        public string Begin_Line { get; set; }
        public string End_Line { get; set; }
        public string Committee { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }

    }

    public class LineItem
    {
        public string Copy { get; set; }
        public string Time { get; set; }
    }

}
