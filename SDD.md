# SDD — FinanceiroAPI

**Data:** 2026-06-11
**Metodologia:** Hibrido — SDD dominante + TDD para integracoes externas
**Baseado no ADR:** v1.0

---

## Estrutura de Arquivos Esperada

```
FinanceiroAPI/
├── Controllers/
│   └── FinanceiroController.cs       ← endpoints REST (existente)
├── Data/
│   ├── FinanceiroDbContext.cs         ← EF Core context (a criar)
│   └── Migrations/                   ← EF Core migrations (a criar)
├── Database/
│   └── create_database.sql           ← script legado (referencia)
├── DTOs/
│   └── FinanceiroDTOs.cs             ← request/response (existente)
├── Models/
│   └── Entities.cs                   ← entidades do dominio (existente)
├── Repositories/
│   ├── IFinanceiroRepository.cs      ← interface (extrair do .cs atual)
│   ├── WriteRepository.cs            ← EF Core — escrita (a criar)
│   └── ReadRepository.cs             ← Dapper — leitura (a criar)
├── Services/
│   └── ResumoService.cs              ← calculo de SaldoAcumulado (a criar)
├── docs/
│   └── adr/
│       └── 0001-saldo-inicial-no-ano-fiscal.md
├── Program.cs
├── appsettings.json
├── PRD.md, ADR.md, SDD.md, ROADMAP.md, DECISIONS.md, CONTEXT.md, README.md
```

---

## Modulos

### AnoFiscal

**Responsabilidade:** Container anual para todos os lancamentos. Armazena o SaldoInicial herdado do ano anterior.

**Entrada:**
```
CreateAnoFiscalRequest { Ano: int, Descricao: string?, SaldoInicial: decimal }
```

**Saida:**
```
AnoFiscalResponse { Id, Ano, Descricao, SaldoInicial, CriadoEm }
Erros: 409 Conflict se o Ano ja existir
```

**Regras que nunca podem ser violadas:**
- Um AnoFiscal nao pode ser deletado se tiver LancamentoReceita ou LancamentoDespesa vinculados
- Ano deve ser um inteiro de 4 digitos (1900–2100)
- SaldoInicial pode ser negativo (saldo devedor do ano anterior e valido)

---

### Cliente

**Responsabilidade:** Cadastro de pessoas fisicas ou juridicas vinculaveis a lancamentos de receita.

**Entrada:**
```
ClienteRequest { Nome: string, ValorMensal: decimal? }
```

**Saida:**
```
ClienteResponse { Id, Nome, ValorMensal, Ativo }
Erros: 404 se Id nao existir
```

**Regras que nunca podem ser violadas:**
- Nome nao pode ser vazio ou nulo
- ValorMensal e o valor contratado/referencia — nao garante o valor real lancado
- Inativar nao deleta — usar PATCH /clientes/{id}/ativo

---

### LancamentoReceita

**Responsabilidade:** Registro de uma entrada financeira em um mes de um AnoFiscal.

**Entrada:**
```
LancamentoReceitaRequest {
  AnoFiscalId: int,
  CategoriaReceitaId: int,   ← nome atual no codigo: SubcategoriaReceitaId
  ClienteId: int?,
  Mes: int (1–12),
  Valor: decimal,
  Observacao: string?
}
```

**Saida:**
```
LancamentoReceitaResponse {
  Id, Ano, GrupoReceita, CategoriaReceita, Cliente?, Mes, NomeMes, Valor, Observacao, LancadoEm
}
Erros: 400 se Mes < 1 ou > 12 | 404 se AnoFiscalId ou CategoriaReceitaId nao existirem
```

**Regras que nunca podem ser violadas:**
- Valor deve ser positivo
- Mes deve estar entre 1 e 12
- AnoFiscalId e CategoriaReceitaId devem existir no banco
- A subcategoria "Saldo anterior" nao deve ser usada para novos lancamentos apos implementacao do SaldoInicial no AnoFiscal

---

### LancamentoDespesa

**Responsabilidade:** Registro de uma saida financeira em um mes de um AnoFiscal.

**Entrada:**
```
LancamentoDespesaRequest {
  AnoFiscalId: int,
  CategoriaDespesaId: int,
  Mes: int (1–12),
  Valor: decimal,
  Observacao: string?
}
```

**Saida:**
```
LancamentoDespesaResponse {
  Id, Ano, GrupoDespesa, CategoriaDespesa, Mes, NomeMes, Valor, Observacao, LancadoEm
}
Erros: 400 se Mes < 1 ou > 12 | 404 se AnoFiscalId ou CategoriaDespesaId nao existirem
```

