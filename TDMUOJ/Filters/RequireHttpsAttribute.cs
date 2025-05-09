using System;
using System.Web.Mvc;

namespace TDMUOJ.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class RequireHttpsAttribute : FilterAttribute, IAuthorizationFilter
    {
        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (!filterContext.HttpContext.Request.IsSecureConnection)
            {
                HandleNonHttpsRequest(filterContext);
            }
            else
            {
                // Thêm HSTS header
                filterContext.HttpContext.Response.AddHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            }
        }

        protected virtual void HandleNonHttpsRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpStatusCodeResult(403, "HTTPS Required");
            //if (filterContext.HttpContext.Request.Url.Host.Contains("localhost"))
            //{
            //    // Cho phép HTTP trong môi trường phát triển
            //    return;
            //}

            //if (string.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            //{
            //    // Chuyển hướng đến HTTPS
            //    string url = "https://" + filterContext.HttpContext.Request.Url.Host + filterContext.HttpContext.Request.RawUrl;
            //    filterContext.Result = new RedirectResult(url);
            //}
            //else
            //{
            //    // Đối với các phương thức không phải GET, trả về lỗi
            //    filterContext.Result = new HttpStatusCodeResult(403, "HTTPS Required");
            //}
        }
    }
}
