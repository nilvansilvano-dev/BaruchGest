import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { useContador } from '../context/ContadorContext';
import {
  ComposedChart, Bar, Line, XAxis, YAxis, CartesianGrid,
  Tooltip, Legend, ResponsiveContainer
} from 'recharts';
import './Resumo.css';

const MESES = ['', 'Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];

function fmt(valor) {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

export default function Resumo() {
  const { user } = useAuth();
  const { viewingUser } = useContador();
  const navigate = useNavigate();

  const isContador = user?.perfil === 'contador';
  const viewingUserId = isContador ? viewingUser?.id : null;
  // Pode criar quando é usuario normal OU quando contador está vendo seus próprios dados
  const podeEscrever = !isContador || (isContador && viewingUser?.id === user?.id);

  const [anosFiscais, setAnosFiscais] = useState([]);
  const [anoFiscalId, setAnoFiscalId] = useState(null);
  const [resumo, setResumo] = useState(null);
  const [loading, setLoading] = useState(false);
  const [erro, setErro] = useState('');
  const [mesSelecionado, setMesSelecionado] = useState(null);

  // Criar Ano Fiscal
  const [mostraCriar, setMostraCriar] = useState(false);
  const [novoAno, setNovoAno] = useState(new Date().getFullYear());
  const [novaDesc, setNovaDesc] = useState('');
  const [novoSaldo, setNovoSaldo] = useState('');
  const [criando, setCriando] = useState(false);
  const [erroCriar, setErroCriar] = useState('');

  useEffect(() => {
    if (isContador && !viewingUserId) return;
    setAnosFiscais([]);
    setAnoFiscalId(null);
    setResumo(null);
    api.getAnosFiscais(viewingUserId).then(data => {
      setAnosFiscais(data || []);
      if (data?.length) setAnoFiscalId(data[0].id);
    });
  }, [viewingUserId, isContador]);

  useEffect(() => {
    if (!anoFiscalId) return;
    const af = anosFiscais.find(a => a.id === anoFiscalId);
    if (!af) return;
    setLoading(true);
    setErro('');
    api.getResumoAnual(af.ano, viewingUserId)
      .then(setResumo)
      .catch(() => setErro('Erro ao carregar resumo.'))
      .finally(() => setLoading(false));
  }, [anoFiscalId, anosFiscais, viewingUserId]);

  if (isContador && !viewingUser) {
    return (
      <div className="page">
        <div className="empty-state">
          <p>Selecione um usuário para visualizar o resumo.</p>
          <button className="btn-primary" style={{ marginTop: 12 }} onClick={() => navigate('/usuarios')}>
            Ir para Usuários
          </button>
        </div>
      </div>
    );
  }

  async function handleCriarAno(e) {
    e.preventDefault();
    setErroCriar('');
    setCriando(true);
    try {
      await api.createAnoFiscal({
        ano: Number(novoAno),
        descricao: novaDesc || null,
        saldoInicial: novoSaldo ? Number(novoSaldo) : 0,
      });
      setMostraCriar(false);
      setNovaDesc(''); setNovoSaldo('');
      // Recarrega a lista e seleciona o novo
      const data = await api.getAnosFiscais(viewingUserId);
      setAnosFiscais(data || []);
      if (data?.length) setAnoFiscalId(data[0].id);
    } catch (err) {
      setErroCriar(err.message || 'Erro ao criar ano fiscal.');
    } finally {
      setCriando(false);
    }
  }

  const anoFiscalAtual = anosFiscais.find(a => a.id === anoFiscalId);
  const mesesFiltrados = mesSelecionado
    ? resumo?.meses?.filter(m => m.mes === mesSelecionado)
    : resumo?.meses?.filter(m => m.totalReceita > 0 || m.totalDespesa > 0);

  return (
    <div className="page">
      <div className="page-header">
        <h2>Resumo</h2>
        <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
          {anosFiscais.length > 0 && (
            <select
              value={anoFiscalId || ''}
              onChange={e => { setAnoFiscalId(Number(e.target.value)); setMesSelecionado(null); }}
              className="select-ano"
            >
              {anosFiscais.map(af => (
                <option key={af.id} value={af.id}>{af.ano} {af.descricao ? `— ${af.descricao}` : ''}</option>
              ))}
            </select>
          )}
          {podeEscrever && (
            <button className="btn-primary" onClick={() => { setMostraCriar(f => !f); setErroCriar(''); }}>
              {mostraCriar ? 'Cancelar' : '+ Ano Fiscal'}
            </button>
          )}
        </div>
      </div>

      {mostraCriar && podeEscrever && (
        <div className="form-card" style={{ marginBottom: 20 }}>
          <h3>Novo Ano Fiscal</h3>
          {erroCriar && <div className="alert-erro">{erroCriar}</div>}
          <form onSubmit={handleCriarAno} style={{ display: 'flex', flexWrap: 'wrap', gap: 14, alignItems: 'flex-end' }}>
            <div className="field">
              <label>Ano</label>
              <input
                type="number" required min="2000" max="2100"
                value={novoAno} onChange={e => setNovoAno(e.target.value)}
                style={{ width: 90 }}
              />
            </div>
            <div className="field">
              <label>Descrição (opcional)</label>
              <input
                type="text" value={novaDesc}
                onChange={e => setNovaDesc(e.target.value)}
                placeholder="Ex: Exercício 2026"
                style={{ width: 200 }}
              />
            </div>
            <div className="field">
              <label>Saldo inicial (R$)</label>
              <input
                type="number" step="0.01" min="0" value={novoSaldo}
                onChange={e => setNovoSaldo(e.target.value)}
                placeholder="0,00"
                style={{ width: 120 }}
              />
            </div>
            <button type="submit" className="btn-primary" disabled={criando}>
              {criando ? 'Criando...' : 'Criar'}
            </button>
          </form>
        </div>
      )}

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

          {resumo.meses?.length > 0 && (() => {
            let acum = anoFiscalAtual?.saldoInicial || 0;
            const dadosGrafico = resumo.meses.map(m => {
              acum += (m.totalReceita - m.totalDespesa);
              return { ...m, saldoAcumulado: acum };
            });
            return (
              <div className="grafico-wrap">
                <div className="section-title" style={{ marginTop: 0 }}>Receitas × Despesas × Saldo</div>
                <ResponsiveContainer width="100%" height={250}>
                  <ComposedChart data={dadosGrafico} margin={{ top: 4, right: 20, left: 10, bottom: 0 }} barGap={2}>
                    <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
                    <XAxis dataKey="nomeMes" tick={{ fill: 'var(--text-muted)', fontSize: 11 }}
                      tickFormatter={v => v.substring(0, 3)} />
                    <YAxis tick={{ fill: 'var(--text-muted)', fontSize: 11 }}
                      tickFormatter={v => `R$${(v / 1000).toFixed(0)}k`} width={58} />
                    <Tooltip
                      formatter={(v, name) => [v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }), name]}
                      contentStyle={{ background: 'var(--bg-card)', border: '1px solid var(--border)', borderRadius: 8, color: 'var(--text-main)', fontSize: 12 }}
                    />
                    <Legend wrapperStyle={{ fontSize: 12, color: 'var(--text-muted)' }} />
                    <Bar dataKey="totalReceita" name="Receita" fill="#10B981" opacity={0.85} radius={[3,3,0,0]} />
                    <Bar dataKey="totalDespesa" name="Despesa" fill="#EF4444" opacity={0.85} radius={[3,3,0,0]} />
                    <Line type="monotone" dataKey="saldoAcumulado" name="Saldo Acumulado"
                      stroke="#6366F1" strokeWidth={2} dot={{ r: 3, fill: '#6366F1' }} activeDot={{ r: 5 }} />
                  </ComposedChart>
                </ResponsiveContainer>
              </div>
            );
          })()}

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
          <p>Nenhum ano fiscal cadastrado ainda.</p>
          {podeEscrever && (
            <button className="btn-primary" style={{ marginTop: 12 }}
              onClick={() => setMostraCriar(true)}>
              + Criar Ano Fiscal
            </button>
          )}
        </div>
      )}
    </div>
  );
}
