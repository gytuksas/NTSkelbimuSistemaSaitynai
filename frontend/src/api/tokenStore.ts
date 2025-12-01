export type TokenPair = {
  accessToken: string | null
  refreshToken: string | null
}

type Listener = (tokens: TokenPair) => void

const ACCESS_KEY = 'nt_access_token'
const REFRESH_KEY = 'nt_refresh_token'
const isBrowser = typeof window !== 'undefined'

let tokenState: TokenPair = {
  accessToken: null,
  refreshToken: null,
}

if (isBrowser) {
  tokenState = {
    accessToken: window.localStorage.getItem(ACCESS_KEY),
    refreshToken: window.localStorage.getItem(REFRESH_KEY),
  }
}

const listeners = new Set<Listener>()

const persist = () => {
  if (!isBrowser) return
  if (tokenState.accessToken) {
    window.localStorage.setItem(ACCESS_KEY, tokenState.accessToken)
  } else {
    window.localStorage.removeItem(ACCESS_KEY)
  }
  if (tokenState.refreshToken) {
    window.localStorage.setItem(REFRESH_KEY, tokenState.refreshToken)
  } else {
    window.localStorage.removeItem(REFRESH_KEY)
  }
}

const notify = () => {
  listeners.forEach((listener) => listener({ ...tokenState }))
}

export const tokenStore = {
  getTokens(): TokenPair {
    return { ...tokenState }
  },
  setTokens(pair: TokenPair) {
    tokenState = { ...pair }
    persist()
    notify()
  },
  clearTokens() {
    tokenState = { accessToken: null, refreshToken: null }
    persist()
    notify()
  },
  subscribe(listener: Listener) {
    listeners.add(listener)
    listener({ ...tokenState })
    return () => {
      listeners.delete(listener)
    }
  },
}
