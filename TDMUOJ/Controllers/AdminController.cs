using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TDMUOJ.Models;

namespace TDMUOJ.Controllers
{
    public class AdminController : Controller
    {
        TDMUOJEntities db = new TDMUOJEntities();
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Overview()
        {
            var overview = new AdminOverview
            {
                accountNumber = db.Accounts.Count(),
                problemNumber = db.Problems.Count(),
                contestNumber = db.Contests.Count(),
                submissionNumber = db.Submissions.Count()
            };
            return PartialView(overview);
        }
        public ActionResult AccountManagement()
        {
            var accounts = db.Accounts.ToList();
            return PartialView(accounts);
        }
        public ActionResult ContestManagement()
        {
            var contestData = db.Contests
                .OrderByDescending(c => c.startTime)
                .Select(c => new
                {
                    id = c.id,
                    title = c.title,
                    startTime = c.startTime,
                    endTime = c.endTime,
                    rules = c.rules
                }).ToList();

            var contests = contestData.Select(c => new Contest
            {
                id = c.id,
                title = c.title,
                startTime = c.startTime,
                endTime = c.endTime,
                rules = c.rules
            }).ToList();
            return PartialView(contests);
        }
        public ActionResult DeleteContest(int id)
        {
            var contest = db.Contests.SingleOrDefault(a => a.id == id);
            db.Contests.Remove(contest);
            var contestProblems = db.ContestProblems.Where(cp => cp.contestId == id).ToList();
            foreach (var contestProblem in contestProblems)
            {
                db.ContestProblems.Remove(contestProblem);
            }
            var comments = db.Comments.Where(c => c.contestId == id).ToList();
            foreach (var comment in comments)
            {
                db.Comments.Remove(comment);
            }
            var contestOrganizers = db.ContestOrganizers.Where(co => co.contestId == id).ToList();
            foreach (var contestOrganizer in contestOrganizers)
            {
                db.ContestOrganizers.Remove(contestOrganizer);
            }
            var contestParticipants = db.ContestParticipants.Where(cp => cp.contestId == id).ToList();
            foreach (var contestParticipant in contestParticipants)
            {
                db.ContestParticipants.Remove(contestParticipant);
            }
            var rankings = db.Rankings.Where(cr => cr.contestId == id).ToList();
            foreach (var ranking in rankings)
            {
                db.Rankings.Remove(ranking);
            }
            var contestRankings = db.ContestRankings.Where(cr => cr.contestId == id).ToList();
            foreach (var contestRanking in contestRankings)
            {
                db.ContestRankings.Remove(contestRanking);
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public JsonResult EditContest(Contest contest)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingContest = db.Contests.Find(contest.id);
                    if (existingContest != null)
                    {
                        existingContest.title = contest.title;
                        existingContest.startTime = contest.startTime;
                        existingContest.endTime = contest.endTime;
                        existingContest.rules = contest.rules;

                        db.SaveChanges();
                        return Json(new { success = true });
                    }
                }
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult AddContest(Contest contest)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Contests.Add(contest);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public ActionResult ProblemManagement()
        {
            var problems = db.Problems.OrderByDescending(c => c.createdAt)
                .Join(db.Accounts, p => p.createdBy, a => a.id, (p, a) => new {p, a})
                .Select(x => new ProblemManagementCustom
                {
                    id = x.p.id,
                    title = x.p.title,
                    description = x.p.description,
                    difficulty = x.p.difficulty,
                    timeLimit = x.p.timeLimit,
                    memoryLimit = x.p.memoryLimit,
                    createdAt = x.p.createdAt,
                    createdBy = x.a.username
                }).ToList();
            return PartialView(problems);
        }
        public ActionResult DeleteProblem(int id)
        {
            var problem = db.Problems.SingleOrDefault(a => a.id == id);
            db.Problems.Remove(problem);
            var problemExamples = db.ProblemExamples.Where(pe => pe.problemId == id).ToList();
            foreach (var problemExample in problemExamples)
            {
                db.ProblemExamples.Remove(problemExample);
            }
            var problemSolved = db.ProblemSolveds.Where(ps => ps.problemId == id).ToList();
            foreach (var problemSolve in problemSolved)
            {
                db.ProblemSolveds.Remove(problemSolve);
            }
            var problemTestcases = db.ProblemTestCases.Where(pt => pt.problemId == id).ToList();
            foreach (var problemTestcase in problemTestcases)
            {
                db.ProblemTestCases.Remove(problemTestcase);
            }
            var submissions = db.Submissions.Where(s => s.problemId == id).ToList();
            foreach (var submission in submissions)
            {
                db.Submissions.Remove(submission);
            }
            var tagProblems = db.TagProblems.Where(t => t.problemId == id).ToList();
            foreach (var tagProblem in tagProblems)
            {
                db.TagProblems.Remove(tagProblem);
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public JsonResult EditProblem(Problem problem)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingProblem = db.Problems.Find(problem.id);
                    if (existingProblem != null)
                    {
                        existingProblem.title = problem.title;
                        existingProblem.description = problem.description;
                        existingProblem.difficulty = problem.difficulty;
                        existingProblem.timeLimit = problem.timeLimit;
                        existingProblem.memoryLimit = problem.memoryLimit;
                        db.SaveChanges();
                        return Json(new { success = true });
                    }
                }
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult AddProblem(Problem problem)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Problems.Add(problem);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public ActionResult SubmissionManagement()
        {
            var submissions = db.Submissions.OrderByDescending(s => s.submittedAt)
                .Join(db.Accounts, submission => submission.userId, account => account.id, (submission, account) => new { submission, account })
                .Join(db.Problems, submission => submission.submission.problemId, problem => problem.id, (submission, problem) => new { submission, problem })
                .Select(x => new SubmissionManagementCustom
                {
                    id = x.submission.submission.id,
                    username = x.submission.account.username,
                    problemTitle = x.problem.title,
                    language = x.submission.submission.language,
                    status = x.submission.submission.result,
                    createdAt = x.submission.submission.submittedAt
                }).ToList();
            return PartialView(submissions);
        }
        public ActionResult TestCaseManagement()
        {
            var problems = db.Problems.Include(p => p.ProblemTestCases).ToList();
            return PartialView(problems);
        }
        [HttpPost]
        public JsonResult AddTestCase(ProblemTestCase testCase)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.ProblemTestCases.Add(testCase);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult AddExample(ProblemExample example)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.ProblemExamples.Add(example);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EditTestCase(ProblemTestCase testCase)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingTestCase = db.ProblemTestCases.Find(testCase.id);
                    if (existingTestCase != null)
                    {
                        existingTestCase.input = testCase.input;
                        existingTestCase.output = testCase.output;
                        db.SaveChanges();
                        return Json(new { success = true });
                    }
                }
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult DeleteTestCase(int id)
        {
            var testCase = db.ProblemTestCases.Find(id);
            if (testCase != null)
            {
                db.ProblemTestCases.Remove(testCase);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        public ActionResult ProblemsForContestManagement()
        {
            var viewModel = new ContestProblemViewModel
            {
                Contests = db.Contests.ToList(),
                AllProblems = db.Problems.ToList()
            };
            return PartialView(viewModel);
        }
        [HttpGet]
        public JsonResult GetContestProblems(int contestId)
        {
            var problems = db.ContestProblems
                .Where(cp => cp.contestId == contestId)
                .Select(cp => new {
                    id = cp.Problem.id,
                    title = cp.Problem.title
                })
                .ToList();
            return Json(problems, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddProblemToContest(int contestId, int problemId)
        {
            try
            {
                var existingContestProblem = db.ContestProblems
                    .FirstOrDefault(cp => cp.contestId == contestId && cp.problemId == problemId);

                if (existingContestProblem != null)
                {
                    return Json(new { success = false, message = "Bài tập đã tồn tại trong kỳ thi này." });
                }

                var contestProblem = new ContestProblem
                {
                    contestId = contestId,
                    problemId = problemId
                };
                db.ContestProblems.Add(contestProblem);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult RemoveProblemFromContest(int contestId, int problemId)
        {
            try
            {
                var contestProblem = db.ContestProblems
                    .FirstOrDefault(cp => cp.contestId == contestId && cp.problemId == problemId);
                if (contestProblem != null)
                {
                    db.ContestProblems.Remove(contestProblem);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Bài tập không tồn tại trong kỳ thi này." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}