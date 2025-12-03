import { useContext } from 'react'
import { AuthContext } from './authContext'

export const useAuth = () => {
  const ctx = useContext(AuthContext)
  if (!ctx) {
    throw new Error('useAuth privalo būti naudojamas AuthProvider viduje')
  }
  return ctx
}
