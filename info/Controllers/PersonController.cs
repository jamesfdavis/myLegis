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
    public class PersonController : ApiController
    {
        private wwwEntities db = new wwwEntities();

        // GET api/Default1
        public IEnumerable<Person> GetPeople()
        {
            return db.Person.AsEnumerable();
        }

        // GET api/Default1/5
        public Person GetPerson(int id)
        {
            Person person = db.Person.Single(p => p.ID == id);
            if (person == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return person;
        }

        // PUT api/Default1/5
        public HttpResponseMessage PutPerson(int id, Person person)
        {
            if (ModelState.IsValid && id == person.ID)
            {
                db.Person.Attach(person);
                db.ObjectStateManager.ChangeObjectState(person, EntityState.Modified);

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // POST api/Default1
        public HttpResponseMessage PostPerson(Person person)
        {
            if (ModelState.IsValid)
            {
                db.Person.AddObject(person);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, person);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = person.ID }));
                return response;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // DELETE api/Default1/5
        public HttpResponseMessage DeletePerson(int id)
        {
            Person person = db.Person.Single(p => p.ID == id);
            if (person == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Person.DeleteObject(person);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            return Request.CreateResponse(HttpStatusCode.OK, person);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}