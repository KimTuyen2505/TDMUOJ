using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TDMUOJ.Models;

namespace TDMUOJ.Controllers
{
    public class MembersController : Controller
    {
        TDMUOJEntities db = new TDMUOJEntities();
        // GET: Members
        public ActionResult Index()
        {
            TopUsers topUsers = new TopUsers
            {
                AccountList = db.Accounts
                    .OrderByDescending(account => account.rating)
                    .Take(5)
                    .Select(account => account).ToList(),
                NewProblemList = db.Problems
                    .OrderByDescending(problem => problem.createdAt)
                    .Take(5)
                    .Select(problem => problem).ToList()
            };
            dynamic dynamicMembers = new ExpandoObject();
            dynamicMembers.topUsers = topUsers;
            var members = db.Accounts.OrderByDescending(x => x.numberOfAccepted).ToList();
            dynamicMembers.members = new List<dynamic>();
            var rank = 0;
            var prev = -1;
            for (var i = 0; i < members.Count; i++)
            {
                dynamic expando = new ExpandoObject();
                // Copy tất cả thuộc tính từ `x` vào `expando`
                foreach (var property in members[i].GetType().GetProperties())
                {
                    ((IDictionary<string, object>)expando)[property.Name] = property.GetValue(members[i]);
                }
                if (members[i].numberOfAccepted != prev)
                {
                    prev = (int)members[i].numberOfAccepted;
                    rank = i + 1;
                }
                expando.rank = rank;
                dynamicMembers.members.Add(expando);
            }
            return View(dynamicMembers);
        }
    }
}