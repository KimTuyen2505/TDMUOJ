using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TDMUOJ.Models;

namespace TDMUOJ.Controllers
{
    public class HomeController : Controller
    {
        TDMUOJEntities db = new TDMUOJEntities();
        // GET: Home
        public ActionResult Index()
        {
            if (Session["User"] == null)
            {
                Account account = db.Accounts.SingleOrDefault(n => n.username == "tuyencute03" && n.password == "123456");
                if (account != null)
                {
                    Session["User"] = account;
                }
            }
            return View();
        }
    }
}