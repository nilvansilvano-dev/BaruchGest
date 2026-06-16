# DECISIONS — FinanceiroAPI

Registro de decisoes estruturais do projeto. Toda mudanca de arquitetura, escopo ou contrato gera uma entrada aqui.

---

## 2026-06-11 — Documentacao inicial do projeto existente

O que mudou: PRD, ADR, SDD, ROADMAP, CONTEXT.md e ADR-0001 criados para formalizar o estado atual e o plano de evolucao do projeto.
Por que: projeto existia sem documentos, dificultando continuidade entre sessoes e planejamento das proximas fases.
Alternativa descartada: comecar do zero (codebase existente tinha estrutura valida, descartavel seria desperdicador de trabalho feito).
Impacto: proximas sessoes partem do ROADMAP — Fase 1 e o proximo passo.
Como reverter: n/a (documentacao nao altera codigo).

---

## 2026-06-11 — Hibrido EF Core (escrita) + Dapper (leitura)

O que mudou: decisao de adotar EF Core para operacoes de escrita e migrations, mantendo Dapper para queries de leitura complexas (views de resumo).
Por que: projeto iniciado com Dapper puro sem migrations — inviavel para produto de longo prazo que precisara adicionar SaldoInicial e outros campos.
Alternativa descartada: Dapper exclusivo (sem migrations), EF Core exclusivo (queries de resumo ficam mais verbosas).
Impacto: Fase 2 do Roadmap implementa essa migracao.
Como reverter: voltar para Dapper exclusivo requer reescrever WriteRepository e remover EF Core.

---

## 2026-06-11 — SaldoInicial como campo do AnoFiscal

O que mudou: saldo herdado do ano anterior sera campo `SaldoInicial` na entidade AnoFiscal, nao mais lancamento na subcategoria "Saldo anterior".
Por que: usar receita para registrar saldo anterior infla TotalReceita e distorce todos os relatorios por categoria. Ver ADR docs/adr/0001-saldo-inicial-no-ano-fiscal.md.
Alternativa descartada: manter workaround de "Saldo anterior" como receita.
Impacto: Fase 3 do Roadmap. Subcategoria "Saldo anterior" sera desativada. Migration necessaria.
Como reverter: remover campo SaldoInicial do AnoFiscal e reativar subcategoria "Saldo anterior".

---

## 2026-06-15 — Fase 4 concluida: Autenticacao JWT com perfis usuario/contador

O que mudou: entidade `Usuario` adicionada com Email, SenhaHash (BCrypt) e Perfil. JWT Bearer configurado. `AuthController` com `POST /api/auth/login` retornando token de 8h. Todos os endpoints protegidos com `[Authorize]`. Endpoints de escrita protegidos com `[Authorize(Roles = "usuario")]`. Dois usuarios de teste criados via `AuthSeeder` na inicializacao. Swagger configurado com suporte a Bearer token.
Por que: PRE-MVP exige autenticacao para isolar perfis usuario e contador antes de expor a API publicamente.
Alternativa descartada: ASP.NET Core Identity (pesado para API simples com apenas 2 perfis).
Impacto: todos os endpoints agora retornam 401 sem token; contador recebe 403 em POST/PUT/DELETE.
Como reverter: remover `[Authorize]` dos controllers, desregistrar JWT em Program.cs, remover migration AddUsuario.

---

## 2026-06-12 — Fase 3 concluida: SaldoInicial implementado e migration aplicada

O que mudou: campo `SaldoInicial` adicionado a entidade e tabela `AnoFiscal`. `ResumoService` criado para calcular `SaldoAcumulado` partindo do `SaldoInicial`. Migration `AddSaldoInicialToAnoFiscal` aplicada. `AnoFiscalController` retorna `AnoFiscalResponse` com `SaldoInicial`. `ResumoController` delegado ao `ResumoService`.
Por que: SaldoAcumulado precisava partir do saldo herdado do ano anterior, nao de zero.
Alternativa descartada: n/a (decisao ja tomada na Fase 3 do Roadmap).
Impacto: endpoint `POST /api/anos-fiscais` agora aceita `SaldoInicial`; `GET /api/resumo/{ano}` retorna `SaldoAcumulado` correto desde o mes 1.
Como reverter: `dotnet ef migrations remove`, reverter alteracoes em Entities, DTOs, Repository e remover ResumoService.

---

## 2026-06-11 — Nomenclatura canonica (GrupoReceita/CategoriaReceita, SaldoMensal)

O que mudou: termos do codigo serao alinhados ao CONTEXT.md — `CategoriaReceita` → `GrupoReceita`, `SubcategoriaReceita` → `CategoriaReceita`, `LucroLiquido` → `SaldoMensal`.
Por que: codigo usava nomenclatura assimetrica (Categoria/Subcategoria para receita vs Grupo/Categoria para despesa) divergindo do modelo mental do usuario. Ver CONTEXT.md.
Alternativa descartada: manter nomenclatura da planilha Excel original.
Impacto: Fase 1 do Roadmap. Afeta Models, DTOs, Repository, Controller, SQL e API.
Como reverter: reverter renomeacoes — sem impacto funcional, apenas nomenclatura.
