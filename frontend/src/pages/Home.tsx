import { useEffect, useMemo, useState } from 'react'
import { client } from '../api/client'
import type { Listing, Viewing } from '../types/api'
import { useAuth } from '../context/AuthContext'
import { formatPrice } from '../utils/text'
import { formatFriendly } from '../utils/dates'

const rentFilterOptions = [
  { value: 'visi', label: 'Visi' },
  { value: 'parduodama', label: 'Parduodama' },
  { value: 'nuoma', label: 'Nuomai' },
] as const

type RentFilter = (typeof rentFilterOptions)[number]['value']

export const HomePage = () => {
  const { isAuthenticated } = useAuth()
  const [listings, setListings] = useState<Listing[]>([])
  const [viewings, setViewings] = useState<Viewing[]>([])
  const [search, setSearch] = useState('')
  const [rentFilter, setRentFilter] = useState<RentFilter>('visi')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    const loadData = async () => {
      if (!isAuthenticated) {
        setListings([])
        setViewings([])
        return
      }
      setLoading(true)
      setError(null)
      try {
        const [listingsResponse, viewingsResponse] = await Promise.all([
          client.get<Listing[]>('/api/Listings'),
          client.get<Viewing[]>('/api/Viewings'),
        ])
        if (!cancelled) {
          setListings(listingsResponse.data ?? [])
          setViewings(viewingsResponse.data ?? [])
        }
      } catch (err) {
        console.error(err)
        setError('Nepavyko įkelti skelbimų iš API.')
      } finally {
        if (!cancelled) {
          setLoading(false)
        }
      }
    }

    void loadData()
    return () => {
      cancelled = true
    }
  }, [isAuthenticated])

  const filteredListings = useMemo(() => {
    return listings.filter((listing) => {
      const matchesQuery = listing.description.toLowerCase().includes(search.toLowerCase())
      const matchesRent =
        rentFilter === 'visi' || (rentFilter === 'nuoma' ? listing.rent : !listing.rent)
      return matchesQuery && matchesRent
    })
  }, [listings, search, rentFilter])

  return (
    <div className="page home">
      <section className="hero">
        <div>
          <p className="hero__eyebrow">Skaitmeninis brokerių partneris</p>
          <h2>Kurkite, valdykite ir dalinkitės NT skelbimais vienoje erdvėje</h2>
          <p>
            Platforma padeda brokeriams prižiūrėti pastatus, butus, skelbimus ir lankytojų srautus, o
            pirkėjams – rasti informaciją apie atvirų durų dienas bei registruotis privačioms apžiūroms.
          </p>
        </div>
        <div className="hero__card">
          <h3>Greita paieška</h3>
          <input
            type="search"
            placeholder="Aprašymas arba raktažodis"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
          <div className="chip-row">
            {rentFilterOptions.map((option) => (
              <button
                key={option.value}
                type="button"
                className={rentFilter === option.value ? 'chip chip--active' : 'chip'}
                onClick={() => setRentFilter(option.value)}
              >
                {option.label}
              </button>
            ))}
          </div>
          {!isAuthenticated && <p className="hint">Prisijunkite, kad matytumėte gyvus API duomenis.</p>}
        </div>
      </section>

      {error && <p className="error-banner">{error}</p>}

      <section>
        <div className="section-heading">
          <h3>Skelbimų galerija</h3>
          {loading && <span>Kraunama...</span>}
        </div>
        <div className="listing-grid">
          {filteredListings.map((listing) => (
            <article key={listing.idListing} className="card listing-card">
              <div className="listing-card__badge">#{listing.idListing}</div>
              <h4>{listing.description}</h4>
              <p className="listing-card__price">{formatPrice(listing.askingprice)}</p>
              <p className="listing-card__meta">{listing.rent ? 'Nuoma' : 'Pardavimas'}</p>
              <p className="listing-card__note">Nuotraukos ID: {listing.fkPictureid}</p>
            </article>
          ))}
          {!loading && filteredListings.length === 0 && (
            <p className="muted">Nėra skelbimų, atitinkančių filtrus.</p>
          )}
        </div>
      </section>

      <section>
        <div className="section-heading">
          <h3>Atvirų durų dienos</h3>
          <p>Greita brokerių prieinamumo ir suplanuotų vizitų apžvalga.</p>
        </div>
        <div className="timeline">
          {viewings.map((viewing) => (
            <div key={viewing.idViewing} className="timeline__item">
              <div>
                <p className="timeline__date">{formatFriendly(viewing.from)}</p>
                <p className="timeline__subtitle">Trukmė iki {formatFriendly(viewing.to)}</p>
              </div>
              <div className={`status status--${viewing.status}`}>
                Būsena #{viewing.status}
              </div>
            </div>
          ))}
          {!isAuthenticated && <p className="muted">Prisijunkite, kad matytumėte suplanuotas apžiūras.</p>}
        </div>
      </section>
    </div>
  )
}
