import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useContador } from '../context/ContadorContext';
import { useTema } from '../context/ThemeContext';
import LogoBaruchGest from '../assets/LogoBaruchGest';
import './Layout.css';

export default function Layout() {
  const { user, logout } = useAuth();
  const { viewingUser, selectUser, clearUser } = useContador();
  const { tema, toggleTema } = useTema();
  const navigate = useNavigate();

  const isContador = user?.perfil === 'contador';

  function handleLogout() {
    clearUser();
    logout();
    navigate('/login');
  }

  function handleMeusDados() {
    selectUser({ id: user.id, email: user.email, nome: user.nome || user.email });
    navigate('/');
  }

  return (
    <div className="layout">
      <aside className="sidebar">
        <div className="sidebar-logo">
          <LogoBaruchGest size={36} />
          <div className="logo-name">
            <span><span className="logo-baruch">Baruch</span><span className="logo-gest">Gest</span></span>
            <span className="logo-slogan">Transformando controle em crescimento.</span>
          </div>
        </div>

        {isContador && (
          <div className="sidebar-viewing">
            {viewingUser ? (
              <>
                <span className="viewing-label">Visualizando</span>
                <span className="viewing-email">{viewingUser.nome || viewingUser.email}</span>
                <button className="btn-trocar" onClick={() => { clearUser(); navigate('/usuarios'); }}>
                  Trocar usuário
                </button>
              </>
            ) : (
              <span className="viewing-empty">Nenhum usuário selecionado</span>
            )}
          </div>
        )}

        <nav className="sidebar-nav">
          <NavLink to="/" end className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>
            📊 Resumo
          </NavLink>
          <NavLink to="/receitas" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>
            💰 Receitas
          </NavLink>
          <NavLink to="/despesas" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>
            💸 Despesas
          </NavLink>
          <NavLink to="/plano-contas" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>
            📋 Plano de Contas
          </NavLink>
          {isContador && (
            <>
              <button className="nav-link nav-btn" onClick={handleMeusDados}
                style={{ background: viewingUser?.id === user?.id ? '#1E293B' : 'transparent',
                         borderLeft: viewingUser?.id === user?.id ? '3px solid #10B981' : '3px solid transparent' }}>
                🗂️ Meus Dados
              </button>
              <NavLink to="/usuarios" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>
                👤 Usuários
              </NavLink>
            </>
          )}
        </nav>

        <div className="sidebar-footer">
          <button className="btn-tema" onClick={toggleTema} title="Alternar tema">
            {tema === 'light' ? '🌙 Tema Escuro' : '☀️ Tema Claro'}
          </button>
          <span className="user-perfil">{user?.perfil}</span>
          <button className="btn-logout" onClick={handleLogout}>Sair</button>
        </div>
      </aside>

      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}
