# ROADMAP — FinanceiroAPI

**Data:** 2026-06-11
**Total de fases:** 9
**Modelo principal:** Sonnet para execucao com SDD claro | Opus para decisoes de arquitetura

---

## Como ler este roadmap

- **Criterio de conclusao** = comportamento observavel, nao tarefa tecnica
- **[AFK]** = Claude Code completa e fecha sozinho
- **[HITL]** = requer sua validacao antes de avancar
- **[PRE-MVP]** = necessario para atingir o MVP Gate
- **[MVP]** = fase cuja conclusao satisfaz o MVP Gate
- **[POS-MVP]** = evolucao apos o MVP

---

## Fases PRE-MVP

### Fase 1 [PRE-MVP][AFK][CONCLUIDA 2026-06-11] — Nomenclatura canonica

**Objetivo:** Todo o codigo usa os termos definidos no CONTEXT.md, sem divergencia entre dominio e implementacao.

**Criterio de conclusao:**
> Os campos `LucroLiquido` foram renomeados para `SaldoMensal` em todos os arquivos. As entidades `CategoriaReceita` e `SubcategoriaReceita` foram renomeadas para `GrupoReceita` e `CategoriaReceita`. O Swagger e os responses da API refletem os novos nomes.

**Tarefas para o Claude Code:**
1. Renomear `LucroLiquido` → `SaldoMensal` em Models, DTOs, Repository, Controller e SQL view
2. Renomear `CategoriaReceita` → `GrupoReceita` e `SubcategoriaReceita` → `CategoriaReceita` em Models, DTOs, Repository, Controller e tabelas SQL
3. Atualizar rotas da API se necessario (`/api/receitas/subcategorias` → `/api/receitas/categorias`)
4. Atualizar README.md para refletir novos nomes

**Modelo recomendado:** Sonnet
**Dependencia:** nenhuma

---

### Fase 2 [PRE-MVP][HITL][CONCLUIDA 2026-06-11] — Migracao para EF Core + Dapper hibrido

**Objetivo:** A camada de persistencia usa EF Core para escrita e Dapper para leitura complexa, com migrations automaticas.

**Criterio de conclusao:**
> Consigo rodar `dotnet ef migrations add Initial` e `dotnet ef database update` sem erros. Todos os endpoints de criacao e atualizacao passam pelo EF Core. Os endpoints de listagem e resumo continuam usando Dapper.

**Tarefas para o Claude Code:**
1. Adicionar pacotes: `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`
2. Criar `Data/FinanceiroDbContext.cs` com as entidades mapeadas
3. Separar `FinanceiroRepository` em `WriteRepository` (EF Core) e `ReadRepository` (Dapper)
4. Registrar ambos no `Program.cs`
5. Gerar migration inicial a partir do schema existente

**Modelo recomendado:** Opus (muda arquitetura da persistencia inteira)
**Dependencia:** Fase 1

---

### Fase 3 [PRE-MVP][HITL][CONCLUIDA 2026-06-12] — SaldoInicial no AnoFiscal

**Objetivo:** O saldo herdado do ano anterior e um campo do AnoFiscal, nao um lancamento de receita. O SaldoAcumulado parte desse valor.

**Criterio de conclusao:**
> Consigo criar um AnoFiscal com `SaldoInicial: 1500.00`. O resumo do mes 1 mostra `SaldoAcumulado = 1500.00 + SaldoMensal`. A subcategoria "Saldo anterior" esta desativada para novos lancamentos.

**Tarefas para o Claude Code:**
1. Adicionar campo `SaldoInicial decimal(18,2) DEFAULT 0` na entidade `AnoFiscal`
2. Gerar EF Core migration para adicionar coluna `SaldoInicial` na tabela `AnoFiscal`
3. Atualizar `AnoFiscalRequest` e `AnoFiscalResponse` para incluir `SaldoInicial`
4. Criar `Services/ResumoService.cs` — calcular `SaldoAcumulado` partindo de `SaldoInicial`
5. Desativar a subcategoria "Saldo anterior" no banco (`Ativo = 0`)
6. Atualizar endpoint `GET /api/resumo/{ano}` para usar `ResumoService`

**Modelo recomendado:** Opus (muda calculo central do sistema)
**Dependencia:** Fase 2

---

### Fase 4 [PRE-MVP][HITL] — Autenticacao (Usuario + Contador)

**Objetivo:** A API exige login. Usuario tem acesso completo. Contador tem somente leitura.

**Criterio de conclusao:**
> Sem token, qualquer endpoint retorna 401. Com token de Usuario, consigo criar lancamentos. Com token de Contador, recebo 403 ao tentar POST/PUT/DELETE. Consigo logar com `POST /auth/login` e receber um JWT valido.

**Tarefas para o Claude Code:**
1. Adicionar `Microsoft.AspNetCore.Authentication.JwtBearer` e `Microsoft.AspNetCore.Identity`
2. Criar entidade `Usuario` com campos: Email, SenhaHash, Perfil (usuario | contador)
3. Criar migration para tabela `Usuario`
4. Criar `AuthController` com `POST /auth/login` retornando JWT
5. Proteger todos os endpoints com `[Authorize]`
6. Proteger POST/PUT/DELETE com `[Authorize(Roles = "usuario")]`
7. Criar seed com dois usuarios de teste (um de cada perfil)
8. Atualizar `Program.cs` com configuracao JWT e CORS restritivo

