import { useState, useEffect, useCallback } from 'react'
import * as api from './api'

function Toast({ message, type, onClose }) {
  useEffect(() => {
    const t = setTimeout(onClose, 3500)
    return () => clearTimeout(t)
  }, [onClose])

  return (
    <div className={`toast ${type === 'error' ? 'toast-err' : 'toast-ok'}`}>
      <span style={{ color: type === 'error' ? 'var(--danger)' : 'var(--accent)', marginRight: 8 }}>
        {type === 'error' ? '✕' : '✓'}
      </span>
      {message}
    </div>
  )
}

function Modal({ title, onClose, children }) {
  useEffect(() => {
    const esc = e => e.key === 'Escape' && onClose()
    window.addEventListener('keydown', esc)
    return () => window.removeEventListener('keydown', esc)
  }, [onClose])

  return (
    <div className="overlay" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="modal">
        <div style={{
          display: 'flex', justifyContent: 'space-between', alignItems: 'center',
          padding: '1.25rem 1.5rem',
          borderBottom: '1px solid var(--border)'
        }}>
          <span className="mono" style={{ fontSize: '0.72rem', letterSpacing: '0.1em', textTransform: 'uppercase', color: 'var(--muted)' }}>
            {title}
          </span>
          <button onClick={onClose} className="btn btn-ghost" style={{ padding: '0.2rem 0.4rem', fontSize: '1rem', lineHeight: 1 }}>
            ×
          </button>
        </div>
        <div style={{ padding: '1.5rem' }}>
          {children}
        </div>
      </div>
    </div>
  )
}

