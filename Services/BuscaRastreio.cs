using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using AutomationCorreios.Captcha;
using System;

namespace AutomationCorreios.Services
{
    public class BuscaRastreio
    {
        private readonly IWebDriver driver;
        private readonly Captcha.Captcha captcha;

        public BuscaRastreio(IWebDriver driver)
        {
            this.driver = driver;
            captcha = new Captcha.Captcha(driver);
        }

        public void Acessar(string url)
        {
            driver.Navigate().GoToUrl(url);
            captcha.IniciarTimerComTentativas();

            // Espera campo de rastreio usando XPath
            var campoObjeto = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                .Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='objeto']")));

            campoObjeto.SendKeys("SS987654321BR"); // Preenche código do objeto

            captcha.FocarCaptchaAutomaticamente(); 
        }

        public void ControlarTempo()
        {
            try
            {
                var btnPesquisar = driver.FindElement(By.Id("b-pesquisar"));
                var campoCaptcha = driver.FindElement(By.Id("captcha"));

                // Adiciona listener para click no botão e Enter no campo captcha
                ((IJavaScriptExecutor)driver).ExecuteScript(@"
                    var btn = arguments[0];
                    var campo = arguments[1];

                    if(!btn.timerListenerAdicionado) {
                        btn.addEventListener('click', function() {
                            window.verificarCaptchaCep = true;
                        });
                        btn.timerListenerAdicionado = true;
                    }

                    if(!campo.enterListenerAdicionado) {
                        campo.addEventListener('keydown', function(e) {
                            if(e.key === 'Enter') {
                                window.verificarCaptchaCep = true;
                            }
                        });
                        campo.enterListenerAdicionado = true;
                    }
                ", btnPesquisar, campoCaptcha);
            }
            catch { }

            for (int i = 0; i < 120; i++)
            {
                // Verifica se objeto não encontrado
                var erroObjetoNaoEncontrado = driver.FindElements(By.CssSelector("div#alerta .msg"))
                    .FirstOrDefault(e => e.Displayed &&
                        e.Text.Contains("Objeto não encontrado na base de dados dos Correios."));

                if (erroObjetoNaoEncontrado != null)
                {
                    captcha.RemoverTimer();
                    Thread.Sleep(5000);
                    driver.Quit();
                    Assert.Fail("Objeto não encontrado. Navegador fechado.");
                }

                bool verificarCaptchaCep = (bool)((IJavaScriptExecutor)driver)
                    .ExecuteScript("return window.verificarCaptchaCep === true;");

                if (verificarCaptchaCep)
                {
                    // Se captcha falhou, registra erro
                    var captchaMsg = driver.FindElements(By.CssSelector(
                        "body > main > div.container > form > div.jumbotron > div.campos.captcha > div:nth-child(2) > div.mensagem"))
                        .FirstOrDefault(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text));

                    if (captchaMsg != null)
                        captcha.RegistrarErroCaptcha();

                    captcha.FocarCaptchaAutomaticamente(); // Foca no captcha
                    ((IJavaScriptExecutor)driver).ExecuteScript("window.verificarCaptchaCep = false;");
                }

                captcha.VerificarTimeout();
                Thread.Sleep(500);
            }
        }
    }
}
