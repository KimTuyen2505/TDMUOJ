using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TDMUOJ.Models;

namespace TDMUOJ.Controllers
{
    public class SubmissionsController : Controller
    {
        TDMUOJEntities db = new TDMUOJEntities();
        // GET: Submissions
        public ActionResult Index()
        {
            var submissions = db.Submissions
                .Select(submission => new
                {
                    id = submission.id,
                    problemId = submission.problemId,
                    userId = submission.userId,
                    language = submission.language,
                    code = submission.code,
                    maxTime = submission.maxTime,
                    maxMemory = submission.maxMemory,
                    result = submission.result,
                    submittedAt = submission.submittedAt,
                    testCaseTotal = submission.testCaseTotal,
                    testCaseAchieved = submission.testCaseAchieved,
                    problemName = db.Problems.Where(problem => problem.id == submission.problemId).Select(problem => problem.title).FirstOrDefault(),
                    accountName = db.Accounts.Where(account => account.id == submission.userId).Select(account => account.username).FirstOrDefault()
                }).OrderByDescending(submission => submission.submittedAt).ToList();
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
            dynamic dynamicSubmissions = new ExpandoObject();
            dynamicSubmissions.topUsers = topUsers;
            dynamicSubmissions.submissions = submissions.Select(x =>
            {
                dynamic expando = new ExpandoObject();
                // Copy tất cả thuộc tính từ `x` vào `expando`
                foreach (var property in x.GetType().GetProperties())
                {
                    ((IDictionary<string, object>)expando)[property.Name] = property.GetValue(x);
                }
                return expando;
            }).ToList();
            return View(dynamicSubmissions);
        }
    }
}