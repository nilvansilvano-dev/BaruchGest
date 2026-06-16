# Controle Financeiro API

API REST em C#/.NET 10 para controle pessoal de receitas e despesas. Substitui planilha Excel com persistência, autenticação e cálculo automático de saldo.

**Status:** MVP concluído (Fase 5).

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server Express (`localhost\SQLEXPRESS`) ou superior

---

## Como instalar e rodar

```bash
# 1. Restaurar dependências e aplicar migrations (cria banco automaticamente)
dotnet restore
dotnet ef database update

# 2. Rodar a API
dotnet run
```

Acesse o Swagger em: `http://localhost:5000/swagger`

O banco `ControleFinanceiro` é criado automaticamente na primeira execução via EF Core migrations. Dois usuários de teste são inseridos na inicialização.

---

## Autenticação

Todos os endpoints exigem JWT. Faça login para obter o token:

```http
POST /api/auth/login
{ "email": "usuario@teste.com", "senha": "Teste@123" }
```

No Swagger: clique em **Authorize** e cole `Bearer {token}`.

| Perfil | Acesso |
|---|---|
| `usuario` | Leitura + escrita (todos os endpoints) |
| `contador` | Somente leitura (GET) |

**Usuários de teste:**

| Email | Senha | Perfil |
|---|---|---|
| `usuario@teste.com` | `Teste@123` | usuario |
| `contador@teste.com` | `Teste@123` | contador |

---

## Endpoints

### Auth
| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/auth/login` | Login — retorna JWT |

### Anos Fiscais
| Método | Rota | Acesso |
|---|---|---|
| GET | `/api/anos-fiscais` | todos |
| GET | `/api/anos-fiscais/{id}` | todos |
| POST | `/api/anos-fiscais` | usuario |

Campos do POST: `{ "ano": 2025, "descricao": "...", "saldoInicial": 1500.00 }`
O `saldoInicial` é o saldo herdado do ano anterior e afeta o `SaldoAcumulado` de todos os meses.

### Clientes
| Método | Rota | Acesso |
|---|---|---|
| GET | `/api/clientes` | todos (`?apenasAtivos=false` para incluir inativos) |
| GET | `/api/clientes/{id}` | todos |
| POST | `/api/clientes` | usuario |
| PUT | `/api/clientes/{id}` | usuario |
| PATCH | `/api/clientes/{id}/ativo?ativo=false` | usuario |

### Receitas
| Método | Rota | Acesso |
|---|---|---|
| GET | `/api/receitas/categorias` | todos |
| GET | `/api/receitas?anoFiscalId=1` | todos (`&mes=3`, `&clienteId=2` opcionais) |
| POST | `/api/receitas` | usuario |
| PUT | `/api/receitas/{id}` | usuario |
| DELETE | `/api/receitas/{id}` | usuario |

### Despesas
| Método | Rota | Acesso |
|---|---|---|
| GET | `/api/despesas/grupos` | todos |
| GET | `/api/despesas/categorias` | todos (`?grupoId=1` para filtrar) |
| GET | `/api/despesas?anoFiscalId=1` | todos (`&mes=3`, `&grupoId=2` opcionais) |
| POST | `/api/despesas` | usuario |
| PUT | `/api/despesas/{id}` | usuario |
| DELETE | `/api/despesas/{id}` | usuario |

### Resumo
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/resumo/{ano}` | Resumo anual: SaldoMensal, SaldoAcumulado, breakdown por grupo |
| GET | `/api/resumo/{ano}/{mes}` | Resumo de um mês específico |

O `SaldoAcumulado` parte do `SaldoInicial` do AnoFiscal e acumula mês a mês.

---

## Exemplo de uso

```http
# 1. Login
POST /api/auth/login
{ "email": "usuario@teste.com", "senha": "Teste@123" }

# 2. Criar ano fiscal com saldo inicial
POST /api/anos-fiscais
{ "ano": 2025, "descricao": "Exercício 2025", "saldoInicial": 1500.00 }

# 3. Lançar receita
POST /api/receitas
{ "anoFiscalId": 1, "categoriaReceitaId": 1, "clienteId": null, "mes": 1, "valor": 2000.00 }

# 4. Lançar despesa
POST /api/despesas
{ "anoFiscalId": 1, "categoriaDespesaId": 5, "mes": 1, "valor": 800.00 }

# 5. Ver resumo — SaldoMensal = 1200, SaldoAcumulado = 1500 + 1200 = 2700
GET /api/resumo/2025
```

---

## Estrutura do projeto

```
FinanceiroAPI/
├── Controllers/
│   ├── AuthController.cs           ← Login JWT
│   └── FinanceiroController.cs     ← Receita, Despesa, Resumo, AnoFiscal, Cliente
├── Data/
│   ├── FinanceiroDbContext.cs       ← EF Core context
│   └── FinanceiroDbContextFactory.cs
├── DTOs/
│   └── FinanceiroDTOs.cs
├── Migrations/                     ← EF Core migrations (cria e evolui o banco)
├── Models/
│   └── Entities.cs
├── Repositories/
│   └── FinanceiroRepository.cs     ← EF Core (escrita) + Dapper (leitura complexa)
├── Services/
│   ├── AuthSeeder.cs               ← Seed de usuários de teste
│   ├── ResumoService.cs            ← Cálculo de SaldoAcumulado
│   └── TokenService.cs             ← Geração de JWT
├── appsettings.json                ← Connection string + Jwt config
└── Program.cs
```
