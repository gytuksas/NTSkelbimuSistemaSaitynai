import axios from 'axios'
import { Fragment, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { client, publicClient } from '../api/client'
import type { Listing, PublicListing } from '../types/api'
import { useAuth } from '../context/useAuth'
import { formatPrice } from '../utils/text'
import { formatFriendly } from '../utils/dates'
import { resolvePictureSrc } from '../utils/pictures'

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
  pictureUrl?: string
  buildingCity?: string
  buildingAddress?: string
  nextViewingFrom?: string | null
  nextViewingTo?: string | null
}

const describeListingBadge = (listing: ListingCard) => {
  if (listing.buildingCity) {
    return listing.buildingAddress ? `${listing.buildingCity} · ${listing.buildingAddress}` : listing.buildingCity
  }
  if (listing.nextViewingFrom) {
    return `Artimiausia apžiūra ${formatFriendly(listing.nextViewingFrom)}`
  }
  return listing.rent ? 'Nuomos pasiūlymas' : 'Pardavimo pasiūlymas'
}

export const ListingsPage = () => {
  const { user } = useAuth()
  const [listings, setListings] = useState<Listing[]>([])
  const [publicListings, setPublicListings] = useState<PublicListing[]>([])
  const [search, setSearch] = useState('')
  const [rentFilter, setRentFilter] = useState<RentFilter>('visi')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const canSeePrivateListings = Boolean(user && (user.role === 'Broker' || user.role === 'Administrator'))

  useEffect(() => {
    let cancelled = false
    const fetchPublicListings = async () => {
      const { data } = await publicClient.get<PublicListing[]>('/api/Listings/public')
      if (!cancelled) {
        setPublicListings(data ?? [])
        setListings([])
      }
    }

    const fetchPrivateListings = async () => {
      const { data } = await client.get<Listing[]>('/api/Listings')
      if (!cancelled) {
        setListings(data ?? [])
        setPublicListings([])
      }
    }

    const loadData = async () => {
      setLoading(true)
      setError(null)
      try {
        if (canSeePrivateListings) {
          await fetchPrivateListings()
        } else {
          await fetchPublicListings()
        }
      } catch (err) {
        const isForbidden = axios.isAxiosError(err) && err.response?.status === 403
        if (canSeePrivateListings && isForbidden) {
          console.warn('Privatūs ištekliai nepasiekiami – rodoma vieša galerija.')
          await fetchPublicListings()
          setError('Brokerio skydelis nepasiekiamas – rodoma vieša galerija.')
        } else {
          console.error(err)
          setError('Nepavyko įkelti skelbimų iš API.')
        }
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
        pictureUrl: resolvePictureSrc(undefined, listing.fkPictureid),
      }))
    }
    return publicListings.map((listing) => ({
      id: listing.id,
      description: listing.description,
      price: listing.askingPrice,
      rent: listing.rent,
      pictureId: listing.pictureId ?? undefined,
      pictureUrl: resolvePictureSrc(listing.pictureUrl ?? undefined, listing.pictureId ?? undefined),
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

  return (
    <div className="page listings">
      <section className="hero">
        <div>
          <p className="hero__eyebrow">Skelbimų valdymas</p>
          <h2>Peržiūrėkite ir filtruokite aktyvius NT pasiūlymus</h2>
          <p>
            Svečiai mato viešą galeriją, o prisijungę brokeriai ir administratoriai – pilną portfelį
            su vidaus nuotraukomis.
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
              Viešą galeriją rodome svečiams ir pirkėjams. Prisijunkite kaip brokeris arba administratorius,
              kad matytumėte savo portfelį.
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
            const card = (
              <article
                className={
                  'card listing-card' + (!canSeePrivateListings ? ' listing-card--interactive' : '')
                }
              >
                <div
                  className={
                    listing.pictureUrl
                      ? 'listing-card__media'
                      : 'listing-card__media listing-card__media--empty'
                  }
                >
                  {listing.pictureUrl ? (
                    <img
                      src={listing.pictureUrl}
                      alt={listing.description}
                      className="listing-card__image"
                      loading="lazy"
                    />
                  ) : (
                    <span>Nuotrauka ruošiama</span>
                  )}
                </div>
                <div className="listing-card__badge">{describeListingBadge(listing)}</div>
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

      {!canSeePrivateListings && (
        <section className="card">
          <h3>Noriu pamatyti atvirų durų grafiką</h3>
          <p>Viešos apžiūrų datos dabar perkeltos į atskirą puslapį.</p>
          <Link className="btn" to="/apziuros">
            Eiti į apžiūrų kalendorių
          </Link>
        </section>
      )}
    </div>
  )
}
