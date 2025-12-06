import { useState, type ChangeEvent, type FormEvent } from 'react'
import axios from 'axios'
import { registerBuyer, registerBroker } from '../api/registration'
import type { RegistrationRole } from '../types/api'

const initialFormState = {
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
    subtitle: 'Prisijunkite prie sistemos ir užsisakykite asmenines apžiūras.',
    cta: 'Registruoti pirkėjo paskyrą',
  },
  broker: {
    title: 'Registracija brokeriui',
    subtitle: 'Tvarkykite skelbimus, kalendorių ir pirkėjų užklausas.',
    cta: 'Registruoti brokerio paskyrą',
  },
}

export const RegisterPage = () => {
  const [role, setRole] = useState<RegistrationRole>('buyer')
  const [formValues, setFormValues] = useState({ ...initialFormState })
  const [submitting, setSubmitting] = useState(false)
  const [feedback, setFeedback] = useState<{ status: 'success' | 'error'; message: string } | null>(null)

  const handleRoleChange = (newRole: RegistrationRole) => {
    setRole(newRole)
    setFeedback(null)
  }

  const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = event.target
    setFormValues((prev) => ({ ...prev, [name]: value }))
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

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
      if (role === 'buyer') {
        await registerBuyer(payload)
      } else {
        await registerBroker(payload)
      }
      setFeedback({ status: 'success', message: 'Registracija sėkminga! Prisijunkite po patvirtinimo.' })
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
    <div className="page register-page">
      <section className="register-card">
        <header className="register-card__header">
          <p className="register-card__eyebrow">Nauja paskyra</p>
          <h2>{roleCopy[role].title}</h2>
          <p>{roleCopy[role].subtitle}</p>
        </header>

        <div className="register-role-toggle" role="tablist" aria-label="Registracijos tipas">
          <button
            type="button"
            role="tab"
            aria-selected={role === 'buyer'}
            className={role === 'buyer' ? 'active' : ''}
            onClick={() => handleRoleChange('buyer')}
          >
            Pirkėjas
          </button>
          <button
            type="button"
            role="tab"
            aria-selected={role === 'broker'}
            className={role === 'broker' ? 'active' : ''}
            onClick={() => handleRoleChange('broker')}
          >
            Brokeris
          </button>
        </div>

        <form className="register-form" onSubmit={handleSubmit}>
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
            <button className="btn" type="submit" disabled={submitting}>
              {submitting ? 'Siunčiama...' : roleCopy[role].cta}
            </button>
          </div>
        </form>
      </section>
    </div>
  )
}
