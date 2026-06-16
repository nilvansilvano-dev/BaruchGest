import { useState, useEffect } from 'react';
import { api } from '../services/api';
import './Lancamentos.css';
import './Usuarios.css';

export default function Usuarios() {
  const [usuarios, setUsuarios] = useState([]);
  const [form, setForm] = useState({ email: '', senha: '' });
  const [mostraForm, setMostraForm] = useState(false);
  const [loading, setLoading] = useState(false);
  const [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState('');

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
      setForm({ email: '', senha: '' });
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

  return (
    <div className="page">
      <div className="page-header">
        <h2>Usuários</h2>
        <button className="btn-primary" onClick={() => setMostraForm(f => !f)}>
          {mostraForm ? 'Cancelar' : '+ Novo Usuário'}
        </button>
      </div>

      {mostraForm && (
        <div className="form-card">
          <h3>Criar Novo Usuário</h3>
          {erro && <div className="alert-erro">{erro}</div>}
          <form onSubmit={handleSubmit} className="lancamento-form">
            <div className="form-row">
              <div className="field">
                <label>Email</label>
                <input
                  type="email" required
                  value={form.email}
                  onChange={e => setForm(f => ({ ...f, email: e.target.value }))}
                  placeholder="cliente@email.com"
                  autoFocus
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
                <th>Email</th>
                <th>Perfil</th>
                <th>Status</th>
                <th>Criado em</th>
                <th>Ação</th>
              </tr>
            </thead>
            <tbody>
              {usuarios.map(u => (
                <tr key={u.id}>
                  <td>{u.email}</td>
                  <td><span className="tag">{u.perfil}</span></td>
                  <td>
                    <span className={`status-badge ${u.ativo ? 'status-ativo' : 'status-inativo'}`}>
                      {u.ativo ? 'Ativo' : 'Inativo'}
                    </span>
                  </td>
                  <td>{new Date(u.criadoEm).toLocaleDateString('pt-BR')}</td>
                  <td>
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
