import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
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
          <span className="logo-icon">₢</span>
          <span className="logo-text">Financeiro</span>
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
        </nav>

        <div className="sidebar-footer">
          <div className="user-info">
            <span className="user-perfil">{user?.perfil}</span>
          </div>
          <button className="btn-logout" onClick={handleLogout}>Sair</button>
        </div>
      </aside>

      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}
