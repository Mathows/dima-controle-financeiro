# Dima — Controle Financeiro

Aplicação fullstack de controle financeiro pessoal. Permite ao usuário cadastrar categorias, registrar entradas e saídas, visualizar um dashboard com resumo financeiro, gráficos por mês e por categoria, além de um fluxo Premium integrado com **Stripe**.

- 🌐 **Web** (Blazor WebAssembly) — em produção no Azure Static Web Apps
- 🔌 **API REST** (.NET 8) — em produção no Azure App Service
- 📱 **Mobile** (.NET MAUI Blazor Hybrid) — projeto incluído como exercício arquitetural, reaproveita 100% do `Dima.Core`

> Projeto baseado no curso **Full Stack .NET** ministrado por [André Baltieri](https://github.com/andrebaltieri) ([balta.io](https://balta.io)).

---

## 🔗 Acesso rápido

- **Web em produção**: https://black-sand-042c6dc03.7.azurestaticapps.net
- **API em produção**: https://dima-api-matheus.azurewebsites.net

---

## 🧰 Stack

**Backend**
- .NET 8 / C# 12
- ASP.NET Core Minimal API
- Entity Framework Core 8 (Code-First com Migrations)
- ASP.NET Identity + **JWT Bearer Tokens**
- SQL Server (Azure SQL Database)
- Resend (e-mail de reset de senha)
- Stripe.net (checkout Premium)
- Swagger / OpenAPI

**Frontend Web**
- Blazor WebAssembly (.NET 8)
- MudBlazor 6.x (Material Design)
- PWA com Service Worker
- Localização pt-BR

**Mobile**
- .NET MAUI Blazor Hybrid (.NET 9)
- MudBlazor 8.x
- Microsoft.Maui.Storage.SecureStorage (tokens JWT criptografados pelo OS)
- Multi-plataforma: Android, iOS, MacCatalyst, Windows
- Reaproveita 100% do `Dima.Core` (DTOs, interfaces, enums)

**Infraestrutura**
- Azure SQL Database (Free Offer — 32 GB, R$ 0/mês)
- Azure App Service (Free Tier F1)
- Azure Static Web Apps (Free)
- GitHub Actions (CI/CD automático)

**Custo total da infra: R$ 0/mês** 🚀

---

## ✨ Funcionalidades

### Web e Mobile
- 🔐 Cadastro, login, logout (autenticação JWT)
- 🔑 "Esqueci minha senha" com e-mail real (Resend)
- 👤 Página de Conta: editar nome/sobrenome, trocar senha
- 🏷️ CRUD de categorias com **tipo** (Entrada/Saída) — escolhe a categoria e o tipo do lançamento vem junto
- 💸 CRUD de transações com filtro por mês/ano
- 📊 Dashboard com:
  - Resumo financeiro do mês selecionado (saldo, entradas, saídas)
  - Lista de lançamentos do período
  - Gráfico de barras: receitas vs despesas (12 meses)
  - Gráfico de pizza: entradas por categoria
  - Gráfico de pizza: despesas por categoria
- 🌗 Tema claro/escuro automático (segue preferência do SO)

### Apenas Web
- 💳 Fluxo Premium integrado com **Stripe Checkout**

---

## 🏗️ Arquitetura

Solução dividida em **quatro projetos**:

```
src/
├── Dima.Core/    ← Contratos: models, requests, responses, interfaces (compartilhado)
├── Dima.Api/     ← Backend: endpoints, handlers, EF Core, Identity, Stripe
├── Dima.Web/     ← Frontend Blazor WebAssembly
└── Dima.Mobile/  ← App MAUI Blazor Hybrid (Android, iOS, Windows, macOS)
```

`Dima.Core` é **referenciado pelos três** (`Api`, `Web`, `Mobile`). Tanto a API quanto Web e Mobile implementam as **mesmas interfaces `IHandler`** do Core — no servidor o handler conversa com o `DbContext`; no cliente (Web e Mobile), faz HTTP para a API.

### Padrão "Endpoint por arquivo" (API)
Cada endpoint é uma classe `static` implementando `IEndpoint`, registrada por reflection em `MapEndpoints()`. Rotas versionadas (`/v1/...`).

### Autenticação JWT
Tanto Web quanto Mobile usam JWT Bearer Tokens. No Web, tokens são guardados em `sessionStorage` via `IJSRuntime`. No Mobile, em `Microsoft.Maui.Storage.SecureStorage` (criptografado pelo OS, mais seguro). A migração de cookies para JWT foi necessária por causa do bloqueio de cookies cross-site em browsers modernos.

### CI/CD
Dois workflows do GitHub Actions deployam automaticamente API e Web a cada PR mergeado no `main`:
- [.github/workflows/deploy-api.yml](.github/workflows/deploy-api.yml) → Azure App Service
- [.github/workflows/deploy-web.yml](.github/workflows/deploy-web.yml) → Azure Static Web Apps

---

## 🚀 Como rodar localmente

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (para API/Web)
- [.NET 9 SDK + MAUI workload](https://dotnet.microsoft.com/download/dotnet/9.0) (para Mobile)
- SQL Server (LocalDB, Docker ou Azure SQL)

### 1. Clone o repositório
```bash
git clone https://github.com/Mathows/dima-controle-financeiro.git
cd dima-controle-financeiro
```

### 2. Configure a connection string (via user-secrets, fora do git)
```bash
cd src/Dima.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=dima;User Id=...;Password=...;Encrypt=True;TrustServerCertificate=False;"
```

### 3. (Opcional) Configure as integrações externas
```bash
dotnet user-secrets set "StripeApiKey" "sk_test_..."
dotnet user-secrets set "ResendApiKey" "re_..."
```

### 4. Aplique as migrations
```bash
dotnet ef database update
```

### 5. Crie as 3 views SQL no banco (apenas relatórios legados)
Execute os scripts em [src/Dima.Api/Data/Views/](src/Dima.Api/Data/Views/) via `sqlcmd`, Azure Data Studio ou SSMS.

> Nota: as views são opcionais. A versão atual usa LINQ direto na tabela `Transaction` para os relatórios, com filtro de mês/ano dinâmico.

### 6. Rode API e Web

**Terminal 1 — API:**
```bash
cd src/Dima.Api && dotnet run
```
API em `http://localhost:5164` (Swagger em `/swagger`).

**Terminal 2 — Web:**
```bash
cd src/Dima.Web && dotnet run
```
Web em `http://localhost:5028`.

### 7. (Opcional) Rode o Mobile

**Windows:**
```bash
cd src/Dima.Mobile
dotnet run -f net9.0-windows10.0.19041.0
```

**Android (com emulador rodando):**
```bash
dotnet run -f net9.0-android
```

---

## 📱 Versão mobile (.NET MAUI Blazor Hybrid)

O `Dima.Mobile` é um app **.NET MAUI Blazor Hybrid** que consome a mesma API publicada no Azure. Foi construído como **exercício arquitetural** para validar o reaproveitamento do `Dima.Core` entre Web (Blazor WASM) e Mobile (MAUI). Possui:

- Login + Esqueci senha
- Dashboard com seletor de período + 3 gráficos
- CRUD completo de Lançamentos e Categorias
- Página de Conta (editar perfil + trocar senha)
- Tema escuro, layout mobile-first

Tokens JWT são guardados em **SecureStorage** (criptografados pelo Android Keystore / iOS Keychain / Windows DPAPI).

> **Nota**: O foco do projeto é a versão web em produção. O Mobile está incluído como demonstração de arquitetura compartilhada (Core reaproveitado entre frontends). Para experimentar, é necessário compilar localmente — basta seguir as instruções acima.

---

## ☁️ Deploy no Azure

O projeto está em produção rodando gratuitamente no Azure:

- **Banco**: Azure SQL Database **Free Offer** (100k vCore-segundos/mês + 32 GB, AutoPause)
- **API**: Azure App Service F1 (Free) — https://dima-api-matheus.azurewebsites.net
- **Web**: Azure Static Web Apps (Free) — https://black-sand-042c6dc03.7.azurestaticapps.net

Custo total: **R$ 0/mês**.

⚠️ Tier gratuito tem cold start: primeira request após ~1h sem uso demora ~30s (Azure SQL Serverless acordando + App Service Free sem Always On).

---

## 🚀 O que foi adicionado além do curso

| Área | Curso original | Esta versão |
|---|---|---|
| **Deploy** | Localhost | Em produção no Azure (App Service + Static Web Apps + SQL Free) |
| **CI/CD** | — | GitHub Actions com deploy automático a cada PR mergeado |
| **Autenticação** | Cookies | JWT Bearer (resolve bloqueio cross-site de browsers modernos) |
| **Reset de senha** | Sem envio de e-mail | Integrado com Resend, e-mail HTML funcionando |
| **Cadastro** | E-mail + senha | + Nome e Sobrenome com claims `GivenName`/`Surname` |
| **Categoria** | Apenas título e descrição | + Tipo (Entrada/Saída) → simplifica criação de lançamentos |
| **Dashboard** | Mês corrente fixo | Seletor de mês/ano em todos os componentes |
| **Lista de lançamentos** | Não havia no dashboard | Card novo mostrando transações do período |
| **Gráfico mensal** | Line chart | Bar chart com labels formatados (1K, 2K) |
| **Plataformas** | Web only | + Projeto Mobile (.NET MAUI Blazor Hybrid) reaproveitando `Dima.Core` |
| **Documentação** | Intro do curso | README completo: arquitetura, deploy, instruções mobile |

---

## 🙏 Créditos

Projeto baseado no curso **Full Stack .NET** ministrado por [André Baltieri](https://github.com/andrebaltieri) ([balta.io](https://balta.io)).

---

## 👤 Autor

**Matheus Alexandre**
- GitHub: [@Mathows](https://github.com/Mathows)
- Portfólio: https://matheus-portfolio-da0.pages.dev
- LinkedIn: [Matheus Alexandre Marques](https://www.linkedin.com/in/matheus-alexandre-marques)

---

## 📄 Licença

Distribuído sob a licença **MIT**. Veja [LICENSE](LICENSE) para mais detalhes.
