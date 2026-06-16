import { createContext, useContext, useState, useCallback } from 'react';

const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const token = localStorage.getItem('token');
    const perfil = localStorage.getItem('perfil');
    return token ? { token, perfil } : null;
  });

  const login = useCallback((token, perfil) => {
    localStorage.setItem('token', token);
    localStorage.setItem('perfil', perfil);
    setUser({ token, perfil });
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('token');
    localStorage.removeItem('perfil');
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
