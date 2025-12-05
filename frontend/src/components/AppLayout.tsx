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
    </div>
  )
}
