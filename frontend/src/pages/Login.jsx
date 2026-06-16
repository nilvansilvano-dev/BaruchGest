import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import { useAuth } from '../context/AuthContext';
import LogoBaruchGest from '../assets/LogoBaruchGest';
import './Login.css';

export default function Login() {
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [erro, setErro] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  async function handleSubmit(e) {
    e.preventDefault();
    setErro('');
    setLoading(true);
    try {
      const data = await api.login(email, senha);
      login(data.token, data.perfil);
      navigate('/');
    } catch {
      setErro('Email ou senha incorretos.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="login-header">
          <div className="login-logo-wrap">
            <LogoBaruchGest size={52} />
          </div>
          <h1 className="login-brand">
            <span className="login-brand-baruch">Baruch</span>
            <span className="login-brand-gest">Gest</span>
          </h1>
          <p className="login-slogan">Transformando controle em crescimento.</p>
        </div>

        <form onSubmit={handleSubmit} className="login-form">
          <div className="field">
            <label>Email</label>
            <input
              type="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              placeholder="seu@email.com"
              required
              autoFocus
            />
          </div>

          <div className="field">
            <label>Senha</label>
            <input
              type="password"
              value={senha}
              onChange={e => setSenha(e.target.value)}
              placeholder="••••••••"
              required
            />
          </div>

          {erro && <div className="login-erro">{erro}</div>}

          <button type="submit" className="btn-entrar" disabled={loading}>
            {loading ? 'Entrando...' : 'Entrar'}
          </button>
        </form>
      </div>
    </div>
  );
}
