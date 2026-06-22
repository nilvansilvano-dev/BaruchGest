import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ContadorProvider } from './context/ContadorContext';
import { ThemeProvider } from './context/ThemeContext';
import Layout from './components/Layout';
import Login from './pages/Login';
import Resumo from './pages/Resumo';
import Receitas from './pages/Receitas';
import Despesas from './pages/Despesas';
import Usuarios from './pages/Usuarios';
import PlanoContas from './pages/PlanoContas';

function PrivateRoute({ children }) {
  const { user } = useAuth();
  return user ? children : <Navigate to="/login" replace />;
}

export default function App() {
  return (
    <BrowserRouter>
      <ThemeProvider>
        <AuthProvider>
          <ContadorProvider>
            <Routes>
              <Route path="/login" element={<Login />} />
              <Route
                path="/"
                element={
                  <PrivateRoute>
                    <Layout />
                  </PrivateRoute>
                }
              >
                <Route index element={<Resumo />} />
                <Route path="receitas" element={<Receitas />} />
                <Route path="despesas" element={<Despesas />} />
                <Route path="usuarios" element={<Usuarios />} />
                <Route path="plano-contas" element={<PlanoContas />} />
              </Route>
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </ContadorProvider>
        </AuthProvider>
      </ThemeProvider>
    </BrowserRouter>
  );
}
