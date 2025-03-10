using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TDMUOJ.Models;

namespace TDMUOJ.Controllers
{
    public class RegisterController : Controller
    {
        TDMUOJEntities db = new TDMUOJEntities();
        // GET: Register
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            string fullName = form["fullName"];
            string username = form["username"];
            string email = form["email"];
            string password = form["password"];
            string confirmPassword = form["confirmPassword"];

            string result = CheckUsername(username);
            if (result != "Success")
            {
                ViewBag.Error = result;
                return View();
            }
            result = CheckEmail(email);
            if (result != "Success")
            {
                ViewBag.Error = result;
                return View();
            }
            result = CheckPassword(password, confirmPassword);
            if (result != "Success")
            {
                ViewBag.Error = result;
                return View();
            }
            if (db.Accounts.Any(u => u.username == username))
            {
                ViewBag.Error = "Tên truy cập đã tồn tại";
                return View();
            }
            if (db.Accounts.Any(u => u.email == email))
            {
                ViewBag.Error = "Địa chỉ email đã tồn tại";
                return View();
            }
            Account account = new Account();
            account.username = username;
            account.password = password;
            account.email = email;
            account.name = fullName;
            account.avatar = "default_avatar.png";
            account.role = "user";
            account.rating = 500;
            account.maxRating = 500;
            account.numberOfAccepted = 0;
            db.Accounts.Add(account);
            db.SaveChanges();
            return RedirectToAction("Index", "Login");
        }

        public string CheckPassword(string password, string confirmPassword)
        {
            // Kiểm tra các tiêu chí của mật khẩu
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                return "Mật khẩu không được để trống";
            }

            if (password != confirmPassword)
            {
                return "Xác nhận mật khẩu thất bại";
            }

            bool hasUppercase = password.Any(char.IsUpper);
            bool hasLowercase = password.Any(char.IsLower);
            bool hasNumber = password.Any(char.IsDigit);
            bool hasSpecialCharacter = Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]");

            if (!hasUppercase || !hasLowercase || !hasNumber || !hasSpecialCharacter)
            {
                return "Mật khẩu phải bao gồm ít nhất 1 chữ cái viết hoa, 1 chữ cái viết thường, 1 chữ số và 1 kí tự đặc biệt";
            }

            // Nếu tất cả điều kiện hợp lệ
            return "Success";
        }
        public string CheckUsername(string username)
        {
            // Kiểm tra username không được để trống
            if (string.IsNullOrEmpty(username))
            {
                return "Tên truy cập không được để trống";
            }

            // Kiểm tra độ dài tối thiểu của username
            if (username.Length < 6)
            {
                return "Tên truy cập phải có ít nhất 6 ký tự";
            }

            // Kiểm tra username không chứa ký tự đặc biệt
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9]+$"))
            {
                return "Tên truy cập chỉ được chứa chữ cái và số";
            }

            // Nếu tất cả điều kiện hợp lệ
            return "Success";
        }
        public string CheckEmail(string email)
        {
            // Kiểm tra email không được để trống
            if (string.IsNullOrEmpty(email))
            {
                return "Địa chỉ email không được để trống";
            }

            // Kiểm tra định dạng email
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                return "Địa chỉ email không hợp lệ";
            }

            // Nếu tất cả điều kiện hợp lệ
            return "Success";
        }
    }
}