**Modelo recomendado:** Opus (novo modulo, afeta todos os endpoints)
**Dependencia:** Fase 2

---

## Fase MVP

### Fase 5 [MVP][HITL] — Validacao do MVP Gate

**Objetivo:** O sistema satisfaz todos os criterios do PRD. Supera a planilha Excel.

**Criterio de conclusao (MVP Gate):**
> 1. Consigo lancar uma receita e ela aparece no resumo do mes com valor correto
> 2. Consigo lancar uma despesa e ela aparece no resumo do mes com valor correto
> 3. O SaldoMensal (receitas - despesas) bate com o esperado
> 4. Um usuario com perfil Contador consegue visualizar os mesmos dados com login separado, sem conseguir criar ou alterar lancamentos

**Tarefas para o Claude Code:**
1. Executar Validation Checklist completa (itens 1-8)
2. Verificar que nenhum lancamento usa a subcategoria "Saldo anterior" desativada
3. Confirmar que SaldoAcumulado parte do SaldoInicial do AnoFiscal
4. Confirmar que CORS esta restritivo (nao AllowAnyOrigin)
5. Atualizar README.md com estado atual do produto

**Modelo recomendado:** Sonnet
**Dependencia:** Fases 1, 2, 3, 4

**[MVP ATINGIDO ao concluir esta fase]**

---

## Fases POS-MVP

### Fase 6 [POS-MVP][HITL] — Interface web ou mobile

**Objetivo:** Existe uma interface visual para usar o sistema sem precisar do Swagger.

**Criterio de conclusao:**
> Consigo lancar uma receita, ver o resumo do mes e navegar entre meses usando a interface, sem abrir o Swagger.

**Tarefas para o Claude Code:**
1. Definir tecnologia (web: React/Blazor | mobile: MAUI/Flutter) — criar ADR antes
2. Implementar telas: login, lancamento de receita, lancamento de despesa, resumo mensal/anual
3. Integrar com a API existente

**Modelo recomendado:** Opus para decisao de tecnologia | Sonnet para implementacao
**Dependencia:** Fase 5

---

### Fase 7 [POS-MVP][AFK] — Relatorios PDF e Excel

**Objetivo:** Consigo exportar o resumo de um mes ou ano em PDF ou Excel.

**Criterio de conclusao:**
> Consigo chamar `GET /api/resumo/2025/exportar?formato=pdf` e baixar um arquivo valido com os dados do resumo.

**Tarefas para o Claude Code:**
1. Adicionar biblioteca de geracao de PDF (ex: QuestPDF) e Excel (ex: ClosedXML)
2. Criar endpoint `GET /api/resumo/{ano}/exportar?formato=pdf|excel`
3. Criar template do relatorio com resumo mensal, anual e breakdown por grupo

**Modelo recomendado:** Sonnet
**Dependencia:** Fase 5

---

### Fase 8 [POS-MVP][HITL] — Importacao manual de extrato

**Objetivo:** Consigo importar um arquivo de extrato bancario (OFX, CSV ou PDF) e os lancamentos aparecem para revisao antes de serem confirmados.

**Criterio de conclusao:**
> Consigo fazer upload de um arquivo OFX do Itau. O sistema exibe os lancamentos detectados com categoria sugerida. Confirmo os que quero importar e eles aparecem no resumo do mes.

**Tarefas para o Claude Code (TDD obrigatorio):**
1. Criar `Services/ImportacaoService.cs` com parser OFX, CSV e PDF
2. Normalizar para estrutura unica conforme `references/domain-finance-br.md`
3. Criar endpoint `POST /api/importacao/preview` (retorna lancamentos sem persistir)
4. Criar endpoint `POST /api/importacao/confirmar` (persiste lancamentos selecionados)
5. Testes: parsing de cada formato, normalizacao, mapeamento para lancamentos

**Modelo recomendado:** Opus (sistema externo, TDD obrigatorio)
**Dependencia:** Fase 5

---

### Fase 9 [POS-MVP][HITL] — Conexao automatica com API do banco

**Objetivo:** O sistema busca transacoes diretamente do banco via API, sem necessidade de upload manual.

**Criterio de conclusao:**
> Consigo autorizar o acesso ao banco uma vez. O sistema busca as transacoes dos ultimos 30 dias automaticamente e as apresenta para revisao antes de importar.

**Tarefas para o Claude Code (TDD obrigatorio):**
1. Pesquisar e definir API disponivel (Open Finance, API proprietaria do banco) — criar ADR
2. Implementar autorizacao OAuth com o banco
3. Criar servico de sincronizacao periodica
4. Reutilizar fluxo de preview/confirmar da Fase 8

**Modelo recomendado:** Opus (integracao de sistema externo nao controlado)
**Dependencia:** Fase 8

---

## Marcos de validacao

| Apos a Fase | Pergunta de validacao |
|---|---|
| 3 (SaldoInicial) | O calculo de saldo esta correto comparado com a planilha? |
| 5 (MVP) | O sistema resolve o problema que a planilha resolvia? Vale usar no dia a dia? |
| 6 (Interface) | Eu e o contador conseguimos usar sem precisar do Swagger? |
| 8 (Importacao) | A importacao de extrato economiza tempo comparado ao lancamento manual? |
