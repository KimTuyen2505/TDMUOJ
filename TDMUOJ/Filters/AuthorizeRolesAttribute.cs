using System;
using System.Web;
using System.Web.Mvc;
using TDMUOJ.Models;

namespace TDMUOJ.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        private readonly string[] _roles;

        public AuthorizeRolesAttribute(params string[] roles)
        {
            _roles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            var user = httpContext.Session["User"] as Account;

            if (user == null)
                return false;

            if (_roles.Length == 0)
                return true;

            return Array.Exists(_roles, role => role.Equals(user.role, StringComparison.OrdinalIgnoreCase));
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Session["User"] == null)
            {
                filterContext.Result = new RedirectResult("~/Login/Index");
            }
            else
            {
                filterContext.Result = new RedirectResult("~/AccessDenied/Index");
            }
        }
    }
}
