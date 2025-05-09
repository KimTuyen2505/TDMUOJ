using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TDMUOJ.Models;
using System.Net;
using System.Net.Mail;
using System.Web.Security;
using System.Configuration;
using Org.BouncyCastle.Crypto.Generators;
using DevOne.Security.Cryptography.BCrypt;

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
        [ValidateAntiForgeryToken]
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

            // Create account with unverified status
            Account account = new Account();
            account.username = username;
            account.password = BCryptHelper.HashPassword(password, BCryptHelper.GenerateSalt());
            account.email = email;
            account.name = fullName;
            account.avatar = "default_avatar.png";
            account.role = "user";
            account.rating = 500;
            account.maxRating = 500;
            account.numberOfAccepted = 0;
            account.isEmailVerified = false; // New field to track email verification

            db.Accounts.Add(account);
            db.SaveChanges();

            // Generate verification token
            string token = Guid.NewGuid().ToString();

            // Save verification details
            var verification = new EmailVerification
            {
                Email = email,
                Token = token,
                ExpiryDate = DateTime.Now.AddDays(1),
                IsVerified = false
            };

            db.EmailVerifications.Add(verification);
            db.SaveChanges();

            // Send verification email
            SendVerificationEmail(email, token);

            ViewBag.SuccessMessage = "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.";
            return View("RegistrationSuccess");
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

            if (password.Length < 8)
            {
                return "Mật khẩu phải có ít nhất 8 ký tự";
            }

            bool hasUppercase = password.Any(char.IsUpper);
            bool hasLowercase = password.Any(char.IsLower);
            bool hasNumber = password.Any(char.IsDigit);
            bool hasSpecialCharacter = password.Any(c => !char.IsLetterOrDigit(c));

            if (!hasUppercase)
            {
                return "Mật khẩu phải chứa ít nhất một chữ cái viết hoa";
            }

            if (!hasLowercase)
            {
                return "Mật khẩu phải chứa ít nhất một chữ cái viết thường";
            }

            if (!hasNumber)
            {
                return "Mật khẩu phải chứa ít nhất một chữ số";
            }

            if (!hasSpecialCharacter)
            {
                return "Mật khẩu phải chứa ít nhất một ký tự đặc biệt";
            }

            // Kiểm tra mật khẩu phổ biến
            string[] commonPasswords = { "Password123!", "Admin123!", "123456789Ab!", "Qwerty123!" };
            if (commonPasswords.Contains(password))
            {
                return "Mật khẩu quá phổ biến, vui lòng chọn mật khẩu khác";
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

        private void SendVerificationEmail(string email, string token)
        {
            var verifyUrl = Url.Action("VerifyEmail", "Register", new { token = token }, Request.Url.Scheme);
            var fromEmail = ConfigurationManager.AppSettings["EmailFrom"];
            var fromPassword = ConfigurationManager.AppSettings["EmailPassword"];

            var message = new MailMessage();
            message.From = new MailAddress(fromEmail);
            message.Subject = "TDMUOJ - Xác thực tài khoản";
            message.To.Add(new MailAddress(email));
            message.Body = $"<html><body>Chào bạn,<br/><br/>Vui lòng xác thực tài khoản của bạn bằng cách nhấp vào liên kết sau: <a href='{verifyUrl}'>Xác thực ngay</a><br/><br/>Liên kết này sẽ hết hạn sau 24 giờ.<br/><br/>Trân trọng,<br/>TDMUOJ Team</body></html>";
            message.IsBodyHtml = true;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true,
            };

            smtpClient.Send(message);
        }

        public ActionResult VerifyEmail(string token)
        {
            var verification = db.EmailVerifications.FirstOrDefault(v => v.Token == token && v.IsVerified == false);

            if (verification == null)
            {
                ViewBag.Message = "Liên kết xác thực không hợp lệ hoặc đã hết hạn.";
                return View("VerificationFailed");
            }

            if (verification.ExpiryDate < DateTime.Now)
            {
                ViewBag.Message = "Liên kết xác thực đã hết hạn.";
                return View("VerificationFailed");
            }

            // Update verification status
            verification.IsVerified = true;

            // Update account status
            var account = db.Accounts.FirstOrDefault(a => a.email == verification.Email);
            if (account != null)
            {
                account.isEmailVerified = true;
            }

            db.SaveChanges();

            ViewBag.Message = "Xác thực email thành công! Bạn có thể đăng nhập ngay bây giờ.";
            return View("VerificationSuccess");
        }

        public ActionResult ResendVerification(string email)
        {
            var account = db.Accounts.FirstOrDefault(a => a.email == email && a.isEmailVerified == false);

            if (account == null)
            {
                ViewBag.Error = "Email không tồn tại hoặc đã được xác thực.";
                return View("ResendVerification");
            }

            // Generate new verification token
            string token = Guid.NewGuid().ToString();

            // Update or create verification record
            var verification = db.EmailVerifications.FirstOrDefault(v => v.Email == email);
            if (verification != null)
            {
                verification.Token = token;
                verification.ExpiryDate = DateTime.Now.AddDays(1);
                verification.IsVerified = false;
            }
            else
            {
                verification = new EmailVerification
                {
                    Email = email,
                    Token = token,
                    ExpiryDate = DateTime.Now.AddDays(1),
                    IsVerified = false
                };
                db.EmailVerifications.Add(verification);
            }

            db.SaveChanges();

            // Send verification email
            SendVerificationEmail(email, token);

            ViewBag.SuccessMessage = "Email xác thực đã được gửi lại. Vui lòng kiểm tra hộp thư của bạn.";
            return View("ResendVerificationSuccess");
        }
    }
}
