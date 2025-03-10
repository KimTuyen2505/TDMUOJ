using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TDMUOJ.Models;

namespace TDMUOJ.Controllers
{
    public class SubmitController : Controller
    {
        TDMUOJEntities db = new TDMUOJEntities();

        private const string Judge0Host = "https://judge0-ce.p.rapidapi.com";
        private const string Judge0ApiKey = "9d6278f51amsh90e372ae79b0394p15510djsn12e35de53995";
        // GET: Submit
        public ActionResult Index(int id)
        {
            Problem problem = db.Problems.SingleOrDefault(x => x.id == id);
            return View(problem);
        }
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Index(FormCollection f)
        {
            string code = f["code"];
            string language = f["language"];
            int problemId = int.Parse(f["problemId"]);
            var problem = db.Problems.SingleOrDefault(x => x.id == problemId);
            var testCases = db.ProblemTestCases.Where(x => x.problemId == problemId).ToList();
            int testCaseAchieved = 0;
            int testCaseTotal = testCases.Count;
            double maxTime = 0;
            double maxMemory = 0;
            string result = "";

            if (string.IsNullOrEmpty(code) || testCases.Count == 0)
            {
                return View(problem);
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
                    return View(problem);
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
                    } else if (message == "")
                    {
                        message = solution.status.description;
                    }
                    double timeSolution, memorySolution;
                    try
                    {
                        timeSolution = double.Parse(solution.time.ToString());
                        memorySolution = double.Parse(solution.memory.ToString());
                    } catch (Exception ex)
                    {
                        continue;
                    }
                    maxTime = Math.Max(maxTime, timeSolution);
                    maxMemory = Math.Max(maxMemory, memorySolution);
                }
                if (testCaseAchieved == testCaseTotal)
                {
                    result = "Accepted";
                } else
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

                Submission submission = new Submission();
                submission.problemId = problemId;
                submission.userId = (Session["User"] as Account).id;
                submission.code = code;
                submission.language = lang;
                submission.maxTime = maxTime;
                submission.maxMemory = maxMemory;
                submission.result = result;
                submission.testCaseTotal = testCaseTotal;
                submission.testCaseAchieved = testCaseAchieved;
                submission.submittedAt = DateTime.Now;

                db.Submissions.Add(submission);

                if (submission.result == "AC")
                {
                    var checkExist = db.ProblemSolveds.FirstOrDefault(ps => ps.problemId == problemId && ps.userId == submission.userId);
                    var account = db.Accounts.FirstOrDefault(a => a.id == submission.userId);
                    if (checkExist == null)
                    {
                        ProblemSolved ps = new ProblemSolved();
                        ps.problemId = problemId;
                        ps.userId = (Session["User"] as Account).id;

                        account.numberOfAccepted = account.numberOfAccepted + 1;

                        db.ProblemSolveds.Add(ps);
                    }
                }
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Submissions");
        }
    }
}