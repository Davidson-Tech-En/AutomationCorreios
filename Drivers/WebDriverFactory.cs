using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AutomationCorreios.Drivers
{
    public static class WebDriverFactory
    {
        public static IWebDriver Create()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized"); // Abre navegador maximizado
            options.AddArgument("--disable-blink-features=AutomationControlled"); // Evita detecção de automação

            return new ChromeDriver(options); // Cria instância do ChromeDriver
        }
    }
}
