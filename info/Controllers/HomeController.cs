using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;

using myLegis.Spider.Models;
using myLegis.Spider;
using info.Models;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.Diagnostics;

namespace info.Controllers
{


    public class HomeController : Controller
    {

        [HttpGet]
        public ActionResult Index()
        {

            //// Create the source, if it does not already exist.
            //if (!EventLog.SourceExists("MYEVENTSOURCE"))
            //{
            //    EventLog.CreateEventSource("MYEVENTSOURCE", "MYEVENTSOURCE");
            //    Debug.WriteLine("CreatingEventSource");
            //}
            ////eventcreate /ID 1 /L APPLICATION /T INFORMATION  /SO MYEVENTSOURCE /D "My first log"
            //// Create an EventLog instance and assign its source.
            //EventLog myLog = new EventLog();
            //myLog.Source = "MYEVENTSOURCE";

            //// Write an informational entry to the event log.    
            //myLog.WriteEntry("Writing to event log.");


            QuartzDatastore qd = new QuartzDatastore();

            if (qd.Jobs == null)
            {
                qd.ScheduleJob();
            }

            return View();

        }


        [HttpGet]
        public ViewResult Search()
        {
            return View();
        }

        [HttpGet]
        public ViewResult Senate()
        {

            List<SessionList> lst = new List<SessionList>();

            lst.Add(BillRepo.CollectBills("SB*", "SB"));
            lst.Add(BillRepo.CollectBills("SR*", "SR"));
            lst.Add(BillRepo.CollectBills("SJR*", "SJR"));
            lst.Add(BillRepo.CollectBills("SCR*", "SCR"));

            return View(lst);

        }

        [HttpGet]
        //[Authorize(Roles="Admin")]
        public ViewResult House()
        {

            List<SessionList> lst = new List<SessionList>();

            lst.Add(BillRepo.CollectBills("HB*", "HB"));
            lst.Add(BillRepo.CollectBills("HR*", "HR"));
            lst.Add(BillRepo.CollectBills("HJR*", "HJR"));
            lst.Add(BillRepo.CollectBills("HCR*", "HCR"));

            return View(lst);

        }

        [HttpGet]
        public ActionResult Bill(String id)
        {

            //Where does the bill live?
            string residence = String.Empty;

            Regex rgxSenate = new Regex("S[A-Z]", RegexOptions.IgnoreCase);
            Regex rgxHouse = new Regex("H[A-Z]", RegexOptions.IgnoreCase);

            if (rgxSenate.IsMatch(id))
                residence = "Senate";

            if (rgxHouse.IsMatch(id))
                residence = "House";

            ViewData["Residence"] = residence;

            Legislation ls = BillRepo.LoadBill(id);

            return View(ls);

        }

        [HttpGet]
        [ActionName("404")]
        public ViewResult FileNotFound()
        {
            return View();
        }

        [HttpGet]
        [ActionName("500")]
        public ViewResult ServerError()
        {
            return View();
        }

        [HttpGet]
        public ViewResult Error()
        {
            return View();
        }

        [HttpGet]
        public ViewResult DBTest()
        {

            string rslt = "Start";
            try
            {
                SqlConnection conn = new SqlConnection(@"data source=.\SQLEXPRESS;attachdbfilename=|DataDirectory|\www.mdf;integrated security=True;user instance=True;multipleactiveresultsets=True;App=EntityFramework");
                conn.Open();
                rslt = "Opened";
                conn.Close();
                rslt = "Closed";
            }
            catch (Exception ex)
            {
                rslt = ex.Message;
            }
            ViewData["Result"] = rslt;

            return View();

        }

    }

}