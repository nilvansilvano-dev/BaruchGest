# ADR — FinanceiroAPI

**Data:** 2026-06-11
**Baseado no PRD:** v1.0

---

## Nucleo do Dominio

- [x] Dados (armazenamento, consulta de lancamentos financeiros)
- [x] Regras de negocio (calculo de SaldoMensal, SaldoAcumulado, SaldoInicial)

---

## Complexidade de Estado

- [x] **Moderada hoje** — multiplos modulos (receitas, despesas, clientes, anos fiscais) com estado compartilhado via AnoFiscal
- [ ] **Alta no horizonte** — autenticacao multi-perfil, importacao de extrato, interface web/mobile, suporte multi-usuario

---

## Ciclo de Vida Esperado

- [x] **Produto de longo prazo** — vai crescer com autenticacao, integracao bancaria, interface e API publica
- Ritmo: entrega rapida ao MVP, evolucao incremental depois

---

## Consumidor

- [x] Equipe pequena (2–3 usuarios de teste: usuario + contador + testes iniciais)
- [x] API publica no horizonte (interface web/mobile planejada)

---

## Decisao de Metodologia

**Escolha:** Hibrido — SDD + TDD

**Justificativa:**
- As regras de negocio (calculo de saldo, SaldoInicial, resumo mensal/anual) tem spec clara e sao verificaveis manualmente — dominam SDD.
- As integracoes com sistemas externos (importacao de extrato bancario, API do banco) nao sao controladas pelo sistema — TDD obrigatorio para garantir contratos e detectar mudancas de comportamento externo.

**Camada dominante:** SDD — lidera quando houver conflito entre especificar e testar primeiro.

---

## Stack Decidida

| Camada | Tecnologia | Motivo |
|---|---|---|
| Framework | ASP.NET Core (.NET 8) | Stack principal do projeto, C# |
| Banco de dados | SQL Server | Ja em uso, suporte robusto a dados financeiros |
| ORM / Persistencia | EF Core | Migrations automaticas, relacionamentos, padrao .NET longo prazo |
| Queries de leitura | Dapper | Queries complexas de resumo (views), performance em leitura |
| Autenticacao | A definir no SDD | Pendente — perfis Usuario e Contador |
| Interface | A definir | Web ou mobile — fora do MVP atual |
| Relatorios | A definir | PDF/Excel — fora do MVP atual |

---

## Decisoes Descartadas

| Opcao | Motivo da rejeicao |
|---|---|
| Dapper exclusivo | Sem suporte a migrations — adicionar SaldoInicial ao AnoFiscal exigiria script manual; inviavel para produto de longo prazo |
| EF Core exclusivo | Queries de resumo com multiplos JOINs e GROUP BY sao mais claras e performaticas em SQL puro via Dapper |
| DDD | Dominio ainda nao tem riqueza suficiente para justificar camada de dominio separada; linguagem ubiqua ja capturada no CONTEXT.md |
