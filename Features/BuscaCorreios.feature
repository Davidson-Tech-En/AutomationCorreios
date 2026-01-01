Feature: Controle de tempo Correios
  # Feature para testar o controle de tempo e tentativas ao acessar CEP e rastreio no site dos Correios

  Scenario: Usuario acessa CEP e rastreio com controle de tempo
    # Step para abrir o site inicial dos Correios
    Given que estou no site dos Correios

    # Step para acessar a busca de CEP
    When eu acesso a busca de CEP
    # Step que monitora o tempo e o captcha para o usuário no CEP
    Then o sistema deve controlar o tempo do usuario no CEP

    # Step para acessar o rastreamento de objetos
    When eu acesso o rastreamento
    # Step que monitora o tempo e o captcha para o usuário no rastreio
    Then o sistema deve controlar o tempo do usuario no rastreio
