import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import { useContador } from '../context/ContadorContext';
import './Lancamentos.css';
import './Usuarios.css';

export default function Usuarios() {
  const [usuarios, setUsuarios] = useState([]);
  const [form, setForm] = useState({ nome: '', email: '', senha: '' });
  const [mostraForm, setMostraForm] = useState(false);
  const [loading, setLoading] = useState(false);
  const [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState('');

  // Convites
  const [convites, setConvites] = useState([]);
  const [mostraConvites, setMostraConvites] = useState(false);
  const [emailConvite, setEmailConvite] = useState('');
  const [diasConvite, setDiasConvite] = useState(7);
  const [gerandoConvite, setGerandoConvite] = useState(false);
  const [linkCopiado, setLinkCopiado] = useState(null);

  const { viewingUser, selectUser } = useContador();
  const navigate = useNavigate();

  useEffect(() => { carregar(); }, []);

  async function carregar() {
    const data = await api.getUsuarios().catch(() => []);
    setUsuarios(data || []);
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setErro(''); setSucesso('');
    setLoading(true);
    try {
      await api.createUsuario(form);
      setSucesso(`Usuário ${form.email} criado com sucesso.`);
      setForm({ nome: '', email: '', senha: '' });
      setMostraForm(false);
      await carregar();
    } catch (err) {
      setErro(err.message || 'Erro ao criar usuário.');
    } finally {
      setLoading(false);
    }
  }

  async function toggleAtivo(u) {
    const acao = u.ativo ? 'desativar' : 'reativar';
    if (!confirm(`Deseja ${acao} o usuário ${u.email}?`)) return;
    try {
      await api.setUsuarioAtivo(u.id, !u.ativo);
      await carregar();
    } catch {
      alert('Erro ao atualizar status.');
    }
  }

  async function carregarConvites() {
    const data = await api.getConvites().catch(() => []);
    setConvites(data || []);
  }

  async function handleGerarConvite(e) {
    e.preventDefault();
    setGerandoConvite(true);
    try {
      const convite = await api.criarConvite({
        emailConvidado: emailConvite || null,
        diasValidade: Number(diasConvite),
      });
      setConvites(c => [convite, ...c]);
      setEmailConvite('');
      await copiarLink(convite.linkConvite, convite.id);
    } catch (err) {
      alert(err.message || 'Erro ao gerar convite.');
    } finally {
      setGerandoConvite(false);
    }
  }

  async function copiarLink(link, id) {
    try {
      await navigator.clipboard.writeText(link);
      setLinkCopiado(id);
      setTimeout(() => setLinkCopiado(null), 3000);
    } catch {
      prompt('Copie o link de convite:', link);
    }
  }

  function handleAcessar(u) {
    selectUser({ id: u.id, email: u.email, nome: u.nome });
    navigate('/');
  }

  return (
    <div className="page">
      <div className="page-header">
        <h2>Usuários</h2>
        <div style={{ display: 'flex', gap: 8 }}>
          <button className="btn-secondary"
            onClick={() => { setMostraConvites(f => !f); if (!mostraConvites) carregarConvites(); setMostraForm(false); }}>
            {mostraConvites ? 'Fechar Convites' : '🔗 Convites'}
          </button>
          <button className="btn-primary" onClick={() => { setMostraForm(f => !f); setMostraConvites(false); }}>
            {mostraForm ? 'Cancelar' : '+ Novo Usuário'}
          </button>
        </div>
      </div>

      {viewingUser && (
        <div className="alert-sucesso" style={{ marginBottom: 16 }}>
          Visualizando dados de: <strong>{viewingUser.email}</strong>
        </div>
      )}

      {mostraConvites && (
        <div className="form-card" style={{ marginBottom: 20 }}>
          <h3>Convites de Cadastro</h3>
          <p style={{ fontSize: 13, color: 'var(--text-muted)', marginBottom: 16 }}>
            Gere um link e envie ao cliente. Quando abrir o link, o formulário de cadastro já abre automaticamente.
          </p>
          <form onSubmit={handleGerarConvite}
            style={{ display: 'flex', flexWrap: 'wrap', gap: 12, alignItems: 'flex-end', marginBottom: 20 }}>
            <div className="field">
              <label>Email do cliente (opcional)</label>
              <input type="email" value={emailConvite}
                onChange={e => setEmailConvite(e.target.value)}
                placeholder="cliente@email.com" style={{ width: 220 }} />
            </div>
            <div className="field">
              <label>Validade (dias)</label>
              <input type="number" min="1" max="30" value={diasConvite}
                onChange={e => setDiasConvite(e.target.value)}
                style={{ width: 80 }} />
            </div>
            <button type="submit" className="btn-primary" disabled={gerandoConvite}>
              {gerandoConvite ? 'Gerando...' : '+ Gerar Link'}
            </button>
          </form>

          {convites.length > 0 && (
            <div className="table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>Email convidado</th>
                    <th>Criado em</th>
                    <th>Expira em</th>
                    <th>Status</th>
                    <th>Link</th>
                  </tr>
                </thead>
                <tbody>
                  {convites.map(c => (
                    <tr key={c.id}>
                      <td>{c.emailConvidado || <span className="text-muted">—</span>}</td>
                      <td>{new Date(c.criadoEm).toLocaleDateString('pt-BR')}</td>
                      <td>{new Date(c.expiraEm).toLocaleDateString('pt-BR')}</td>
                      <td>
                        {c.usado
                          ? <span className="status-badge status-inativo">Usado</span>
                          : new Date(c.expiraEm) < new Date()
                            ? <span className="status-badge status-inativo">Expirado</span>
                            : <span className="status-badge status-ativo">Válido</span>}
                      </td>
                      <td>
                        <button className="btn-acessar" style={{ fontSize: 11 }}
                          onClick={() => copiarLink(c.linkConvite, c.id)}
                          disabled={c.usado || new Date(c.expiraEm) < new Date()}>
                          {linkCopiado === c.id ? '✓ Copiado!' : 'Copiar Link'}
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
          {convites.length === 0 && (
            <div className="empty">Nenhum convite gerado ainda.</div>
          )}
        </div>
      )}

      {mostraForm && (
        <div className="form-card">
          <h3>Criar Novo Usuário</h3>
          {erro && <div className="alert-erro">{erro}</div>}
          <form onSubmit={handleSubmit} className="lancamento-form">
            <div className="form-row">
              <div className="field field-full">
                <label>Nome completo / Empresa</label>
                <input
                  type="text" required
                  value={form.nome}
                  onChange={e => setForm(f => ({ ...f, nome: e.target.value }))}
                  placeholder="Ex: João Silva ou Empresa Ltda"
                  autoFocus
                />
              </div>
            </div>
            <div className="form-row">
              <div className="field">
                <label>Email</label>
                <input
                  type="email" required
                  value={form.email}
                  onChange={e => setForm(f => ({ ...f, email: e.target.value }))}
                  placeholder="cliente@email.com"
                />
              </div>
              <div className="field">
                <label>Senha inicial</label>
                <input
                  type="password" required minLength={6}
                  value={form.senha}
                  onChange={e => setForm(f => ({ ...f, senha: e.target.value }))}
                  placeholder="Mínimo 6 caracteres"
                />
              </div>
            </div>
            <div className="usuario-perfil-info">
              O usuário será criado com perfil <strong>usuario</strong> — acesso completo aos lançamentos.
            </div>
            <div className="form-actions">
              <button type="submit" className="btn-primary" disabled={loading}>
                {loading ? 'Criando...' : 'Criar Usuário'}
              </button>
            </div>
          </form>
        </div>
      )}

      {sucesso && <div className="alert-sucesso">{sucesso}</div>}

      <div className="table-wrap">
        {usuarios.length === 0 ? (
          <div className="empty">Nenhum usuário cadastrado ainda.</div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Nome</th>
                <th>Email</th>
                <th>Perfil</th>
                <th>Status</th>
                <th>Criado em</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              {usuarios.map(u => (
                <tr key={u.id} className={viewingUser?.id === u.id ? 'row-selecionado' : ''}>
                  <td><strong>{u.nome || '—'}</strong></td>
                  <td>{u.email}</td>
                  <td><span className="tag">{u.perfil}</span></td>
                  <td>
                    <span className={`status-badge ${u.ativo ? 'status-ativo' : 'status-inativo'}`}>
                      {u.ativo ? 'Ativo' : 'Inativo'}
                    </span>
                  </td>
                  <td>{new Date(u.criadoEm).toLocaleDateString('pt-BR')}</td>
                  <td className="td-acoes">
                    <button
                      className="btn-acessar"
                      onClick={() => handleAcessar(u)}
                      title="Ver dados financeiros deste usuário"
                    >
                      {viewingUser?.id === u.id ? '✓ Acessando' : 'Acessar'}
                    </button>
                    <button
                      className={u.ativo ? 'btn-desativar' : 'btn-reativar'}
                      onClick={() => toggleAtivo(u)}
                    >
                      {u.ativo ? 'Desativar' : 'Reativar'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
