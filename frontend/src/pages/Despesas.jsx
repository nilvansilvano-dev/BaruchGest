import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { useContador } from '../context/ContadorContext';
import './Lancamentos.css';

const MESES = ['', 'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'];

function fmt(valor) {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

const formVazio = { anoFiscalId: '', categoriaDespesaId: '', mes: '', valor: '', observacao: '' };

export default function Despesas() {
  const { user } = useAuth();
  const { viewingUser } = useContador();
  const navigate = useNavigate();

  const isContador = user?.perfil === 'contador';
  const viewingUserId = isContador ? viewingUser?.id : null;
  const podeEscrever = user?.perfil === 'usuario' || (isContador && viewingUser?.id === user?.id);

  const [anosFiscais, setAnosFiscais] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [lancamentos, setLancamentos] = useState([]);
  const [filtroAno, setFiltroAno] = useState('');
  const [filtroMes, setFiltroMes] = useState('');
  const [form, setForm] = useState(formVazio);
  const [mostraForm, setMostraForm] = useState(false);
  const [loading, setLoading] = useState(false);
  const [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState('');

  useEffect(() => {
    if (isContador && !viewingUserId) return;
    setAnosFiscais([]); setFiltroAno(''); setLancamentos([]);
    Promise.all([
      api.getAnosFiscais(viewingUserId),
      api.getCategoriasDespesa(viewingUserId),
    ]).then(([anos, cats]) => {
      setAnosFiscais(anos || []);
      setCategorias(cats || []);
      if (anos?.length) {
        setFiltroAno(String(anos[0].id));
        setForm(f => ({ ...f, anoFiscalId: String(anos[0].id) }));
      }
    });
  }, [viewingUserId, isContador]);

  useEffect(() => {
    if (!filtroAno) return;
    api.getLancamentosDespesa(Number(filtroAno), filtroMes ? Number(filtroMes) : undefined, viewingUserId)
      .then(data => setLancamentos(data || []));
  }, [filtroAno, filtroMes, viewingUserId]);

  if (isContador && !viewingUser) {
    return (
      <div className="page">
        <div className="empty-state">
          <p>Selecione um usuário para visualizar as despesas.</p>
          <button className="btn-primary" style={{ marginTop: 12 }} onClick={() => navigate('/usuarios')}>
            Ir para Usuários
          </button>
        </div>
      </div>
    );
  }

  function setField(field, value) {
    setForm(f => ({ ...f, [field]: value }));
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setErro(''); setSucesso('');
    setLoading(true);
    try {
      await api.createLancamentoDespesa({
        anoFiscalId:       Number(form.anoFiscalId),
        categoriaDespesaId: Number(form.categoriaDespesaId),
        mes:               Number(form.mes),
        valor:             Number(form.valor),
        observacao:        form.observacao || null,
      });
      setSucesso('Despesa lançada!');
      setForm({ ...formVazio, anoFiscalId: form.anoFiscalId });
      setMostraForm(false);
      const data = await api.getLancamentosDespesa(
        Number(filtroAno), filtroMes ? Number(filtroMes) : undefined, viewingUserId);
      setLancamentos(data || []);
    } catch (err) {
      setErro(err.message || 'Erro ao salvar.');
    } finally {
      setLoading(false);
    }
  }

  async function handleDelete(id) {
    if (!confirm('Remover este lançamento?')) return;
    try {
      await api.deleteLancamentoDespesa(id);
      setLancamentos(l => l.filter(x => x.id !== id));
    } catch {
      alert('Erro ao remover.');
    }
  }

  const totalFiltrado = lancamentos.reduce((s, l) => s + l.valor, 0);

  // Agrupar categorias por grupo para o select
  const grupos = [...new Set(categorias.map(c => c.grupo?.nome))].filter(Boolean);

  return (
    <div className="page">
      <div className="page-header">
        <h2>Despesas</h2>
        {podeEscrever && (
          <button className="btn-primary" onClick={() => setMostraForm(f => !f)}>
            {mostraForm ? 'Cancelar' : '+ Novo Lançamento'}
          </button>
        )}
      </div>

      {mostraForm && podeEscrever && (
        <div className="form-card">
          <h3>Novo Lançamento de Despesa</h3>
          {erro && <div className="alert-erro">{erro}</div>}
          <form onSubmit={handleSubmit} className="lancamento-form">
            <div className="form-row">
              <div className="field">
                <label>Ano Fiscal</label>
                <select required value={form.anoFiscalId} onChange={e => setField('anoFiscalId', e.target.value)}>
                  <option value="">Selecione...</option>
                  {anosFiscais.map(a => <option key={a.id} value={a.id}>{a.ano}</option>)}
                </select>
              </div>
              <div className="field">
                <label>Mês</label>
                <select required value={form.mes} onChange={e => setField('mes', e.target.value)}>
                  <option value="">Selecione...</option>
                  {MESES.slice(1).map((m, i) => <option key={i+1} value={i+1}>{m}</option>)}
                </select>
              </div>
            </div>
            <div className="form-row">
              <div className="field field-full">
                <label>Categoria</label>
                <select required value={form.categoriaDespesaId} onChange={e => setField('categoriaDespesaId', e.target.value)}>
                  <option value="">Selecione...</option>
                  {grupos.map(grupo => (
                    <optgroup key={grupo} label={grupo}>
                      {categorias.filter(c => c.grupo?.nome === grupo).map(c => (
                        <option key={c.id} value={c.id}>{c.nome}</option>
                      ))}
                    </optgroup>
                  ))}
                </select>
              </div>
            </div>
            <div className="form-row">
              <div className="field">
                <label>Valor (R$)</label>
                <input
                  type="number" step="0.01" min="0.01" required
                  value={form.valor} onChange={e => setField('valor', e.target.value)}
                  placeholder="0,00"
                />
              </div>
              <div className="field">
                <label>Observação</label>
                <input
                  type="text" value={form.observacao}
                  onChange={e => setField('observacao', e.target.value)}
                  placeholder="Opcional"
                />
              </div>
            </div>
            <div className="form-actions">
              <button type="submit" className="btn-primary" disabled={loading}>
                {loading ? 'Salvando...' : 'Salvar'}
              </button>
            </div>
          </form>
        </div>
      )}

      {sucesso && <div className="alert-sucesso">{sucesso}</div>}

      <div className="filtros">
        <select value={filtroAno} onChange={e => setFiltroAno(e.target.value)} className="select-sm">
          {anosFiscais.map(a => <option key={a.id} value={a.id}>{a.ano}</option>)}
        </select>
        <select value={filtroMes} onChange={e => setFiltroMes(e.target.value)} className="select-sm">
          <option value="">Todos os meses</option>
          {MESES.slice(1).map((m, i) => <option key={i+1} value={i+1}>{m}</option>)}
        </select>
        {lancamentos.length > 0 && (
          <span className="total-label">Total: <strong className="text-despesa">{fmt(totalFiltrado)}</strong></span>
        )}
      </div>

      <div className="table-wrap">
        {lancamentos.length === 0 ? (
          <div className="empty">Nenhum lançamento encontrado.</div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Mês</th>
                <th>Grupo / Categoria</th>
                <th className="right">Valor</th>
                <th>Observação</th>
                {podeEscrever && <th></th>}
              </tr>
            </thead>
            <tbody>
              {lancamentos.map(l => (
                <tr key={l.id}>
                  <td>{l.nomeMes}</td>
                  <td><span className="tag">{l.grupo}</span> {l.categoria}</td>
                  <td className="right text-despesa fw-bold">{fmt(l.valor)}</td>
                  <td>{l.observacao || <span className="text-muted">—</span>}</td>
                  {podeEscrever && (
                    <td>
                      <button className="btn-del" onClick={() => handleDelete(l.id)}>✕</button>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
