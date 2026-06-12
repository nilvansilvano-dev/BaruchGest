# SaldoInicial como campo do AnoFiscal, nao como lancamento de receita

O saldo herdado do ano anterior era lancado como uma SubcategoriaReceita chamada "Saldo anterior", o que inflava o TotalReceita e distorcia todos os relatorios de receita por categoria. Decidimos adicionar um campo `SaldoInicial` diretamente na entidade `AnoFiscal`. O `SaldoAcumulado` parte desse valor e nao de zero, eliminando o lancamento espurio.

## Consequencias

- A SubcategoriaReceita "Saldo anterior" deve ser removida ou desativada apos migracao dos dados existentes.
- O campo `SaldoInicial` precisa ser adicionado na tabela `AnoFiscal` (migration necessaria).
- O calculo do `SaldoAcumulado` no controller deve ser atualizado para partir do `SaldoInicial`.
