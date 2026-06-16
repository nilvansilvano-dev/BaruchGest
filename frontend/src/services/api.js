const BASE_URL = 'http://localhost:5000';

function headers() {
  const token = localStorage.getItem('token');
  return {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

async function req(method, path, body) {
  const res = await fetch(`${BASE_URL}${path}`, {
    method,
    headers: headers(),
    ...(body !== undefined ? { body: JSON.stringify(body) } : {}),
  });

  if (res.status === 401) {
    localStorage.removeItem('token');
    localStorage.removeItem('perfil');
    window.location.href = '/login';
    return;
  }

  if (res.status === 204 || res.status === 201) {
    const text = await res.text();
    return text ? JSON.parse(text) : null;
  }

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || `Erro HTTP ${res.status}`);
  }

  return res.json();
}

export const api = {
  login: (email, senha) => req('POST', '/api/auth/login', { email, senha }),

  getAnosFiscais: () => req('GET', '/api/anos-fiscais'),
  createAnoFiscal: (data) => req('POST', '/api/anos-fiscais', data),

  getClientes: () => req('GET', '/api/clientes'),

  getCategoriasReceita: () => req('GET', '/api/receitas/categorias'),
  getLancamentosReceita: (anoFiscalId, mes) =>
    req('GET', `/api/receitas?anoFiscalId=${anoFiscalId}${mes ? `&mes=${mes}` : ''}`),
  createLancamentoReceita: (data) => req('POST', '/api/receitas', data),
  updateLancamentoReceita: (id, data) => req('PUT', `/api/receitas/${id}`, data),
  deleteLancamentoReceita: (id) => req('DELETE', `/api/receitas/${id}`),

  getGruposDespesa: () => req('GET', '/api/despesas/grupos'),
  getCategoriasDespesa: () => req('GET', '/api/despesas/categorias'),
  getLancamentosDespesa: (anoFiscalId, mes) =>
    req('GET', `/api/despesas?anoFiscalId=${anoFiscalId}${mes ? `&mes=${mes}` : ''}`),
  createLancamentoDespesa: (data) => req('POST', '/api/despesas', data),
  updateLancamentoDespesa: (id, data) => req('PUT', `/api/despesas/${id}`, data),
  deleteLancamentoDespesa: (id) => req('DELETE', `/api/despesas/${id}`),

  getResumoAnual: (ano) => req('GET', `/api/resumo/${ano}`),
};