function AuthPage({ onLogin }) {
  const [mode, setMode] = useState('login')
  const [form, setForm] = useState({ name: '', login: '', password: '' })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const f = k => e => setForm(p => ({ ...p, [k]: e.target.value }))

  const submit = async e => {
    e.preventDefault()
    setLoading(true); setError(''); setSuccess('')
    try {
      if (mode === 'login') {
        const response = await api.login(form.login, form.password)

        const token = response.token || response;

        if (token) {
          localStorage.setItem('token', token)

          const userData = await api.validateToken(token)

          if (userData && userData.userId) {
            localStorage.setItem('userId', userData.userId)
            onLogin()
          } else {
            throw new Error("Não foi possível recuperar os dados do usuário.")
          }
        }
      } else {
        await api.register(form.name, form.login, form.password)
        setSuccess('Conta criada. Faça login.')
        setMode('login')
      }
    } catch (e) {
      setError(e.message)
    }
    setLoading(false)
  }

  return (
    <div style={{
      minHeight: '100vh', display: 'grid',
      gridTemplateColumns: '1fr 420px',
      overflow: 'hidden'
    }}>
      <div style={{
        background: 'var(--surface)',
        borderRight: '1px solid var(--border)',
        display: 'flex', flexDirection: 'column',
        justifyContent: 'space-between',
        padding: '3rem',
      }}>
        <div>
          <div className="display" style={{ fontSize: '5rem', color: 'var(--text)' }}>CIS</div>
          <div className="mono" style={{ fontSize: '0.7rem', color: 'var(--muted)', letterSpacing: '0.15em', textTransform: 'uppercase', marginTop: '0.5rem' }}>
            Collective Idea System
          </div>
        </div>
        <div>
          <div className="display" style={{ fontSize: '6.5rem', lineHeight: 0.88, color: 'var(--text)', opacity: 0.08 }}>
            IDEAS<br />THAT<br />MATTER
          </div>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
          <div style={{ width: 8, height: 8, background: 'var(--accent)', borderRadius: '50%' }} />
          <span className="mono" style={{ fontSize: '0.65rem', color: 'var(--muted)', letterSpacing: '0.08em' }}>
            v1.0 — CAPSTONE DS3
          </span>
        </div>
      </div>

      <div style={{
        display: 'flex', flexDirection: 'column', justifyContent: 'center',
        padding: '3rem 2.5rem',
        background: 'var(--bg)'
      }}>
        <div style={{ display: 'flex', gap: '0', marginBottom: '2.5rem', borderBottom: '1px solid var(--border)' }}>
          {[['login', 'Entrar'], ['register', 'Cadastrar']].map(([m, label]) => (
            <button key={m} onClick={() => { setMode(m); setError(''); setSuccess('') }} style={{
              padding: '0.6rem 1.2rem', background: 'none', border: 'none',
              fontFamily: 'JetBrains Mono, monospace', fontSize: '0.7rem',
              letterSpacing: '0.08em', textTransform: 'uppercase', cursor: 'pointer',
              color: mode === m ? 'var(--text)' : 'var(--muted)',
              borderBottom: `1px solid ${mode === m ? 'var(--accent)' : 'transparent'}`,
              marginBottom: '-1px', transition: 'color 0.15s'
            }}>
              {label}
            </button>
          ))}
        </div>

        <form onSubmit={submit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {mode === 'register' && (
            <div className="anim-up s0">
              <label className="label">Nome</label>
              <input className="field" placeholder="Seu nome completo" value={form.name} onChange={f('name')} required />
            </div>
          )}
          <div className={`anim-up ${mode === 'register' ? 's1' : 's0'}`}>
            <label className="label">Login</label>
            <input className="field" placeholder="identificador único" value={form.login} onChange={f('login')} required autoComplete="username" />
          </div>
          <div className={`anim-up ${mode === 'register' ? 's2' : 's1'}`}>
            <label className="label">Senha</label>
            <input className="field" type="password" placeholder="••••••••••" value={form.password} onChange={f('password')} required autoComplete="current-password" />
          </div>

          {error && (
            <div style={{ padding: '0.7rem 0.9rem', background: 'rgba(240,68,68,0.06)', borderLeft: '2px solid var(--danger)', fontFamily: 'JetBrains Mono, monospace', fontSize: '0.72rem', color: 'var(--danger)' }}>
              {error}
            </div>
          )}
          {success && (
            <div style={{ padding: '0.7rem 0.9rem', background: 'rgba(200,240,60,0.06)', borderLeft: '2px solid var(--accent)', fontFamily: 'JetBrains Mono, monospace', fontSize: '0.72rem', color: 'var(--accent)' }}>
              {success}
            </div>
          )}

          <button className="btn btn-accent anim-up s3" type="submit" disabled={loading} style={{ marginTop: '0.5rem', width: '100%' }}>
            {loading ? <span className="spinner" /> : mode === 'login' ? '→ Entrar' : '→ Criar conta'}
          </button>
        </form>
      </div>
    </div>
  )
}

function Nav({ onBack, backLabel, title, actions }) {
  return (
    <div className="nav">
      <div style={{
        maxWidth: 900, margin: '0 auto',
        padding: '0 1.5rem',
        height: 52,
        display: 'flex', alignItems: 'center', justifyContent: 'space-between',
        gap: '1rem'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
          {onBack && (
            <button onClick={onBack} className="btn btn-ghost" style={{ padding: '0.3rem 0.6rem', fontSize: '0.7rem' }}>
              ← {backLabel || 'voltar'}
            </button>
          )}
          {!onBack && (
            <span className="display" style={{ fontSize: '1.4rem', color: 'var(--text)' }}>CIS</span>
          )}
          {title && (
            <>
              <span style={{ color: 'var(--border-hi)' }}>·</span>
              <span className="mono" style={{ fontSize: '0.72rem', color: 'var(--muted)', letterSpacing: '0.05em', maxWidth: 240, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                {title}
              </span>
            </>
          )}
        </div>
        <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
          {actions}
        </div>
      </div>
    </div>
  )
}

function TopicsPage({ onSelectTopic, onLogout, toast }) {
  const [topics, setTopics] = useState([])
  const [loading, setLoading] = useState(true)
  const [showModal, setShowModal] = useState(false)
  const [editTarget, setEditTarget] = useState(null)
  const [form, setForm] = useState({ title: '', description: '' })
  const [saving, setSaving] = useState(false)
  const currentUserId = localStorage.getItem('userId')

  const load = useCallback(async () => {
    setLoading(true)
    try { setTopics((await api.getTopics()) || []) }
    catch (e) { toast(e.message, 'error') }
    setLoading(false)
  }, [toast])

  useEffect(() => { load() }, [load])

  const openCreate = () => {
    setEditTarget(null)
    setForm({ title: '', description: '' })
    setShowModal(true)
  }

  const openEdit = (topic, e) => {
    e.stopPropagation()
    setEditTarget(topic)
    setForm({ title: topic.title, description: topic.description || '' })
    setShowModal(true)
  }

  const submit = async e => {
    e.preventDefault(); setSaving(true)
    try {
      if (editTarget) {
        await api.updateTopic(editTarget.id, form.title, form.description)
        toast('Tópico atualizado')
      } else {
        await api.createTopic(form.title, form.description)
        toast('Tópico criado')
      }
      setShowModal(false)
      setForm({ title: '', description: '' })
      setEditTarget(null)
      load()
    } catch (e) { toast(e.message, 'error') }
    setSaving(false)
  }

  const handleDelete = async (topic, e) => {
    e.stopPropagation()
    if (!confirm(`Deletar "${topic.title}"? Isso também removerá todas as ideias.`)) return
    try {
      await api.deleteTopic(topic.id)
      toast('Tópico removido')
      load()
    } catch (e) { toast(e.message, 'error') }
  }

  return (
    <>
      <Nav
        actions={
          <>
            <button className="btn btn-outline" onClick={openCreate} style={{ padding: '0.5rem 1rem' }}>
              + Novo tópico
            </button>
            <button className="btn btn-ghost" onClick={onLogout}>Sair</button>
          </>
        }
      />

      <div style={{ maxWidth: 900, margin: '0 auto', padding: '2.5rem 1.5rem' }}>
        <div className="anim-up s0" style={{ marginBottom: '2.5rem' }}>
          <div style={{ display: 'flex', alignItems: 'baseline', gap: '1rem' }}>
            <span className="display" style={{ fontSize: '3.5rem' }}>Tópicos</span>
            {!loading && (
              <span className="mono" style={{ fontSize: '0.72rem', color: 'var(--muted)' }}>
                {topics.length} encontrados
              </span>
            )}
          </div>
          <div style={{ width: 32, height: 2, background: 'var(--accent)', marginTop: '0.6rem' }} />
        </div>

        {loading ? (
          <div style={{ display: 'flex', justifyContent: 'center', padding: '5rem' }}>
            <span className="spinner" style={{ width: 24, height: 24 }} />
          </div>
        ) : topics.length === 0 ? (
          <div className="anim-up" style={{ textAlign: 'center', padding: '5rem 2rem' }}>
            <div className="display" style={{ fontSize: '4rem', opacity: 0.06, marginBottom: '1rem' }}>VAZIO</div>
            <p className="mono" style={{ color: 'var(--muted)', fontSize: '0.78rem' }}>Nenhum tópico. Crie o primeiro.</p>
          </div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '1px' }}>
            {topics.map((t, i) => (
              <TopicRow
                key={t.id}
                topic={t}
                index={i}
                isOwner={t.createdByUserId === currentUserId}
                onClick={() => onSelectTopic(t)}
                onEdit={openEdit}
                onDelete={handleDelete}
              />
            ))}
          </div>
        )}
      </div>

      {showModal && (
        <Modal title={editTarget ? 'Editar Tópico' : 'Novo Tópico'} onClose={() => setShowModal(false)}>
          <form onSubmit={submit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            <div>
              <label className="label">Título *</label>
              <input className="field" placeholder="Nome do tópico" value={form.title} onChange={e => setForm(p => ({ ...p, title: e.target.value }))} required />
            </div>
            <div>
              <label className="label">Descrição</label>
              <textarea className="field" placeholder="Contexto ou objetivo do tópico..." value={form.description} onChange={e => setForm(p => ({ ...p, description: e.target.value }))} rows={3} style={{ resize: 'vertical' }} />
            </div>
            <button className="btn btn-accent" type="submit" disabled={saving} style={{ width: '100%' }}>
              {saving ? <span className="spinner" /> : editTarget ? '→ Salvar' : '→ Criar'}
            </button>
          </form>
        </Modal>
      )}
    </>
  )
}

function TopicRow({ topic, onClick, onEdit, onDelete, isOwner, index }) {
  return (
    <div
      className={`card card-interactive anim-up s${Math.min(index, 6)}`}
      onClick={onClick}
      style={{ padding: '1.1rem 1.4rem', display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: '1.5rem' }}
    >
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={{ fontWeight: 400, fontSize: '0.95rem', marginBottom: topic.description ? '0.25rem' : 0, color: 'var(--text)' }}>
          {topic.title}
        </div>
        {topic.description && (
          <div className="mono" style={{ fontSize: '0.72rem', color: 'var(--muted)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
            {topic.description}
          </div>
        )}
      </div>
      <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', flexShrink: 0 }}>
        <span className="tag">{topic.id?.slice(0, 6)}…</span>
        {isOwner && (
          <>
            <button
              className="btn btn-ghost"
              onClick={e => onEdit(topic, e)}
              style={{ padding: '0.3rem 0.6rem', fontSize: '0.68rem', color: 'var(--muted)' }}
              title="Editar"
            >
              editar
            </button>
            <button
              className="btn btn-danger-soft"
              onClick={e => onDelete(topic, e)}
              title="Deletar"
            >
              deletar
            </button>
          </>
        )}
        <span style={{ color: 'var(--muted)', fontSize: '0.85rem' }}>→</span>
      </div>
    </div>
  )
}

function IdeasPage({ topic, onBack, toast }) {
  const [ideas, setIdeas] = useState([])
  const [loading, setLoading] = useState(true)
  const [showModal, setShowModal] = useState(false)
  const [editTarget, setEditTarget] = useState(null)
  const [form, setForm] = useState({ title: '', description: '' })
  const [saving, setSaving] = useState(false)
  const currentUserId = localStorage.getItem('userId')

  const load = useCallback(async () => {
    setLoading(true)
    try { setIdeas((await api.getIdeasByTopic(topic.id)) || []) }
    catch (e) { toast(e.message, 'error') }
    setLoading(false)
  }, [topic.id, toast])

  useEffect(() => { load() }, [load])

  const openCreate = () => {
    setEditTarget(null)
    setForm({ title: '', description: '' })
    setShowModal(true)
  }

  const openEdit = (idea) => {
    setEditTarget(idea)
    setForm({ title: idea.title, description: idea.description || '' })
    setShowModal(true)
  }

  const submit = async e => {
    e.preventDefault(); setSaving(true)
    try {
      if (editTarget) {
        await api.updateIdea(editTarget.id, form.title, form.description)
        toast('Ideia atualizada')
      } else {
        await api.createIdea(topic.id, form.title, form.description)
        toast('Ideia publicada')
      }
      setShowModal(false)
      setForm({ title: '', description: '' })
      setEditTarget(null)
      load()
    } catch (e) { toast(e.message, 'error') }
    setSaving(false)
  }

  const handleVote = async id => {
    try { await api.castVote(id); toast('Voto registrado'); load() }
    catch (e) { toast(e.message, 'error') }
  }

  const handleCancelVote = async id => {
    try { await api.cancelVote(id); toast('Voto removido'); load() }
    catch (e) { toast(e.message, 'error') }
  }

  const handleDelete = async id => {
    if (!confirm('Deletar esta ideia?')) return
    try { await api.deleteIdea(id); toast('Ideia removida'); load() }
    catch (e) { toast(e.message, 'error') }
  }

  const sorted = [...ideas].sort((a, b) => (b.voteCount ?? 0) - (a.voteCount ?? 0))

  return (
    <>
      <Nav
        onBack={onBack}
        backLabel="tópicos"
        title={topic.title}
        actions={
          <button className="btn btn-outline" onClick={openCreate} style={{ padding: '0.5rem 1rem' }}>
            + Nova ideia
          </button>
        }
      />

      <div style={{ maxWidth: 900, margin: '0 auto', padding: '2.5rem 1.5rem' }}>
        <div className="anim-up s0" style={{ marginBottom: '2.5rem' }}>
          <div style={{ display: 'flex', alignItems: 'baseline', gap: '1rem', flexWrap: 'wrap' }}>
            <span className="display" style={{ fontSize: '3rem' }}>{topic.title}</span>
            {!loading && (
              <span className="mono" style={{ fontSize: '0.7rem', color: 'var(--muted)' }}>
                {sorted.length} {sorted.length === 1 ? 'ideia' : 'ideias'}
              </span>
            )}
          </div>
          {topic.description && (
            <p style={{ color: 'var(--muted)', fontSize: '0.88rem', marginTop: '0.5rem', fontWeight: 300 }}>
              {topic.description}
            </p>
          )}
          <div style={{ width: 32, height: 2, background: 'var(--accent)', marginTop: '0.75rem' }} />
        </div>

        {loading ? (
          <div style={{ display: 'flex', justifyContent: 'center', padding: '5rem' }}>
            <span className="spinner" style={{ width: 24, height: 24 }} />
          </div>
        ) : sorted.length === 0 ? (
          <div className="anim-up" style={{ textAlign: 'center', padding: '5rem 2rem' }}>
            <div className="display" style={{ fontSize: '4rem', opacity: 0.06, marginBottom: '1rem' }}>VAZIO</div>
            <p className="mono" style={{ color: 'var(--muted)', fontSize: '0.78rem' }}>Sem ideias ainda. Publique a primeira.</p>
          </div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '1px' }}>
            {sorted.map((idea, i) => (
              <IdeaCard
                key={idea.id}
                idea={idea}
                index={i}
                isOwner={idea.createdByUserId === currentUserId}
                onVote={handleVote}
                onCancelVote={handleCancelVote}
                onEdit={openEdit}
                onDelete={handleDelete}
              />
            ))}
          </div>
        )}
      </div>

      {showModal && (
        <Modal title={editTarget ? 'Editar Ideia' : 'Nova Ideia'} onClose={() => setShowModal(false)}>
          <form onSubmit={submit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            <div>
              <label className="label">Título *</label>
              <input className="field" placeholder="Sua ideia em poucas palavras" value={form.title} onChange={e => setForm(p => ({ ...p, title: e.target.value }))} required />
            </div>
            <div>
              <label className="label">Descrição</label>
              <textarea className="field" placeholder="Detalhe sua proposta..." value={form.description} onChange={e => setForm(p => ({ ...p, description: e.target.value }))} rows={4} style={{ resize: 'vertical' }} />
            </div>
            <button className="btn btn-accent" type="submit" disabled={saving} style={{ width: '100%' }}>
              {saving ? <span className="spinner" /> : editTarget ? '→ Salvar' : '→ Publicar'}
            </button>
          </form>
        </Modal>
      )}
    </>
  )
}

/* ─────────────────────────────────────────────────────────────────────────────
   IDEA CARD
───────────────────────────────────────────────────────────────────────────── */
function IdeaCard({ idea, index, isOwner, onVote, onCancelVote, onEdit, onDelete }) {
  const [voting, setVoting] = useState(false)
  const votes = idea.voteCount ?? 0

  const doVote = async () => {
    setVoting(true); await onVote(idea.id); setVoting(false)
  }
  const doCancel = async () => {
    setVoting(true); await onCancelVote(idea.id); setVoting(false)
  }

  return (
    <div className={`card anim-up s${Math.min(index, 6)}`} style={{
      display: 'flex', gap: '0', overflow: 'hidden'
    }}>
      {/* Vote column */}
      <div style={{
        display: 'flex', flexDirection: 'column', alignItems: 'center',
        justifyContent: 'center', gap: '6px',
        padding: '1.2rem 1rem',
        borderRight: '1px solid var(--border)',
        background: votes > 0 ? 'rgba(200,240,60,0.02)' : 'transparent',
        minWidth: 72,
      }}>
        <button className="vote-btn" onClick={doVote} disabled={voting} title="Votar" style={{ opacity: voting ? 0.4 : 1 }}>
          {voting ? <span className="spinner" style={{ width: 12, height: 12 }} /> : '▲'}
        </button>
        <span className={`score ${votes > 0 ? 'pos' : 'zero'}`}>{votes}</span>
        <button className="vote-btn" onClick={doCancel} disabled={voting} title="Remover voto" style={{ fontSize: '0.65rem', opacity: voting ? 0.4 : 1 }}>
          ▼
        </button>
      </div>

      {/* Content */}
      <div style={{ flex: 1, padding: '1.1rem 1.3rem', display: 'flex', flexDirection: 'column', justifyContent: 'space-between', gap: '0.6rem' }}>
        <div>
          <div style={{ fontWeight: 400, fontSize: '0.95rem', color: 'var(--text)', marginBottom: idea.description ? '0.35rem' : 0 }}>
            {idea.title}
          </div>
          {idea.description && (
            <p style={{ color: 'var(--muted)', fontSize: '0.84rem', fontWeight: 300, lineHeight: 1.55 }}>
              {idea.description}
            </p>
          )}
        </div>

        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: '0.5rem' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <span className="tag">{idea.id?.slice(0, 6)}…</span>
            {votes > 0 && <span className="tag tag-green">↑ {votes}</span>}
          </div>
          {isOwner && (
            <div style={{ display: 'flex', gap: '0.4rem' }}>
              <button
                className="btn btn-ghost"
                onClick={(e) => { e.stopPropagation(); onEdit(idea || topic); }}
                style={{ padding: '0.4rem', minWidth: '32px' }}
                title="Editar"
              >
                ✏️
              </button>
              <button
                className="btn btn-danger-soft"
                onClick={(e) => { e.stopPropagation(); onDelete(idea?.id || topic?.id); }}
                style={{ padding: '0.4rem', minWidth: '32px' }}
                title="Excluir"
              >
                🗑️
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

export default function App() {
  const [page, setPage] = useState('auth')
  const [topic, setTopic] = useState(null)
  const [toast, setToast] = useState(null)

  useEffect(() => {
    if (localStorage.getItem('token')) setPage('topics')
  }, [])

  const showToast = useCallback((message, type = 'success') => {
    setToast({ message, type, id: Date.now() })
  }, [])

  const handleLogout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('userId')
    setPage('auth')
    setTopic(null)
  }

  return (
    <>
      {page === 'auth' && <AuthPage onLogin={() => setPage('topics')} />}

      {page === 'topics' && (
        <TopicsPage
          onSelectTopic={t => { setTopic(t); setPage('ideas') }}
          onLogout={handleLogout}
          toast={showToast}
        />
      )}

      {page === 'ideas' && topic && (
        <IdeasPage
          topic={topic}
          onBack={() => setPage('topics')}
          toast={showToast}
        />
      )}

      {toast && (
        <Toast
          key={toast.id}
          message={toast.message}
          type={toast.type}
          onClose={() => setToast(null)}
        />
      )}
    </>
  )
}