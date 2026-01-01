using OpenQA.Selenium;
using NUnit.Framework;
using System.Threading;
using System;

namespace AutomationCorreios.Captcha
{
    public class Captcha
    {
        private readonly IWebDriver driver;

        public Captcha(IWebDriver driver)
        {
            this.driver = driver;
        }

        public void IniciarTimerComTentativas()
        {
            string js = @"
                if (window.timerAtivo) return; // Evita criar mais de um popup

                window.timerAtivo = true; 
                window.tempo = 30; // Tempo inicial do captcha
                window.tentativas = 3; // Quantidade de tentativas
                window.expirou = false; // Marca se o tempo expirou

                const d = document.createElement('div');
                d.id = 'popup-timer';
                d.style = `
                    position: fixed;
                    top: 12%;
                    left: 65%;
                    transform: translateX(-50%);
                    background: white;
                    border: 1px solid red;
                    padding: 10px;
                    z-index: 99999;
                    font-family: Arial;
                    font-size: 12px;
                    text-align: center;
                    width: 220px;
                `;

                // PopUp exibido com tempo e tentativas
                d.innerHTML = `
                    <h3 style='font-size:18px; margin:5px 0;'>Preencha o captcha</h3>
                    <p style='font-size:12px; margin:3px 0;'>Tempo restante: <span id='tempo'>30</span> segundos</p>
                    <p style='font-size:12px; margin:3px 0;'>Tentativas restantes: <span id='tentativas'>3</span></p>
                `;

                document.body.appendChild(d);

                // Contador de tempo
                window.intervalo = setInterval(() => {
                    window.tempo--;
                    document.getElementById('tempo').innerText = window.tempo;

                    if (window.tempo <= 0) {
                        window.expirou = true; // Marca que tempo acabou
                        clearInterval(window.intervalo);

                        const msg = document.createElement('p');
                        msg.innerText = 'Tempo para preencher o captcha esgotou!';
                        msg.style.color = 'red';
                        document.getElementById('popup-timer').appendChild(msg);
                    }

                }, 1000);
            ";

            ((IJavaScriptExecutor)driver).ExecuteScript(js);
        }

        public void FocarCaptchaAutomaticamente()
        {
            // Coloca o foco no campo de captcha
            ((IJavaScriptExecutor)driver).ExecuteScript(@"
                var campoCaptcha = document.getElementById('captcha');
                if(campoCaptcha) campoCaptcha.focus();
            ");
        }

        public void ReiniciarTimerComTentativas()
        {
            // Reseta o timer para 30 segundos
            ((IJavaScriptExecutor)driver).ExecuteScript("window.tempo = 30;");
        }

        public void RegistrarErroCaptcha()
        {
            ((IJavaScriptExecutor)driver).ExecuteScript(@"
                window.tentativas--; // Diminui tentativas
                document.getElementById('tentativas').innerText = window.tentativas;

                if (window.tentativas <= 0) {
                    clearInterval(window.intervalo); // Para timer
                    const msg = document.createElement('p');
                    msg.innerText = 'Tentativas esgotadas. Fechando navegador em 5 segundos...';
                    msg.style.color = 'red';
                    document.getElementById('popup-timer').appendChild(msg);
                } else {
                    window.tempo = 30; // Reinicia tempo
                }
            ");

            bool tentativasEsgotadas = (bool)((IJavaScriptExecutor)driver)
                .ExecuteScript("return window.tentativas <= 0;");

            if (tentativasEsgotadas)
            {
                // Fecha o navegador após 5 segundos
                new Thread(() =>
                {
                    Thread.Sleep(5000);
                    try { driver.Quit(); } catch { }
                }).Start();
            }
        }

        public void RemoverTimer()
        {
            // Remove popup e limpa timer
            ((IJavaScriptExecutor)driver).ExecuteScript(@"
                clearInterval(window.intervalo);
                window.timerAtivo = false;
                var popup = document.getElementById('popup-timer');
                if(popup) popup.remove();
            ");
        }

        public void VerificarTimeout()
        {
            bool expirou = (bool)((IJavaScriptExecutor)driver)
                .ExecuteScript("return window.expirou === true;");

            if (expirou)
            {
                driver.Quit(); 
                Assert.Fail("Tentativas esgotadas. Navegador fechado.");
            }
        }
    }
}
