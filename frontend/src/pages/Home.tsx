import { Fragment, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { client, publicClient, baseURL } from '../api/client'
import type { Listing, PublicListing, Viewing } from '../types/api'
import { useAuth } from '../context/AuthContext'
import { formatPrice } from '../utils/text'
import { formatFriendly } from '../utils/dates'

const rentFilterOptions = [
  { value: 'visi', label: 'Visi' },
  { value: 'parduodama', label: 'Parduodama' },
  { value: 'nuoma', label: 'Nuomai' },
] as const

type RentFilter = (typeof rentFilterOptions)[number]['value']

type ListingCard = {
  id: number
  description: string
  price?: number
  rent: boolean
  pictureId?: string
  buildingCity?: string
  buildingAddress?: string
  nextViewingFrom?: string | null
  nextViewingTo?: string | null
}

export const HomePage = () => {
  const { user } = useAuth()
  const [listings, setListings] = useState<Listing[]>([])
  const [viewings, setViewings] = useState<Viewing[]>([])
  const [publicListings, setPublicListings] = useState<PublicListing[]>([])
  const [search, setSearch] = useState('')
  const [rentFilter, setRentFilter] = useState<RentFilter>('visi')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const canSeePrivateListings = Boolean(user && (user.role === 'Broker' || user.role === 'Administrator'))

  useEffect(() => {
    let cancelled = false
    const loadData = async () => {
      setLoading(true)
      setError(null)
      try {
        if (canSeePrivateListings) {
          const [listingsResponse, viewingsResponse] = await Promise.all([
            client.get<Listing[]>('/api/Listings'),
            client.get<Viewing[]>('/api/Viewings'),
          ])
          if (!cancelled) {
            setListings(listingsResponse.data ?? [])
            setPublicListings([])
            setViewings(viewingsResponse.data ?? [])
          }
        } else {
          const { data } = await publicClient.get<PublicListing[]>('/api/Listings/public')
          if (!cancelled) {
            setPublicListings(data ?? [])
            setListings([])
            setViewings([])
          }
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
  }, [canSeePrivateListings])

  const listingCards = useMemo<ListingCard[]>(() => {
    if (canSeePrivateListings) {
      return listings.map((listing) => ({
        id: listing.idListing,
        description: listing.description,
        price: listing.askingprice,
        rent: listing.rent,
        pictureId: listing.fkPictureid,
      }))
    }
    return publicListings.map((listing) => ({
      id: listing.id,
      description: listing.description,
      price: listing.askingPrice,
      rent: listing.rent,
      pictureId: listing.pictureId ?? undefined,
      buildingCity: listing.buildingCity ?? undefined,
      buildingAddress: listing.buildingAddress ?? undefined,
      nextViewingFrom: listing.nextViewingFrom ?? null,
      nextViewingTo: listing.nextViewingTo ?? null,
    }))
  }, [canSeePrivateListings, listings, publicListings])

  const filteredListings = useMemo(() => {
    return listingCards.filter((listing) => {
      const matchesQuery = listing.description.toLowerCase().includes(search.toLowerCase())
      const matchesRent =
        rentFilter === 'visi' || (rentFilter === 'nuoma' ? listing.rent : !listing.rent)
      return matchesQuery && matchesRent
    })
  }, [listingCards, search, rentFilter])

  const publicViewingCards = useMemo(() => {
    return publicListings
      .filter((listing) => Boolean(listing.nextViewingFrom))
      .map((listing) => ({
        id: listing.id,
        from: listing.nextViewingFrom as string,
        to: listing.nextViewingTo ?? null,
        city: listing.buildingCity ?? undefined,
      }))
      .sort((a, b) => new Date(a.from).getTime() - new Date(b.from).getTime())
  }, [publicListings])

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
          {!canSeePrivateListings && (
            <p className="hint">
              Viešą galeriją rodome svečiams ir pirkėjams. Prisijunkite kaip brokeris arba administratorius, kad
              matytumėte savo portfelį.
            </p>
          )}
        </div>
      </section>

      {error && <p className="error-banner">{error}</p>}

      <section>
        <div className="section-heading">
          <h3>Skelbimų galerija</h3>
          {loading && <span>Kraunama...</span>}
        </div>
        <div className="listing-grid">
          {filteredListings.map((listing) => {
            const coverStyle = listing.pictureId
              ? { backgroundImage: `url(${baseURL}/uploads/${listing.pictureId})` }
              : undefined
            const card = (
              <article
                className={
                  'card listing-card' + (!canSeePrivateListings ? ' listing-card--interactive' : '')
                }
              >
                <div
                  className={
                    listing.pictureId
                      ? 'listing-card__media'
                      : 'listing-card__media listing-card__media--empty'
                  }
                  style={coverStyle}
                >
                  {!listing.pictureId && <span>Nuotrauka ruošiama</span>}
                </div>
                <div className="listing-card__badge">#{listing.id}</div>
                <h4>{listing.description}</h4>
                <p className="listing-card__price">{formatPrice(listing.price)}</p>
                <p className="listing-card__meta">{listing.rent ? 'Nuoma' : 'Pardavimas'}</p>
                {listing.buildingCity && (
                  <p className="listing-card__meta">
                    {listing.buildingCity}
                    {listing.buildingAddress ? ` · ${listing.buildingAddress}` : ''}
                  </p>
                )}
                {listing.nextViewingFrom && (
                  <p className="listing-card__meta">
                    Artimiausia apžiūra {formatFriendly(listing.nextViewingFrom)}
                  </p>
                )}
                {listing.pictureId && !canSeePrivateListings && (
                  <p className="listing-card__note">Nuotrauka #{listing.pictureId}</p>
                )}
                {!canSeePrivateListings && (
                  <div className="listing-card__cta">
                    <span>Žiūrėti detaliau</span>
                    <span aria-hidden="true">→</span>
                  </div>
                )}
              </article>
            )

            return canSeePrivateListings ? (
              <Fragment key={listing.id}>{card}</Fragment>
            ) : (
              <Link key={listing.id} to={`/skelbimai/${listing.id}`} className="listing-card__link">
                {card}
              </Link>
            )
          })}
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
        {canSeePrivateListings ? (
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
            {viewings.length === 0 && <p className="muted">Šiuo metu neturite suplanuotų apžiūrų.</p>}
          </div>
        ) : (
          <div className="timeline">
            {publicViewingCards.map((viewing) => (
              <div key={viewing.id} className="timeline__item">
                <div>
                  <p className="timeline__date">{formatFriendly(viewing.from)}</p>
                  <p className="timeline__subtitle">
                    {viewing.city ? `${viewing.city} · ` : ''}
                    {viewing.to ? `iki ${formatFriendly(viewing.to)}` : 'trukmė neviešinama'}
                  </p>
                </div>
                <div className="status">Vieša apžiūra #{viewing.id}</div>
              </div>
            ))}
            {publicViewingCards.length === 0 && (
              <p className="muted">Šiuo metu neviešinama jokių atvirų durų dienų.</p>
            )}
          </div>
        )}
      </section>
    </div>
  )
}
