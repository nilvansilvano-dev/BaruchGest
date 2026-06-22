import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { api } from '../services/api';
import { useAuth } from '../context/AuthContext';
import LogoBaruchGest from '../assets/LogoBaruchGest';
import './Login.css';

function mascaraCpf(v) {
  return v.replace(/\D/g, '').slice(0, 11)
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d{1,2})$/, '$1-$2');
}

function mascaraCnpj(v) {
  return v.replace(/\D/g, '').slice(0, 14)
    .replace(/(\d{2})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1/$2')
    .replace(/(\d{4})(\d{1,2})$/, '$1-$2');
}

export default function Login() {
  const [searchParams] = useSearchParams();
  const conviteToken = searchParams.get('convite');

  const [modo, setModo] = useState(conviteToken ? 'cadastro' : 'login'); // 'login' | 'cadastro' | 'esqueci'
  const [conviteInfo, setConviteInfo] = useState(null); // { valido, emailConvidado }

  // Login
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');

  // Cadastro
  const [cadNome, setCadNome] = useState('');
  const [cadEmail, setCadEmail] = useState(conviteToken ? '' : ''); // será preenchido após validar convite
  const [cadTelefone, setCadTelefone] = useState('');
  const [cadTipoDoc, setCadTipoDoc] = useState('CPF');
  const [cadDocumento, setCadDocumento] = useState('');
  const [cadEndereco, setCadEndereco] = useState('');
  const [cadSenha, setCadSenha] = useState('');
  const [cadConfirma, setCadConfirma] = useState('');

  // Esqueci
  const [esqEmail, setEsqEmail] = useState('');
  const [esqSenha, setEsqSenha] = useState('');
  const [esqConfirma, setEsqConfirma] = useState('');

  const [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState('');
  const [loading, setLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  // Valida o convite ao abrir a página com ?convite=
  useEffect(() => {
    if (!conviteToken) return;
    api.validarConvite(conviteToken).then(info => {
      setConviteInfo(info);
      if (info?.valido && info.emailConvidado) {
        setCadEmail(info.emailConvidado);
      }
    }).catch(() => {});
  }, [conviteToken]);

  // Após cadastro com convite, marca como usado
  async function marcarConviteUsado() {
    if (conviteToken) {
      await api.usarConvite(conviteToken).catch(() => {});
    }
  }

  function trocarModo(novoModo) {
    setErro('');
    setSucesso('');
    setModo(novoModo);
  }

  async function handleLogin(e) {
    e.preventDefault();
    setErro('');
    setLoading(true);
    try {
      const data = await api.login(email, senha);
      login(data.token, data.perfil, data.id);
      navigate('/');
    } catch {
      setErro('Email ou senha incorretos.');
    } finally {
      setLoading(false);
    }
  }

  async function handleCadastro(e) {
    e.preventDefault();
    setErro('');
    if (cadSenha !== cadConfirma) {
      setErro('As senhas não coincidem.');
      return;
    }
    if (cadSenha.length < 6) {
      setErro('A senha deve ter no mínimo 6 caracteres.');
      return;
    }
    setLoading(true);
    try {
      await api.registro(cadNome, cadEmail, cadSenha, cadTelefone || null, cadTipoDoc, cadDocumento || null, cadEndereco || null);
      await marcarConviteUsado();
      setSucesso('Conta criada com sucesso! Faça login para continuar.');
      setCadNome(''); setCadEmail(''); setCadSenha(''); setCadConfirma('');
      setCadTelefone(''); setCadDocumento(''); setCadEndereco('');
      setTimeout(() => trocarModo('login'), 2000);
    } catch (err) {
      setErro(err.message || 'Erro ao criar conta.');
    } finally {
      setLoading(false);
    }
  }

  async function handleRedefinir(e) {
    e.preventDefault();
    setErro('');
    if (esqSenha !== esqConfirma) {
      setErro('As senhas não coincidem.');
      return;
    }
    if (esqSenha.length < 6) {
      setErro('A senha deve ter no mínimo 6 caracteres.');
      return;
    }
    setLoading(true);
    try {
      await api.redefinirSenha(esqEmail, esqSenha);
      setSucesso('Senha redefinida com sucesso! Faça login para continuar.');
      setEsqEmail(''); setEsqSenha(''); setEsqConfirma('');
      setTimeout(() => trocarModo('login'), 2000);
    } catch (err) {
      setErro(err.message || 'Email não encontrado.');
    } finally {
      setLoading(false);
    }
  }

  const titulos = {
    login:   { h2: 'Bem-vindo de volta', sub: 'Entre com suas credenciais' },
    cadastro: { h2: 'Criar conta', sub: 'Preencha os dados para se cadastrar' },
    esqueci:  { h2: 'Redefinir senha', sub: 'Informe seu email e a nova senha' },
  };

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
          <p className="login-slogan">{titulos[modo].sub}</p>
        </div>

        {/* ── LOGIN ── */}
        {modo === 'login' && (
          <form onSubmit={handleLogin} className="login-form">
            <div className="field">
              <label>Email</label>
              <input type="email" value={email} onChange={e => setEmail(e.target.value)}
                placeholder="seu@email.com" required autoFocus />
            </div>
            <div className="field">
              <label>Senha</label>
              <input type="password" value={senha} onChange={e => setSenha(e.target.value)}
                placeholder="••••••••" required />
            </div>

            {erro && <div className="login-erro">{erro}</div>}

            <button type="submit" className="btn-entrar" disabled={loading}>
              {loading ? 'Entrando...' : 'Entrar'}
            </button>

            <div className="login-links">
              <button type="button" className="login-link" onClick={() => trocarModo('esqueci')}>
                Esqueci minha senha
              </button>
              <span className="login-links-sep">·</span>
              <button type="button" className="login-link" onClick={() => trocarModo('cadastro')}>
                Criar conta
              </button>
            </div>
          </form>
        )}

        {/* ── CADASTRO ── */}
        {modo === 'cadastro' && (
          <form onSubmit={handleCadastro} className="login-form">
            {conviteToken && conviteInfo?.valido && (
              <div className="login-convite">
                Você foi convidado pelo seu contador para criar uma conta.
              </div>
            )}
            {conviteToken && conviteInfo && !conviteInfo.valido && (
              <div className="login-erro" style={{ marginBottom: 12 }}>
                {conviteInfo.motivo || 'Este convite não é válido.'}
              </div>
            )}
            <div className="field">
              <label>Nome completo / Empresa</label>
              <input type="text" value={cadNome} onChange={e => setCadNome(e.target.value)}
                placeholder="Seu nome ou razão social" required autoFocus />
            </div>
            <div className="field">
              <label>Email</label>
              <input type="email" value={cadEmail} onChange={e => setCadEmail(e.target.value)}
                placeholder="seu@email.com" required />
            </div>
            <div className="field">
              <label>Telefone</label>
              <input type="tel" value={cadTelefone} onChange={e => setCadTelefone(e.target.value)}
                placeholder="(11) 99999-9999" />
            </div>
            <div className="field" style={{ display: 'flex', gap: 8 }}>
              <div style={{ flex: '0 0 90px' }}>
                <label>Documento</label>
                <select value={cadTipoDoc} onChange={e => { setCadTipoDoc(e.target.value); setCadDocumento(''); }}
                  className="field-select">
                  <option value="CPF">CPF</option>
                  <option value="CNPJ">CNPJ</option>
                </select>
              </div>
              <div style={{ flex: 1 }}>
                <label>&nbsp;</label>
                <input type="text" value={cadDocumento}
                  onChange={e => setCadDocumento(cadTipoDoc === 'CPF' ? mascaraCpf(e.target.value) : mascaraCnpj(e.target.value))}
                  placeholder={cadTipoDoc === 'CPF' ? '000.000.000-00' : '00.000.000/0000-00'}
                  maxLength={cadTipoDoc === 'CPF' ? 14 : 18} />
              </div>
            </div>
            <div className="field">
              <label>Endereço</label>
              <input type="text" value={cadEndereco} onChange={e => setCadEndereco(e.target.value)}
                placeholder="Rua, número, cidade - UF" />
            </div>
            <div className="field">
              <label>Senha</label>
              <input type="password" value={cadSenha} onChange={e => setCadSenha(e.target.value)}
                placeholder="mínimo 6 caracteres" required />
            </div>
            <div className="field">
              <label>Confirmar senha</label>
              <input type="password" value={cadConfirma} onChange={e => setCadConfirma(e.target.value)}
                placeholder="••••••••" required />
            </div>

            {erro    && <div className="login-erro">{erro}</div>}
            {sucesso && <div className="login-sucesso">{sucesso}</div>}

            <button type="submit" className="btn-entrar" disabled={loading}>
              {loading ? 'Criando...' : 'Criar Conta'}
            </button>

            <div className="login-links">
              <button type="button" className="login-link" onClick={() => trocarModo('login')}>
                ← Voltar para o login
              </button>
            </div>
          </form>
        )}

        {/* ── ESQUECI A SENHA ── */}
        {modo === 'esqueci' && (
          <form onSubmit={handleRedefinir} className="login-form">
            <div className="field">
              <label>Email cadastrado</label>
              <input type="email" value={esqEmail} onChange={e => setEsqEmail(e.target.value)}
                placeholder="seu@email.com" required autoFocus />
            </div>
            <div className="field">
              <label>Nova senha</label>
              <input type="password" value={esqSenha} onChange={e => setEsqSenha(e.target.value)}
                placeholder="mínimo 6 caracteres" required />
            </div>
            <div className="field">
              <label>Confirmar nova senha</label>
              <input type="password" value={esqConfirma} onChange={e => setEsqConfirma(e.target.value)}
                placeholder="••••••••" required />
            </div>

            {erro    && <div className="login-erro">{erro}</div>}
            {sucesso && <div className="login-sucesso">{sucesso}</div>}

            <button type="submit" className="btn-entrar" disabled={loading}>
              {loading ? 'Redefinindo...' : 'Redefinir Senha'}
            </button>

            <div className="login-links">
              <button type="button" className="login-link" onClick={() => trocarModo('login')}>
                ← Voltar para o login
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
