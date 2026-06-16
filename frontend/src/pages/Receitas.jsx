import { useState, useEffect } from 'react';
import { api } from '../services/api';
import { useAuth } from '../context/AuthContext';
import './Lancamentos.css';

const MESES = ['', 'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'];

function fmt(valor) {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

const formVazio = { anoFiscalId: '', categoriaReceitaId: '', clienteId: '', mes: '', valor: '', observacao: '' };

export default function Receitas() {
  const { user } = useAuth();
  const podeEscrever = user?.perfil === 'usuario';

  const [anosFiscais, setAnosFiscais] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [clientes, setClientes] = useState([]);
  const [lancamentos, setLancamentos] = useState([]);
  const [filtroAno, setFiltroAno] = useState('');
  const [filtroMes, setFiltroMes] = useState('');
  const [form, setForm] = useState(formVazio);
  const [mostraForm, setMostraForm] = useState(false);
  const [loading, setLoading] = useState(false);
  const [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState('');

  useEffect(() => {
    Promise.all([
      api.getAnosFiscais(),
      api.getCategoriasReceita(),
      api.getClientes(),
    ]).then(([anos, cats, clis]) => {
      setAnosFiscais(anos || []);
      setCategorias(cats || []);
      setClientes(clis || []);
      if (anos?.length) {
        setFiltroAno(String(anos[0].id));
        setForm(f => ({ ...f, anoFiscalId: String(anos[0].id) }));
      }
    });
  }, []);

  useEffect(() => {
    if (!filtroAno) return;
    api.getLancamentosReceita(Number(filtroAno), filtroMes ? Number(filtroMes) : undefined)
      .then(data => setLancamentos(data || []));
  }, [filtroAno, filtroMes]);

  function setField(field, value) {
    setForm(f => ({ ...f, [field]: value }));
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setErro(''); setSucesso('');
    setLoading(true);
    try {
      await api.createLancamentoReceita({
        anoFiscalId:        Number(form.anoFiscalId),
        categoriaReceitaId: Number(form.categoriaReceitaId),
        clienteId:          form.clienteId ? Number(form.clienteId) : null,
        mes:                Number(form.mes),
        valor:              Number(form.valor),
        observacao:         form.observacao || null,
      });
      setSucesso('Receita lançada!');
      setForm({ ...formVazio, anoFiscalId: form.anoFiscalId });
      setMostraForm(false);
      const data = await api.getLancamentosReceita(
        Number(filtroAno), filtroMes ? Number(filtroMes) : undefined);
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
      await api.deleteLancamentoReceita(id);
      setLancamentos(l => l.filter(x => x.id !== id));
    } catch {
      alert('Erro ao remover.');
    }
  }

  const totalFiltrado = lancamentos.reduce((s, l) => s + l.valor, 0);

  return (
    <div className="page">
      <div className="page-header">
        <h2>Receitas</h2>
        {podeEscrever && (
          <button className="btn-primary" onClick={() => setMostraForm(f => !f)}>
            {mostraForm ? 'Cancelar' : '+ Novo Lançamento'}
          </button>
        )}
      </div>

      {mostraForm && podeEscrever && (
        <div className="form-card">
          <h3>Novo Lançamento de Receita</h3>
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
              <div className="field">
                <label>Categoria</label>
                <select required value={form.categoriaReceitaId} onChange={e => setField('categoriaReceitaId', e.target.value)}>
                  <option value="">Selecione...</option>
                  {categorias.map(c => <option key={c.id} value={c.id}>{c.grupo?.nome} / {c.nome}</option>)}
                </select>
              </div>
              <div className="field">
                <label>Cliente (opcional)</label>
                <select value={form.clienteId} onChange={e => setField('clienteId', e.target.value)}>
                  <option value="">Nenhum</option>
                  {clientes.map(c => <option key={c.id} value={c.id}>{c.nome}</option>)}
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
          <span className="total-label">Total: <strong className="text-receita">{fmt(totalFiltrado)}</strong></span>
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
                <th>Cliente</th>
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
                  <td>{l.cliente || <span className="text-muted">—</span>}</td>
                  <td className="right text-receita fw-bold">{fmt(l.valor)}</td>
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
