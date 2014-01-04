using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using info.Models;

namespace info.Controllers
{
    public class PhraseController : ApiController
    {
        private Entities db = new Entities();

        // GET api/Phrase
        public IEnumerable<Phrase> GetPhrases()
        {
            return db.Phrases.AsEnumerable();
        }

        // GET api/Phrase/5
        [HttpGet]
        public Phrase GetPhrase(int id)
        {

            Phrase phrase = db.Phrases.Single(p => p.ID == id);

            if (phrase == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return phrase;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}