using System.Web;
using System.Web.Mvc;

/// <summary>
/// For ensuring AntiForgeryToken is being supplied to the client no-caching is required and can be set on controller level with the nocache attribute 
/// </summary>
/// <example>[NoCache] // Apply the filter to the entire controller\n\npublic class MyController : Controller</example>
public class NoCacheAttribute : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        // For ensuring the AntiForgeryToken is being supplied to the client no-caching is required 
        filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache); // Set Cache-Control: no-cache        
        filterContext.HttpContext.Response.Cache.SetNoStore();                              // Set Cache-Control: no-store
    }
}