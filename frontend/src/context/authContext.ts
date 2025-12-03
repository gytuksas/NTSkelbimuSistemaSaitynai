import { createContext } from 'react'
import type { UserRole } from '../types/api'

export type Identity = {
  id: number
  role: UserRole
  name?: string
  surname?: string
  email?: string
}

export type AuthContextValue = {
  isAuthenticated: boolean
  user: Identity | null
  loading: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined)
