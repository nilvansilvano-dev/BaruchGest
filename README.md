# Controle Financeiro API

ASP.NET Core Web API + SQL Server para controle de receitas e despesas.

---

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local ou remoto)
- Dapper (`dotnet add package Dapper`)
- Microsoft.Data.SqlClient (`dotnet add package Microsoft.Data.SqlClient`)

---

## 1. Criar o banco de dados

Execute o script `Database/create_database.sql` no SQL Server Management Studio (SSMS) ou via `sqlcmd`:

```bash
sqlcmd -S localhost -E -i Database/create_database.sql
```

Isso cria:
- O banco `ControleFinanceiro`
- Todas as tabelas (AnoFiscal, Cliente, Receita, Despesa...)
- As views de resumo
- Os dados iniciais (categorias, grupos, subcategorias)

---

## 2. Configurar a connection string

Edite `appsettings.json` com os dados do seu SQL Server:

```json
"ConnectionStrings": {
  "SqlServer": "Server=SEU_SERVIDOR;Database=ControleFinanceiro;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Para autenticação por usuário/senha:
```
Server=SEU_SERVIDOR;Database=ControleFinanceiro;User Id=seu_usuario;Password=sua_senha;TrustServerCertificate=True;
```

---

## 3. Rodar o projeto

```bash
dotnet restore
dotnet run
```

Acesse o Swagger em: `http://localhost:5000/swagger`

---

## Endpoints disponíveis

### Anos Fiscais
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/anos-fiscais` | Lista todos os anos |
| POST | `/api/anos-fiscais` | Cria um ano fiscal |

### Clientes
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/clientes` | Lista clientes (`?apenasAtivos=false` para inativos) |
| GET | `/api/clientes/{id}` | Busca cliente por ID |
| POST | `/api/clientes` | Cria cliente |
| PUT | `/api/clientes/{id}` | Atualiza cliente |
| PATCH | `/api/clientes/{id}/ativo?ativo=false` | Ativa/desativa cliente |

### Receitas
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/receitas/categorias` | Lista categorias de receita |
| GET | `/api/receitas?anoFiscalId=1` | Lista lançamentos (`&mes=3`, `&clienteId=2` opcionais) |
| POST | `/api/receitas` | Cria lançamento de receita |
| PUT | `/api/receitas/{id}` | Atualiza valor/observação |
| DELETE | `/api/receitas/{id}` | Remove lançamento |

### Despesas
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/despesas/grupos` | Lista grupos de despesa |
| GET | `/api/despesas/categorias` | Lista categorias (`?grupoId=1` para filtrar) |
| GET | `/api/despesas?anoFiscalId=1` | Lista lançamentos (`&mes=3`, `&grupoId=2` opcionais) |
| POST | `/api/despesas` | Cria lançamento de despesa |
| PUT | `/api/despesas/{id}` | Atualiza valor/observação |
| DELETE | `/api/despesas/{id}` | Remove lançamento |

### Resumo
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/resumo/{ano}` | Resumo anual com saldo acumulado e breakdown por grupo |
| GET | `/api/resumo/{ano}/{mes}` | Resumo de um mês específico |

---

## Estrutura do projeto

```
FinanceiroAPI/
├── Database/
│   └── create_database.sql     ← Execute primeiro no SQL Server
├── Models/
│   └── Entities.cs             ← Entidades (tabelas)
├── DTOs/
│   └── FinanceiroDTOs.cs       ← Request/Response da API
├── Repositories/
│   └── FinanceiroRepository.cs ← Acesso ao banco (Dapper)
├── Controllers/
│   └── FinanceiroController.cs ← Endpoints REST
├── Program.cs                  ← Inicialização
└── appsettings.json            ← Connection string
```

---

## Exemplo de uso

### 1. Criar ano fiscal
```json
POST /api/anos-fiscais
{ "ano": 2025, "descricao": "Exercício 2025" }
```

### 2. Lançar receita de cliente
```json
POST /api/receitas
{
  "anoFiscalId": 1,
  "subcategoriaReceitaId": 1,
  "clienteId": 5,
  "mes": 1,
  "valor": 1200.00,
  "observacao": null
}
```

### 3. Lançar despesa
```json
POST /api/despesas
{
  "anoFiscalId": 1,
  "categoriaDespesaId": 5,
  "mes": 1,
  "valor": 938.42,
  "observacao": "Fatura janeiro"
}
```

### 4. Ver resumo do ano
```
GET /api/resumo/2025
```
