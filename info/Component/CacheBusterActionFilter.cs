
using System.Web.Mvc;
using System;
using System.Web;

namespace info.ActionFilters
{

    public class CacheBuster : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            ////throw new System.NotImplementedException();
            filterContext.RequestContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            filterContext.RequestContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
            filterContext.RequestContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            filterContext.RequestContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            filterContext.RequestContext.HttpContext.Response.Cache.SetNoStore();           
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //throw new System.NotImplementedException();

        }
    }
}