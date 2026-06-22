import { createContext, useContext, useState, useCallback } from 'react';

const ContadorContext = createContext();

export function ContadorProvider({ children }) {
  const [viewingUser, setViewingUser] = useState(() => {
    const stored = localStorage.getItem('viewingUser');
    return stored ? JSON.parse(stored) : null;
  });

  const selectUser = useCallback((user) => {
    localStorage.setItem('viewingUser', JSON.stringify(user));
    setViewingUser(user);
  }, []);

  const clearUser = useCallback(() => {
    localStorage.removeItem('viewingUser');
    setViewingUser(null);
  }, []);

  return (
    <ContadorContext.Provider value={{ viewingUser, selectUser, clearUser }}>
      {children}
    </ContadorContext.Provider>
  );
}

export const useContador = () => useContext(ContadorContext);
