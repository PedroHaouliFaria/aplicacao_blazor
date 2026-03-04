# 📋 Documentação Completa — BlazorWeatherApp

## Sumário

1. [Visão Geral do Projeto](#1-visão-geral-do-projeto)
2. [Arquitetura da Aplicação](#2-arquitetura-da-aplicação)
3. [Pré-requisitos](#3-pré-requisitos)
4. [Estrutura do Projeto](#4-estrutura-do-projeto)
5. [Camada de Serviços e API Controller](#5-camada-de-serviços-e-api-controller)
6. [Testes Automatizados](#6-testes-automatizados)
7. [Containerização com Docker](#7-containerização-com-docker)
8. [CI/CD com GitHub Actions](#8-cicd-com-github-actions)
9. [Deploy no Render.com](#9-deploy-no-rendercom)
10. [Como Executar Localmente](#10-como-executar-localmente)
11. [Diferenciais — "Indo Além"](#11-diferenciais--indo-além)

---

## 1. Visão Geral do Projeto

Esta aplicação é um **Blazor Web App** construído com .NET 10.0 que utiliza o template padrão do Blazor Weather. O projeto foi expandido com:

- **API REST Controller** para expor os dados de previsão do tempo via endpoint `/api/weatherforecast`
- **Injeção de Dependência (DI)** com serviço separado (`IWeatherForecastService`)
- **12 testes automatizados** usando xUnit + Moq
- **Dockerfile multi-stage** otimizado para produção
- **Pipeline CI/CD** completo via GitHub Actions
- **Deploy automático** no Render.com

---

## 2. Arquitetura da Aplicação

```
┌─────────────────────────────────────────────────┐
│                  Blazor Web App                  │
│                                                  │
│  ┌──────────────────┐  ┌──────────────────────┐ │
│  │  Weather.razor    │  │  WeatherForecast     │ │
│  │  (Componente UI)  │  │  Controller (API)    │ │
│  └────────┬─────────┘  └────────┬─────────────┘ │
│           │                      │               │
│           └──────┐   ┌──────────┘               │
│                  │   │                           │
│           ┌──────▼───▼──────┐                   │
│           │ IWeatherForecast │                   │
│           │    Service       │                   │
│           └────────┬────────┘                    │
│                    │                             │
│           ┌────────▼────────┐                   │
│           │ WeatherForecast  │                   │
│           │    Service       │                   │
│           └─────────────────┘                    │
└─────────────────────────────────────────────────┘
```

**Padrão utilizado:** Service Layer Pattern com Dependency Injection, garantindo:
- **Separação de responsabilidades**: lógica de negócio isolada no serviço
- **Testabilidade**: o controller pode ser testado com mocks do serviço
- **Reutilização**: mesmo serviço é usado pelo componente Blazor e pela API REST

---

## 3. Pré-requisitos

| Ferramenta | Versão Mínima | Uso |
|------------|---------------|-----|
| .NET SDK | 10.0+ | Build e execução |
| Git | 2.x+ | Controle de versão |
| Docker | 20.x+ | Containerização (opcional local) |
| Conta GitHub | — | CI/CD com Actions |
| Conta Render | — | Deploy da aplicação |

---

## 4. Estrutura do Projeto

```
aplicacao_blazor/
├── .github/
│   └── workflows/
│       └── ci-cd.yml              # Pipeline CI/CD
├── BlazorWeatherApp/
│   ├── Components/
│   │   ├── Layout/                # Layout principal
│   │   ├── Pages/
│   │   │   ├── Counter.razor      # Página Counter (template)
│   │   │   ├── Home.razor         # Página Home (template)
│   │   │   └── Weather.razor      # Página Weather (usa DI)
│   │   ├── App.razor              # Root component
│   │   └── Routes.razor           # Roteamento
│   ├── Controllers/
│   │   └── WeatherForecastController.cs  # API REST
│   ├── Models/
│   │   └── WeatherForecast.cs     # Modelo de dados
│   ├── Services/
│   │   ├── IWeatherForecastService.cs    # Interface do serviço
│   │   └── WeatherForecastService.cs     # Implementação
│   ├── Program.cs                 # Entry point + DI config
│   └── BlazorWeatherApp.csproj    # Projeto principal
├── BlazorWeatherApp.Tests/
│   ├── WeatherForecastControllerTests.cs  # Testes do controller
│   ├── WeatherForecastServiceTests.cs     # Testes do serviço
│   └── BlazorWeatherApp.Tests.csproj      # Projeto de testes
├── Dockerfile                     # Multi-stage build
├── .dockerignore                  # Exclusões do Docker
├── .gitignore                     # Exclusões do Git
├── BlazorWeatherApp.sln           # Solution file
└── docs/
    └── documentacao.md            # Este documento
```

---

## 5. Camada de Serviços e API Controller

### 5.1 Modelo — `WeatherForecast.cs`

O modelo representa uma previsão do tempo com as propriedades:
- **Date** (`DateOnly`): data da previsão
- **TemperatureC** (`int`): temperatura em Celsius
- **TemperatureF** (`int`): calculado automaticamente — `32 + (int)(TemperatureC / 0.5556)`
- **Summary** (`string?`): descrição textual do clima

### 5.2 Interface — `IWeatherForecastService.cs`

Define o contrato do serviço com o método:
```csharp
Task<WeatherForecast[]> GetForecastsAsync(DateOnly startDate);
```

A utilização de interface permite:
- **Mock** nos testes unitários
- **Substituição** da implementação sem alterar dependentes
- **Inversão de dependência** (princípio SOLID)

### 5.3 Implementação — `WeatherForecastService.cs`

Gera 5 previsões a partir da data fornecida, com temperaturas aleatórias entre -20°C e 54°C e summaries de um conjunto pré-definido.

### 5.4 Controller — `WeatherForecastController.cs`

Endpoint REST disponível em `GET /api/weatherforecast` que:
1. Recebe a requisição HTTP
2. Chama `IWeatherForecastService.GetForecastsAsync()`
3. Retorna `200 OK` com o array de previsões em JSON

### 5.5 Configuração de DI — `Program.cs`

```csharp
builder.Services.AddControllers();
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddHealthChecks();
```

O serviço é registrado como **Scoped** (uma instância por requisição), e também configuramos:
- **Controllers** para a API REST
- **Health Checks** para o endpoint `/health` (usado pelo Docker e Render)

---

## 6. Testes Automatizados

### 6.1 Framework e Ferramentas

| Pacote | Versão | Função |
|--------|--------|--------|
| xUnit | 2.9.3 | Framework de testes |
| Moq | 4.20.72 | Criação de mocks |
| coverlet | 6.0.4 | Cobertura de código |

### 6.2 Testes do Controller (5 testes)

| Teste | O que valida |
|-------|-------------|
| `Get_ReturnsOkResult_WithForecasts` | Retorna `200 OK` com lista de previsões |
| `Get_ReturnsNonEmptyList` | Lista retornada não está vazia |
| `Get_CallsServiceExactlyOnce` | O serviço é chamado exatamente 1 vez |
| `Get_ReturnsCorrectForecastData` | Dados retornados correspondem ao mock |
| `Get_TemperatureF_IsCalculatedCorrectly` | Conversão Celsius→Fahrenheit está correta |

**Destaque:** Os testes do controller utilizam **Moq** para criar mocks do `IWeatherForecastService`, isolando completamente o controller da implementação real do serviço.

```csharp
_mockService = new Mock<IWeatherForecastService>();
_controller = new WeatherForecastController(_mockService.Object);
```

### 6.3 Testes do Serviço (7 testes)

| Teste | O que valida |
|-------|-------------|
| `GetForecastsAsync_ReturnsExactlyFiveForecasts` | Retorna exatamente 5 previsões |
| `GetForecastsAsync_ReturnsNonEmptyList` | Lista não está vazia |
| `GetForecastsAsync_DatesAreInFuture` | Todas as datas são futuras |
| `GetForecastsAsync_TemperaturesAreInValidRange` | Temperaturas entre -20°C e 54°C |
| `GetForecastsAsync_SummariesAreNotNull` | Nenhum summary é nulo ou vazio |
| `GetForecastsAsync_SummariesAreFromValidSet` | Summaries são do conjunto pré-definido |
| `GetForecastsAsync_DatesAreSequential` | Datas são sequenciais (dia 1, 2, 3...) |

### 6.4 Executando os Testes

```bash
dotnet test BlazorWeatherApp.Tests/ --verbosity normal
```

Resultado esperado: **12 testes, 0 falhas, 12 sucessos**

---

## 7. Containerização com Docker

### 7.1 Estratégia: Multi-stage Build

O `Dockerfile` utiliza **duas etapas**:

1. **Build stage** (`sdk:10.0`): restaura pacotes, executa testes e publica a aplicação
2. **Runtime stage** (`aspnet:10.0`): imagem leve apenas com o runtime

Vantagens:
- Imagem final **menor** (não inclui SDK, código-fonte ou testes)
- Testes são executados **durante o build**, impedindo imagens com falhas
- **Layer caching** nos arquivos `.csproj` acelera builds subsequentes

### 7.2 Detalhes do Dockerfile

```dockerfile
# Stage 1: Build com SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# Copia .csproj primeiro para cache de restore
COPY BlazorWeatherApp.sln .
COPY BlazorWeatherApp/BlazorWeatherApp.csproj BlazorWeatherApp/
RUN dotnet restore
# Copia tudo, testa e publica
COPY . .
RUN dotnet test BlazorWeatherApp.Tests/
RUN dotnet publish BlazorWeatherApp/ -c Release -o /app/publish

# Stage 2: Runtime leve
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BlazorWeatherApp.dll"]
```

### 7.3 Health Check

O Dockerfile inclui um `HEALTHCHECK` que verifica o endpoint `/health` a cada 30 segundos, utilizado pelo Render.com para determinar se a aplicação está saudável.

---

## 8. CI/CD com GitHub Actions

### 8.1 Visão Geral do Pipeline

O arquivo `.github/workflows/ci-cd.yml` define 3 jobs:

```
Push/PR na main → build-and-test → docker-build → deploy
```

### 8.2 Job 1: Build & Test (CI)

| Step | Ação | Descrição |
|------|------|-----------|
| 1 | `actions/checkout@v4` | Clona o repositório |
| 2 | `actions/setup-dotnet@v4` | Instala .NET 10.0 SDK |
| 3 | `dotnet restore` | Restaura pacotes NuGet |
| 4 | `dotnet build` | Compila em modo Release |
| 5 | `dotnet test` | Executa os 12 testes |
| 6 | `actions/upload-artifact@v4` | Salva resultados como artifact |

**Trigger:** Executa em todo push e PR para `main`.

### 8.3 Job 2: Docker Build

Constrói a imagem Docker para validar que o `Dockerfile` está correto. Depende do Job 1 ter passado.

### 8.4 Job 3: Deploy (CD)

Dispara o **Deploy Hook** do Render.com via HTTP POST. Só executa se:
- Jobs 1 e 2 passaram ✅
- Branch é `main` ✅
- Evento é `push` (não PR) ✅

---

## 9. Deploy no Render.com

### 9.1 Passo a Passo para Configurar o Render

1. **Criar conta** em [render.com](https://render.com)

2. **Novo Web Service**:
   - Clique em **"New +"** → **"Web Service"**
   - Conecte seu repositório GitHub: `PedroHaouliFaria/aplicacao_blazor`
   - Configurações:
     - **Name**: `blazor-weather-app`
     - **Region**: escolha a mais próxima (ex: Oregon)
     - **Runtime**: **Docker**
     - **Branch**: `main`
     - **Plan**: Free

3. **Obter Deploy Hook URL**:
   - No dashboard do serviço, vá em **Settings** → **Deploy Hook**
   - Copie a URL gerada (formato: `https://api.render.com/deploy/srv-xxxxx?key=yyyy`)

4. **Configurar Secret no GitHub**:
   - No repositório GitHub, vá em **Settings** → **Secrets and variables** → **Actions**
   - Clique em **New repository secret**
   - **Nome**: `RENDER_DEPLOY_HOOK_URL`
   - **Valor**: cole a URL do Deploy Hook

5. **Primeiro deploy**: Faça um push na branch `main`. O pipeline vai:
   1. Compilar e testar ✅
   2. Validar Docker ✅
   3. Disparar deploy no Render ✅

### 9.2 Variáveis de Ambiente no Render

O Render detecta automaticamente o `Dockerfile`. As variáveis já estão configuradas no Dockerfile:

| Variável | Valor | Descrição |
|----------|-------|-----------|
| `ASPNETCORE_URLS` | `http://+:8080` | Porta da aplicação |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Modo de execução |

> **Nota:** O Render atribui automaticamente uma porta via variável `PORT`. Se necessário, a aplicação responde na porta 8080 conforme configurado.

---

## 10. Como Executar Localmente

### 10.1 Sem Docker

```bash
# Clonar o repositório
git clone https://github.com/PedroHaouliFaria/aplicacao_blazor.git
cd aplicacao_blazor

# Restaurar e compilar
dotnet build

# Executar testes
dotnet test BlazorWeatherApp.Tests/ --verbosity normal

# Rodar a aplicação
dotnet run --project BlazorWeatherApp/
```

Acesse:
- **Aplicação Blazor**: `http://localhost:5000`
- **Weather Page**: `http://localhost:5000/weather`
- **API REST**: `http://localhost:5000/api/weatherforecast`
- **Health Check**: `http://localhost:5000/health`

### 10.2 Com Docker

```bash
# Build da imagem
docker build -t blazor-weather-app .

# Executar container
docker run -p 8080:8080 blazor-weather-app
```

Acesse: `http://localhost:8080`

---

## 11. Diferenciais — "Indo Além"

Este projeto vai além dos requisitos básicos nos seguintes aspectos:

### 🏗️ Arquitetura com Service Layer Pattern
Em vez de ter a lógica diretamente no controller, implementamos o padrão Service Layer com injeção de dependência. Isso demonstra conhecimento de princípios SOLID (especificamente **Dependency Inversion** e **Single Responsibility**).

### 🧪 Testes com Moq (Mocking)
Os testes do controller utilizam **Moq** para criar mocks do serviço, demonstrando:
- Isolamento de camadas nos testes
- Verificação de comportamento (não apenas estado)
- Testes determinísticos e rápidos

### 🐳 Dockerfile Otimizado
- **Multi-stage build** para imagem leve
- **Testes durante o build** Docker (falha impede imagem com bugs)
- **Health check** integrado para monitoramento
- **Layer caching** otimizado para builds mais rápidos

### 🔄 Pipeline CI/CD Completo com 3 Jobs
- Separação clara entre CI (build + test) e CD (deploy)
- Validação da imagem Docker como etapa intermediária
- Upload de artifacts com resultados dos testes
- Deploy condicional (apenas `main` + push)

### 📊 12 Testes Abrangentes
- 5 testes do controller (com mocks)
- 7 testes do serviço (validação de dados)
- Cobertura de cenários: retorno, tipos, ranges, cálculos, sequencialidade

### 🏥 Health Check Endpoint
Endpoint `/health` nativo do ASP.NET Core para monitoramento da saúde da aplicação, utilizado tanto pelo Docker quanto pelo Render.com.

### 📖 Documentação Detalhada
Esta documentação cobre todos os aspectos do projeto, desde a arquitetura até o deploy, servindo como referência completa para reprodução e avaliação.
