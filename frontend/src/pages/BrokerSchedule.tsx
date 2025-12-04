import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { client } from '../api/client'
import type { Availability, Viewing, Listing } from '../types/api'
import { useAuth } from '../context/useAuth'
import { formatFriendly, toDateTimeLocalInput, toUtcDateTimeString } from '../utils/dates'

const CONFIRMED_VIEWING_STATUS = 2

type AvailabilityFormState = {
  from: string
  to: string
}

const createDefaultAvailability = (): AvailabilityFormState => {
  const start = new Date(Date.now() + 1000 * 60 * 60 * 24)
  const end = new Date(start.getTime() + 1000 * 60 * 60)
  return {
    from: toDateTimeLocalInput(start),
    to: toDateTimeLocalInput(end),
  }
}

export const BrokerSchedulePage = () => {
  const { user } = useAuth()
  const navigate = useNavigate()
  const isBroker = user && (user.role === 'Broker' || user.role === 'Administrator')

  const [availabilities, setAvailabilities] = useState<Availability[]>([])
  const [viewings, setViewings] = useState<Viewing[]>([])
  const [listings, setListings] = useState<Listing[]>([])
  const [availabilityForm, setAvailabilityForm] = useState<AvailabilityFormState>(createDefaultAvailability)
  const [feedback, setFeedback] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  const viewingStatsByAvailability = useMemo(() => {
    const map = new Map<number, { total: number; confirmed: number }>()
    viewings.forEach((viewing) => {
      const entry = map.get(viewing.fkAvailabilityidAvailability) ?? { total: 0, confirmed: 0 }
      entry.total += 1
      if (viewing.status === CONFIRMED_VIEWING_STATUS) {
        entry.confirmed += 1
      }
      map.set(viewing.fkAvailabilityidAvailability, entry)
    })
    return map
  }, [viewings])

  const canDeleteAvailability = (slotId: number) => (viewingStatsByAvailability.get(slotId)?.confirmed ?? 0) === 0

  const getSlotStats = (slotId: number) => viewingStatsByAvailability.get(slotId) ?? { total: 0, confirmed: 0 }

  const fromDate = availabilityForm.from ? new Date(availabilityForm.from) : null
  const toDate = availabilityForm.to ? new Date(availabilityForm.to) : null
  const isAvailabilityFormValid = Boolean(
    fromDate &&
      toDate &&
      !Number.isNaN(fromDate.getTime()) &&
      !Number.isNaN(toDate.getTime()) &&
      fromDate < toDate,
  )

  const availabilityValidationMessage = isAvailabilityFormValid ? null : 'Patikrinkite pradžios ir pabaigos laikus.'

  useEffect(() => {
    const loadScheduleData = async () => {
      if (!isBroker) {
        setLoading(false)
        return
      }
      try {
        setLoading(true)
        const [availabilityRes, viewingRes, listingRes] = await Promise.all([
          client.get<Availability[]>('/api/Availabilities'),
          client.get<Viewing[]>('/api/Viewings'),
          client.get<Listing[]>('/api/Listings'),
        ])
        setAvailabilities(availabilityRes.data ?? [])
        setViewings(viewingRes.data ?? [])
        setListings(listingRes.data ?? [])
      } catch (error) {
        console.error(error)
        setFeedback('Nepavyko įkelti grafiko duomenų. Bandykite dar kartą.')
      } finally {
        setLoading(false)
      }
    }

    void loadScheduleData()
  }, [isBroker])

  const handleAvailabilityCreate = async () => {
    if (!user) return
    if (!isAvailabilityFormValid) {
      setFeedback('Patikrinkite pradžios ir pabaigos laikus.')
      return
    }
    try {
      const payload = {
        from: toUtcDateTimeString(availabilityForm.from),
        to: toUtcDateTimeString(availabilityForm.to),
        fkBrokeridUser: user.id,
      }
      const { data } = await client.post<Availability>('/api/Availabilities', payload)
      setAvailabilities((prev) => [...prev, data])
      setAvailabilityForm(createDefaultAvailability())
      setFeedback('Prieinamumo langas išsaugotas!')
    } catch (error) {
      console.error(error)
      setFeedback('Nepavyko užregistruoti laiko lango.')
    }
  }

  const handleAvailabilityDelete = async (availabilityId: number) => {
    if (!canDeleteAvailability(availabilityId)) {
      setFeedback('Negalite pašalinti šio laiko, nes yra patvirtintų apžiūrų.')
      return
    }
    if (typeof window !== 'undefined' && !window.confirm('Ar tikrai norite pašalinti šį laiko langą?')) {
      return
    }
    try {
      await client.delete(`/api/Availabilities/${availabilityId}`)
      setAvailabilities((prev) => prev.filter((slot) => slot.idAvailability !== availabilityId))
      setViewings((prev) => prev.filter((viewing) => viewing.fkAvailabilityidAvailability !== availabilityId))
      setFeedback('Laiko langas pašalintas.')
    } catch (error) {
      console.error(error)
      setFeedback('Nepavyko pašalinti laiko lango.')
    }
  }

  const handleViewingDecision = async (id: number, status: number) => {
    try {
      await client.patch(`/api/Viewings/${id}`, { status })
      setViewings((prev) => prev.map((viewing) => (viewing.idViewing === id ? { ...viewing, status } : viewing)))
    } catch (error) {
      console.error(error)
      setFeedback('Nepavyko atnaujinti peržiūros būsenos.')
    }
  }

  const handleListingView = (listingId: number | undefined) => {
    if (!listingId) return
    navigate(`/skelbimai/${listingId}`)
  }

  if (!isBroker) {
    return <p className="muted">Ši skiltis pasiekiama tik brokeriams ir administratoriams.</p>
  }

  return (
    <div className="page brokers brokers-schedule">
      {feedback && <p className="hint">{feedback}</p>}

      <section className="card card--subtle">
        <div className="apartments-header">
          <button className="btn btn--ghost" onClick={() => navigate('/brokeriams')}>
            ← Grįžti į butų valdymą
          </button>
          <div>
            <p className="muted">Tvarkote brokerio darbo laikus</p>
            <h3>Susitikimų grafikas</h3>
          </div>
        </div>
      </section>

      <section className="grid-two">
        <div className="card">
          <h3>Laisvi laikai</h3>
          <p className="muted">Registruokite laikus, kada galite priimti klientus.</p>
          <div className="timeline">
            {loading && <p className="muted">Kraunama...</p>}
            {!loading &&
              availabilities.map((slot) => {
                const slotStats = getSlotStats(slot.idAvailability)
                const deletable = canDeleteAvailability(slot.idAvailability)
                return (
                  <div key={slot.idAvailability} className="timeline__item timeline__item--actions">
                    <div>
                      <p>{formatFriendly(slot.from)}</p>
                      <p className="muted">iki {formatFriendly(slot.to)}</p>
                      {slotStats.total > 0 && (
                        <p className="muted">
                          Užklausos: {slotStats.total} · Patvirtinta: {slotStats.confirmed}
                        </p>
                      )}
                    </div>
                    <div className="timeline__actions">
                      <button
                        className="btn btn--ghost"
                        disabled={!deletable}
                        onClick={() => handleAvailabilityDelete(slot.idAvailability)}
                      >
                        Pašalinti laiką
                      </button>
                      {!deletable && <p className="muted">Negalite pašalinti – yra patvirtintų apžiūrų.</p>}
                    </div>
                  </div>
                )
              })}
            {!loading && availabilities.length === 0 && <p className="muted">Dar neregistravote prieinamumo.</p>}
          </div>
          <div className="form-grid">
            <label>
              Pradžia
              <input
                type="datetime-local"
                lang="lt-LT"
                inputMode="numeric"
                placeholder="2025-12-04T13:30"
                value={availabilityForm.from}
                step="900"
                pattern="[0-9]{4}-[0-9]{2}-[0-9]{2}T([01][0-9]|2[0-3]):[0-5][0-9]"
                onChange={(event) => setAvailabilityForm((prev) => ({ ...prev, from: event.target.value }))}
              />
            </label>
            <label>
              Pabaiga
              <input
                type="datetime-local"
                lang="lt-LT"
                inputMode="numeric"
                placeholder="2025-12-04T14:30"
                value={availabilityForm.to}
                step="900"
                pattern="[0-9]{4}-[0-9]{2}-[0-9]{2}T([01][0-9]|2[0-3]):[0-5][0-9]"
                onChange={(event) => setAvailabilityForm((prev) => ({ ...prev, to: event.target.value }))}
              />
            </label>
          </div>
          {availabilityValidationMessage && <p className="hint">{availabilityValidationMessage}</p>}
          <p className="muted">Laikas rodomas pagal jūsų kompiuterio laiko juostą.</p>
          <button className="btn" onClick={handleAvailabilityCreate} disabled={!isAvailabilityFormValid}>
            Pridėti laiką
          </button>
        </div>

        <div className="card">
          <h3>Peržiūrų užklausos</h3>
          <p className="muted">Patvirtinkite ar atmeskite suplanuotas apžiūras.</p>
          <div className="table">
            <div className="table__row table__row--head">
              <span>Skelbimas</span>
              <span>Laikas</span>
              <span>Veiksmas</span>
            </div>
            {loading && <p className="muted">Kraunama...</p>}
            {!loading &&
              viewings.map((viewing) => {
                const relatedListing = listings.find((listing) => listing.idListing === viewing.fkListingidListing)
                return (
                  <div key={viewing.idViewing} className="table__row">
                    <span>
                      <strong>{relatedListing?.description ?? 'Skelbimo duomenys nepasiekiami'}</strong>
                      <p className="muted">
                        {relatedListing
                          ? relatedListing.rent
                            ? 'Nuomos pasiūlymas'
                            : 'Pardavimo pasiūlymas'
                          : 'Patikrinkite ar skelbimas dar egzistuoja'}
                      </p>
                    </span>
                    <span>
                      <p>{formatFriendly(viewing.from)}</p>
                      <p className="muted">iki {formatFriendly(viewing.to)}</p>
                    </span>
                    <span className="table__actions">
                      <button
                        className="btn"
                        disabled={!relatedListing}
                        onClick={() => handleListingView(relatedListing?.idListing)}
                      >
                        Peržiūrėti skelbimą
                      </button>
                      <button className="btn btn--ghost" onClick={() => handleViewingDecision(viewing.idViewing, 2)}>
                        Patvirtinti
                      </button>
                      <button className="btn btn--ghost" onClick={() => handleViewingDecision(viewing.idViewing, 3)}>
                        Atmesti
                      </button>
                    </span>
                  </div>
                )
              })}
            {!loading && viewings.length === 0 && <p className="muted">Šiuo metu neturite užklausų.</p>}
          </div>
        </div>
      </section>
    </div>
  )
}
