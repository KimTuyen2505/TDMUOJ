using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TDMUOJ.Models;

namespace TDMUOJ.Controllers
{
    public class ProfileController : Controller
    {
        TDMUOJEntities db = new TDMUOJEntities();
        // GET: Profile
        public ActionResult Index(int id)
        {
            var profile = db.Accounts.SingleOrDefault(x => x.id == id);
            return View(profile);
        }
        [HttpPost]
        public ActionResult UpdateProfile(string name, string email, HttpPostedFileBase avatar)
        {
            try
            {
                int userId = (Session["User"] as Account).id;
                var user = db.Accounts.Find(userId);

                if (user != null)
                {
                    user.name = name;
                    user.email = email;

                    if (avatar != null && avatar.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(avatar.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/IMAGE"), fileName);
                        avatar.SaveAs(path);
                        user.avatar = fileName;
                    }

                    db.SaveChanges();
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Không tìm thấy người dùng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}