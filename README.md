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

### 🔧 Direto com .NET

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

### 🐳 Via Docker

1. Rode o comando a baixo na raiz do projeto e preencha `ConnectionStrings:DefaultConnection`, `Jwt:Secret` com os dados corretos.
   ```
   cp .env.example .env 
   ```
2. Sobe a API em `http://localhost:8080`:
   ```
   docker compose -f docker/docker-compose.yml --env-file .env up --build
   ```

3. Para desenvolver com banco local:
   ```
   docker compose -f docker/docker-compose.yml --env-file .env --profile dev up -d db
   ```

## ✅ Testes

```bash
dotnet test
```

Cobre os serviços de negócio (`ninx.Application/Services`), validadores e testes de
integração dos fluxos principais (login, venda, assinatura eletrônica). 188 testes na suíte
atual.
