# 📋 Documentação — BlazorWeatherApp

## Sumário

1. [Visão Geral](#1-visão-geral)
2. [Estrutura do Projeto](#2-estrutura-do-projeto)
3. [Serviços e API Controller](#3-serviços-e-api-controller)
4. [Testes Automatizados](#4-testes-automatizados)
5. [Docker](#5-docker)
6. [CI/CD com GitHub Actions](#6-cicd-com-github-actions)
7. [Deploy no Render.com](#7-deploy-no-rendercom)
8. [Como Executar Localmente](#8-como-executar-localmente)
9. [Diferenciais — "Indo Além"](#9-diferenciais--indo-além)

---

## 1. Visão Geral

Blazor Web App (.NET 9.0) com o front-end padrão Weather, expandido com:
- **API REST** em `/api/weatherforecast`
- **Service Layer** com Injeção de Dependência
- **12 testes automatizados** (xUnit + Moq)
- **Dockerfile multi-stage** otimizado
- **CI/CD** via GitHub Actions (3 jobs)
- **Deploy automático** no Render.com

---

## 2. Estrutura do Projeto

```
aplicacao_blazor/
├── .github/workflows/ci-cd.yml     # Pipeline CI/CD
├── BlazorWeatherApp/
│   ├── Components/Pages/
│   │   ├── Counter.razor            # Página Counter
│   │   ├── Home.razor               # Página Home
│   │   └── Weather.razor            # Página Weather
│   ├── Controllers/
│   │   └── WeatherForecastController.cs
│   ├── Models/
│   │   └── WeatherForecast.cs
│   ├── Services/
│   │   ├── IWeatherForecastService.cs
│   │   └── WeatherForecastService.cs
│   └── Program.cs
├── BlazorWeatherApp.Tests/
│   ├── WeatherForecastControllerTests.cs
│   └── WeatherForecastServiceTests.cs
├── Dockerfile
├── global.json                       # Pina SDK 9.0
└── BlazorWeatherApp.sln
```

---

## 3. Serviços e API Controller

### Modelo (`WeatherForecast.cs`)
- `Date`, `TemperatureC`, `Summary`
- `TemperatureF`: calculado via `32 + (int)(TemperatureC / 0.5556)`

### Interface (`IWeatherForecastService.cs`)
```csharp
Task<WeatherForecast[]> GetForecastsAsync(DateOnly startDate);
```

### Service (`WeatherForecastService.cs`)
Gera 5 previsões com temperaturas entre -20°C e 54°C.

### Controller (`WeatherForecastController.cs`)
- Endpoint: `GET /api/weatherforecast`
- Usa DI para chamar `IWeatherForecastService`

### DI no `Program.cs`
```csharp
builder.Services.AddControllers();
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddHealthChecks();
```

---

## 4. Testes Automatizados

| Framework | Versão | Função |
|-----------|--------|--------|
| xUnit | 2.9.3 | Testes |
| Moq | 4.20.72 | Mocks |

### Testes do Controller (5 testes com Moq)
| Teste | Valida |
|-------|--------|
| `Get_ReturnsOkResult_WithForecasts` | Retorna 200 OK |
| `Get_ReturnsNonEmptyList` | Lista não vazia |
| `Get_CallsServiceExactlyOnce` | Service chamado 1x |
| `Get_ReturnsCorrectForecastData` | Dados corretos |
| `Get_TemperatureF_IsCalculatedCorrectly` | Conversão C→F |

### Testes do Service (7 testes)
| Teste | Valida |
|-------|--------|
| `ReturnsExactlyFiveForecasts` | 5 previsões |
| `ReturnsNonEmptyList` | Não vazio |
| `DatesAreInFuture` | Datas futuras |
| `TemperaturesAreInValidRange` | -20 a 54°C |
| `SummariesAreNotNull` | Sem nulos |
| `SummariesAreFromValidSet` | Conjunto válido |
| `DatesAreSequential` | Datas em sequência |

```bash
dotnet test BlazorWeatherApp.Tests/ --verbosity normal
# Resultado: 12 passed, 0 failed
```

---

## 5. Docker

### Multi-stage Build
1. **Build**: SDK 9.0 → restaura, testa, publica
2. **Runtime**: ASP.NET 9.0 → imagem leve

### Porta Dinâmica
O `Program.cs` lê `PORT` do ambiente (Render atribui dinamicamente):
```csharp
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");
```

---

## 6. CI/CD com GitHub Actions

### 3 Jobs

1. **Build & Test**: checkout → setup .NET 9.0 → restore → build → test
2. **Docker Build**: valida o Dockerfile
3. **Deploy**: dispara Deploy Hook do Render (só na `main`)

### Secret necessário
`RENDER_DEPLOY_HOOK_URL` — URL do Deploy Hook do Render.com

---

## 7. Deploy no Render.com

1. Criar conta em [render.com](https://render.com)
2. **New Web Service** → conectar repo GitHub → Runtime: **Docker**
3. Em **Settings → Deploy Hook**, copiar URL
4. No GitHub: **Settings → Secrets → Actions** → `RENDER_DEPLOY_HOOK_URL`
5. Push na `main` aciona o pipeline completo

---

## 8. Como Executar Localmente

```bash
git clone https://github.com/PedroHaouliFaria/aplicacao_blazor.git
cd aplicacao_blazor
dotnet test BlazorWeatherApp.Tests/
dotnet run --project BlazorWeatherApp/
```

Acessar: `http://localhost:8080`

---

## 9. Diferenciais — "Indo Além"

- **Service Layer + DI** (princípios SOLID)
- **Moq** para isolamento de testes
- **12 testes abrangentes** (controller + service)
- **Dockerfile multi-stage** com testes no build
- **Health Check** em `/health`
- **CI/CD com 3 jobs** separados
- **Deploy automático** no Render.com
- **Documentação completa** em PT-BR
