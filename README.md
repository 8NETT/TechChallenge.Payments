# Tech Challenge - Módulo de Pagamentos

Este repositório contém o código-fonte do serviço de processamento de pagamentos, uma Azure Function desenvolvida como parte do Tech Challenge da pós-graduação. O serviço utiliza uma arquitetura orientada a eventos para processar pagamentos de pedidos de forma assíncrona, escalável e resiliente.

## Estrutura do Projeto

A solução é baseada em uma arquitetura orientada a eventos e está organizada da seguinte forma para separar as responsabilidades:

-   `TechChallenge.Payments/`: Projeto principal da Azure Function. Contém a lógica de negócio, os modelos de eventos e as implementações de serviços.
    -   `Functions/`: Define os gatilhos da aplicação. `OrderPlacedHandler` é a função principal, acionada por eventos do Azure Event Hub.
    -   `Contracts/`: Contém as interfaces (abstrações) para os serviços, como `IPaymentGateway`, promovendo o baixo acoplamento.
    -   `Services/`: Fornece as implementações concretas dos contratos. `PaymentGateway` simula o processamento de pagamentos.
    -   `Models/`: Define as classes que representam os dados dos eventos, como `OrderPlacedEvent` e `PaymentProcessedEvent`.
    -   `Program.cs`: Ponto de entrada da aplicação, onde a injeção de dependência e os serviços são configurados.

-   `TechChallenge.Payments.Tests/`: Projeto de testes unitários, utilizando xUnit para garantir a qualidade e o comportamento esperado das funcionalidades.

-   `.github/`: Contém os workflows de CI/CD do GitHub Actions, que automatizam o build, teste e deploy da aplicação.

-   `Dockerfile`: Permite a containerização da aplicação, facilitando o deploy e a portabilidade entre ambientes.

## Tecnologias Utilizadas

-   **.NET 9**: A versão mais recente da plataforma de desenvolvimento da Microsoft, utilizada para construir uma aplicação performática e com recursos modernos.

-   **Azure Functions v4**: Plataforma de computação *serverless* que permite executar o código em resposta a eventos, como mensagens em um Event Hub, sem a necessidade de gerenciar a infraestrutura.

-   **Azure Event Hubs**: Serviço de streaming de Big Data e ingestão de eventos, utilizado como o barramento de eventos para a comunicação assíncrona entre os serviços.

-   **OpenTelemetry**: Padrão de observabilidade utilizado para instrumentar a aplicação, coletando traces e métricas para monitoramento e análise de performance com o Application Insights.

-   **xUnit**: Framework de testes unitários para a plataforma .NET, usado para criar e executar testes que validam a lógica da aplicação.

-   **Docker**: Plataforma de containerização utilizada para empacotar a aplicação e suas dependências. O `Dockerfile` no projeto utiliza um build multi-stage para criar uma imagem final otimizada e segura.

-   **GitHub Actions**: Automação de CI/CD integrada ao GitHub. O pipeline configurado realiza o build, executa os testes e, em caso de push para a branch `main`, publica a imagem Docker e faz o deploy no Azure Container Apps.

## Como Executar a Aplicação

### Pré-requisitos

-   [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
-   [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools)
-   [Docker](https://www.docker.com/products/docker-desktop) (Opcional)
-   [Azurite](https://github.com/Azure/Azurite) (para emulação do Azure Storage) ou uma conta de armazenamento do Azure.

### Usando o .NET CLI

1.  Clone o repositório:
    ```bash
    git clone https://github.com/seu-usuario/TechChallenge.Payments.git
    cd TechChallenge.Payments
    ```

2.  Crie e configure o arquivo `local.settings.json` na pasta `TechChallenge.Payments`:
    ```json
    {
      "IsEncrypted": false,
      "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "EVENT_HUB_CONNECTION": "sua-connection-string-do-event-hub",
        "PURCHASES_HUB_NAME": "nome-do-hub-de-compras",
        "EVENT_HUB_CONSUMER_GROUP": "payments"
      }
    }
    ```

3.  Inicie o Azurite (se estiver usando para emulação de storage) e execute a aplicação a partir da pasta do projeto:
    ```bash
    cd TechChallenge.Payments
    func start
    ```

### Usando Docker

1.  Na raiz do projeto, construa a imagem Docker:
    ```bash
    docker build -t techchallenge-payments .
    ```

2.  Execute o container. Você precisará passar as variáveis de ambiente necessárias para a conexão com o Event Hub.
    ```bash
    docker run -p 8080:80 -e EVENT_HUB_CONNECTION="sua-connection-string" -e PURCHASES_HUB_NAME="seu-hub" -e EVENT_HUB_CONSUMER_GROUP="seu-grupo" techchallenge-payments
    ```
    A função estará escutando os eventos na porta `8080` do seu host.

## Como Executar os Testes

Para executar todos os testes da solução, navegue até a raiz do projeto e execute o seguinte comando:

```bash
dotnet test
```
