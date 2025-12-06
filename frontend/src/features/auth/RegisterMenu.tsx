import { useCallback, useEffect, useRef, useState, type ChangeEvent, type FormEvent } from 'react'
import axios from 'axios'
import { registerBuyer, registerBroker } from '../../api/registration'
import type { RegistrationRole } from '../../types/api'

interface FormState {
  name: string
  surname: string
  email: string
  phone: string
  password: string
  confirmPassword: string
}

const initialFormState: FormState = {
  name: '',
  surname: '',
  email: '',
  phone: '',
  password: '',
  confirmPassword: '',
}

const roleCopy: Record<RegistrationRole, { title: string; subtitle: string; cta: string }> = {
  buyer: {
    title: 'Registracija pirkėjui',
    subtitle: 'Sukurkite paskyrą, kad galėtumėte užsisakyti apžiūras ir patvirtinti tapatybę.',
    cta: 'Registruoti pirkėjo paskyrą',
  },
  broker: {
    title: 'Registracija brokeriui',
    subtitle: 'Sukurkite paskyrą, kad galėtumėte administruoti skelbimus ir grafikus.',
    cta: 'Registruoti brokerio paskyrą',
  },
}

export const RegisterMenu = () => {
  const [menuOpen, setMenuOpen] = useState(false)
  const [activeRole, setActiveRole] = useState<RegistrationRole | null>(null)
  const [formValues, setFormValues] = useState<FormState>(() => ({ ...initialFormState }))
  const [submitting, setSubmitting] = useState(false)
  const [feedback, setFeedback] = useState<{ status: 'success' | 'error'; message: string } | null>(null)
  const menuRef = useRef<HTMLDivElement | null>(null)

  const openForm = useCallback((role: RegistrationRole) => {
    setActiveRole(role)
    setFeedback(null)
  setFormValues({ ...initialFormState })
    setMenuOpen(false)
  }, [])

  const closeForm = useCallback(() => {
    if (submitting) return
    setActiveRole(null)
    setFeedback(null)
  setFormValues({ ...initialFormState })
  }, [submitting])

  useEffect(() => {
    if (!activeRole) return

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        event.preventDefault()
        closeForm()
      }
    }

    window.addEventListener('keydown', handleKeyDown)
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [activeRole, closeForm])

  useEffect(() => {
    if (!menuOpen) {
      return
    }

    const handlePointerDown = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setMenuOpen(false)
      }
    }

    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setMenuOpen(false)
      }
    }

    document.addEventListener('mousedown', handlePointerDown)
    document.addEventListener('keydown', handleEscape)

    return () => {
      document.removeEventListener('mousedown', handlePointerDown)
      document.removeEventListener('keydown', handleEscape)
    }
  }, [menuOpen])

  const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = event.target
    setFormValues((prev) => ({ ...prev, [name]: value }))
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!activeRole) {
      return
    }
    if (formValues.password !== formValues.confirmPassword) {
      setFeedback({ status: 'error', message: 'Slaptažodžiai nesutampa.' })
      return
    }

    setSubmitting(true)
    setFeedback(null)

    const payload = {
      name: formValues.name.trim(),
      surname: formValues.surname.trim(),
      email: formValues.email.trim(),
      phone: formValues.phone.trim(),
      password: formValues.password,
    }

    try {
      if (activeRole === 'buyer') {
        await registerBuyer(payload)
      } else {
        await registerBroker(payload)
      }
      setFeedback({
        status: 'success',
        message: 'Registracija sėkminga! Prisijunkite po patvirtinimo.',
      })
  setFormValues({ ...initialFormState })
    } catch (error) {
      let message = 'Nepavyko užsiregistruoti. Pabandykite dar kartą vėliau.'
      if (axios.isAxiosError(error)) {
        const responseMessage =
          typeof error.response?.data === 'string'
            ? error.response.data
            : (error.response?.data as { title?: string; detail?: string } | undefined)?.detail ??
              (error.response?.data as { title?: string } | undefined)?.title
        if (responseMessage) {
          message = responseMessage
        }
      }
      setFeedback({ status: 'error', message })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <>
      <div className="register-menu" ref={menuRef}>
        <button
          type="button"
          className="btn btn--ghost register-menu__trigger"
          onClick={() => setMenuOpen((prev) => !prev)}
        >
          Registruotis
        </button>
        <div className={`register-menu__options ${menuOpen ? 'register-menu__options--open' : ''}`}>
          <button type="button" onClick={() => openForm('buyer')}>
            Kaip pirkėjas
          </button>
          <button type="button" onClick={() => openForm('broker')}>
            Kaip brokeris
          </button>
        </div>
      </div>

      {activeRole && (
        <div className="register-modal" role="dialog" aria-modal="true" aria-label="Registracija">
          <div className="register-modal__backdrop" onClick={closeForm} />
          <form className="register-modal__card" onSubmit={handleSubmit}>
            <div className="register-modal__header">
              <h3>{roleCopy[activeRole].title}</h3>
              <p>{roleCopy[activeRole].subtitle}</p>
            </div>
            <div className="register-form__grid">
              <label>
                <span>Vardas</span>
                <input
                  name="name"
                  type="text"
                  value={formValues.name}
                  onChange={handleChange}
                  required
                  minLength={2}
                  disabled={submitting}
                />
              </label>
              <label>
                <span>Pavardė</span>
                <input
                  name="surname"
                  type="text"
                  value={formValues.surname}
                  onChange={handleChange}
                  required
                  minLength={2}
                  disabled={submitting}
                />
              </label>
              <label>
                <span>El. paštas</span>
                <input
                  name="email"
                  type="email"
                  value={formValues.email}
                  onChange={handleChange}
                  required
                  disabled={submitting}
                />
              </label>
              <label>
                <span>Telefonas</span>
                <input
                  name="phone"
                  type="tel"
                  value={formValues.phone}
                  onChange={handleChange}
                  required
                  disabled={submitting}
                />
              </label>
              <label>
                <span>Slaptažodis</span>
                <input
                  name="password"
                  type="password"
                  value={formValues.password}
                  onChange={handleChange}
                  required
                  minLength={8}
                  disabled={submitting}
                />
              </label>
              <label>
                <span>Pakartokite slaptažodį</span>
                <input
                  name="confirmPassword"
                  type="password"
                  value={formValues.confirmPassword}
                  onChange={handleChange}
                  required
                  minLength={8}
                  disabled={submitting}
                />
              </label>
            </div>
            {feedback && (
              <p
                className={`register-form__feedback register-form__feedback--${feedback.status}`}
                role={feedback.status === 'error' ? 'alert' : 'status'}
              >
                {feedback.message}
              </p>
            )}
            <div className="register-form__actions">
              <button type="button" className="btn btn--ghost" onClick={closeForm} disabled={submitting}>
                Atšaukti
              </button>
              <button className="btn" type="submit" disabled={submitting}>
                {submitting ? 'Siunčiama...' : roleCopy[activeRole].cta}
              </button>
            </div>
          </form>
        </div>
      )}
    </>
  )
}
