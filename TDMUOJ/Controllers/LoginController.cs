using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TDMUOJ.Models;

namespace TDMUOJ.Controllers
{
    public class LoginController : Controller
    {
        TDMUOJEntities db = new TDMUOJEntities();
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            string username = form["username"];
            string password = form["password"];
            Account account = db.Accounts.SingleOrDefault(n => n.username == username && n.password == password);
            if (account != null)
            {
                Session["User"] = account;
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Tên tài khoản hoặc mật khẩu không đúng";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Login");
        }
    }
}