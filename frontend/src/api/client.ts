import axios, { AxiosError, AxiosHeaders } from 'axios'
import type { AxiosRequestConfig } from 'axios'
import { tokenStore } from './tokenStore'

type RetriableConfig = AxiosRequestConfig & { _retry?: boolean }

const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080'

const publicClient = axios.create({ baseURL })
const client = axios.create({ baseURL })

client.interceptors.request.use((config) => {
  const { accessToken } = tokenStore.getTokens()
  if (accessToken) {
    if (!config.headers) {
      config.headers = new AxiosHeaders()
    }
    if (config.headers instanceof AxiosHeaders) {
      config.headers.set('Authorization', `Bearer ${accessToken}`)
    } else {
      const headers = AxiosHeaders.from(config.headers)
      headers.set('Authorization', `Bearer ${accessToken}`)
      config.headers = headers
    }
  }
  return config
})

let refreshPromise: Promise<void> | null = null

const requestTokenRefresh = async () => {
  if (!refreshPromise) {
    const { refreshToken } = tokenStore.getTokens()
    if (!refreshToken) {
      tokenStore.clearTokens()
      throw new Error('Refresh token missing')
    }
    refreshPromise = publicClient
      .post('/api/Authentication/refresh', { refreshToken })
      .then((response) => {
        const { accessToken, refreshToken: newRefresh } = response.data
        tokenStore.setTokens({ accessToken, refreshToken: newRefresh })
      })
      .catch((error) => {
        tokenStore.clearTokens()
        throw error
      })
      .finally(() => {
        refreshPromise = null
      })
  }

  return refreshPromise
}

client.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const status = error.response?.status
    const originalRequest = error.config as RetriableConfig | undefined
    if (status === 401 && originalRequest && !originalRequest._retry) {
      originalRequest._retry = true
      try {
        await requestTokenRefresh()
        return client(originalRequest)
      } catch (refreshError) {
        return Promise.reject(refreshError)
      }
    }
    return Promise.reject(error)
  },
)

export { client, publicClient, baseURL }
