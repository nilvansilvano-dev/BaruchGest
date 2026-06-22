import { createContext, useContext, useState, useCallback } from 'react';

const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const token  = localStorage.getItem('token');
    const perfil = localStorage.getItem('perfil');
    const id     = localStorage.getItem('userId');
    return token ? { token, perfil, id: id ? parseInt(id) : null } : null;
  });

  const login = useCallback((token, perfil, id) => {
    localStorage.setItem('token',  token);
    localStorage.setItem('perfil', perfil);
    localStorage.setItem('userId', String(id));
    setUser({ token, perfil, id });
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('token');
    localStorage.removeItem('perfil');
    localStorage.removeItem('userId');
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
