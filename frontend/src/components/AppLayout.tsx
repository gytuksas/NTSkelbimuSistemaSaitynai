import { useState } from 'react'
import { NavLink } from 'react-router-dom'
import { LoginPanel } from '../features/auth/LoginPanel'

const links = [
  { to: '/', label: 'Pagrindinis' },
  { to: '/pirkejams', label: 'Pirkėjams' },
  { to: '/brokeriams', label: 'Brokeriams' },
  { to: '/administratoriams', label: 'Administratoriui' },
]

export const AppLayout = ({ children }: { children: React.ReactNode }) => {
  const [open, setOpen] = useState(false)

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
          {links.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
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
