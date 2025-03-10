using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
            contests.contestsEnded = db.Contests.Where(x => x.endTime < DateTime.Now)
                .OrderByDescending(x => x.endTime)
                .Select(x => new ContestEndedCustom
                {
                    contest = x,
                    participantedCount = db.ContestParticipants.Count(cp => cp.contestId == x.id)
                })
                .ToList();

            return View(contests);
        }

        public ActionResult DetailExam(int id)
        {
            var exam = db.Contests.Where(x => x.id == id).SingleOrDefault();
            if (exam == null)
            {
                return HttpNotFound();
            }

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

            // Kiểm tra xem người dùng đã tham gia kỳ thi chưa
            if (Session["User"] != null)
            {
                var user = (Account)Session["User"];
                var participant = db.ContestParticipants
                    .FirstOrDefault(cp => cp.contestId == id && cp.memberId == user.id && cp.isVirtual == false);

                contestDetail.IsParticipant = participant != null;

            }
            else
            {
                contestDetail.IsParticipant = false;
                contestDetail.IsVirtualParticipant = false;
            }

            return View(contestDetail);
        }

        [HttpPost]
        public ActionResult AddComment(int contestId, string content)
        {
            if (Session["User"] != null)
            {
                var user = (Account)Session["User"];
                var comment = new Comment
                {
                    content = content,
                    createdAt = DateTime.Now,
                    userId = user.id,
                    contestId = contestId
                };

                db.Comments.Add(comment);
                db.SaveChanges();

                return Json(new { success = true, comment = comment });
            }
            return Json(new { success = false, message = "Bạn cần đăng nhập để bình luận." });
        }

        [HttpPost]
        public ActionResult JoinContest(int id)
        {
            var contest = db.Contests.Find(id);
            if (contest == null)
            {
                return Json(new { success = false, message = "Không tìm thấy kỳ thi." });
            }

            var user = (Account)Session["User"];

            // Kiểm tra xem người dùng đã tham gia kỳ thi chưa
            var existingParticipant = db.ContestParticipants
                .FirstOrDefault(cp => cp.contestId == id && cp.memberId == user.id && cp.isVirtual == false);

            if (existingParticipant != null)
            {
                return Json(new { success = false, message = "Bạn đã tham gia kỳ thi này rồi." });
            }

            // Kiểm tra thời gian kỳ thi
            if (contest.startTime > DateTime.Now)
            {
                return Json(new { success = false, message = "Kỳ thi chưa bắt đầu." });
            }

            if (contest.endTime < DateTime.Now)
            {
                return Json(new { success = false, message = "Kỳ thi đã kết thúc." });
            }

            // Thêm người dùng vào danh sách tham gia
            var participant = new ContestParticipant
            {
                contestId = id,
                memberId = user.id,
                score = 0,
                penalty = 0,
                currentRating = user.rating,
                ratingChange = 0,
                isVirtual = false
            };

            db.ContestParticipants.Add(participant);
            db.SaveChanges();

            return Json(new { success = true, message = "Tham gia kỳ thi thành công." });
        }

        public ActionResult ContestProblems(int id)
        {
            var contest = db.Contests.Find(id);
            if (contest == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra xem người dùng đã tham gia kỳ thi chưa
            bool isParticipant = false;
            bool isVirtualParticipant = false;

            if (Session["User"] != null)
            {
                var user = (Account)Session["User"];
                var participant = db.ContestParticipants
                    .FirstOrDefault(cp => cp.contestId == id && cp.memberId == user.id && cp.isVirtual == false);

                isParticipant = participant != null;

            }

            // Kiểm tra điều kiện để xem danh sách bài tập
            bool canViewProblems = false;

            // Nếu kỳ thi đang diễn ra và người dùng đã tham gia
            if (contest.startTime <= DateTime.Now && contest.endTime >= DateTime.Now && isParticipant)
            {
                canViewProblems = true;
            }

            // Nếu kỳ thi đã kết thúc
            if (contest.endTime < DateTime.Now)
            {
                canViewProblems = true;
            }

            if (!canViewProblems)
            {
                return RedirectToAction("DetailExam", new { id = id });
            }

            var problems = db.ContestProblems
                .Where(cp => cp.contestId == id)
                .Join(db.Problems, cp => cp.problemId, p => p.id, (cp, p) => new { cp, p })
                .Select((x, index) => new ProblemViewModel
                {
                    id = x.p.id,
                    title = x.p.title,
                    difficulty = x.p.difficulty,
                    timeLimit = x.p.timeLimit,
                    memoryLimit = x.p.memoryLimit,
                    numberOfAccepted = db.ProblemSolveds.Count(ps => ps.problemId == x.p.id),
                    problemIndex = ((char)('A' + index)).ToString() // Đánh chỉ mục A, B, C, ... cho các bài tập
                })
                .ToList();

            var model = new ContestProblemsViewModel
            {
                ContestId = id,
                ContestTitle = contest.title,
                StartTime = contest.startTime,
                EndTime = contest.endTime,
                Problems = problems,
                IsVirtualParticipant = isVirtualParticipant
            };

            return View(model);
        }

        public ActionResult ContestProblemDetail(int contestId, int problemId)
        {
            var contest = db.Contests.Find(contestId);
            var problem = db.Problems.Find(problemId);

            if (contest == null || problem == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra xem bài tập có thuộc kỳ thi không
            var contestProblem = db.ContestProblems
                .FirstOrDefault(cp => cp.contestId == contestId && cp.problemId == problemId);

            if (contestProblem == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra xem người dùng đã tham gia kỳ thi chưa
            bool isParticipant = false;
            bool isVirtualParticipant = false;

            if (Session["User"] != null)
            {
                var user = (Account)Session["User"];
                var participant = db.ContestParticipants
                    .FirstOrDefault(cp => cp.contestId == contestId && cp.memberId == user.id && cp.isVirtual == false);

                isParticipant = participant != null;
            }

            // Kiểm tra điều kiện để xem chi tiết bài tập
            bool canViewProblem = false;

            // Nếu kỳ thi đang diễn ra và người dùng đã tham gia
            if (contest.startTime <= DateTime.Now && contest.endTime >= DateTime.Now && isParticipant)
            {
                canViewProblem = true;
            }

            // Nếu kỳ thi đã kết thúc
            if (contest.endTime < DateTime.Now)
            {
                canViewProblem = true;
            }

            if (!canViewProblem)
            {
                return RedirectToAction("DetailExam", new { id = contestId });
            }

            // Lấy chỉ mục của bài tập trong kỳ thi (A, B, C, ...)
            var problemIndex = db.ContestProblems
                .Where(cp => cp.contestId == contestId)
                .OrderBy(cp => cp.id)
                .Select(cp => cp.problemId)
                .ToList()
                .IndexOf(problemId);

            var problemChar = (char)('A' + problemIndex);

            // Lấy các ví dụ của bài tập
            var examples = db.ProblemExamples
                .Where(pe => pe.problemId == problemId)
                .Select(pe => new ProblemExampleViewModel
                {
                    Input = pe.input,
                    Output = pe.output
                })
                .ToList();

            var currentUser = (Account)Session["User"];
            var currentUserId = currentUser.id;
            var currentUsername = currentUser.username;
            
            var model = new ContestProblemDetailViewModel
            {
                Contest = contest,
                ContestId = contestId,
                ContestTitle = contest.title,
                ProblemId = problemId,
                ProblemTitle = problem.title,
                ProblemDescription = problem.description,
                TimeLimit = problem.timeLimit,
                MemoryLimit = problem.memoryLimit,
                ProblemIndex = problemChar.ToString(),
                Examples = examples,
                IsVirtualParticipant = isVirtualParticipant,
                IsAccepted = db.ProblemSolveds.Any(ps => ps.problemId == problemId && ps.userId == currentUserId),
                Submissions = db.Submissions
                    .Where(s => s.contestId == contestId && s.problemId == problemId && s.userId == currentUserId && s.isVirtual == false)
                    .OrderByDescending(s => s.submittedAt)
                    .Select(x => new SubmissionViewModel
                    {
                        Id = x.id,
                        ProblemId = x.problemId,
                        ProblemName = problem.title,
                        UserId = x.userId,
                        Username = currentUsername,
                        ContestId = x.contestId,
                        Language = x.language,
                        MaxTime = x.maxTime,
                        MaxMemory = x.maxMemory,
                        Result = x.result,
                        SubmittedAt = x.submittedAt,
                        TestCaseTotal = x.testCaseTotal,
                        TestCaseAchieved = x.testCaseAchieved,
                        IsVirtual = x.isVirtual
                    })
                    .ToList()
            };

            return View(model);
        }

        public async Task<ActionResult> Compile(string code, string language, int problemId)
        {
            string Judge0Host = "https://judge0-ce.p.rapidapi.com";
            string Judge0ApiKey = "9d6278f51amsh90e372ae79b0394p15510djsn12e35de53995";

            var problem = db.Problems.SingleOrDefault(x => x.id == problemId);
            var testCases = db.ProblemTestCases.Where(x => x.problemId == problemId).ToList();
            int testCaseAchieved = 0;
            int testCaseTotal = testCases.Count;
            double maxTime = 0;
            double maxMemory = 0;
            string result = "";

            if (string.IsNullOrEmpty(code) || testCases.Count == 0)
            {
                return Json(new { success = false, message = "Không thể chấm bài." });
            }
            if (Session["User"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            List<TestCaseSubmit> testCaseSubmissions = new List<TestCaseSubmit>();
            foreach (var testCase in testCases)
            {
                testCaseSubmissions.Add(new TestCaseSubmit
                {
                    language_id = int.Parse(language),
                    source_code = code,
                    stdin = testCase.input,
                    expected_output = testCase.output,
                    cpu_time_limit = problem.timeLimit,
                    memory_limit = problem.memoryLimit * 1024
                });
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-rapidapi-host", "judge0-ce.p.rapidapi.com");
                client.DefaultRequestHeaders.Add("x-rapidapi-key", Judge0ApiKey);
                client.DefaultRequestHeaders.Add("accept", "application/json");

                var response = await client.PostAsync($"{Judge0Host}/submissions/batch",
                    new StringContent(JsonConvert.SerializeObject(new { submissions = testCaseSubmissions }), Encoding.UTF8, "application/json"));

                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                string tokens = "";
                foreach (var token in jsonResponse)
                {
                    tokens += token.token + ",";
                }
                tokens = tokens.Remove(tokens.Length - 1);

                if (string.IsNullOrEmpty(tokens))
                {
                    return Json(new { success = false, message = "Không thể chấm bài." });
                }
                List<dynamic> solutions = new List<dynamic>();

                do
                {
                    var resultResponse = await client.GetAsync($"{Judge0Host}/submissions/batch?tokens={tokens}&base64_encoded=true&fields=*");
                    resultResponse.EnsureSuccessStatusCode();
                    if (!resultResponse.IsSuccessStatusCode) break;

                    var resultContent = await resultResponse.Content.ReadAsStringAsync();
                    var resultData = JsonConvert.DeserializeObject<dynamic>(resultContent);
                    solutions.Clear();
                    foreach (var item in resultData.submissions)
                    {
                        solutions.Add(item.ToObject<object>());
                    }
                } while (solutions.Any(s => s.stdout == null &&
                       s.time == null &&
                       s.memory == null &&
                       s.stderr == null &&
                       s.compile_output == null));

                string message = "";
                foreach (var solution in solutions)
                {
                    if (solution.status.description == "Accepted")
                    {
                        testCaseAchieved++;
                    }
                    else if (message == "")
                    {
                        message = solution.status.description;
                    }
                    double timeSolution, memorySolution;
                    try
                    {
                        timeSolution = double.Parse(solution.time.ToString());
                        memorySolution = double.Parse(solution.memory.ToString());
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    maxTime = Math.Max(maxTime, timeSolution);
                    maxMemory = Math.Max(maxMemory, memorySolution);
                }
                if (testCaseAchieved == testCaseTotal)
                {
                    result = "Accepted";
                }
                else
                {
                    result = message;
                }
                if (result == "Accepted") result = "AC";
                else if (result.Contains("Wrong")) result = "WA";
                else if (result.Contains("Time")) result = "TLE";
                else if (result.Contains("Memory")) result = "MLE";
                else if (result.Contains("Runtime")) result = "RTE";
                else if (result.Contains("Compilation")) result = "CE";
                else result = "IE";

                string lang = "";
                if (language == "50") lang = "C";
                else if (language == "54") lang = "C++";
                else if (language == "51") lang = "C#";
                else if (language == "71") lang = "Python";
                else if (language == "62") lang = "Java";
                else if (language == "63") lang = "JavaScript";

                return Json(new CompileResultModel {
                    result = result,
                    testCaseAchieved = testCaseAchieved,
                    maxTime = maxTime,
                    maxMemory = maxMemory
                });
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> SubmitSolution(int contestId, int problemId, string code, string language)
        {
            string lang = "";
            if (language == "C") lang = "50";
            else if (language == "C++") lang = "54";
            else if (language == "C#") lang = "51";
            else if (language == "Python") lang = "71";
            else if (language == "Java") lang = "62";
            else if (language == "JavaScript") lang = "63";

            var contest = db.Contests.Find(contestId);
            var problem = db.Problems.Find(problemId);

            if (contest == null || problem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy kỳ thi hoặc bài tập." });
            }

            var user = (Account)Session["User"];

            // Kiểm tra xem người dùng đã tham gia kỳ thi chưa
            var participant = db.ContestParticipants
                .FirstOrDefault(cp => cp.contestId == contestId && cp.memberId == user.id);

            if (participant == null)
            {
                return Json(new { success = false, message = "Bạn chưa tham gia kỳ thi này." });
            }

            // Kiểm tra thời gian nộp bài
            bool isVirtual = participant.isVirtual;
            bool canSubmit = false;

            if (!isVirtual)
            {
                // Nếu là tham gia thực, kiểm tra thời gian kỳ thi
                canSubmit = contest.startTime <= DateTime.Now && contest.endTime >= DateTime.Now;
            }

            if (!canSubmit)
            {
                return Json(new { success = false, message = "Không thể nộp bài ngoài thời gian kỳ thi." });
            }

            // Tạo bài nộp mới
            var submission = new Submission
            {
                problemId = problemId,
                userId = user.id,
                contestId = contestId,
                code = code,
                language = language,
                result = "PENDING", // Kết quả ban đầu là đang chờ
                submittedAt = DateTime.Now,
                testCaseTotal = db.ProblemTestCases.Count(ptc => ptc.problemId == problemId),
                testCaseAchieved = 0,
                isVirtual = isVirtual
            };

            db.Submissions.Add(submission);
            db.SaveChanges();

            try
            {
                var compiled = await Compile(code, lang, problemId);
                var jsonResult = compiled as JsonResult;
                var data = jsonResult.Data as CompileResultModel;

                submission.result = data.result;
                submission.testCaseAchieved = data.testCaseAchieved;
                submission.maxTime = data.maxTime;
                submission.maxMemory = data.maxMemory;

                db.SaveChanges();

                // Cập nhật điểm và penalty cho người tham gia
                if (data.result == "AC" && !isVirtual)
                {
                    // Kiểm tra xem đây có phải là lần đầu tiên giải được bài này trong kỳ thi không
                    var previousAccepted = db.Submissions
                        .Any(s => s.problemId == problemId && s.userId == user.id &&
                              s.contestId == contestId && s.result == "AC" &&
                              s.id < submission.id);

                    if (!previousAccepted)
                    {
                        // Tính điểm và penalty
                        participant.score += 1;

                        // Tính penalty dựa trên thời gian nộp bài và số lần nộp sai
                        TimeSpan timeSinceStart = submission.submittedAt.Value - contest.startTime;
                        int minutesSinceStart = (int)timeSinceStart.TotalMinutes;

                        participant.penalty += minutesSinceStart;
                        
                        db.SaveChanges();

                        // Cập nhật bảng ProblemSolved nếu chưa có
                        var problemSolved = db.ProblemSolveds
                            .FirstOrDefault(ps => ps.problemId == problemId && ps.userId == user.id);

                        if (problemSolved == null)
                        {
                            db.ProblemSolveds.Add(new ProblemSolved
                            {
                                problemId = problemId,
                                userId = user.id
                            });

                            // Cập nhật số bài đã giải của người dùng
                            var currentUser = db.Accounts.FirstOrDefault(a => a.id == user.id);
                            currentUser.numberOfAccepted += 1;

                            db.SaveChanges();
                        }
                    }
                }
                else if (data.result != "AC" && !isVirtual)
                {
                    var previousAccepted = db.Submissions
                        .Any(s => s.problemId == problemId && s.userId == user.id &&
                              s.contestId == contestId && s.result == "AC" &&
                              s.id < submission.id);
                    if (!previousAccepted)
                    {
                        // Cập nhật penalty cho người tham gia
                        // 20 phút penalty cho mỗi lần nộp sai và số phút từ lúc bắt đầu kỳ thi
                        TimeSpan timeSinceStart = submission.submittedAt.Value - contest.startTime;
                        int minutesSinceStart = (int)timeSinceStart.TotalMinutes;
                        int wrongSubmissions = db.Submissions
                            .Count(s => s.problemId == problemId && s.userId == user.id &&
                                   s.contestId == contestId && s.result != "AC" &&
                                   s.id < submission.id);
                        participant.penalty += minutesSinceStart + 20;
                        db.SaveChanges();
                    }
                }

                return Json(new { success = true, message = "Bài nộp đã được chấm thành công." });
            } catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Thêm action method cho ContestLeaderboard
        public ActionResult ContestLeaderboard(int id)
        {
            var contest = db.Contests.Find(id);
            if (contest == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách bài tập trong kỳ thi
            var problems = db.ContestProblems
                .Where(cp => cp.contestId == id)
                .Join(db.Problems, cp => cp.problemId, p => p.id, (cp, p) => new { cp, p })
                .AsEnumerable()
                .Select((x, index) => new ProblemViewModel
                {
                    id = x.p.id,
                    title = x.p.title,
                    problemIndex = ((char)('A' + index)).ToString() // Đánh chỉ mục A, B, C, ... cho các bài tập
                })
                .ToList();

            List<List<int> > userProblemSolveds = new List<List<int> >();
            for (var i = 0; i < 1000; i++)
            {
                userProblemSolveds.Add(new List<int>());
            }

            var listParticipants = db.ContestParticipants
                .Where(cp => cp.contestId == id && cp.isVirtual == false)
                .Join(db.Accounts, cp => cp.memberId, a => a.id, (cp, a) => new { cp, a })
                .Select(x => x.a.id)
                .ToList();
            List<ProblemSolved> problemSolveds = db.ProblemSolveds.ToList();
            for (var i = 0; i < listParticipants.Count(); i++)
            {
                List<int> lst = new List<int>();
                for (var j = 0; j < problemSolveds.Count(); j++)
                {
                    for (var k = 0; k < problems.Count(); k++)
                    {
                        if (problemSolveds[j].problemId == problems[k].id && problemSolveds[j].userId == listParticipants[i])
                        {
                            lst.Add(problems[k].id);
                        }
                    }
                }
                userProblemSolveds[listParticipants[i]] = lst;
            }

            // Lấy danh sách người tham gia và sắp xếp theo điểm và penalty
            var sortedList = db.ContestParticipants
                    .Where(cp => cp.contestId == id && cp.isVirtual == false)
                    .Join(db.Accounts, cp => cp.memberId, a => a.id, (cp, a) => new { cp, a })
                    .OrderByDescending(x => x.cp.score ?? 0)
                    .ThenBy(x => x.cp.penalty ?? 0)
                    .AsEnumerable()
                    .ToList();

                int rank = 0;
                int prevScore = -1;
                int prevPenalty = -1;
                var participants = sortedList.Select(x => {
                    int currentScore = x.cp.score ?? 0;
                    int currentPenalty = x.cp.penalty ?? 0;
                    // Nếu điểm hoặc penalty thay đổi thì tăng rank
                    if (currentScore != prevScore || currentPenalty != prevPenalty)
                    {
                        rank++;
                        prevScore = currentScore;
                        prevPenalty = currentPenalty;
                    }
                    return new ContestParticipantViewModel
                    {
                        Rank = rank,
                        UserId = x.a.id,
                        Username = x.a.username,
                        Score = currentScore,
                        Penalty = currentPenalty,
                        CurrentRating = x.cp.currentRating ?? 0,
                        RatingChange = x.cp.ratingChange ?? 0,
                        SolvedProblems = userProblemSolveds[x.a.id]
                    };
                }).ToList();

            // Lấy thông tin bài nộp của từng người tham gia cho từng bài tập
            var submissions = db.Submissions
                .Where(s => s.contestId == id && s.isVirtual == false)
                .ToList();

            var model = new ContestLeaderboardViewModel
            {
                Contest = contest,
                Participants = participants,
                Problems = problems,
                Submissions = submissions
            };

            return View(model);
        }

        public ActionResult ContestSubmissions(int id)
        {
            var contest = db.Contests.Find(id);
            if (contest == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách bài nộp trong kỳ thi
            var submissions = db.Submissions
                .Where(s => s.contestId == id && s.isVirtual == false)
                .OrderByDescending(s => s.submittedAt)
                .Join(db.Problems, s => s.problemId, p => p.id, (s, p) => new { s, p })
                .Join(db.Accounts, sp => sp.s.userId, a => a.id, (sp, a) => new { sp, a })
                .Select(x => new SubmissionViewModel
                {
                    Id = x.sp.s.id,
                    ProblemId = x.sp.s.problemId,
                    ProblemName = x.sp.p.title,
                    UserId = x.sp.s.userId,
                    Username = x.a.username,
                    ContestId = x.sp.s.contestId,
                    Language = x.sp.s.language,
                    MaxTime = x.sp.s.maxTime,
                    MaxMemory = x.sp.s.maxMemory,
                    Result = x.sp.s.result,
                    SubmittedAt = x.sp.s.submittedAt,
                    TestCaseTotal = x.sp.s.testCaseTotal,
                    TestCaseAchieved = x.sp.s.testCaseAchieved,
                    IsVirtual = x.sp.s.isVirtual
                })
                .ToList();

            // Lấy danh sách bài tập trong kỳ thi
            var problems = db.ContestProblems
                .Where(cp => cp.contestId == id)
                .Join(db.Problems, cp => cp.problemId, p => p.id, (cp, p) => new { cp, p })
                .AsEnumerable()
                .Select((x, index) => new ProblemViewModel
                {
                    id = x.p.id,
                    title = x.p.title,
                    problemIndex = ((char)('A' + index)).ToString() // Đánh chỉ mục A, B, C, ... cho các bài tập
                })
                .ToList();

            var model = new ContestSubmissionsViewModel
            {
                Contest = contest,
                ContestId = id,
                ContestTitle = contest.title,
                StartTime = contest.startTime,
                EndTime = contest.endTime,
                Submissions = submissions,
                Problems = problems
            };

            return View(model);
        }
    }
}