import { useState, useEffect } from 'react';
import { api } from '../services/api';
import './Resumo.css';

const MESES = ['', 'Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];

function fmt(valor) {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

export default function Resumo() {
  const [anosFiscais, setAnosFiscais] = useState([]);
  const [anoFiscalId, setAnoFiscalId] = useState(null);
  const [resumo, setResumo] = useState(null);
  const [loading, setLoading] = useState(false);
  const [erro, setErro] = useState('');
  const [mesSelecionado, setMesSelecionado] = useState(null);

  useEffect(() => {
    api.getAnosFiscais().then(data => {
      setAnosFiscais(data || []);
      if (data?.length) setAnoFiscalId(data[0].id);
    });
  }, []);

  useEffect(() => {
    if (!anoFiscalId) return;
    const af = anosFiscais.find(a => a.id === anoFiscalId);
    if (!af) return;
    setLoading(true);
    setErro('');
    api.getResumoAnual(af.ano)
      .then(setResumo)
      .catch(() => setErro('Erro ao carregar resumo.'))
      .finally(() => setLoading(false));
  }, [anoFiscalId, anosFiscais]);

  const anoFiscalAtual = anosFiscais.find(a => a.id === anoFiscalId);
  const mesesFiltrados = mesSelecionado
    ? resumo?.meses?.filter(m => m.mes === mesSelecionado)
    : resumo?.meses?.filter(m => m.totalReceita > 0 || m.totalDespesa > 0);

  return (
    <div className="page">
      <div className="page-header">
        <h2>Resumo</h2>
        <select
          value={anoFiscalId || ''}
          onChange={e => { setAnoFiscalId(Number(e.target.value)); setMesSelecionado(null); }}
          className="select-ano"
        >
          {anosFiscais.map(af => (
            <option key={af.id} value={af.id}>{af.ano} {af.descricao ? `— ${af.descricao}` : ''}</option>
          ))}
        </select>
      </div>

      {erro && <div className="alert-erro">{erro}</div>}
      {loading && <div className="loading">Carregando...</div>}

      {resumo && !loading && (
        <>
          <div className="cards">
            <div className="card card-receita">
              <span className="card-label">Total Receitas</span>
              <span className="card-valor">{fmt(resumo.totalReceita)}</span>
            </div>
            <div className="card card-despesa">
              <span className="card-label">Total Despesas</span>
              <span className="card-valor">{fmt(resumo.totalDespesa)}</span>
            </div>
            <div className={`card ${resumo.saldoAnual >= 0 ? 'card-positivo' : 'card-negativo'}`}>
              <span className="card-label">Saldo do Ano</span>
              <span className="card-valor">{fmt(resumo.saldoAnual)}</span>
            </div>
            {anoFiscalAtual?.saldoInicial > 0 && (
              <div className="card card-neutro">
                <span className="card-label">Saldo Inicial</span>
                <span className="card-valor">{fmt(anoFiscalAtual.saldoInicial)}</span>
              </div>
            )}
          </div>

          <div className="section-title">
            Meses
            <div className="mes-pills">
              <button
                className={`pill ${!mesSelecionado ? 'pill-active' : ''}`}
                onClick={() => setMesSelecionado(null)}
              >Todos</button>
              {resumo.meses?.filter(m => m.totalReceita > 0 || m.totalDespesa > 0).map(m => (
                <button
                  key={m.mes}
                  className={`pill ${mesSelecionado === m.mes ? 'pill-active' : ''}`}
                  onClick={() => setMesSelecionado(m.mes)}
                >{MESES[m.mes]}</button>
              ))}
            </div>
          </div>

          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Mês</th>
                  <th className="right">Receita</th>
                  <th className="right">Despesa</th>
                  <th className="right">Saldo Mensal</th>
                  <th className="right">Saldo Acumulado</th>
                </tr>
              </thead>
              <tbody>
                {mesesFiltrados?.map(m => (
                  <tr key={m.mes}>
                    <td>{m.nomeMes}</td>
                    <td className="right text-receita">{fmt(m.totalReceita)}</td>
                    <td className="right text-despesa">{fmt(m.totalDespesa)}</td>
                    <td className={`right ${m.saldoMensal >= 0 ? 'text-receita' : 'text-despesa'}`}>
                      {fmt(m.saldoMensal)}
                    </td>
                    <td className={`right fw-bold ${m.saldoAcumulado >= 0 ? 'text-receita' : 'text-despesa'}`}>
                      {fmt(m.saldoAcumulado)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {resumo.despesasPorGrupo?.length > 0 && (
            <>
              <div className="section-title">Despesas por Grupo</div>
              <div className="grupos-wrap">
                {resumo.despesasPorGrupo.map(g => (
                  <div key={g.grupo} className="grupo-row">
                    <div className="grupo-info">
                      <span className="grupo-nome">{g.grupo}</span>
                      <span className="grupo-pct">{g.percentualSobreTotal}%</span>
                    </div>
                    <div className="grupo-bar-bg">
                      <div className="grupo-bar" style={{ width: `${g.percentualSobreTotal}%` }} />
                    </div>
                    <span className="grupo-total">{fmt(g.total)}</span>
                  </div>
                ))}
              </div>
            </>
          )}
        </>
      )}

      {!loading && anosFiscais.length === 0 && (
        <div className="empty-state">
          Nenhum ano fiscal cadastrado ainda. Crie um em Anos Fiscais.
        </div>
      )}
    </div>
  );
}
