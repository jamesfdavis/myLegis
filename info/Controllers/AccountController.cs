using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using info.Models;
using info.ActionFilters;
using System.Data;
using System.Configuration;
namespace info.Controllers
{
    /// <summary>
    /// Simple class that represents a OpenId validated user in the system
    /// </summary>
    public class UserData
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string ClaimedIdentifier { get; set; }
        public bool IsAdmin { get; set; }
        public override string ToString()
        {
            return String.Format("{0}-{1}", Email, FullName);
        }
    }

    /// <summary>
    /// Provides actions/methods for creating an OpenId request and how to 
    /// obtain information from a authenticated OpenId response, and also 
    /// has methods for creating a Forms Authentication Ticket
    /// </summary>
    public class AccountController : Controller
    {

        private Models.Entities db = new Models.Entities();

        #region Actions

        [HttpGet]
        [Authorize]
        [CacheBuster]
        public ViewResult Index()
        {
            //RoundTrip to the DB
            User usr = (from u in db.Users.Where(n => n.ClaimedIdentifier == User.Identity.Name)
                        select u).FirstOrDefault();

            return View(usr);
        }

        [HttpGet]
        [Authorize(Roles="Admin")]
        [CacheBuster]
        public ViewResult Users() {

            //RoundTrip to the DB
            List<User> usrs = (from u in db.Users
                        select u).ToList();

            return View(usrs);
        }

        /// <summary>
        /// Show default logon page, and also see if user is already authenticated. 
        /// If they are we can examine the IAuthenticationResponse to get the 
        /// user data from the OpenId provider
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult LogOn()
        {

            ViewData["message"] = "You are not logged in";

            OpenIdRelyingParty openid = new OpenIdRelyingParty();
            IAuthenticationResponse response = openid.GetResponse();

            //check for ReturnUrl, which we should have if we use forms 
            //authentication and [Authorise] on our controllers
            if (Request.Params["ReturnUrl"] != null)
                Session["ReturnUrl"] = Request.Params["ReturnUrl"];

            if (response != null && response.Status == AuthenticationStatus.Authenticated)
            {
                string cid = response.ClaimedIdentifier;

                var claimUntrusted = response.GetUntrustedExtension<ClaimsResponse>();
                var fetchUntrusted = response.GetUntrustedExtension<FetchResponse>();

                var claim = response.GetExtension<ClaimsResponse>();
                var fetch = response.GetExtension<FetchResponse>();

                UserData userData = null;

                if (claim != null)
                {
                    userData = new UserData();
                    userData.ClaimedIdentifier = cid;
                    userData.Email = claim.Email;
                    userData.FullName = claim.FullName;
                    //Grab Google Profile details
                    if (String.IsNullOrEmpty(claim.FullName) && fetch.Attributes.Count() != 0)
                        userData.FullName = String.Format("{0} {1}",
                            fetch.Attributes["http://axschema.org/namePerson/first"].Values[0].ToString(),
                            fetch.Attributes["http://axschema.org/namePerson/last"].Values[0].ToString());
                }

                //fallback to claim untrusted, as some OpenId providers may not
                //provide the trusted ClaimsResponse, so we have to fallback to 
                //trying the untrusted on
                if (claimUntrusted != null && userData == null)
                {
                    userData = new UserData();
                    userData.ClaimedIdentifier = cid;
                    userData.Email = claimUntrusted.Email;
                    userData.FullName = claimUntrusted.FullName;  
                }

                //RoundTrip to the DB
                User usr = (from u in db.Users.Where(n => n.ClaimedIdentifier == cid)
                            select u).FirstOrDefault();

                if (usr != null)
                {
                    //Update the User
                    usr.Email = userData.Email;
                    usr.Name = userData.FullName;
                    db.ObjectStateManager.ChangeObjectState(usr, EntityState.Modified);
                    //Setup role
                    if (ConfigurationManager.AppSettings["appAdmin"] == cid)
                        userData.IsAdmin = true;
                    db.SaveChanges();
                }
                else
                {
                    //Insert the User
                    db.Users.AddObject(new User
                    {
                        ClaimedIdentifier = response.ClaimedIdentifier,
                        Email = userData.Email,
                        IsAdmin = ConfigurationManager.AppSettings["appAdmin"] == cid ? true : false,
                        Name = userData.FullName
                    });
                    db.SaveChanges();
                }

                //now store Forms Authorization cookie 
                IssueAuthTicket(userData, true);

                //store ClaimedIdentifier it in Session 
                //(this would more than likely be something you would store in a database I guess
                Session["ClaimedIdentifierMessage"] = response.ClaimedIdentifier;

                //If we have a ReturnUrl we MUST be using forms authentication, 
                //so redirect to the original ReturnUrl
                if (Session["ReturnUrl"] != null)
                {
                    string url = Session["ReturnUrl"].ToString();
                    return new RedirectResult(url);
                }
                //This should not happen if all controllers have [Authorise] used on them
                else
                    return RedirectToAction("Index", new { Controller = "acct" });
            }
            return View("LogOn");
        }

        /// <summary>
        /// Logon post request, so redirect to OpenId provider and when they authenticate, 
        /// redirect back to orginal url which is the one that needed authenticating in the 1st place
        /// </summary>
        [HttpPost]
        public ActionResult LogOn(string openid_identifier)
        {
            var openid = new OpenIdRelyingParty();

            IAuthenticationRequest request = openid.CreateRequest(Identifier.Parse(openid_identifier));

            //FetchRequest fr = null;
            //Let's tell Google, what we want to have from the user:
            var fetch = new FetchRequest();
            fetch.Attributes.AddRequired(WellKnownAttributes.Contact.Email);
            fetch.Attributes.AddRequired(WellKnownAttributes.Name.First);
            fetch.Attributes.AddRequired(WellKnownAttributes.Name.Last);
            request.AddExtension(fetch);

            var fields = new ClaimsRequest();
            fields.Email = DemandLevel.Require;
            fields.FullName = DemandLevel.Require;
            request.AddExtension(fields);

            return request.RedirectingResponse.AsActionResult();

        }

        /// <summary>
        /// Logoff and clears forms authentication cookie
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOff()
        {
            Session.RemoveAll();
            Session.Clear();
            Session.Abandon();
            Response.Cookies.Remove("AUTHCOOKIE");
            FormsAuthentication.SignOut();

            return View("LogOff");
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Issue forms authentication ticket for authenticated user, and store the cookie
        /// </summary>
        private void IssueAuthTicket(UserData userData, bool rememberMe)
        {
            FormsAuthenticationTicket ticket =
                new FormsAuthenticationTicket(1, userData.ClaimedIdentifier.ToString(),
                    DateTime.Now, DateTime.Now.AddDays(10),
                    rememberMe, userData.IsAdmin == true ? "Admin" : "");

            string ticketString = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, ticketString);
            if (rememberMe)
                cookie.Expires = DateTime.Now.AddDays(10);

            HttpContext.Response.Cookies.Add(cookie);
        }
        #endregion

    }
}
