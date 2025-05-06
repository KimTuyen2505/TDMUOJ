using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;

namespace hovaten_FunctionTest
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

            driver.FindElement(By.XPath("/html/body/section/div[4]/div[6]/form/div[1]/input")).SendKeys("Dang Thi Bich Thuy");
            driver.FindElement(By.Name("username")).SendKeys("dangthibichthuy");
            driver.FindElement(By.Name("email")).SendKeys("dangthibichthuy@gmail.com");
            driver.FindElement(By.Name("password")).SendKeys("BichThuy@2202");
            driver.FindElement(By.Name("confirmPassword")).SendKeys("BichThuy@2202");

            driver.FindElement(By.XPath("/html/body/section/div[4]/div[6]/form/div[6]/input")).Click();

            wait.Until(d => d.Url.Equals(baseURL + "/Login", StringComparison.OrdinalIgnoreCase));

            Assert.That(driver.Url, Is.EqualTo(baseURL + "/Login"),
                "After successful registration, user should be redirected to the Login page.");
        }
    }
}