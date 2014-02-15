using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using info.Models;

namespace info.Controllers
{
    public class WatchController : ApiController
    {
        private Models.Entities db = new Models.Entities();

        public class MyWatch
        {
            public string Name { get; set; }
        }

        // GET: /Watch/
        [HttpGet]
        public bool GetWatch(String ID)
        {
            if (User.Identity.IsAuthenticated)
            {
                var wtc = (from w in db.Watches.Where(n => (n.ClaimedIdentifier == User.Identity.Name) && (n.Name == ID))
                           select w).FirstOrDefault();

                if (wtc == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
            }
        }

        [HttpPut]
        public bool PutWatch(MyWatch W)
        {

            if (User.Identity.IsAuthenticated)
            {

                var wtc = (from w in db.Watches.Where(n => (n.ClaimedIdentifier == User.Identity.Name) && (n.Name == W.Name))
                           select w).FirstOrDefault();

                if (wtc == null)
                {
                    db.Watches.AddObject(new Models.Watch { ClaimedIdentifier = User.Identity.Name, Name = W.Name });
                    db.SaveChanges();
                    return true;
                }
                else
                {
                    db.Watches.DeleteObject(wtc);
                    db.SaveChanges();
                    return false;
                }
            }
            else
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
            }
        }

        // lst.Add(BillRepo.CollectBills("SB*", "SB", new string[] { "SB1", "SB2" }));
        // GET api/Phrase
        [HttpGet]
        public IEnumerable<ItemOverview> GetWatchList()
        {

            if (User.Identity.IsAuthenticated)
            {
                string[] filter = (from l in db.Watches.Where(n => n.ClaimedIdentifier == User.Identity.Name)
                                   select l.Name).ToArray<string>();

                SessionList sl = BillRepo.CollectBills("*", "Watches", filter);

                return sl.Bills;
            }
            else
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
            }

        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
