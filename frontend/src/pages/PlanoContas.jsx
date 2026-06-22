import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { useContador } from '../context/ContadorContext';
import './Lancamentos.css';
import './PlanoContas.css';

export default function PlanoContas() {
  const { user } = useAuth();
  const { viewingUser } = useContador();
  const navigate = useNavigate();

  const isContador = user?.perfil === 'contador';
  const viewingUserId = isContador ? viewingUser?.id : null;
  const podeEscrever = !isContador || (isContador && viewingUser?.id === user?.id);

  const [aba, setAba] = useState('receita'); // 'receita' | 'despesa'

  // Receita
  const [gruposReceita, setGruposReceita] = useState([]);
  const [novoGrupoR, setNovoGrupoR] = useState('');
  const [novaCatR, setNovaCatR] = useState({ grupoId: '', nome: '' });
  const [adicionandoGrupoR, setAdicionandoGrupoR] = useState(false);
  const [adicionandoCatR, setAdicionandoCatR] = useState(false);
  const [erroR, setErroR] = useState('');

  // Despesa
  const [gruposDespesa, setGruposDespesa] = useState([]);
  const [novoGrupoD, setNovoGrupoD] = useState('');
  const [novaCatD, setNovaCatD] = useState({ grupoId: '', nome: '' });
  const [adicionandoGrupoD, setAdicionandoGrupoD] = useState(false);
  const [adicionandoCatD, setAdicionandoCatD] = useState(false);
  const [erroD, setErroD] = useState('');

  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isContador && !viewingUser) return;
    carregar();
  }, [viewingUserId, isContador]);

  async function carregar() {
    setLoading(true);
    try {
      const [gR, cR, gD, cD] = await Promise.all([
        api.getGruposReceita(viewingUserId),
        api.getCategoriasReceita(viewingUserId),
        api.getGruposDespesa(viewingUserId),
        api.getCategoriasDespesa(viewingUserId),
      ]);

      // Montar estrutura hierárquica para receita
      const gruposRComCats = (gR || []).map(g => ({
        ...g,
        categorias: (cR || []).filter(c => c.grupoReceitaId === g.id),
      }));
      setGruposReceita(gruposRComCats);

      // Montar estrutura hierárquica para despesa
      const gruposDComCats = (gD || []).map(g => ({
        ...g,
        categorias: (cD || []).filter(c => c.grupoDespesaId === g.id),
      }));
      setGruposDespesa(gruposDComCats);
    } finally {
      setLoading(false);
    }
  }

  // ── Receita ──
  async function handleAddGrupoR(e) {
    e.preventDefault();
    setErroR('');
    if (!novoGrupoR.trim()) return;
    try {
      await api.createGrupoReceita({ nome: novoGrupoR.trim() });
      setNovoGrupoR('');
      setAdicionandoGrupoR(false);
      await carregar();
    } catch (err) { setErroR(err.message); }
  }

  async function handleAddCatR(e) {
    e.preventDefault();
    setErroR('');
    if (!novaCatR.nome.trim() || !novaCatR.grupoId) return;
    try {
      await api.createCategoriaReceita({ grupoId: Number(novaCatR.grupoId), nome: novaCatR.nome.trim() });
      setNovaCatR({ grupoId: '', nome: '' });
      setAdicionandoCatR(false);
      await carregar();
    } catch (err) { setErroR(err.message); }
  }

  async function handleDelGrupoR(id) {
    if (!confirm('Remover este grupo? As categorias vinculadas também serão desativadas.')) return;
    await api.deleteGrupoReceita(id).catch(() => {});
    await carregar();
  }

  async function handleDelCatR(id) {
    if (!confirm('Remover esta categoria?')) return;
    await api.deleteCategoriaReceita(id).catch(() => {});
    await carregar();
  }

  // ── Despesa ──
  async function handleAddGrupoD(e) {
    e.preventDefault();
    setErroD('');
    if (!novoGrupoD.trim()) return;
    try {
      await api.createGrupoDespesa({ nome: novoGrupoD.trim() });
      setNovoGrupoD('');
      setAdicionandoGrupoD(false);
      await carregar();
    } catch (err) { setErroD(err.message); }
  }

  async function handleAddCatD(e) {
    e.preventDefault();
    setErroD('');
    if (!novaCatD.nome.trim() || !novaCatD.grupoId) return;
    try {
      await api.createCategoriaDespesa({ grupoId: Number(novaCatD.grupoId), nome: novaCatD.nome.trim() });
      setNovaCatD({ grupoId: '', nome: '' });
      setAdicionandoCatD(false);
      await carregar();
    } catch (err) { setErroD(err.message); }
  }

  async function handleDelGrupoD(id) {
    if (!confirm('Remover este grupo?')) return;
    await api.deleteGrupoDespesa(id).catch(() => {});
    await carregar();
  }

  async function handleDelCatD(id) {
    if (!confirm('Remover esta categoria?')) return;
    await api.deleteCategoriaDespesa(id).catch(() => {});
    await carregar();
  }

  if (isContador && !viewingUser) {
    return (
      <div className="page">
        <div className="empty-state">
          <p>Selecione um usuário para gerenciar o plano de contas.</p>
          <button className="btn-primary" style={{ marginTop: 12 }} onClick={() => navigate('/usuarios')}>
            Ir para Usuários
          </button>
        </div>
      </div>
    );
  }

  const gruposAtivos = aba === 'receita' ? gruposReceita : gruposDespesa;
  const erro = aba === 'receita' ? erroR : erroD;

  return (
    <div className="page">
      <div className="page-header">
        <h2>Plano de Contas</h2>
        {podeEscrever && (
          <div style={{ display: 'flex', gap: 8 }}>
            <button className="btn-secondary"
              onClick={() => { setAdicionandoGrupoR(false); setAdicionandoGrupoD(false); setAdicionandoCatR(f => !f); setAdicionandoCatD(f => !f); }}>
              {(aba === 'receita' ? adicionandoCatR : adicionandoCatD) ? 'Cancelar' : '+ Categoria'}
            </button>
            <button className="btn-primary"
              onClick={() => { setAdicionandoCatR(false); setAdicionandoCatD(false); setAdicionandoGrupoR(f => !f); setAdicionandoGrupoD(f => !f); }}>
              {(aba === 'receita' ? adicionandoGrupoR : adicionandoGrupoD) ? 'Cancelar' : '+ Grupo'}
            </button>
          </div>
        )}
      </div>

      {/* Abas */}
      <div className="plano-abas">
        <button className={`plano-aba ${aba === 'receita' ? 'aba-ativa' : ''}`} onClick={() => setAba('receita')}>
          💰 Receitas
        </button>
        <button className={`plano-aba ${aba === 'despesa' ? 'aba-ativa' : ''}`} onClick={() => setAba('despesa')}>
          💸 Despesas
        </button>
      </div>

      {erro && <div className="alert-erro">{erro}</div>}
      {loading && <div className="loading">Carregando...</div>}

      {/* Formulário novo grupo */}
      {podeEscrever && ((aba === 'receita' && adicionandoGrupoR) || (aba === 'despesa' && adicionandoGrupoD)) && (
        <div className="form-card" style={{ marginBottom: 16 }}>
          <h3>Novo Grupo</h3>
          <form onSubmit={aba === 'receita' ? handleAddGrupoR : handleAddGrupoD}
            style={{ display: 'flex', gap: 12, alignItems: 'flex-end' }}>
            <div className="field" style={{ flex: 1 }}>
              <label>Nome do grupo</label>
              <input type="text" required autoFocus
                value={aba === 'receita' ? novoGrupoR : novoGrupoD}
                onChange={e => aba === 'receita' ? setNovoGrupoR(e.target.value) : setNovoGrupoD(e.target.value)}
                placeholder="Ex: Honorários, Serviços, Vendas..." />
            </div>
            <button type="submit" className="btn-primary">Salvar</button>
          </form>
        </div>
      )}

      {/* Formulário nova categoria */}
      {podeEscrever && ((aba === 'receita' && adicionandoCatR) || (aba === 'despesa' && adicionandoCatD)) && (
        <div className="form-card" style={{ marginBottom: 16 }}>
          <h3>Nova Categoria</h3>
          <form onSubmit={aba === 'receita' ? handleAddCatR : handleAddCatD}
            style={{ display: 'flex', gap: 12, alignItems: 'flex-end', flexWrap: 'wrap' }}>
            <div className="field">
              <label>Grupo</label>
              <select required
                value={aba === 'receita' ? novaCatR.grupoId : novaCatD.grupoId}
                onChange={e => aba === 'receita'
                  ? setNovaCatR(c => ({ ...c, grupoId: e.target.value }))
                  : setNovaCatD(c => ({ ...c, grupoId: e.target.value }))}>
                <option value="">Selecione...</option>
                {gruposAtivos.map(g => <option key={g.id} value={g.id}>{g.nome}</option>)}
              </select>
            </div>
            <div className="field" style={{ flex: 1 }}>
              <label>Nome da categoria</label>
              <input type="text" required autoFocus
                value={aba === 'receita' ? novaCatR.nome : novaCatD.nome}
                onChange={e => aba === 'receita'
                  ? setNovaCatR(c => ({ ...c, nome: e.target.value }))
                  : setNovaCatD(c => ({ ...c, nome: e.target.value }))}
                placeholder="Ex: Consultoria mensal, Imposto IRPJ..." />
            </div>
            <button type="submit" className="btn-primary">Salvar</button>
          </form>
          {gruposAtivos.length === 0 && (
            <p style={{ fontSize: 12, color: 'var(--text-muted)', marginTop: 8 }}>
              Crie um grupo primeiro para poder adicionar categorias.
            </p>
          )}
        </div>
      )}

      {/* Lista de grupos e categorias */}
      {!loading && (
        <>
          {gruposAtivos.length === 0 ? (
            <div className="empty-state">
              <p>Nenhum grupo cadastrado para {aba === 'receita' ? 'receitas' : 'despesas'} ainda.</p>
              {podeEscrever && (
                <button className="btn-primary" style={{ marginTop: 12 }}
                  onClick={() => { setAdicionandoGrupoR(aba === 'receita'); setAdicionandoGrupoD(aba === 'despesa'); }}>
                  + Criar primeiro grupo
                </button>
              )}
            </div>
          ) : (
            <div className="plano-lista">
              {gruposAtivos.map(g => (
                <div key={g.id} className="plano-grupo">
                  <div className="plano-grupo-header">
                    <span className="plano-grupo-nome">{g.nome}</span>
                    <span className="plano-grupo-count">{g.categorias?.length || 0} categorias</span>
                    {podeEscrever && (
                      <button className="btn-del"
                        onClick={() => aba === 'receita' ? handleDelGrupoR(g.id) : handleDelGrupoD(g.id)}
                        title="Remover grupo">✕</button>
                    )}
                  </div>
                  <div className="plano-cats">
                    {g.categorias?.length === 0 && (
                      <span className="plano-cat-vazia">Sem categorias</span>
                    )}
                    {g.categorias?.map(c => (
                      <div key={c.id} className="plano-cat">
                        <span>{c.nome}</span>
                        {podeEscrever && (
                          <button className="btn-del"
                            onClick={() => aba === 'receita' ? handleDelCatR(c.id) : handleDelCatD(c.id)}
                            title="Remover">✕</button>
                        )}
                      </div>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          )}
        </>
      )}
    </div>
  );
}
