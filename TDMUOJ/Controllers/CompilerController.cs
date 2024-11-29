using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace TDMUOJ.Controllers
{
    public class CompilerController : Controller
    {
        private const string Judge0Host = "https://judge0-ce.p.rapidapi.com";
        private const string Judge0ApiKey = "9d6278f51amsh90e372ae79b0394p15510djsn12e35de53995";
        // GET: Compiler
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Index(FormCollection f)
        {
            string code = f["code"];
            string input = f["input"];
            string output = f["output"];
            string language = f["language"];
            ViewBag.SelectedLang = language;
            if (string.IsNullOrEmpty(code))
            {
                ViewBag.Code = code;
                ViewBag.Input = input;
                ViewBag.Output = output;
                return View();
            }
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-rapidapi-host", "judge0-ce.p.rapidapi.com");
                client.DefaultRequestHeaders.Add("x-rapidapi-key", Judge0ApiKey);
                client.DefaultRequestHeaders.Add("accept", "application/json");

                var requestBody = new
                {
                    source_code = code,
                    stdin = input,
                    language_id = int.Parse(language),
                    cpu_time_limit = 1,
                    memory_limit = 262144
                };

                var response = await client.PostAsync($"{Judge0Host}/submissions",
                    new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Output = "Lỗi biên dịch";
                    ViewBag.Code = code;
                    ViewBag.Input = input;
                    return View();
                }

                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                string token = jsonResponse?.token;

                if (string.IsNullOrEmpty(token))
                {
                    ViewBag.Output = "Lỗi máy chủ";
                    ViewBag.Code = code;
                    ViewBag.Input = input;
                    return View();
                }

                var getSolutionUrl = $"{Judge0Host}/submissions/{token}?base64_encoded=true";
                dynamic jsonGetSolution = new
                {
                    stdout = (string)null,
                    time = (string)null,
                    memory = (string)null,
                    status = new { description = "Queue" },
                    stderr = (string)null,
                    compile_output = (string)null
                };

                while (jsonGetSolution.stdout == null &&
                       jsonGetSolution.time == null &&
                       jsonGetSolution.memory == null &&
                       jsonGetSolution.stderr == null &&
                       jsonGetSolution.compile_output == null)
                {
                    var getSolutionResponse = await client.GetAsync(getSolutionUrl);
                    jsonGetSolution = JsonConvert.DeserializeObject<dynamic>(await getSolutionResponse.Content.ReadAsStringAsync());
                }

                if (jsonGetSolution.status.description.ToString() != "Accepted")
                {
                    ViewBag.Output = jsonGetSolution.status.description.ToString();
                }
                else if (jsonGetSolution.stdout != null)
                {
                    string decodedOutput = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(jsonGetSolution.stdout.ToString()));
                    ViewBag.Output = $"Kết quả:\n\n{decodedOutput}\n\nThời gian chạy: {jsonGetSolution.time} giây\nBộ nhớ: {jsonGetSolution.memory} kilobytes";
                }
                else if (jsonGetSolution.stderr != null)
                {
                    string error = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(jsonGetSolution.stderr.ToString()));
                    ViewBag.Output = $"Lỗi: {error}";
                }
                else if (!jsonGetSolution.stdout.HasValues)
                {
                    ViewBag.Output = $"Kết quả:\n\n{""}\n\nThời gian chạy: {jsonGetSolution.time} giây\nBộ nhớ: {jsonGetSolution.memory} kilobytes";
                }
                else
                {
                    string compilationError = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(jsonGetSolution.compile_output.ToString()));
                    ViewBag.Output = $"Lỗi biên dịch: {compilationError}";
                }
            }

            ViewBag.Code = code;
            ViewBag.Input = input;
            return View();
        }
    }
}