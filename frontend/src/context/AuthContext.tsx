import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { jwtDecode } from 'jwt-decode'
import { client, publicClient } from '../api/client'
import { tokenStore, type TokenPair } from '../api/tokenStore'
import type { AppUser, TokenPayload } from '../types/api'
import { AuthContext, type AuthContextValue, type Identity } from './authContext'

const decodeIdentity = (accessToken: string | null): Identity | null => {
  if (!accessToken) return null
  try {
    const payload = jwtDecode<TokenPayload>(accessToken)
    const rawId =
      payload.id ??
      payload.sub ??
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']
    const parsedId = rawId ? Number(rawId) : Number.NaN
    const rawRole =
      payload.role ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']

    if (Number.isNaN(parsedId) || !rawRole) {
      console.error('Nepavyko nuskaityti rolės arba ID iš žetono', payload)
      return null
    }

    return {
      id: parsedId,
      role: rawRole,
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

export const AuthProvider = ({ children }: { children: ReactNode }) => {
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
