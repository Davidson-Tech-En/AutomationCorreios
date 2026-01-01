using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Linq;
using System.Threading;
using System;
using AutomationCorreios.Captcha;

namespace AutomationCorreios.Services
{
    public class BuscaCep
    {
        private readonly IWebDriver driver;
        private readonly Captcha.Captcha captcha;

        private const string URL_RASTREIO = "https://rastreamento.correios.com.br/app/index.php";

        private const string CEP_ERRO_ID = "mensagem-resultado-alerta";
        private const string CEP_DADOS_NAO_ENCONTRADOS = "h6";
        private const string CEP_LOGRADOURO = "td[data-th='Logradouro/Nome']";
        private const string CEP_BAIRRO = "td[data-th='Bairro/Distrito']";
        private const string CEP_LOCALIDADE = "td[data-th='Localidade/UF']";
        private const string CEP_CEP = "td[data-th='CEP']";

        private bool erroCepVisivel = false; // Marca se erro de CEP apareceu
        private bool segundoCepExecutado = false; // Marca se já tentou CEP alternativo
        private bool verificarCaptchaCep = false; // Marca se precisa registrar erro de captcha

        public BuscaCep(IWebDriver driver)
        {
            this.driver = driver;
            captcha = new Captcha.Captcha(driver);
        }

        public void Acessar(string url)
        {
            driver.Navigate().GoToUrl(url);
            captcha.IniciarTimerComTentativas();

            var campoCep = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                .Until(ExpectedConditions.ElementIsVisible(By.Id("endereco")));

            campoCep.SendKeys("80070000"); // Preenche CEP 

            captcha.FocarCaptchaAutomaticamente(); // Foca no captcha
        }

        public void ControlarTempo()
        {
            for (int i = 0; i < 120; i++)
            {
                // Verifica se erro de CEP apareceu
                var erroCep = driver.FindElements(By.Id(CEP_ERRO_ID))
                    .FirstOrDefault(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text));

                if (erroCep != null && !erroCepVisivel)
                {
                    captcha.ReiniciarTimerComTentativas(); // Reinicia timer
                    erroCepVisivel = true;
                }
                if (erroCep == null) erroCepVisivel = false;

                try
                {
                    var btnPesquisar = driver.FindElement(By.Name("btn_pesquisar"));

                    // Adiciona listener para click no botão
                    ((IJavaScriptExecutor)driver).ExecuteScript(@"
                        var btn = arguments[0];
                        if(!btn.timerListenerAdicionado) {
                            btn.addEventListener('click', function() {
                                window.verificarCaptchaCep = true;
                            });
                            btn.timerListenerAdicionado = true;
                        }
                    ", btnPesquisar);
                }
                catch { }

                verificarCaptchaCep = (bool)((IJavaScriptExecutor)driver)
                    .ExecuteScript("return window.verificarCaptchaCep === true;");

                if (verificarCaptchaCep)
                {
                    // Se captcha falhou, registra erro
                    var captchaMsg = driver.FindElements(By.CssSelector("#alerta > div.msg"))
                        .FirstOrDefault(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text));

                    if (captchaMsg != null)
                        captcha.RegistrarErroCaptcha();

                    ((IJavaScriptExecutor)driver).ExecuteScript("window.verificarCaptchaCep = false;");
                }

                // Verifica se dados não encontrados
                var dadosNaoEncontrados = driver.FindElements(By.CssSelector(CEP_DADOS_NAO_ENCONTRADOS))
                    .FirstOrDefault(e => e.Displayed && e.Text.Contains("Dados não encontrado"));

                if (dadosNaoEncontrados != null && !segundoCepExecutado)
                {
                    captcha.RemoverTimer();
                    Thread.Sleep(3000);

                    driver.Navigate().Refresh();

                    var campoCep = new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                        .Until(ExpectedConditions.ElementIsVisible(By.Id("endereco")));

                    campoCep.Clear();
                    campoCep.SendKeys("01013001"); // Tenta segundo CEP

                    captcha.FocarCaptchaAutomaticamente();
                    segundoCepExecutado = true;
                    captcha.IniciarTimerComTentativas();
                    continue;
                }

                // Verifica se dados corretos foram encontrados
                var logradouroEl = driver.FindElements(By.CssSelector(CEP_LOGRADOURO))
                    .FirstOrDefault(e => e.Displayed && e.Text.Contains("Rua Quinze de Novembro"));

                if (logradouroEl != null)
                {
                    var bairroEl = driver.FindElement(By.CssSelector(CEP_BAIRRO));
                    var localidadeEl = driver.FindElement(By.CssSelector(CEP_LOCALIDADE));
                    var cepEl = driver.FindElement(By.CssSelector(CEP_CEP));

                    if (bairroEl.Text.Contains("Centro") &&
                        localidadeEl.Text.Contains("São Paulo/SP") &&
                        cepEl.Text.Contains("01013-001"))
                    {
                        captcha.RemoverTimer();
                        Thread.Sleep(5000);
                        driver.Navigate().GoToUrl(URL_RASTREIO); // Vai para rastreio
                        return;
                    }
                }

                captcha.VerificarTimeout();
                Thread.Sleep(500);
            }
        }
    }
}
