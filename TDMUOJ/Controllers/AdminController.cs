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
                    rules = c.rules,
                    isCalculatedRating = c.isCalculatedRating
                }).ToList();

            var contests = contestData.Select(c => new Contest
            {
                id = c.id,
                title = c.title,
                startTime = c.startTime,
                endTime = c.endTime,
                rules = c.rules,
                isCalculatedRating = c.isCalculatedRating
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
        // Phương thức tính toán rating sau khi kỳ thi kết thúc
        [HttpPost]
        public ActionResult CalculateRatings(int id)
        {
            var contest = db.Contests.Find(id);
            if (contest == null)
            {
                return Json(new { success = false, message = "Không tìm thấy kỳ thi." });
            }

            // Kiểm tra xem kỳ thi đã kết thúc chưa
            if (contest.endTime > DateTime.Now)
            {
                return Json(new { success = false, message = "Kỳ thi chưa kết thúc." });
            }

            // Lấy danh sách người tham gia và sắp xếp theo điểm và penalty
            var participants = db.ContestParticipants
                .Where(cp => cp.contestId == id && cp.isVirtual == false)
                .Join(db.Accounts, cp => cp.memberId, a => a.id, (cp, a) => new { cp, a })
                .OrderByDescending(x => x.cp.score ?? 0)
                .ThenBy(x => x.cp.penalty ?? 0)
                .ToList();

            int totalParticipants = participants.Count;
            if (totalParticipants <= 1)
            {
                return Json(new { success = false, message = "Kỳ thi cần có 2 người tham gia trở lên." });
            }

            // Tính toán rating cho từng người tham gia
            foreach (var participant in participants)
            {
                // Tính thứ hạng (rank) của người tham gia
                int rank = participants.IndexOf(participant) + 1;

                // Tính điểm thực tế dựa trên thứ hạng
                double actualScore = (double)(totalParticipants - rank + 1) / totalParticipants;

                // Tính điểm kỳ vọng dựa trên rating hiện tại
                double expectedScore = 0;
                foreach (var opponent in participants)
                {
                    if (opponent != participant)
                    {
                        double ratingDiff = (opponent.a.rating ?? 500) - (participant.a.rating ?? 500);
                        expectedScore += 1.0 / (1.0 + Math.Pow(10, ratingDiff / 400.0));
                    }
                }
                expectedScore /= (totalParticipants - 1);

                // Tính hệ số K dựa trên rating hiện tại
                int k = 32;
                if ((participant.a.rating ?? 500) >= 2100 && (participant.a.rating ?? 500) < 2400)
                {
                    k = 24;
                }
                else if ((participant.a.rating ?? 500) >= 2400)
                {
                    k = 16;
                }

                // Tính thay đổi rating
                int ratingChange = (int)Math.Round(k * (actualScore - expectedScore));

                // Cập nhật rating của người tham gia
                participant.cp.ratingChange = ratingChange;
                participant.a.rating += ratingChange;

                // Cập nhật maxRating nếu cần
                if ((participant.a.rating ?? 0) > (participant.a.maxRating ?? 0))
                {
                    participant.a.maxRating = participant.a.rating;
                }

                // Lưu thông tin vào bảng Ranking
                var ranking = new Ranking
                {
                    contestId = id,
                    userId = participant.a.id,
                    score = participant.cp.score ?? 0,
                    rank = rank
                };

                db.Rankings.Add(ranking);

                // Lưu thông tin vào bảng ContestRanking
                db.SaveChanges(); // Lưu trước để có id của ranking

                var contestRanking = new ContestRanking
                {
                    contestId = id,
                    rankingId = ranking.id
                };

                db.ContestRankings.Add(contestRanking);
            }

            contest.isCalculatedRating = true;
            db.SaveChanges();

            return Json(new { success = true, message = "Đã tính toán rating thành công." });
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
                    code = x.submission.submission.code,
                    createdAt = x.submission.submission.submittedAt
                }).ToList();
            return PartialView(submissions);
        }
        public ActionResult TestCaseManagement()
        {
            var viewModel = new TestCaseManagementViewModel
            {
                Problems = db.Problems.Include(p => p.ProblemTestCases).ToList(),
                Examples = db.ProblemExamples.ToList()
            };
            return PartialView(viewModel);
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
        public ActionResult DeleteExample(int id)
        {
            var example = db.ProblemExamples.Find(id);
            if (example != null)
            {
                db.ProblemExamples.Remove(example);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        public ActionResult ProblemsForContestManagement()
        {
            var viewModel = new ContestProblemsViewModel
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
                    title = cp.Problem.title,
                    difficulty = cp.Problem.difficulty
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