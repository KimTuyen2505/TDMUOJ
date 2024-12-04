using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using TDMUOJ.Models;

namespace TDMUOJ.Controllers
{
    public class ProblemsController : Controller
    {
        TDMUOJEntities db = new TDMUOJEntities();
        // GET: Problems
        public ActionResult Index()
        {
            var problems = db.Problems.GroupJoin(
                    db.ProblemSolveds,
                    problem => problem.id,
                    solved => solved.problemId,
                    (problem, solved) => new
                    {
                        id = problem.id,
                        title = problem.title,
                        difficulty = problem.difficulty,
                        numberOfAccepted = solved.Count()
                    }
                 ).ToList().Select(x => new
                 {
                     id = x.id,
                     title = x.title,
                     difficulty = x.difficulty,
                     numberOfAccepted = x.numberOfAccepted
                 });

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
            dynamic dynamicProblems = new ExpandoObject();
            dynamicProblems.topUsers = topUsers;
            dynamicProblems.problems = problems.Select(x =>
            {
                dynamic expando = new ExpandoObject();
                // Copy tất cả thuộc tính từ `x` vào `expando`
                foreach (var property in x.GetType().GetProperties())
                {
                    ((IDictionary<string, object>)expando)[property.Name] = property.GetValue(x);
                }
                return expando;
            }).ToList();
            return View(dynamicProblems);
        }

        [HttpGet]
        public ActionResult DetailProblem(int id)
        {
            var detailProblem = db.Problems
                .Where(problem => problem.id == id)
                .Select(problem => new DetailProblem
                {
                    id = problem.id,
                    title = problem.title,
                    description = problem.description,
                    timeLimit = problem.timeLimit,
                    memoryLimit = problem.memoryLimit,
                    createdBy = db.Accounts
                        .Where(account => account.id == problem.createdBy)
                        .Select(account => account.username)
                        .FirstOrDefault(),
                    examples = db.ProblemExamples
                        .Where(example => example.problemId == problem.id)
                        .Select(example => example).ToList()
                })
                .FirstOrDefault();
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
            dynamic dynamicProblems = new ExpandoObject();
            dynamicProblems.topUsers = topUsers;
            dynamicProblems.detailProblem = detailProblem;
            return View(dynamicProblems);
        }

    }
}