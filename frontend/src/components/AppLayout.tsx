import { useState } from 'react'
import { NavLink } from 'react-router-dom'
import { LoginPanel } from '../features/auth/LoginPanel'
import { useAuth } from '../context/useAuth'

export const AppLayout = ({ children }: { children: React.ReactNode }) => {
  const { user, isAuthenticated } = useAuth()
  const [open, setOpen] = useState(false)
  const isBroker = user?.role === 'Broker'
  const isAdmin = user?.role === 'Administrator'
  const isBuyer = user?.role === 'Buyer'

  const navLinks = [
    { to: '/', label: 'Pagrindinis', show: true, end: true },
    { to: '/skelbimai', label: 'Skelbimai', show: true },
    { to: '/apziuros', label: 'Apžiūros', show: true },
  { to: '/mano-apziuros', label: 'Mano apžiūros', show: isBuyer },
    { to: '/brokeriams', label: 'Brokeriams', show: isBroker, end: true },
    { to: '/brokeriams/grafikas', label: 'Brokerio grafikas', show: isBroker },
    { to: '/administratoriams', label: 'Administratoriui', show: isAdmin },
    { to: '/profilis', label: 'Profilis', show: isAuthenticated },
  ].filter((link) => link.show)

  return (
    <div className={`app-shell ${open ? 'app-shell--menu' : ''}`}>
      <header className="app-header">
        <div className="brand">
          <button className="brand__toggle" onClick={() => setOpen((prev) => !prev)}>
            <span />
            <span />
            <span />
          </button>
          <div>
            <p className="brand__eyebrow">NT platforma</p>
            <h1 className="brand__title">Atviri namai</h1>
          </div>
        </div>
        <nav className="main-nav">
          {navLinks.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              end={link.end}
              className={({ isActive }) => (isActive ? 'nav-link nav-link--active' : 'nav-link')}
              onClick={() => setOpen(false)}
            >
              {link.label}
            </NavLink>
          ))}
        </nav>
        <LoginPanel />
      </header>
      <main className="app-main">{children}</main>
      <footer className="app-footer">
        <p className="app-footer__text">2025</p>
        <a
          className="app-footer__link"
          href="https://github.com/gytuksas/NTSkelbimuSistemaSaitynai"
          target="_blank"
          rel="noreferrer"
          aria-label="GitHub repozitorija"
        >
          <svg viewBox="0 0 24 24" role="img" aria-hidden="true">
            <path d="M12 2C6.477 2 2 6.486 2 12.021c0 4.42 2.865 8.17 6.839 9.504.5.092.683-.217.683-.482 0-.237-.009-.866-.013-1.7-2.782.606-3.369-1.342-3.369-1.342-.454-1.155-1.11-1.463-1.11-1.463-.908-.621.069-.609.069-.609 1.004.071 1.532 1.034 1.532 1.034.893 1.53 2.345 1.088 2.914.833.091-.648.35-1.088.636-1.338-2.22-.253-4.555-1.114-4.555-4.955 0-1.094.39-1.988 1.03-2.688-.104-.254-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0 1 12 6.844c.85.004 1.705.115 2.504.337 1.908-1.296 2.747-1.027 2.747-1.027.546 1.379.204 2.397.1 2.651.642.7 1.028 1.594 1.028 2.688 0 3.85-2.339 4.699-4.566 4.947.36.311.68.92.68 1.855 0 1.338-.012 2.417-.012 2.747 0 .268.18.58.688.481A10.02 10.02 0 0 0 22 12.02C22 6.487 17.523 2 12 2" />
          </svg>
          <span>GitHub</span>
        </a>
      </footer>
    </div>
  )
}
