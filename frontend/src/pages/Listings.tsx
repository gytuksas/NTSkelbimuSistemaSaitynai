import { useCallback, useEffect, useLayoutEffect, useMemo, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { publicClient } from '../api/client'
import { PaginationControls } from '../components/PaginationControls'
import { usePagination } from '../hooks/usePagination'
import type { PublicListing } from '../types/api'
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
  const [publicListings, setPublicListings] = useState<PublicListing[]>([])
  const [search, setSearch] = useState('')
  const [rentFilter, setRentFilter] = useState<RentFilter>('visi')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [maxCardHeight, setMaxCardHeight] = useState(0)
  const cardRefs = useRef(new Map<number, HTMLDivElement>())
  const rafRef = useRef<number | null>(null)

  useEffect(() => {
    let cancelled = false

    const loadData = async () => {
      setLoading(true)
      setError(null)
      try {
        const { data } = await publicClient.get<PublicListing[]>('/api/Listings/public')
        if (!cancelled) {
          setPublicListings(data ?? [])
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
  }, [])

  const listingCards = useMemo<ListingCard[]>(() => {
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
  }, [publicListings])

  const filteredListings = useMemo(() => {
    return listingCards.filter((listing) => {
      const matchesQuery = listing.description.toLowerCase().includes(search.toLowerCase())
      const matchesRent =
        rentFilter === 'visi' || (rentFilter === 'nuoma' ? listing.rent : !listing.rent)
      return matchesQuery && matchesRent
    })
  }, [listingCards, search, rentFilter])

  const listingPagination = usePagination(filteredListings)
  const displayedListings = listingPagination.pageItems
  const showPagination = filteredListings.length > 0
  const listingCardStyle = useMemo(() => (maxCardHeight ? { minHeight: maxCardHeight } : undefined), [maxCardHeight])
  const placeholderCount = filteredListings.length === 0 ? 0 : Math.max(0, listingPagination.pageSize - displayedListings.length)

  const recalcCardHeights = useCallback(() => {
    if (typeof window === 'undefined') {
      return
    }

    if (rafRef.current) {
      window.cancelAnimationFrame(rafRef.current)
    }

    rafRef.current = window.requestAnimationFrame(() => {
      let tallest = 0
      cardRefs.current.forEach((element) => {
        if (element) {
          tallest = Math.max(tallest, element.offsetHeight)
        }
      })
      setMaxCardHeight(tallest)
      rafRef.current = null
    })
  }, [])

  const registerCardRef = useCallback(
    (id: number) => (element: HTMLDivElement | null) => {
      if (element) {
        cardRefs.current.set(id, element)
      } else {
        cardRefs.current.delete(id)
      }
      recalcCardHeights()
    },
    [recalcCardHeights],
  )

  useLayoutEffect(() => {
    recalcCardHeights()
  }, [displayedListings, recalcCardHeights])

  useEffect(() => {
    if (typeof window === 'undefined') {
      return
    }
    const handleResize = () => {
      recalcCardHeights()
    }
    window.addEventListener('resize', handleResize)
    return () => {
      window.removeEventListener('resize', handleResize)
    }
  }, [recalcCardHeights])

  useEffect(() => {
    return () => {
      if (typeof window !== 'undefined' && rafRef.current) {
        window.cancelAnimationFrame(rafRef.current)
      }
    }
  }, [])

  return (
    <div className="page listings">
      <section className="hero">
        <div>
          <p className="hero__eyebrow">Skelbimų galerija</p>
          <h2>Peržiūrėkite ir filtruokite aktyvius NT pasiūlymus</h2>
          <p>Viešas katalogas pasiekiamas visiems lankytojams – raskite jus dominantį objektą akimirksniu.</p>
        </div>
        <div className="hero__card">
          <h3>Greita paieška</h3>
          <input
            type="search"
            placeholder="Aprašymas arba raktažodis"
            value={search}
            onChange={(event) => {
              setSearch(event.target.value)
              listingPagination.reset()
            }}
          />
          <br></br>
          <div className="chip-row">
            {rentFilterOptions.map((option) => (
              <button
                key={option.value}
                type="button"
                className={rentFilter === option.value ? 'chip chip--active' : 'chip'}
                onClick={() => {
                  if (rentFilter !== option.value) {
                    setRentFilter(option.value)
                    listingPagination.reset()
                  }
                }}
              >
                {option.label}
              </button>
            ))}
          </div>
          <p className="hint">Viešą galeriją gali naršyti visi – atraskite tinkamiausią pasiūlymą arba pasikalbėkite su mūsų brokeriais.</p>
        </div>
      </section>

      {error && <p className="error-banner">{error}</p>}

      <section>
        <div className="section-heading">
          <h3>Skelbimų galerija</h3>
          {loading && <span>Kraunama...</span>}
        </div>
        <div className="listing-grid">
          {displayedListings.map((listing) => {
            const card = (
              <article
                ref={registerCardRef(listing.id)}
                className="card listing-card listing-card--interactive"
                style={listingCardStyle}
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
                      onLoad={recalcCardHeights}
                      onError={recalcCardHeights}
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
                <div className="listing-card__cta">
                  <span>Žiūrėti detaliau</span>
                  <span aria-hidden="true">→</span>
                </div>
              </article>
            )

            return (
              <Link key={listing.id} to={`/skelbimai/${listing.id}`} className="listing-card__link">
                {card}
              </Link>
            )
          })}
          {placeholderCount > 0 &&
            Array.from({ length: placeholderCount }, (_, index) => (
              <article
                key={`listing-placeholder-${index}`}
                className="card listing-card listing-card--placeholder"
                style={listingCardStyle}
                aria-hidden="true"
              >
                <div className="listing-card__media listing-card__media--empty listing-card__media--placeholder">
                  <span></span>
                </div>
                <div className="listing-card__placeholder-lines">
                  <span className="listing-card__placeholder-line listing-card__placeholder-line--long"></span>
                  <span className="listing-card__placeholder-line listing-card__placeholder-line--medium"></span>
                  <span className="listing-card__placeholder-line listing-card__placeholder-line--short"></span>
                </div>
              </article>
            ))}
          {!loading && filteredListings.length === 0 && (
            <p className="muted">Nėra skelbimų, atitinkančių filtrus.</p>
          )}
        </div>
        {showPagination && (
          <PaginationControls
            page={listingPagination.page}
            pageSize={listingPagination.pageSize}
            totalItems={listingPagination.totalItems}
            totalPages={listingPagination.totalPages}
            rangeStart={listingPagination.rangeStart}
            rangeEnd={listingPagination.rangeEnd}
            pageSizeOptions={listingPagination.pageSizeOptions}
            onPageChange={listingPagination.goToPage}
            onPageSizeChange={listingPagination.setPageSize}
          />
        )}
      </section>
    </div>
  )
}
