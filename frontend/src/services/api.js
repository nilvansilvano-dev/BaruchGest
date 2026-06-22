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
    localStorage.removeItem('userId');
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

// Adiciona ?usuarioId=X ao path se fornecido
function uid(usuarioId) {
  return usuarioId ? `?usuarioId=${usuarioId}` : '';
}
function uidAnd(usuarioId) {
  return usuarioId ? `&usuarioId=${usuarioId}` : '';
}

export const api = {
  login:          (email, senha)            => req('POST', '/api/auth/login',           { email, senha }),
  registro:       (nome, email, senha, telefone, tipoDocumento, documento, endereco) =>
    req('POST', '/api/auth/registro', { nome, email, senha, telefone, tipoDocumento, documento, endereco }),
  redefinirSenha: (email, novaSenha)        => req('POST', '/api/auth/redefinir-senha', { email, novaSenha }),

  getUsuarios:    ()             => req('GET', '/api/usuarios'),
  createUsuario:  (data)         => req('POST', '/api/usuarios', data),
  setUsuarioAtivo:(id, ativo)    => req('PATCH', `/api/usuarios/${id}/ativo?ativo=${ativo}`),

  getConvites:    ()             => req('GET', '/api/convites'),
  criarConvite:   (data)         => req('POST', '/api/convites', data),
  validarConvite: (token)        => req('GET', `/api/convites/validar/${token}`),
  usarConvite:    (token)        => req('POST', `/api/convites/usar/${token}`),

  getAnosFiscais: (usuarioId)    => req('GET', `/api/anos-fiscais${uid(usuarioId)}`),
  createAnoFiscal:(data)         => req('POST', '/api/anos-fiscais', data),

  getClientes:    (usuarioId)    => req('GET', `/api/clientes${uid(usuarioId)}`),
  createCliente:  (data)         => req('POST', '/api/clientes', data),
  updateCliente:  (id, data)     => req('PUT',  `/api/clientes/${id}`, data),
  setClienteAtivo:(id, ativo)    => req('PATCH', `/api/clientes/${id}/ativo?ativo=${ativo}`),

  getGruposReceita:    (usuarioId) => req('GET', `/api/receitas/grupos${uid(usuarioId)}`),
  getCategoriasReceita:(usuarioId) => req('GET', `/api/receitas/categorias${uid(usuarioId)}`),
  createGrupoReceita:  (data)      => req('POST', '/api/receitas/grupos', data),
  createCategoriaReceita:(data)    => req('POST', '/api/receitas/categorias', data),
  deleteGrupoReceita:  (id)        => req('DELETE', `/api/receitas/grupos/${id}`),
  deleteCategoriaReceita:(id)      => req('DELETE', `/api/receitas/categorias/${id}`),
  getLancamentosReceita: (anoFiscalId, mes, usuarioId) =>
    req('GET', `/api/receitas?anoFiscalId=${anoFiscalId}${mes ? `&mes=${mes}` : ''}${uidAnd(usuarioId)}`),
  createLancamentoReceita: (data)           => req('POST', '/api/receitas', data),
  updateLancamentoReceita: (id, data)       => req('PUT',  `/api/receitas/${id}`, data),
  deleteLancamentoReceita: (id)             => req('DELETE', `/api/receitas/${id}`),

  getGruposDespesa:    (usuarioId) => req('GET', `/api/despesas/grupos${uid(usuarioId)}`),
  getCategoriasDespesa:(usuarioId) => req('GET', `/api/despesas/categorias${uid(usuarioId)}`),
  createGrupoDespesa:  (data)      => req('POST', '/api/despesas/grupos', data),
  createCategoriaDespesa:(data)    => req('POST', '/api/despesas/categorias', data),
  deleteGrupoDespesa:  (id)        => req('DELETE', `/api/despesas/grupos/${id}`),
  deleteCategoriaDespesa:(id)      => req('DELETE', `/api/despesas/categorias/${id}`),
  getLancamentosDespesa: (anoFiscalId, mes, usuarioId) =>
    req('GET', `/api/despesas?anoFiscalId=${anoFiscalId}${mes ? `&mes=${mes}` : ''}${uidAnd(usuarioId)}`),
  createLancamentoDespesa: (data)           => req('POST', '/api/despesas', data),
  updateLancamentoDespesa: (id, data)       => req('PUT',  `/api/despesas/${id}`, data),
  deleteLancamentoDespesa: (id)             => req('DELETE', `/api/despesas/${id}`),

  getResumoAnual: (ano, usuarioId) =>
    req('GET', `/api/resumo/${ano}${uid(usuarioId)}`),
};
