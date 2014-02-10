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

namespace info.Controllers
{
    public class HomeController : Controller
    {

        [HttpGet]
        public ActionResult Index()
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
        [Authorize(Roles="Admin")]
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

    }

}