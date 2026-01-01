# Automação dos Correios

Automação para busca de CEP e rastreamento de objetos nos Correios.

=========================================================================================================================

## Tecnologias

- C# (.NET 7.0)
- SpecFlow (Gherkin) + NUnit
- Selenium WebDriver
- SeleniumExtras.WaitHelpers
- OpenQA.Selenium.Support.UI (WebDriverWait)
- JavaScript (controle de CAPTCHA)
- System.Threading (timers e pausas)

**Localizadores implementados no projeto (requisito do teste):**

- **ID** → ex.: campo CEP (`endereco`), botão pesquisar (`b-pesquisar`)  
- **XPath** → ex.: campo de código do rastreio (`//*[@id="objeto"]`)  
- **CSS Selector** → ex.: mensagens de erro e alertas 

=========================================================================================================================

## Pré-requisitos

- .NET 7 SDK
- Chrome instalado
- ChromeDriver compatível com a versão do Chrome
- IDE com suporte a C# (ex.: Visual Studio, Rider)

=========================================================================================================================

## Como Funciona

### 1. Busca de CEP
- Exibe popup com tempo (30s) e tentativas (3) para preencher o CAPTCHA
- Valida logradouro, bairro, localidade e CEP
- Se dados não encontrados, tenta um segundo CEP automaticamente

### 2. Rastreamento de Objeto
- Mesma lógica de popup e controle de tentativas
- Valida existência do objeto ou exibe erro

=========================================================================================================================

## Fluxo de Teste Automático

1. Acessa o site dos Correios  
2. Busca de CEP  
   - CEP inicial: `80700000` → confirma que não existe  
   - Segundo CEP: `01013-001` → confirma logradouro, bairro e localidade (`Rua Quinze de Novembro, São Paulo/SP`)  
3. Rastreamento de objeto  
   - Código: `SS987654321BR` → confirma que não existe  
4. Navegador é fechado ao final do fluxo  

=========================================================================================================================


## Arquitetura do Projeto

		AutomationCorreios
		│
		├── Captcha
		│ └── Captcha.cs # Controle centralizado de CAPTCHA (popup, tempo, tentativas e timeout)
		│
		├── Drivers
		│ └── WebDriverFactory.cs # Criação e configuração do WebDriver
		│
		├── Features
		│ └── BuscaCorreios.feature # Cenários BDD (SpecFlow / Gherkin)
		│
		├── Hooks
		│ └── Hooks.cs # Inicialização e finalização do WebDriver
		│
		├── Services
		│ ├── BuscaCep.cs # Fluxo de busca de CEP
		│ └── BuscaRastreio.cs # Fluxo de rastreamento de objetos
		│
		├── Steps
		│ └── BuscaSteps.cs # Steps do SpecFlow
		│
		└── AutomationCorreios.csproj


=========================================================================================================================


## Como Executar

1. Abra o projeto na IDE de sua preferência
2. Certifique-se que o ChromeDriver está no PATH ou configurado
3. Execute os testes via SpecFlow/NUnit
4. Observe os popups de tempo e tentativas durante a execução

---

## Observações

- Cada CAPTCHA tem 30 segundos e até 3 tentativas  
- Navegador é fechado automaticamente se as tentativas se esgotarem  
- Scripts validam dados específicos de CEP e rastreio 
