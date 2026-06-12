# PRD — FinanceiroAPI

**Data:** 2026-06-11
**Versão:** 1.0

---

## Problema

Controle financeiro feito em planilha Excel dificultava o acesso e a consulta aos dados.
O sistema substitui a planilha por uma API centralizada com acesso mais facil e estruturado.

---

## Usuarios

- Usuario comum: acesso aos proprios lancamentos e resumos
- Acesso gerencial (contador): acompanhamento e supervisao do que esta sendo lancado

---

## Menor uso com valor real (MVP)

Conseguir lancar uma receita, lancar uma despesa, e ver o resumo do mes com o saldo batendo — paridade funcional com a planilha.

---

## Escopo atual (tudo que entra no produto)

- Lancamento de receitas e despesas
- Resumo mensal e anual com saldo acumulado
- Autenticacao com dois perfis: usuario e gerencial (contador)
- Interface web ou app mobile (a definir)
- Relatorios exportaveis em PDF ou Excel
- Importacao manual de extrato bancario (OFX, CSV ou PDF)
- Conexao automatica via API do banco (Open Finance ou similar)
- Suporte a multiplos usuarios e/ou empresas

---

## Fora do escopo (agora)

- Integracao bancaria (manual e via API) — planejada, implementar em fase futura
- Interface web / app mobile — planejada, implementar em fase futura

---

## Restricoes conhecidas

- Stack atual: C#/.NET 8, SQL Server, Dapper
- Sem restricoes de plataforma, custo ou infraestrutura definidas ate o momento

---

## Dependencias Externas

**Extrato bancario (fase futura):**
- Modo manual: usuario faz upload de arquivo (OFX, CSV, PDF) gerado pelo banco
- Modo automatico: conexao via API do banco (Open Finance ou API proprietaria)
- O que vem do banco: dados de transacoes (data, valor, descricao, tipo)
- O que o sistema controla: importacao, classificacao e vinculacao ao lancamento correto
- Status: nao implementado — entra no roadmap como fase POS-MVP

---

## MVP Gate

Criterios observaveis para considerar que o sistema "ja funciona":

1. Consigo lancar uma receita e ela aparece no resumo do mes com valor correto
2. Consigo lancar uma despesa e ela aparece no resumo do mes com valor correto
3. O saldo do mes (receitas - despesas) bate com o esperado
4. Um usuario gerencial (contador) consegue visualizar os mesmos dados com login separado