**Regras que nunca podem ser violadas:**
- Valor deve ser positivo
- Mes deve estar entre 1 e 12
- AnoFiscalId e CategoriaDespesaId devem existir no banco

---

### ResumoService

**Responsabilidade:** Calcular SaldoMensal e SaldoAcumulado para um AnoFiscal.

**Entrada:**
```
int ano
```

**Saida:**
```
ResumoAnualResponse {
  Ano, TotalReceita, TotalDespesa, SaldoAnual,
  Meses: [{ Mes, NomeMes, TotalReceita, TotalDespesa, SaldoMensal, SaldoAcumulado }],
  DespesasPorGrupo: [{ Grupo, Total, PercentualSobreTotal }]
}
```

**Regras que nunca podem ser violadas:**
- SaldoMensal = TotalReceita - TotalDespesa do mes
- SaldoAcumulado do mes 1 = SaldoInicial do AnoFiscal + SaldoMensal do mes 1
- SaldoAcumulado de cada mes seguinte = SaldoAcumulado do mes anterior + SaldoMensal do mes atual
- PercentualSobreTotal so e calculado se TotalDespesa > 0 (evitar divisao por zero)
- Os 12 meses do ano sao sempre retornados, mesmo sem lancamentos (valor zero)

---

### Autenticacao [A IMPLEMENTAR — PRE-MVP]

**Responsabilidade:** Controlar acesso a API com dois perfis: Usuario e Contador.

**Contrato esperado:**
```
POST /auth/login { Email, Senha } -> { Token: JWT, Perfil: "usuario" | "contador" }
```

**Perfis:**
- Usuario: pode criar, editar e deletar lancamentos proprios; ver resumos
- Contador: somente leitura — GET em todos os endpoints; sem POST/PUT/DELETE

**Regras que nunca podem ser violadas:**
- Todo endpoint exceto GET /auth/login requer token JWT valido
- Contador nao pode criar nem alterar lancamentos — retornar 403
- Tecnologia a definir no ADR antes de implementar (JWT + ASP.NET Identity ou solucao simples)

---

### ImportacaoExtrato [A IMPLEMENTAR — POS-MVP]

**Responsabilidade:** Importar transacoes de extrato bancario (manual ou via API) e converter para LancamentoReceita/LancamentoDespesa.

**Regras que nunca podem ser violadas:**
- Normalizar para estrutura unica antes de persistir (ver references/domain-finance-br.md)
- Nunca descartar o campo `raw` — essencial para debug de parser
- Tratar encodings: ISO-8859-1 (Bradesco, Caixa) e UTF-8
- Importacao manual (OFX/CSV/PDF) e automatica (API do banco) devem produzir a mesma estrutura normalizada
- TDD obrigatorio: cobrir parsing, normalizacao e mapeamento para lancamentos

---

## Contratos entre Modulos

| De | Para | O que passa | Formato |
|---|---|---|---|
| Controller | WriteRepository | Entidade mapeada do DTO | Objeto C# |
| Controller | ReadRepository | Parametros de filtro | Primitivos |
| Controller | ResumoService | Ano (int) | int |
| ResumoService | ReadRepository | Ano para busca de resumos | int |
| ResumoService | ReadRepository | SaldoInicial do AnoFiscal | decimal |
| ImportacaoExtrato | LancamentoReceita/Despesa | Lancamento normalizado | Entidade C# |

---

## Fora do Escopo Tecnico deste SDD

- Interface web ou mobile (definida em SDD separado quando iniciar)
- Relatorios PDF/Excel (definido em SDD separado quando iniciar)
- Notificacoes ou alertas
- Multi-tenancy (multiplas empresas isoladas)
- Historico de auditoria de alteracoes

---

## Convencoes Obrigatorias

- **Nomenclatura:** usar termos canonicos do CONTEXT.md — SaldoMensal (nunca LucroLiquido), GrupoReceita/CategoriaReceita (nunca Categoria/Subcategoria), SaldoAcumulado, SaldoInicial
- **Valores monetarios:** sempre decimal(18,2) — nunca float ou double
- **Datas:** sempre DateTime2 no SQL Server; ISO 8601 na API
- **Meses:** inteiro 1–12 — nunca string, nunca zero-based
- **Escrita:** EF Core (WriteRepository) — migrations obrigatorias para qualquer mudanca de schema
- **Leitura complexa:** Dapper (ReadRepository) — views e queries com JOIN/GROUP BY
- **Erros:** retornar ProblemDetails padrao do ASP.NET Core (400, 404, 409, 403)
- **Valores negativos:** permitidos apenas em SaldoInicial e SaldoAcumulado — Valor de lancamento sempre positivo
