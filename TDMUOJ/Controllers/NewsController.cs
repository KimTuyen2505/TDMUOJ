using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TDMUOJ.Models;

namespace TDMUOJ.Controllers
{
    public class NewsController : Controller
    {
        TDMUOJEntities db = new TDMUOJEntities();
        public class NewsCustom
        {
            public int id { get; set; }
            public string title { get; set; }
            public string content { get; set; }
            public string author { get; set; }
            public string background { get; set; }
            public Nullable<System.DateTime> createdAt { get; set; }
        }
        // GET: News
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
            dynamic dynamicNews = new ExpandoObject();
            dynamicNews.topUsers = topUsers;
            dynamicNews.news = db.News
                    .Select(news => new NewsCustom
                    {
                        id = news.id,
                        title = news.title,
                        content = news.content,
                        author = db.Accounts.Where(a => a.id == news.authorId).Select(a => a.username).FirstOrDefault(),
                        background = news.background,
                        createdAt = news.createdAt
                    })
                    .OrderByDescending(news => news.createdAt)
                    .ToList();

            return View(dynamicNews);
        }
    }
}