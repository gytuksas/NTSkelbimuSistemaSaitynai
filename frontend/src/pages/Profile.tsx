import { useEffect, useState } from 'react'
import { client } from '../api/client'
import type { Buyer } from '../types/api'
import { useAuth } from '../context/useAuth'
import { toDateTimeLocal } from '../utils/dates'

export const ProfilePage = () => {
  const { user, isAuthenticated } = useAuth()
  const [buyerInfo, setBuyerInfo] = useState<Buyer | null>(null)
  const [confirmDate, setConfirmDate] = useState(() => toDateTimeLocal(new Date(Date.now() + 1000 * 60 * 60 * 24)))
  const [confirmationMessage, setConfirmationMessage] = useState<string | null>(null)

  useEffect(() => {
    const loadBuyer = async () => {
      if (!user || user.role !== 'Buyer') {
        setBuyerInfo(null)
        return
      }
      try {
        const { data } = await client.get<Buyer>(`/api/Buyers/${user.id}`)
        setBuyerInfo(data)
      } catch (error) {
        console.warn(error)
        setBuyerInfo(null)
      }
    }
    void loadBuyer()
  }, [user])

  const requestConfirmation = async () => {
    if (!user || user.role !== 'Buyer') return
    try {
      await client.post('/api/Confirmations', {
        expires: confirmDate,
        fkBuyeridUser: user.id,
      })
      setConfirmationMessage('Tapatybės patvirtinimo užklausa išsiųsta!')
    } catch (error) {
      console.error(error)
      setConfirmationMessage('Nepavyko pateikti užklausos. Įsitikinkite, kad turite pirkėjo rolę.')
    }
  }

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
              <span>El. paštas</span>
              <strong>{user.email ?? 'Nenurodytas'}</strong>
            </li>
            <li>
              <span>Rolė</span>
              <strong>{user.role}</strong>
            </li>
          </ul>
        ) : (
          <p>Nepavyko nuskaityti profilio.</p>
        )}
      </section>

      <section className="card">
        <h3>Tapatybės patvirtinimas</h3>
        {user?.role === 'Buyer' ? (
          <>
            <p>Po patvirtinimo galėsite matyti pardavėjo kontaktus bei rezervuoti apžiūras.</p>
            <label>
              Galiojimo pabaiga
              <input type="text" value={confirmDate} onChange={(event) => setConfirmDate(event.target.value)} />
            </label>
            <button className="btn" onClick={requestConfirmation}>
              Siųsti prašymą
            </button>
            {confirmationMessage && <p className="hint">{confirmationMessage}</p>}
            <ul className="info-list">
              <li>
                <span>Pirkėjo būsena</span>
                <strong>{buyerInfo ? (buyerInfo.confirmed ? 'Patvirtintas' : 'Nepatvirtintas') : 'Nerasta įrašo'}</strong>
              </li>
              <li>
                <span>Blokavimas</span>
                <strong>{buyerInfo?.blocked ? 'Sustabdyta' : 'Aktyvus'}</strong>
              </li>
            </ul>
          </>
        ) : (
          <p className="muted">
            Norėdami prašyti patvirtinimo, užregistruokite pirkėjo paskyrą. Šiuo metu jūsų rolė: {user?.role ?? 'nenurodyta'}.
          </p>
        )}
      </section>
    </div>
  )
}
