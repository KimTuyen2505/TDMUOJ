using DevOne.Security.Cryptography.BCrypt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using TDMUOJ.Models;
using TDMUOJ.Utilities;

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
        [ValidateAntiForgeryToken]
        public ActionResult Index(FormCollection form)
        {
            // Xác thực CAPTCHA
            if (!RecaptchaValidator.Validate(Request))
            {
                ViewBag.Error = "Xác thực CAPTCHA thất bại. Vui lòng thử lại.";
                return View();
            }
            string username = form["username"];
            string password = form["password"];

            Account account = db.Accounts.SingleOrDefault(n => n.username == username);

            if (account != null && BCryptHelper.CheckPassword(password, account.password))
            {
                // Kiểm tra xác thực email nếu tính năng đã được bật
                if (account.isEmailVerified == false)
                {
                    ViewBag.Error = "Tài khoản chưa được xác thực. Vui lòng kiểm tra email của bạn.";
                    return View();
                }

                // Kiểm tra tài khoản có bị khóa không
                if (account.isLocked == true)
                {
                    if (account.lockoutEnd.HasValue && account.lockoutEnd.Value > DateTime.Now)
                    {
                        ViewBag.Error = $"Tài khoản đã bị khóa. Vui lòng thử lại sau {(account.lockoutEnd.Value - DateTime.Now).Minutes} phút.";
                        return View();
                    }
                    else
                    {
                        // Mở khóa tài khoản nếu thời gian khóa đã hết
                        account.isLocked = false;
                        account.failedLoginAttempts = 0;
                        account.lockoutEnd = null;
                        db.SaveChanges();
                    }
                }

                // Reset số lần đăng nhập thất bại
                account.failedLoginAttempts = 0;
                db.SaveChanges();

                Session["User"] = account;
                return RedirectToAction("Index", "Home");
            }

            // Xử lý đăng nhập thất bại
            if (account != null)
            {
                account.failedLoginAttempts = (account.failedLoginAttempts ?? 0) + 1;

                // Khóa tài khoản sau 5 lần đăng nhập thất bại
                if (account.failedLoginAttempts >= 5)
                {
                    account.isLocked = true;
                    account.lockoutEnd = DateTime.Now.AddMinutes(15); // Khóa 15 phút
                }

                db.SaveChanges();
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
