# 💼 Controle Financeiro — Projeto Completo

Arquitetura completa: **API C# no Azure + Frontend React + App MAUI (Android/iOS)**

---

## Estrutura

```
FC/
├── API/           → ASP.NET Core Web API (Azure App Service)
├── Frontend/      → React Web App (Azure Static Web Apps)
└── MauiApp/       → App nativo Android/iOS (.NET MAUI)
```

---

## 1. 🗄️ Banco de dados — Azure SQL

1. Acesse o [Portal Azure](https://portal.azure.com)
2. Crie um **Azure SQL Database** (tier Basic/S0 para começar ~$5/mês)
3. Execute o script `API/Database/create_database.sql` no Query Editor do portal
4. Copie a connection string e guarde para o próximo passo

---

## 2. 🚀 API — Azure App Service

### Configurar

Edite `API/appsettings.json`:
```json
"ConnectionStrings": {
  "SqlServer": "SUA_CONNECTION_STRING_DO_AZURE_SQL"
},
"Jwt": {
  "Key": "CHAVE_SECRETA_32_CHARS_MINIMO_TROQUE_ISSO",
  "Issuer": "FinanceiroAPI",
  "Audience": "FinanceiroClients",
  "ExpiraEmHoras": 12
},
"Cors": {
  "AllowedOrigins": [
    "https://seu-app.azurestaticapps.net",
    "http://localhost:3000"
  ]
}
```

### Deploy manual
```bash
cd API
dotnet publish -c Release -o ./publish
# Faça upload da pasta publish no Azure App Service > Deploy Center
```

### Deploy automático (GitHub Actions)
1. Crie um App Service no Azure (nome: `financeiro-api`)
2. Baixe o **Publish Profile** no portal
3. Adicione como secret no GitHub: `AZURE_WEBAPP_PUBLISH_PROFILE`
4. Qualquer push na branch `main` faz deploy automático

### Credenciais padrão (troque em AuthController.cs!)
- Usuário: `admin`
- Senha: `admin123`

---

## 3. 🌐 Frontend React — Azure Static Web Apps

### Rodar localmente
```bash
cd Frontend
# Crie o arquivo .env com a URL da sua API
echo "REACT_APP_API_URL=https://financeiro-api.azurewebsites.net/api" > .env
npm install
npm start
# Acesse http://localhost:3000
```

### Deploy no Azure Static Web Apps
1. No portal Azure, crie um **Static Web App**
2. Conecte ao seu repositório GitHub
3. Build preset: **React**
4. App location: `/Frontend`
5. Output location: `build`
6. Adicione a variável de ambiente `REACT_APP_API_URL` no portal

### Funcionalidades do frontend
- 🔐 Login com JWT
- 📊 Dashboard com gráfico mensal e breakdown por grupo
- 💰 Lançamento e listagem de receitas por cliente/mês
- 💸 Lançamento e listagem de despesas por grupo/mês
- 👥 Cadastro e gerenciamento de clientes
- 📱 Responsivo — funciona no celular pelo navegador

---

## 4. 📱 App MAUI (Android/iOS)

### Pré-requisitos
- Visual Studio 2022 com workload **MAUI**
- Conta de desenvolvedor Apple (para iOS)

### Configurar
Em `MauiApp/Services/ApiService.cs`, altere a URL base:
```csharp
private const string BASE = "https://financeiro-api.azurewebsites.net/api";
```

### Instalar pacotes
```bash
cd MauiApp
dotnet add package CommunityToolkit.Mvvm
dotnet add package Microsoft.Extensions.Http
```

### Rodar no Android
```bash
dotnet build -t:Run -f net8.0-android
```

### Gerar APK para distribuição
```bash
dotnet publish -f net8.0-android -c Release
# APK gerado em: bin/Release/net8.0-android/publish/
```

---

## 📋 Endpoints da API

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/auth/login` | Login — retorna JWT |
| GET | `/api/anos-fiscais` | Lista anos |
| POST | `/api/anos-fiscais` | Cria ano fiscal |
| GET/POST | `/api/clientes` | Clientes |
| GET/POST/PUT/DELETE | `/api/receitas` | Receitas |
| GET/POST/PUT/DELETE | `/api/despesas` | Despesas |
| GET | `/api/resumo/{ano}` | Resumo anual |
| GET | `/api/resumo/{ano}/{mes}` | Resumo mensal |
| GET | `/health` | Health check Azure |

---

## 💰 Custo estimado Azure (por mês)

| Serviço | Tier | Custo aprox. |
|---------|------|-------------|
| App Service | F1 (Free) | Grátis |
| Azure SQL | Basic (2GB) | ~$5 |
| Static Web App | Free | Grátis |
| **Total** | | **~$5/mês** |

> Dica: o F1 do App Service tem limitações (sem custom domain, 60min/dia CPU). Para uso contínuo, suba para B1 (~$13/mês).
