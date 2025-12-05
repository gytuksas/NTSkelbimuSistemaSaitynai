import { useCallback, useEffect, useState } from 'react'
import { isAxiosError } from 'axios'
import type { ChangeEvent, FormEvent } from 'react'
import { client } from '../api/client'
import type { AppUser, Broker, Buyer } from '../types/api'
import { useAuth } from '../context/useAuth'

export const ProfilePage = () => {
  const { user, isAuthenticated, refreshProfile } = useAuth()
  const [buyerInfo, setBuyerInfo] = useState<Buyer | null>(null)
  const [brokerInfo, setBrokerInfo] = useState<Broker | null>(null)
  const [profileDetails, setProfileDetails] = useState<AppUser | null>(null)
  const [contactForm, setContactForm] = useState({ email: '', phone: '' })
  const [passwordForm, setPasswordForm] = useState({ currentPassword: '', password: '', confirmPassword: '' })
  const [contactStatus, setContactStatus] = useState<string | null>(null)
  const [passwordStatus, setPasswordStatus] = useState<string | null>(null)
  const [contactSaving, setContactSaving] = useState(false)
  const [passwordSaving, setPasswordSaving] = useState(false)

  const loadProfileDetails = useCallback(async () => {
    if (!user) {
      setProfileDetails(null)
      setContactForm({ email: '', phone: '' })
      return
    }
    try {
      const { data } = await client.get<AppUser>(`/api/Users/${user.id}`)
      setProfileDetails(data)
      setContactForm({ email: data.email, phone: data.phone })
    } catch (error) {
      console.warn(error)
      setProfileDetails(null)
    }
  }, [user])

  useEffect(() => {
    void loadProfileDetails()
  }, [loadProfileDetails])

  useEffect(() => {
    const loadRoleSnapshot = async () => {
      if (!user) {
        setBuyerInfo(null)
        setBrokerInfo(null)
        return
      }

      if (user.role === 'Buyer') {
        try {
          const { data } = await client.get<Buyer>(`/api/Buyers/${user.id}`)
          setBuyerInfo(data)
        } catch (error) {
          console.warn(error)
          setBuyerInfo(null)
        }
        setBrokerInfo(null)
        return
      }

      if (user.role === 'Broker') {
        try {
          const { data } = await client.get<Broker>(`/api/Brokers/${user.id}`)
          setBrokerInfo(data)
        } catch (error) {
          console.warn(error)
          setBrokerInfo(null)
        }
        setBuyerInfo(null)
        return
      }

      setBuyerInfo(null)
      setBrokerInfo(null)
    }

    void loadRoleSnapshot()
  }, [user])

  const handleContactFieldChange = (field: 'email' | 'phone') => (event: ChangeEvent<HTMLInputElement>) => {
    setContactForm((prev) => ({ ...prev, [field]: event.target.value }))
  }

  const handlePasswordFieldChange = (field: 'currentPassword' | 'password' | 'confirmPassword') =>
    (event: ChangeEvent<HTMLInputElement>) => {
      setPasswordForm((prev) => ({ ...prev, [field]: event.target.value }))
    }

  const handleContactSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!user) return
    setContactSaving(true)
    setContactStatus(null)
    try {
      const email = contactForm.email.trim()
      const phone = contactForm.phone.trim()
      await client.patch(`/api/Users/${user.id}/contact`, {
        email,
        phone,
      })
      await Promise.all([loadProfileDetails(), refreshProfile()])
      setContactStatus('Kontaktinė informacija sėkmingai atnaujinta.')
    } catch (error: unknown) {
      console.error(error)
      if (isAxiosError(error) && error.response?.status === 409) {
        setContactStatus('Toks el. pašto adresas jau naudojamas kitam vartotojui.')
      } else {
        setContactStatus('Nepavyko atnaujinti kontaktinės informacijos.')
      }
    } finally {
      setContactSaving(false)
    }
  }

  const handlePasswordSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!user) return
    if (!passwordForm.currentPassword) {
      setPasswordStatus('Įveskite dabartinį slaptažodį.')
      return
    }
    if (passwordForm.password.length < 8) {
      setPasswordStatus('Slaptažodis turi būti sudarytas bent iš 8 simbolių.')
      return
    }
    if (passwordForm.password !== passwordForm.confirmPassword) {
      setPasswordStatus('Slaptažodžiai nesutampa.')
      return
    }
    setPasswordSaving(true)
    setPasswordStatus(null)
    try {
      await client.patch(`/api/Users/${user.id}/password`, {
        currentPassword: passwordForm.currentPassword,
        password: passwordForm.password,
      })
      setPasswordForm({ currentPassword: '', password: '', confirmPassword: '' })
      setPasswordStatus('Slaptažodis sėkmingai atnaujintas.')
    } catch (error: unknown) {
      console.error(error)
      if (isAxiosError(error) && error.response?.status === 400) {
        setPasswordStatus('Neteisingas dabartinis slaptažodis.')
      } else {
        setPasswordStatus('Nepavyko atnaujinti slaptažodžio.')
      }
    } finally {
      setPasswordSaving(false)
    }
  }

  const confirmationDetails = user?.role === 'Buyer' ? buyerInfo : user?.role === 'Broker' ? brokerInfo : null
  const showConfirmation = user?.role === 'Buyer' || user?.role === 'Broker'

  if (!isAuthenticated) {
    return (
      <div className="page profile">
        <section className="card">
          <h3>Profilis</h3>
          <p>Prisijunkite, kad galėtumėte matyti ir atnaujinti savo informaciją.</p>
        </section>
      </div>
    )
  }

  return (
    <div className="page profile">
      <section className="card">
        <h3>Mano informacija</h3>
        {user ? (
          <ul className="info-list">
            <li>
              <span>Vardas ir pavardė</span>
              <strong>
                {[user.name, user.surname].filter(Boolean).join(' ') || 'Nenurodyta'}
              </strong>
            </li>
            <li>
              <span>Rolė</span>
              <strong>{user.role}</strong>
            </li>
            <li>
              <span>El. paštas</span>
              <strong>{profileDetails?.email ?? 'Nenurodyta'}</strong>
            </li>
            <li>
              <span>Telefonas</span>
              <strong>{profileDetails?.phone ?? 'Nenurodyta'}</strong>
            </li>
          </ul>
        ) : (
          <p>Nepavyko nuskaityti profilio.</p>
        )}
      </section>

      {showConfirmation && (
        <section className="card">
          <h3>Tapatybės patvirtinimas</h3>
          {confirmationDetails ? (
            <ul className="info-list info-list--stacked">
              <li>
                <span>Būsena</span>
                <strong>{confirmationDetails.confirmed ? 'Patvirtintas' : 'Nepatvirtintas'}</strong>
              </li>
              <li>
                <span>Prieigos būsena</span>
                <strong>{confirmationDetails.blocked ? 'Laikinai sustabdyta' : 'Aktyvus'}</strong>
              </li>
            </ul>
          ) : (
            <p className="muted">Nepavyko nuskaityti patvirtinimo būsenos.</p>
          )}
        </section>
      )}

      <section className="card">
        <h3>Kontaktinė informacija</h3>
        <form className="form-grid" onSubmit={handleContactSubmit}>
          <label>
            Naujas el. paštas
            <input
              type="email"
              required
              value={contactForm.email}
              onChange={handleContactFieldChange('email')}
              placeholder="vardas@pavyzdys.lt"
            />
          </label>
          <label>
            Naujas telefono numeris
            <input
              type="tel"
              required
              value={contactForm.phone}
              onChange={handleContactFieldChange('phone')}
              placeholder="+370..."
            />
          </label>
          <div className="form-actions">
            <button className="btn" type="submit" disabled={contactSaving}>
              {contactSaving ? 'Saugoma…' : 'Atnaujinti kontaktus'}
            </button>
          </div>
          {contactStatus && <p className="hint">{contactStatus}</p>}
        </form>
      </section>

      <section className="card">
        <h3>Slaptažodžio keitimas</h3>
        <form className="form-grid" onSubmit={handlePasswordSubmit}>
          <label>
            Dabartinis slaptažodis
            <input
              type="password"
              required
              value={passwordForm.currentPassword}
              onChange={handlePasswordFieldChange('currentPassword')}
              placeholder="********"
            />
          </label>
          <label>
            Naujas slaptažodis
            <input
              type="password"
              required
              minLength={8}
              value={passwordForm.password}
              onChange={handlePasswordFieldChange('password')}
              placeholder="********"
            />
          </label>
          <label>
            Pakartokite slaptažodį
            <input
              type="password"
              required
              minLength={8}
              value={passwordForm.confirmPassword}
              onChange={handlePasswordFieldChange('confirmPassword')}
              placeholder="********"
            />
          </label>
          <div className="form-actions">
            <button className="btn" type="submit" disabled={passwordSaving}>
              {passwordSaving ? 'Saugoma…' : 'Atnaujinti slaptažodį'}
            </button>
          </div>
          {passwordStatus && <p className="hint">{passwordStatus}</p>}
        </form>
      </section>
    </div>
  )
}
