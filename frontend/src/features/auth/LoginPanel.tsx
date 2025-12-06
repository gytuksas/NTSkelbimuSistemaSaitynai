import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { isAxiosError } from 'axios'
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
      if (isAxiosError(err)) {
        const headers = err.response?.headers as
          | Record<string, string | string[] | undefined>
          | undefined
        const unconfirmedHeader = headers
          ? headers['x-account-unconfirmed'] ?? headers['X-Account-Unconfirmed']
          : undefined
        const isUnconfirmed = Array.isArray(unconfirmedHeader)
          ? unconfirmedHeader.includes('true')
          : unconfirmedHeader === 'true'
        if (err.response?.status === 401 && isUnconfirmed) {
          setError('Jūsų paskyra dar nepatvirtinta. Palaukite administratoriaus patvirtinimo.')
          return
        }
      }
      setError('Prisijungti nepavyko. Patikrinkite duomenis.')
    }
  }

  if (isAuthenticated && user) {
    return (
      <div className="auth-panel">
        <div>
          <p className="auth-panel__hello">Sveiki, {user.name ?? user.surname ?? 'naudotojau'}!</p>
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
          placeholder="Prisijungimo ID"
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
      <div className="auth-panel__actions">
        <button className="btn" type="submit" disabled={loading}>
          {loading ? 'Jungiama...' : 'Prisijungti'}
        </button>
        <Link className="btn btn--ghost" to="/registracija">
          Registruotis
        </Link>
      </div>
      {error && <p className="auth-panel__error">{error}</p>}
    </form>
  )
}
