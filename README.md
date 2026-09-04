# ninx-api

API de gestão de comércio, estoque e vendas do sistema **Ninx** — backend multi-tenant para pequenos comércios (ERP/POS), com fluxo de assinatura eletrônica de documentos de venda.

## Sobre o sistema Ninx

Este repositório é um dos três que compõem o Ninx:

| Repositório | Papel |
|---|---|
| **ninx-api** (este) | Backend: autenticação, regras de negócio, dados. |
| [ninx-front](../ninx-front) | Aplicativo desktop (ERP/POS) usado pelo lojista. |
| [ninx-signatureWebPage](../ninx-signatureWebPage) | Página pública onde o cliente final assina documentos de venda. |

Fluxo de integração: o `ninx-front` cria uma venda/recibo via este backend, que gera um `AssinaturaEletronica` com um `DocumentoGuid` público. O front monta um link/QR code `{signature-page}/?guid={DocumentoGuid}`; o cliente abre esse link no `ninx-signatureWebPage`, que consome os endpoints públicos `GET /api/AssinaturaEletronica/{guid}` e `POST /api/AssinaturaEletronica/confirmar/{guid}` deste backend. O `ninx-front` então faz polling em `GET /api/AssinaturaEletronica/assinado/{guid}` para saber quando a assinatura foi concluída.

## Stack

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core (SQL Server / Azure SQL)
- Autenticação JWT + autorização por `Cargo` (papel) com escopo por tenant (`Comercio`)
- Mapster (mapeamento de entidades para DTOs)
- Swashbuckle/Swagger (documentação da API)
- Brevo (envio de e-mail transacional, usado na redefinição de senha)

## Arquitetura

Clean Architecture, dividida em projetos sob `src/`:

- `ninx.Api` — controllers, pipeline HTTP, Swagger, DI (composição via `ninx.Ioc`).
- `ninx.Application` — serviços de negócio, mapeamentos (Mapster) e validações.
- `ninx.Communication` — DTOs de request/response.
- `ninx.Domain` — entidades, enums, interfaces, exceções.
- `ninx.Infra` — repositórios (EF Core), geração de token JWT, cliente de e-mail.
- `ninx.Data` — `DbContext`, configurações de mapeamento EF e migrations.
- `ninx.Ioc` — composição de dependências (extensões `Add*`).

## Como rodar localmente

1. Pré-requisitos: .NET 10 SDK, acesso a um SQL Server (local ou Azure SQL).
2. Ajuste `src/ninx.Api/appsettings.json` conforme seu ambiente — **os valores commitados são um modelo de exemplo para permitir subir o projeto localmente, não segredos reais de produção**. Evite commitar valores próprios de ambiente (ex.: apontar para um banco local).
3. Aplique as migrations:
   ```
   dotnet ef database update --project src/ninx.Data --startup-project src/ninx.Api
   ```
4. Rode a API:
   ```
   dotnet run --project src/ninx.Api
   ```
5. O Swagger fica disponível na raiz da API (habilitado em todos os ambientes).

## Testes

```
dotnet test
```

Cobre os serviços de negócio (`ninx.Application/Services`), validadores e testes de integração dos fluxos principais (login, venda, assinatura eletrônica).

## Deploy

Build via `Dockerfile` na raiz (multi-stage, .NET 10), publicado no Render.com (porta `8080`, `ASPNETCORE_ENVIRONMENT=Production`).

## Fluxo de branches

Desenvolvimento na branch `dsv`; PR para `master`/`main` quando a funcionalidade estiver pronta (GitFlow).
