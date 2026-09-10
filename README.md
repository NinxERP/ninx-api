<div align="center">

# 🧾 ninx-api

**Backend do sistema Ninx — API multi-tenant de gestão de comércio, estoque e vendas, com fluxo de assinatura eletrônica de documentos.**

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-SQL%20Server-CC2927?logo=microsoftsqlserver&logoColor=white)
![JWT](https://img.shields.io/badge/Auth-JWT-000000?logo=jsonwebtokens&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Dockerfile-2496ED?logo=docker&logoColor=white)

</div>

---

API de gestão de comércio, estoque e vendas do sistema **Ninx** — backend multi-tenant para
pequenos comércios (ERP/POS), com fluxo de assinatura eletrônica de documentos de venda.

## 🧩 Sobre o sistema Ninx

Este repositório é um dos cinco que compõem o Ninx:

| Repositório | Papel |
|---|---|
| **ninx-api** (este) | Backend: autenticação, regras de negócio, dados. |
| [ninx-front](../ninx-front) | Cliente desktop atual (ERP/POS), em Tauri v2 + React. |
| [ninx-signature](../ninx-signature) | Página pública onde o cliente final assina documentos de venda. |

## ⚙️ Stack

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core (SQL Server / Azure SQL)
- Autenticação JWT + autorização por `Cargo` (papel, hierarquia por peso numérico) com escopo por tenant (`Comercio`)
- Mapster (mapeamento de entidades para DTOs)
- Swashbuckle/Swagger (documentação da API, disponível em todos os ambientes)
- Brevo (envio de e-mail transacional, usado na redefinição de senha)

## 🏗️ Arquitetura

Clean Architecture, dividida em projetos sob `src/`:

- `ninx.Api` — controllers, pipeline HTTP, Swagger, DI (composição via `ninx.Ioc`).
- `ninx.Application` — serviços de negócio, mapeamentos (Mapster) e validações (FluentValidation).
- `ninx.Communication` — DTOs de request/response.
- `ninx.Domain` — entidades, enums, interfaces, exceções.
- `ninx.Infra` — repositórios (EF Core), geração de token JWT, cliente de e-mail.
- `ninx.Data` — `DbContext`, configurações de mapeamento EF e migrations.
- `ninx.Ioc` — composição de dependências (extensões `Add*`).

## 🔐 Autenticação

JWT emitido em `POST /api/Login` e reemitido por completo em
`POST /api/TrocarComercio/{comercioId}` ao trocar de tenant (não existe refresh token — ao
expirar, o cliente precisa autenticar de novo). Claims: `usuarioId`, `nome`, `email`,
`comercioId`, `cargoId`, `cargoNome`, `cargoPeso`, `nomeComercio`, `admin`. Expiração
configurável via `Jwt:ExpiresInMinutes`.

## 🚀 Como rodar localmente

### Direto com .NET

1. Pré-requisitos: .NET 10 SDK, acesso a um SQL Server.
2. Rode o comando a baixo na raiz do projeto e preencha `ConnectionStrings:DefaultConnection`, `Jwt:Secret` com os dados corretos.
   ```
   cp .env.example .env 
   ```
3. Aplique as migrations:
   ```
   dotnet ef database update --project src/ninx.Data --startup-project src/ninx.Api
   ```
4. Swagger disponível em `/swagger`.

### Via Docker

1. Rode o comando a baixo na raiz do projeto e preencha `ConnectionStrings:DefaultConnection`, `Jwt:Secret` com os dados corretos.

   ```
   cp .env.example .env 
   ```
2. ```
docker compose -f docker/docker-compose.yml --env-file .env up --build
```
Sobe a API em `http://localhost:8080`.

3. Para desenvolver com banco local:
   `docker compose -f docker/docker-compose.yml --env-file .env --profile dev up -d db`
   e troque `DB_CONNECTION_STRING` no `.env` conforme o comentário no próprio arquivo.

## 📦 Bibliotecas (NuGet)

| Pacote | Versão | Projeto(s) | Finalidade |
|---|---|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.5 | Api, Ioc | Validação de Bearer JWT nas requisições. |
| `System.IdentityModel.Tokens.Jwt` | 8.16.0 | (todos) | Emissão/leitura de tokens JWT. |
| `Microsoft.EntityFrameworkCore` + `.SqlServer` + `.Design` | 10.0.5 | Data, Api | ORM e migrations contra SQL Server/Azure SQL. |
| `Mapster` | 10.0.3 | Application | Mapeamento Entity ↔ DTO sem boilerplate manual. |
| `FluentValidation` | 12.1.1 | Application, Tests | Validação de Request DTOs (via `ValidationActionFilter`). |
| `BCrypt.Net-Next` | 4.1.0 | Application | Hash de senha e de código de redefinição de senha. |
| `itext7` + `itext7.pdfhtml` + `itext.bouncy-castle-adapter` | 9.6.0 / 6.3.3 / 9.6.0 | Application | Geração de PDF dos documentos de assinatura eletrônica. |
| `Microsoft.OpenApi` | 2.11.0 | Api | Modelos OpenAPI usados pelo Swagger. |
| `Swashbuckle.AspNetCore` + `.Annotations` | 10.1.5 | Api | Geração da UI/spec do Swagger, com anotações de resumo/tag por endpoint. |
| `Microsoft.Extensions.Http` | 10.0.5 | Ioc | `HttpClientFactory` para o client tipado do Brevo (e-mail). |
| `Microsoft.Extensions.Configuration.Abstractions` | 10.0.5 | (todos) | Contrato de configuração (`IConfiguration`) usado nas camadas internas sem depender do host web. |
| `xunit` + `xunit.runner.visualstudio` | 2.9.2 / 2.8.2 | Tests | Framework de testes. |
| `Moq` | 4.20.72 | Tests | Mocks de repositórios/serviços nos testes unitários. |
| `FluentAssertions` | 6.12.1 | Tests | Assertions mais legíveis nos testes. |
| `Microsoft.AspNetCore.Mvc.Testing` | 10.0.5 | Tests | Testes de integração (`WebApplicationFactory`). |
| `Microsoft.EntityFrameworkCore.InMemory` / `.Sqlite` | 10.0.5 | Tests | Banco em memória/SQLite para os testes de integração, sem depender do SQL Server real. |

## ✅ Testes

```bash
dotnet test
```

Cobre os serviços de negócio (`ninx.Application/Services`), validadores e testes de
integração dos fluxos principais (login, venda, assinatura eletrônica). 188 testes na suíte
atual.
