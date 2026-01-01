using TechTalk.SpecFlow;
using OpenQA.Selenium;
using AutomationCorreios.Hooks;
using AutomationCorreios.Services;

namespace AutomationCorreios.Steps
{
    [Binding]
    public class BuscaSteps
    {
        private IWebDriver driver => HooksConfig.driver!; 

        private const string URL_INICIAL = "https://www.correios.com.br/";
        private const string URL_CEP = "https://buscacepinter.correios.com.br/app/endereco/index.php";
        private const string URL_RASTREIO = "https://rastreamento.correios.com.br/app/index.php";

        [Given(@"que estou no site dos Correios")]
        public void EstouNoSite()
        {
            driver.Navigate().GoToUrl(URL_INICIAL); // Acessa página inicial
        }

        [When(@"eu acesso a busca de CEP")]
        public void AcessoCep()
        {
            var cep = new BuscaCep(driver);
            cep.Acessar(URL_CEP);
        }

        [Then(@"o sistema deve controlar o tempo do usuario no CEP")]
        public void ControlarCep()
        {
            var cep = new BuscaCep(driver);
            cep.ControlarTempo(); // Controla tempo e tentativas
        }

        [When(@"eu acesso o rastreamento")]
        public void AcessoRastreio()
        {
            var rastreio = new BuscaRastreio(driver);
            rastreio.Acessar(URL_RASTREIO);
        }

        [Then(@"o sistema deve controlar o tempo do usuario no rastreio")]
        public void ControlarRastreio()
        {
            var rastreio = new BuscaRastreio(driver);
            rastreio.ControlarTempo(); // Controla tempo e tentativas
        }
    }
}
