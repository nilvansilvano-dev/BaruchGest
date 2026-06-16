import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import LogoBaruchGest from '../assets/LogoBaruchGest';
import './Layout.css';

export default function Layout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate('/login');
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
          {user?.perfil === 'contador' && (
            <NavLink to="/usuarios" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>
              👤 Usuários
            </NavLink>
          )}
        </nav>

        <div className="sidebar-footer">
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
