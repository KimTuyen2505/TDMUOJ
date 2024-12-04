using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TDMUOJ.Models;

namespace TDMUOJ.Controllers
{
    public class ExamsController : Controller
    {
        TDMUOJEntities db = new TDMUOJEntities();
        // GET: Exams
        public ActionResult Index()
        {
            ContestStatus contests = new ContestStatus();
            contests.contestsUpComming = db.Contests.Where(x => x.startTime > DateTime.Now).ToList();
            contests.contestsRunning = db.Contests.Where(x => x.startTime <= DateTime.Now && x.endTime >= DateTime.Now).ToList();
            contests.contestsEnded = db.Contests.Where(x => x.endTime < DateTime.Now).ToList();
            return View(contests);
        }
        public ActionResult DetailExam(int id)
        {
            var exam = db.Contests.Where(x => x.id == id).SingleOrDefault();
            var problems = db.Problems.ToList();
            var contestProblems = db.ContestProblems.Where(x => x.contestId == id).ToList();
            var problemSolved = db.ProblemSolveds.ToList();
            var contestDetail = new ContestDetail
            {
                id = exam.id,
                title = exam.title,
                startTime = exam.startTime,
                endTime = exam.endTime,
                rules = exam.rules,
                problems = db.ContestProblems
                            .Where(cp => cp.contestId == id)
                            .Join(db.Problems, cp => cp.problemId, p => p.id, (cp, p) => new { cp, p })
                            .Select(x => new ProblemViewModel
                            {
                                id = x.p.id,
                                title = x.p.title,
                                numberOfAccepted = db.ProblemSolveds.Count(ps => ps.problemId == x.p.id)
                            })
                            .ToList(),
                comments = db.Comments.Where(x => x.contestId == id).OrderByDescending(c => c.createdAt)
                            .Join(db.Accounts, comment => comment.userId, account => account.id, (comment, account) => new { comment, account })
                            .Select(x => new CommentViewModel
                            {
                                id = x.comment.id,
                                content = x.comment.content,
                                createdAt = x.comment.createdAt,
                                userId = x.comment.userId,
                                username = x.account.username
                            }).ToList()
            };

            return View(contestDetail);
        }
        [HttpPost]
        public ActionResult AddComment(int contestId, string content)
        {
            if (Session["User"] != null)
            {
                var userId = (Account)Session["User"];
                var comment = new Comment
                {
                    content = content,
                    createdAt = DateTime.Now,
                    userId = userId.id,
                    contestId = contestId
                };

                db.Comments.Add(comment);
                db.SaveChanges();

                return Json(new { success = true, comment = comment });
            }
            return Json(new { success = false, message = "Bạn cần đăng nhập để bình luận." });
        }
    }
}