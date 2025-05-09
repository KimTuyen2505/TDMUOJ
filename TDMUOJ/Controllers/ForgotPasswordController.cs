using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using TDMUOJ.Models;
using DevOne.Security.Cryptography.BCrypt;

namespace TDMUOJ.Controllers
{
    public class ForgotPasswordController : Controller
    {
        private TDMUOJEntities db = new TDMUOJEntities();

        // GET: ForgotPassword
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string email)
        {
            var account = db.Accounts.FirstOrDefault(a => a.email == email);

            if (account == null)
            {
                ViewBag.Error = "Email không tồn tại trong hệ thống.";
                return View();
            }

            // Generate 6-digit OTP
            Random random = new Random();
            string otp = random.Next(100000, 999999).ToString();

            // Save OTP to database
            var otpVerification = new OTPVerification
            {
                Email = email,
                OTPCode = otp,
                ExpiryDate = DateTime.Now.AddMinutes(15),
                IsUsed = false
            };

            db.OTPVerifications.Add(otpVerification);
            db.SaveChanges();

            // Send OTP email
            SendOTPEmail(email, otp);

            // Redirect to OTP verification page
            return RedirectToAction("VerifyOTP", new { email = email });
        }

        public ActionResult VerifyOTP(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public ActionResult VerifyOTP(string email, string otp)
        {
            var otpVerification = db.OTPVerifications
                .Where(o => o.Email == email && o.OTPCode == otp && o.IsUsed == false)
                .OrderByDescending(o => o.ExpiryDate)
                .FirstOrDefault();

            if (otpVerification == null)
            {
                ViewBag.Email = email;
                ViewBag.Error = "Mã OTP không hợp lệ.";
                return View();
            }

            if (otpVerification.ExpiryDate < DateTime.Now)
            {
                ViewBag.Email = email;
                ViewBag.Error = "Mã OTP đã hết hạn.";
                return View();
            }

            // Mark OTP as used
            otpVerification.IsUsed = true;
            db.SaveChanges();

            // Redirect to reset password page
            return RedirectToAction("ResetPassword", new { email = email, token = otpVerification.OTPCode });
        }

        public ActionResult ResetPassword(string email, string token)
        {
            var otpVerification = db.OTPVerifications
                .FirstOrDefault(o => o.Email == email && o.OTPCode == token && o.IsUsed == true);

            if (otpVerification == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string email, string token, string newPassword, string confirmPassword)
        {
            var otpVerification = db.OTPVerifications
                .FirstOrDefault(o => o.Email == email && o.OTPCode == token && o.IsUsed == true);

            if (otpVerification == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Email = email;
                ViewBag.Token = token;
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            // Validate password strength
            var registerController = new RegisterController();
            string passwordCheck = registerController.CheckPassword(newPassword, confirmPassword);
            if (passwordCheck != "Success")
            {
                ViewBag.Email = email;
                ViewBag.Token = token;
                ViewBag.Error = passwordCheck;
                return View();
            }

            // Update password
            var account = db.Accounts.FirstOrDefault(a => a.email == email);
            if (account != null)
            {
                account.password = BCryptHelper.HashPassword(newPassword, BCryptHelper.GenerateSalt());
                db.SaveChanges();

                ViewBag.SuccessMessage = "Mật khẩu đã được đặt lại thành công.";
                return View("ResetSuccess");
            }

            ViewBag.Email = email;
            ViewBag.Token = token;
            ViewBag.Error = "Không thể đặt lại mật khẩu. Vui lòng thử lại.";
            return View();
        }

        private void SendOTPEmail(string email, string otp)
        {
            var fromEmail = ConfigurationManager.AppSettings["EmailFrom"];
            var fromPassword = ConfigurationManager.AppSettings["EmailPassword"];

            var message = new MailMessage();
            message.From = new MailAddress(fromEmail);
            message.Subject = "TDMUOJ - Mã xác thực OTP";
            message.To.Add(new MailAddress(email));
            message.Body = $"<html><body>Chào bạn,<br/><br/>Mã xác thực OTP của bạn là: <strong>{otp}</strong><br/><br/>Mã này sẽ hết hạn sau 15 phút.<br/><br/>Trân trọng,<br/>TDMUOJ Team</body></html>";
            message.IsBodyHtml = true;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true,
            };

            smtpClient.Send(message);
        }
    }
}
