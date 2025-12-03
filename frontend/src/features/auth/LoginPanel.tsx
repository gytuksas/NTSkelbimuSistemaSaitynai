import { useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '../../context/useAuth'

export const LoginPanel = () => {
  const { isAuthenticated, user, login, logout, loading } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError(null)
    try {
      await login(email, password)
      setPassword('')
    } catch (err) {
      console.error(err)
      setError('Prisijungti nepavyko. Patikrinkite duomenis.')
    }
  }

  if (isAuthenticated && user) {
    return (
      <div className="auth-panel">
        <div>
          <p className="auth-panel__hello">Sveiki, {user.name ?? user.email ?? 'naudotojau'}!</p>
          <p className="auth-panel__role">Rolė: {user.role}</p>
        </div>
        <button className="btn btn--ghost" onClick={() => logout()}>
          Atsijungti
        </button>
      </div>
    )
  }

  return (
    <form className="auth-panel" onSubmit={handleSubmit}>
      <div className="auth-panel__fields">
        <input
          type="email"
          placeholder="El. paštas"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          required
        />
        <input
          type="password"
          placeholder="Slaptažodis"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          required
        />
      </div>
      <button className="btn" type="submit" disabled={loading}>
        {loading ? 'Jungiama...' : 'Prisijungti'}
      </button>
      {error && <p className="auth-panel__error">{error}</p>}
    </form>
  )
}
