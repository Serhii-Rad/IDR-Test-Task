using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Interactions;
using System.IO;
using SeleniumExtras.WaitHelpers;

namespace IDRTestTask
{
    [TestClass]
    public class SFSTest
    {
        public IWebDriver driver;
        public readonly string URL = "http://sfs.gov.ua/";
        public TestContext TestContext { get; set; }
        private ChromeOptions options = new ChromeOptions();
        

        public Actions Action()
        {
            Actions action = new Actions(driver);
            return action;
        }


        [TestInitialize]
        public void SetUp()
        {
            //options.AddArguments("headless");
            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(URL);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            
        }

        public void WaitVisibilityOfElement(long timeToWait, By locator)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeToWait));
            wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(locator));
        }

        [TestCleanup]
        public void CleanUp()
        {
            var takeScreenshot = driver.TakeScreenshot();
            var check = TestContext.CurrentTestOutcome;
            if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
            {
                var filePathToScreenshot = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Screenshot " + DateTime.Now.ToString().Replace(".", "_").Replace(":", "_") + ".png";
                takeScreenshot.SaveAsFile(filePathToScreenshot);

                if (File.Exists(filePathToScreenshot))
                {
                    TestContext.AddResultFile(filePathToScreenshot);
                }
            }
            driver.Close();
        }
        
        [DataRow("")]
        [DataRow("1")]
        [DataRow("лист")]
        [DataRow("дфс")]

        [TestMethod]
        public void MainTestMethod(string value)
        {
            IWebElement searchField = driver.FindElement(By.XPath("//input[contains(@placeholder, 'Пошук')]"));
            IWebElement submitSearchButton = driver.FindElement(By.XPath("//button[@type='submit']"));

            searchField.Click();
            searchField.SendKeys(value);
            //Action().SendKeys("");

            string urlBeforeSearch = driver.Url;

            submitSearchButton.Click();

            if (value == null || value == "")
            {
                Assert.AreEqual(driver.Url, urlBeforeSearch);
                return;
            }

            Assert.IsTrue(driver.FindElement(By.XPath("//div[contains(@class, 'search__settings')]/label[1]/input[@checked='checked']")).Displayed, "Radio button \"по слову\" is not selected");
            Assert.AreEqual(driver.FindElement(By.XPath("//div[contains(@class, 'search__info')]/p/strong")).Text, "«" + value + "»");
            
            string totalCounterMessage = driver.FindElement(By.XPath("//div[contains(@class, 'search__info')]/p[2]")).Text;

            var res = totalCounterMessage.ToCharArray().Where(n => char.IsDigit(n)).ToArray();
            string countstr = (new string(res));
            int totalCount;
            Int32.TryParse(countstr, out totalCount);
           
            if (totalCount == 0)
            {
                Assert.IsTrue(driver.FindElement(By.XPath("//table[contains(@class, 'table_search')]//tbody")).Size.Height == 0);
                return;
            }

            var countersInTable = driver.FindElements(By.XPath("//table[contains(@class, 'table_search')]//tr/td[3]"));
            int countInOneLine = 0;
            int summ = 0;

            foreach (IWebElement element in countersInTable)
            {
                int.TryParse(element.Text, out countInOneLine);
                summ += countInOneLine;
            }
            
            Assert.AreEqual(totalCount, summ);
        }
    }
}
