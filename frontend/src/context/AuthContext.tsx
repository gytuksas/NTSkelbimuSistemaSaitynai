import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import { jwtDecode } from 'jwt-decode'
import { client, publicClient } from '../api/client'
import { tokenStore, type TokenPair } from '../api/tokenStore'
import type { AppUser, TokenPayload, UserRole } from '../types/api'

type Identity = {
  id: number
  role: UserRole
  name?: string
  surname?: string
  email?: string
}

type AuthContextValue = {
  isAuthenticated: boolean
  user: Identity | null
  loading: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

const decodeIdentity = (accessToken: string | null): Identity | null => {
  if (!accessToken) return null
  try {
    const payload = jwtDecode<TokenPayload>(accessToken)
    return {
      id: Number(payload.id),
      role: payload.role,
    }
  } catch (error) {
    console.error('Nepavyko iškoduoti žetono', error)
    return null
  }
}

async function fetchProfile(id: number): Promise<Partial<Identity>> {
  try {
    const { data } = await client.get<AppUser>(`/api/Users/${id}`)
    return {
      name: data.name,
      surname: data.surname,
      email: data.email,
    }
  } catch (error) {
    console.warn('Nepavyko įkelti naudotojo profilio', error)
    return {}
  }
}

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<Identity | null>(decodeIdentity(tokenStore.getTokens().accessToken))
  const [loading, setLoading] = useState(false)

  const syncProfile = useCallback(async (tokens: TokenPair) => {
    if (!tokens.accessToken) {
      setUser(null)
      return
    }
    const identity = decodeIdentity(tokens.accessToken)
    if (!identity) {
      setUser(null)
      return
    }
    const profile = await fetchProfile(identity.id)
    setUser({ ...identity, ...profile })
  }, [])

  useEffect(() => {
    const unsubscribe = tokenStore.subscribe((tokens) => {
      void syncProfile(tokens)
    })
    return unsubscribe
  }, [syncProfile])

  const login = useCallback(async (email: string, password: string) => {
    setLoading(true)
    try {
      const { data } = await publicClient.post<{ accessToken: string; refreshToken: string }>(
        '/api/Authentication/login',
        { email, password },
      )
      tokenStore.setTokens({ accessToken: data.accessToken, refreshToken: data.refreshToken })
    } finally {
      setLoading(false)
    }
  }, [])

  const logout = useCallback(async () => {
    const { refreshToken } = tokenStore.getTokens()
    try {
      if (refreshToken) {
        await client.post('/api/Authentication/logout', { refreshToken })
      }
    } catch (error) {
      console.warn('Atsijungimo klaida', error)
    } finally {
      tokenStore.clearTokens()
    }
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      isAuthenticated: Boolean(user),
      user,
      loading,
      login,
      logout,
    }),
    [user, loading, login, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export const useAuth = () => {
  const ctx = useContext(AuthContext)
  if (!ctx) {
    throw new Error('useAuth privalo būti naudojamas AuthProvider viduje')
  }
  return ctx
}
