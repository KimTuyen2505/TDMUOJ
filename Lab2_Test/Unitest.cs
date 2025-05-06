using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;

namespace Lab2_Test
{
    [TestFixture]
    public class Unitest
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private string baseURL = "http://tdmuoj.somee.com";

        [SetUp]
        public void SetUp()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
            driver.Dispose();
        }

        [Test]
        public void TC_REG_01_Register_Successful()
        {
            driver.Navigate().GoToUrl(baseURL + "/Register");

            driver.FindElement(By.Name("fullName")).SendKeys("Trần Thị Quỳnh Nga");
            driver.FindElement(By.Name("username")).SendKeys("tranthiquynhnga");
            driver.FindElement(By.Name("email")).SendKeys("tranthiquynhnga@gmail.com");
            driver.FindElement(By.Name("password")).SendKeys("Quynhnga@123");
            driver.FindElement(By.Name("confirmPassword")).SendKeys("Quynhnga@123");

            driver.FindElement(By.ClassName("register")).Click();

            wait.Until(d => d.Url.Equals(baseURL + "/Login", StringComparison.OrdinalIgnoreCase));

            Assert.That(driver.Url, Is.EqualTo(baseURL + "/Login"),
                "After successful registration, user should be redirected to the Login page.");
        }

        [Test]
        public void TC_AE_01_AddExercise_AllValidInputs()
        {
            driver.Navigate().GoToUrl(baseURL + "/Login");

            driver.FindElement(By.Name("username")).SendKeys("tuyencute03");
            driver.FindElement(By.Name("password")).SendKeys("123456");
            driver.FindElement(By.XPath("/html/body/section/div[4]/div[6]/div/form/div[3]/input")).Click();

            wait.Until(d => d.Url.Equals(baseURL + "/", StringComparison.OrdinalIgnoreCase));

            driver.Navigate().GoToUrl(baseURL + "/Admin");

            driver.FindElement(By.XPath("/html/body/div[1]/ul/li[4]/a")).Click();

            driver.FindElement(By.Id("toggleNewProblemForm")).Click();

            driver.FindElement(By.XPath("/html/body/div[2]/div[5]/div[2]/form/div[1]/div[1]/input"))
                  .SendKeys("Sorting Problem");

            var difficultySelect = new SelectElement(driver.FindElement(
                By.XPath("/html/body/div[2]/div[5]/div[2]/form/div[1]/div[2]/select")));
            difficultySelect.SelectByText("Trung bình");

            var timeInput = driver.FindElement(By.XPath(
                "/html/body/div[2]/div[5]/div[2]/form/div[1]/div[3]/input"));
            timeInput.Clear();
            timeInput.SendKeys("1");

            var memInput = driver.FindElement(By.XPath(
                "/html/body/div[2]/div[5]/div[2]/form/div[1]/div[4]/input"));
            memInput.Clear();
            memInput.SendKeys("256");

            driver.FindElement(By.XPath("/html/body/div[2]/div[5]/div[2]/form/div[2]/textarea"))
                  .SendKeys("Sort an array in ascending order.");

            driver.FindElement(By.XPath("/html/body/div[2]/div[5]/div[2]/form/div[3]/button"))
                  .Click();

            System.Threading.Thread.Sleep(3000);

            var dashboardLink = driver.FindElement(By.XPath("/html/body/div[1]/ul/li[1]/a"));
            Assert.IsTrue(dashboardLink.GetAttribute("class").Contains("active"),
                "Expected the dashboard tab to become active after saving a new problem.");
        }

        [Test]
        public void TC_AE_02_AddExercise_InvalidMemoryLimit()
        {
            driver.Navigate().GoToUrl(baseURL + "/Login");

            driver.FindElement(By.Name("username")).SendKeys("tuyencute03");
            driver.FindElement(By.Name("password")).SendKeys("123456");
            driver.FindElement(By.XPath("/html/body/section/div[4]/div[6]/div/form/div[3]/input")).Click();

            wait.Until(d => d.Url.Equals(baseURL + "/", StringComparison.OrdinalIgnoreCase));

            driver.Navigate().GoToUrl(baseURL + "/Admin");

            driver.FindElement(By.XPath("/html/body/div[1]/ul/li[4]/a")).Click();

            driver.FindElement(By.Id("toggleNewProblemForm")).Click();

            driver.FindElement(By.XPath("/html/body/div[2]/div[5]/div[2]/form/div[1]/div[1]/input"))
                  .SendKeys("Graph Task");

            new SelectElement(driver.FindElement(
                By.XPath("/html/body/div[2]/div[5]/div[2]/form/div[1]/div[2]/select")))
                .SelectByText("Khó");

            var timeInput = driver.FindElement(By.XPath(
                "/html/body/div[2]/div[5]/div[2]/form/div[1]/div[3]/input"));
            timeInput.Clear();
            timeInput.SendKeys("1");

            var memInput = driver.FindElement(By.XPath(
                "/html/body/div[2]/div[5]/div[2]/form/div[1]/div[4]/input"));
            memInput.Clear();
            memInput.SendKeys("-128");

            driver.FindElement(By.XPath("/html/body/div[2]/div[5]/div[2]/form/div[2]/textarea"))
                  .SendKeys("Find shortest path.");

            driver.FindElement(By.XPath("/html/body/div[2]/div[5]/div[2]/form/div[3]/button"))
                  .Click();

            System.Threading.Thread.Sleep(3000);

            var dashboardLink = driver.FindElement(By.XPath("/html/body/div[1]/ul/li[1]/a"));

            Assert.IsFalse(
                dashboardLink.GetAttribute("class").Contains("active"),
                "Dashboard tab should not become active when Memory Limit is invalid."
            );
        }

        [Test]
        public void TC_SEITE_01_SubmitExercisesInTheExam_Successfully()
        {
            driver.Navigate().GoToUrl(baseURL + "/Login");
            driver.FindElement(By.Name("username")).SendKeys("tranthiquynhnga1234");
            driver.FindElement(By.Name("password")).SendKeys("Quynhnga@123");
            driver.FindElement(By.XPath("/html/body/section/div[4]/div[6]/div/form/div[3]/input")).Click();

            wait.Until(d => d.Url.Equals(baseURL + "/", StringComparison.OrdinalIgnoreCase));

            driver.FindElement(By.XPath("/html/body/div[1]/header/header/div/nav/div[1]/div[3]/div[4]/a")).Click();

            driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div[2]/div[2]/div/div[2]/h3/a")).Click();

            var joinButtons = driver.FindElements(By.XPath("/html/body/div[1]/main/div/div/div[1]/div[2]/button"));
            if (joinButtons.Count > 0)
            {
                joinButtons[0].Click();
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.AlertIsPresent());
                driver.SwitchTo().Alert().Accept(); //tu dong ok
            }
            System.Threading.Thread.Sleep(2000);

            var button = driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div[3]/div[2]/div/table/tbody/tr[1]/td[2]/a"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", button);
            System.Threading.Thread.Sleep(3000);

            var langSelect = driver.FindElement(By.Id("language-select"));
            ((IJavaScriptExecutor)driver)
              .ExecuteScript(
                "var sel=arguments[0]; sel.value='Python'; sel.dispatchEvent(new Event('change'));",
                langSelect
              );

            var codeArea = driver.FindElement(By.XPath(
                "/html/body/div[1]/main/div/div/div[5]/div[2]/div[2]/textarea"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value = 'print(\"Hello world!\")';", codeArea);

            var btn = driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div[5]/div[2]/div[3]/button"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btn);
            System.Threading.Thread.Sleep(2000);

            var resultDiv = wait.Until(d =>
                d.FindElement(By.XPath("/html/body/div[1]/main/div/div/div[5]/div[2]/div[4]")));
            StringAssert.Contains("Bài nộp đã được chấm thành công.", resultDiv.Text);
        }

        [Test]
        public void TC_SEITE_02_SubmitExercisesInTheExam_EmptyCode()
        {
            
            driver.Navigate().GoToUrl(baseURL + "/Login");
            driver.FindElement(By.Name("username")).SendKeys("tranthiquynhnga1234");
            driver.FindElement(By.Name("password")).SendKeys("Quynhnga@123");
            driver.FindElement(By.XPath("/html/body/section/div[4]/div[6]/div/form/div[3]/input")).Click();

            wait.Until(d => d.Url.Equals(baseURL + "/", StringComparison.OrdinalIgnoreCase));

            driver.FindElement(By.XPath("/html/body/div[1]/header/header/div/nav/div[1]/div[3]/div[4]/a")).Click();

            driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div[2]/div[2]/div/div[2]/h3/a")).Click();

            var joinButtons = driver.FindElements(By.XPath("/html/body/div[1]/main/div/div/div[1]/div[2]/button"));
            if (joinButtons.Count > 0)
            {
                joinButtons[0].Click();
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.AlertIsPresent());
                driver.SwitchTo().Alert().Accept();
            }
            System.Threading.Thread.Sleep(2000);

            var button = driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div[3]/div[2]/div/table/tbody/tr[1]/td[2]/a"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", button);
            System.Threading.Thread.Sleep(3000);

            var langSelect = driver.FindElement(By.Id("language-select"));
            ((IJavaScriptExecutor)driver)
              .ExecuteScript(
                "var sel=arguments[0]; sel.value='Python'; sel.dispatchEvent(new Event('change'));",
                langSelect
              );

            var codeArea = driver.FindElement(By.XPath(
                "/html/body/div[1]/main/div/div/div[5]/div[2]/div[2]/textarea"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value = '';", codeArea);

            var btn = driver.FindElement(By.XPath("/html/body/div[1]/main/div/div/div[5]/div[2]/div[3]/button"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btn);
            System.Threading.Thread.Sleep(2000);

            var resultDiv = wait.Until(d =>
                d.FindElement(By.XPath("/html/body/div[1]/main/div/div/div[5]/div[2]/div[4]")));
            StringAssert.Contains("Vui lòng nhập code trước khi nộp bài.", resultDiv.Text);
        }
    }
}