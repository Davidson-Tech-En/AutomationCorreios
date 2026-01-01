using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TechTalk.SpecFlow;
using System;
using AutomationCorreios.Drivers;

namespace AutomationCorreios.Hooks
{
    [Binding]
    public class HooksConfig
    {
        public static IWebDriver? driver { get; private set; }

        [BeforeScenario]
        public void BeforeScenario()
        {
            // Cria driver antes de cada cenário
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");

            driver = WebDriverFactory.Create();

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2); 
        }

        [AfterScenario]
        public void AfterScenario()
        {
            driver?.Quit();
        }
    }
}
