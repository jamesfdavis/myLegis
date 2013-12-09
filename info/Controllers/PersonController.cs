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
using info.Models.DTO;

namespace info.Controllers
{
    public class PersonController : ApiController
    {
        private Models.Entities db = new Models.Entities();

        //// GET api/Person
        //[HttpGet]
        //public IEnumerable<Person> GetPeople()
        //{
        //    var people = db.People.Include("Phrase");
        //    return people.AsEnumerable();
        //}

        //// GET api/Person/5
        //[HttpGet]
        //public Person GetPerson(int id)
        //{
        //    Person person = db.People.Single(p => p.ID == id);
        //    if (person == null)
        //    {
        //        throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
        //    }

        //    return person;
        //}

        //// PUT api/Person/5
        //[HttpPut]
        //public HttpResponseMessage PutPerson(int id, Person person)
        //{
        //    if (ModelState.IsValid && id == person.ID)
        //    {
        //        db.People.Attach(person);
        //        db.ObjectStateManager.ChangeObjectState(person, EntityState.Modified);

        //        try
        //        {
        //            db.SaveChanges();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound);
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    else
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest);
        //    }
        //}

        // POST api/Person
        [HttpPost]
        public HttpResponseMessage PostPerson(Person person)
        {
            if (ModelState.IsValid)
            {
                //Fetch the Copy FK (Phrase), add if necessary.
                
                //Create new Person Entity
                
                //db.People.AddObject(person);
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

        // DELETE api/Person/5
        //[HttpDelete]
        //public HttpResponseMessage DeletePerson(int id)
        //{
        //    Person person = db.People.Single(p => p.ID == id);
        //    if (person == null)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.NotFound);
        //    }

        //    db.People.DeleteObject(person);

        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.NotFound);
        //    }

        //    return Request.CreateResponse(HttpStatusCode.OK, person);
        //}

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}