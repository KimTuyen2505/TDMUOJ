using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TDMUOJ.Models;

namespace TDMUOJ.Controllers
{
    public class ProblemsController : Controller
    {
        TDMUOJEntities db = new TDMUOJEntities();
        // GET: Problems
        public ActionResult Index()
        {
            var problems = db.Problems.ToList();
            return View(problems);
        }

        [HttpGet]
        public ActionResult DetailProblem(int id)
        {
            var detailProblem = db.Problems.Join(db.ProblemExamples, problem => problem.id, example => example.problemId, (problem, example) => new { problem, example })
                .Where(x => x.problem.id == id)
                .Select(x => new { id = x.problem.id, title = x.problem.title, description = x.problem.description, timeLimit = x.problem.timeLimit, memoryLimit = x.problem.memoryLimit, createdBy = x.problem.createdBy, input = x.example.input, output = x.example.output })
                .Join(db.Accounts, problem => problem.createdBy, user => user.id, (problem, user) => new {problem, user})
                .Select(x => new { id = x.problem.id, title = x.problem.title, description = x.problem.description, timeLimit = x.problem.timeLimit, memoryLimit = x.problem.memoryLimit, createdBy = x.user.username, input = x.problem.input, output = x.problem.output })
                .SingleOrDefault();
            dynamic model = new ExpandoObject();

            if (detailProblem != null)
            {
                model.id = detailProblem.id;
                model.title = detailProblem.title;
                model.description = detailProblem.description;
                model.timeLimit = detailProblem.timeLimit;
                model.memoryLimit = detailProblem.memoryLimit;
                model.createdBy = detailProblem.createdBy;
                model.input = detailProblem.input;
                model.output = detailProblem.output;
            }
            return View(model);
        }
    }
}