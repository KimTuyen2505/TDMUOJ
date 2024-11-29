using System;
using System.Collections.Generic;
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
    }
}