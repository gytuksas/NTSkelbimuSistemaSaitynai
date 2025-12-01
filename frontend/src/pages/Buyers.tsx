import { useEffect, useState } from 'react'
import { client } from '../api/client'
import type { Buyer } from '../types/api'
import { useAuth } from '../context/AuthContext'
import { toDateTimeLocal } from '../utils/dates'

export const BuyersPage = () => {
  const { user, isAuthenticated } = useAuth()
  const [buyerInfo, setBuyerInfo] = useState<Buyer | null>(null)
  const [confirmDate, setConfirmDate] = useState(toDateTimeLocal(new Date(Date.now() + 1000 * 60 * 60 * 24)))
  const [confirmationMessage, setConfirmationMessage] = useState<string | null>(null)
  const [viewingPayload, setViewingPayload] = useState({
    listingId: '',
    availabilityId: '',
    start: toDateTimeLocal(new Date(Date.now() + 1000 * 60 * 60 * 48)),
    end: toDateTimeLocal(new Date(Date.now() + 1000 * 60 * 60 * 49)),
  })
  const [viewingStatus, setViewingStatus] = useState<string | null>(null)

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
    if (!user) return
    try {
      await client.post('/api/Confirmations', {
        expires: confirmDate,
        fkBuyeridUser: user.id,
      })
      setConfirmationMessage('Tapatybės patvirtinimo užklausa išsiųsta!')
    } catch (error) {
      console.error(error)
      setConfirmationMessage('Nepavyko pateikti užklausos. Įsitikinkite, kad esate pirkėjas.')
    }
  }

  const requestViewing = async () => {
    if (!user) return
    setViewingStatus('Siunčiama...')
    try {
      await client.post('/api/Viewings', {
        from: viewingPayload.start,
        to: viewingPayload.end,
        status: 1,
        fkAvailabilityidAvailability: Number(viewingPayload.availabilityId),
        fkListingidListing: Number(viewingPayload.listingId),
      })
      setViewingStatus('Pateikta brokeriui patvirtinti!')
    } catch (error) {
      console.error(error)
      setViewingStatus('Nepavyko pateikti – brokeris turi patvirtinti laiką.')
    }
  }

  return (
    <div className="page buyers">
      <section className="card">
        <h3>Pirkėjo būsena</h3>
        {!isAuthenticated && <p>Prisijunkite, kad matytumėte savo patvirtinimo statusą.</p>}
        {buyerInfo ? (
          <ul className="info-list">
            <li>
              <span>Būsena</span>
              <strong>{buyerInfo.confirmed ? 'Patvirtintas' : 'Nepatvirtintas'}</strong>
            </li>
            <li>
              <span>Blokavimas</span>
              <strong>{buyerInfo.blocked ? 'Sustabdyta' : 'Aktyvus'}</strong>
            </li>
          </ul>
        ) : (
          isAuthenticated && <p>Nerasta įrašo pirkėjų sąraše.</p>
        )}
      </section>

      <section className="card">
        <h3>Tapatybės patvirtinimas</h3>
        <p>Po patvirtinimo galėsite matyti pardavėjo kontaktus bei rezervuoti apžiūras.</p>
        <label>
          Galiojimo pabaiga
          <input type="text" value={confirmDate} onChange={(event) => setConfirmDate(event.target.value)} />
        </label>
        <button className="btn" onClick={requestConfirmation} disabled={!user || user.role !== 'Buyer'}>
          Siųsti prašymą
        </button>
        {confirmationMessage && <p className="hint">{confirmationMessage}</p>}
      </section>

      <section className="card">
        <h3>Privati apžiūra</h3>
        <p>Pasirinkite skelbimą ir brokerio prieinamumo langą. Užklausa bus išsiųsta brokeriui.</p>
        <div className="form-grid">
          <label>
            Skelbimo ID
            <input
              type="number"
              value={viewingPayload.listingId}
              onChange={(event) => setViewingPayload((prev) => ({ ...prev, listingId: event.target.value }))}
              placeholder="pvz. 1"
            />
          </label>
          <label>
            Laisvo laiko ID
            <input
              type="number"
              value={viewingPayload.availabilityId}
              onChange={(event) =>
                setViewingPayload((prev) => ({ ...prev, availabilityId: event.target.value }))
              }
              placeholder="pvz. 3"
            />
          </label>
          <label>
            Pradžia
            <input
              type="text"
              value={viewingPayload.start}
              onChange={(event) => setViewingPayload((prev) => ({ ...prev, start: event.target.value }))}
            />
          </label>
          <label>
            Pabaiga
            <input
              type="text"
              value={viewingPayload.end}
              onChange={(event) => setViewingPayload((prev) => ({ ...prev, end: event.target.value }))}
            />
          </label>
        </div>
        <button className="btn" onClick={requestViewing} disabled={!isAuthenticated}>
          Pateikti prašymą
        </button>
        {viewingStatus && <p className="hint">{viewingStatus}</p>}
      </section>
    </div>
  )
}
