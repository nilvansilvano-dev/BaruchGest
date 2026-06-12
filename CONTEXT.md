# FinanceiroAPI

API de controle financeiro pessoal e empresarial que substitui planilha Excel, permitindo lancamento de receitas e despesas e visualizacao de resumos e saldos.

## Language

### Estrutura de classificacao

**GrupoReceita**:
Agrupamento de primeiro nivel para receitas (ex: "Entradas", "Outros").
_Avoid_: CategoriaReceita (nome atual no codigo — diverge do modelo mental)

**CategoriaReceita**:
Classificacao especifica de uma receita dentro de um GrupoReceita (ex: "Clientes", "BO Financeiro - IA", "Estagio SMN").
_Avoid_: SubcategoriaReceita (nome atual no codigo)

**GrupoDespesa**:
Agrupamento de primeiro nivel para despesas (ex: "Pessoal", "Transporte", "Empresa").
_Avoid_: categoria, tipo

**CategoriaDespesa**:
Classificacao especifica de uma despesa dentro de um GrupoDespesa (ex: "Supermercado", "Combustivel").
_Avoid_: subcategoria

### Lancamentos

**LancamentoReceita**:
Registro de uma entrada financeira em um mes e ano fiscal, vinculada a uma CategoriaReceita e opcionalmente a um Cliente.
_Avoid_: entrada, transacao de receita

**LancamentoDespesa**:
Registro de uma saida financeira em um mes e ano fiscal, vinculada a uma CategoriaDespesa.
_Avoid_: saida, transacao de despesa

### Entidades de suporte

**AnoFiscal**:
Container anual (janeiro a dezembro) ao qual todos os lancamentos pertencem. Possui um SaldoInicial que representa o saldo herdado do ano anterior.
_Avoid_: ano, exercicio

**SaldoInicial**:
Valor monetario herdado do ano fiscal anterior, registrado diretamente no AnoFiscal. Nao e um lancamento de receita.
_Avoid_: "Saldo anterior" como SubcategoriaReceita (workaround legado a ser removido)

**Cliente**:
Pessoa fisica ou juridica associada a lancamentos de receita. Possui ValorMensal como referencia do valor contratado, que pode diferir do valor real lancado.
_Avoid_: pagador, contratante

### Perfis de acesso

**Usuario**:
Perfil padrao com acesso a lancamentos e resumos financeiros proprios.
_Avoid_: user, operador

**Contador**:
Perfil gerencial com acesso de visualizacao e supervisao sobre todos os lancamentos. Nao faz lancamentos — acompanha.
_Avoid_: admin, gerente, gestor

### Calculos e resumos

**SaldoMensal**:
Resultado do mes calculado como TotalReceita - TotalDespesa. Representa o quanto sobrou (ou faltou) no mes.
_Avoid_: LucroLiquido (nome atual no codigo — conotacao empresarial inadequada para financas pessoais)

**SaldoAcumulado**:
Saldo corrente que parte do SaldoInicial do AnoFiscal e acumula o SaldoMensal de cada mes subsequente.
_Avoid_: saldo do ano, lucro acumulado

**Resumo**:
Visao agregada de um mes ou ano, contendo totais de receita, despesa, SaldoMensal, SaldoAcumulado e breakdown por GrupoDespesa.
_Avoid_: relatorio, dashboard
