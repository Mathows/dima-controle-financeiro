# Dima — Controle Financeiro

Aplicação **fullstack** de controle financeiro pessoal. Permite ao usuário cadastrar categorias, registrar entradas e saídas, visualizar um dashboard com resumo financeiro, gráficos por mês e por categoria, além de um fluxo Premium integrado com **Stripe**.

> ⚠️ Este projeto foi construído a partir do curso **[Fullstack .NET](https://balta.io/cursos/fullstack-net)** do [balta.io](https://balta.io), ministrado por [André Baltieri](https://github.com/andrebaltieri). Foi adaptado, corrigido (alguns bugs do código original) e publicado em meu portfólio como exercício prático.

---

## 🧰 Stack

**Backend**
- .NET 8 / C# 12
- ASP.NET Core Minimal API
- Entity Framework Core 8 (Code-First com Migrations)
- ASP.NET Identity (autenticação via cookie)
- SQL Server (Azure SQL Database)
- Stripe.net
- Swagger / OpenAPI

**Frontend**
- Blazor WebAssembly (.NET 8)
- MudBlazor 6.x (Material Design)
- PWA com Service Worker
- Localização pt-BR

**Infraestrutura**
- Azure SQL Database (Free Offer — 32 GB, R$ 0/mês)
- Azure App Service (planejado para deploy da API)
- Azure Static Web Apps (planejado para deploy do Web)

---

## ✨ Funcionalidades

- 🔐 Cadastro e login de usuário (cookie auth)
- 📁 CRUD de categorias
- 💸 CRUD de transações (entradas e saídas) com filtro por mês/ano
- 📊 Dashboard com:
  - Resumo financeiro do mês corrente (saldo, entradas, saídas)
  - Gráfico mensal de receitas vs despesas (12 meses)
  - Gráfico de entradas por categoria
  - Gráfico de despesas por categoria
- 💳 Fluxo Premium integrado com **Stripe Checkout**
- 🌗 Tema claro/escuro automático

---

## 🏗️ Arquitetura

Solução dividida em **três projetos**:

```
src/
├── Dima.Core/   ← Contratos: models, requests, responses, interfaces de handler
├── Dima.Api/    ← Backend: endpoints, handlers, EF Core, Identity, Stripe
└── Dima.Web/    ← Frontend: páginas Razor, MudBlazor, HTTP handlers
```

Tanto a API quanto o Web implementam as **mesmas interfaces `IHandler`** do Core. No servidor, o handler conversa com o `DbContext`; no cliente, faz HTTP para a API. As páginas Blazor consomem `ICategoryHandler` etc. e não diferenciam se estão usando a rede ou banco direto.

### Padrão "Endpoint por arquivo"
Cada endpoint é uma classe `static` implementando `IEndpoint`, registrada por reflection em `MapEndpoints()`. Rotas versionadas (`/v1/...`).

### Relatórios via SQL Views
Os 3 gráficos do dashboard consomem **SQL Views** dedicadas:
- `vwGetIncomesAndExpenses` (12 meses, agrupado por mês)
- `vwGetIncomesByCategory`
- `vwGetExpensesByCategory`

Veja [src/Dima.Api/Data/Views/](src/Dima.Api/Data/Views/).

---

## 🚀 Como rodar localmente

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, Docker ou Azure SQL)
- (Opcional) Conta no [Stripe](https://stripe.com) para fluxo Premium

### 1. Clone o repositório
```bash
git clone https://github.com/mathows/dima-controle-financeiro.git
cd dima-controle-financeiro
```

### 2. Configure a connection string (via user-secrets, fora do git)
```bash
cd src/Dima.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=dima;User Id=...;Password=...;Encrypt=True;TrustServerCertificate=False;"
```

### 3. (Opcional) Configure a Stripe API Key
```bash
dotnet user-secrets set "StripeApiKey" "sk_test_..."
```

### 4. Aplique as migrations
```bash
dotnet ef database update
```

### 5. Crie as 3 views SQL no banco
Execute os scripts em [src/Dima.Api/Data/Views/](src/Dima.Api/Data/Views/) via `sqlcmd`, Azure Data Studio ou SSMS.

### 6. Rode API e Web
**Terminal 1:**
```bash
cd src/Dima.Api
dotnet run
```
API em `http://localhost:5164` (Swagger em `/swagger`).

**Terminal 2:**
```bash
cd src/Dima.Web
dotnet run
```
Web em `http://localhost:5028`.

---

## ☁️ Deploy no Azure

O projeto está preparado para rodar gratuitamente no Azure:

- **Banco**: Azure SQL Database **Free Offer** (100k vCore-segundos/mês + 32 GB, AutoPause)
- **API**: Azure App Service F1 (Free)
- **Web**: Azure Static Web Apps (Free)

Custo total: **R$ 0/mês**.

URLs de produção (após deploy):
- API: _(será preenchido após deploy)_
- Web: _(será preenchido após deploy)_

---

## 🔧 Customizações em relação ao curso original

Algumas mudanças que fiz no código do balta:

- ✅ **Bug do dashboard**: o card "Resumo Financeiro" ficava com spinner eterno quando o usuário não tinha transações no mês. Corrigido em [Home.razor.cs](src/Dima.Web/Pages/Home.razor.cs).
- ✅ **Link duplicado no menu**: havia dois itens "Categorias" em [NavMenu.razor](src/Dima.Web/Components/NavMenu.razor). O segundo apontava para uma página inexistente. Removido.
- ✅ **Deploy real no Azure** (não coberto no curso).

---

## 🙏 Créditos

Projeto baseado no curso **[Fullstack .NET](https://balta.io/cursos/fullstack-net)** do [balta.io](https://balta.io), ministrado por [André Baltieri](https://github.com/andrebaltieri) (11x Microsoft MVP).

Repositório original do curso: https://github.com/balta-io/3054

---

## 👤 Autor

**Matheus Alexandre**
- GitHub: [@mathows](https://github.com/mathows)

---

## 📄 Licença

Distribuído sob a licença **MIT**. Veja [LICENSE](LICENSE) para mais detalhes.
