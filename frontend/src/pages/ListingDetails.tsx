import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { client, publicClient } from '../api/client'
import type { AvailabilitySlot, PublicListingDetails } from '../types/api'
import { formatPrice } from '../utils/text'
import { formatFriendly } from '../utils/dates'
import { useAuth } from '../context/AuthContext'
import { buildCoverStyle, resolvePictureSrc } from '../utils/pictures'

const makeSlotKey = (slot: AvailabilitySlot) => `${slot.availabilityId}-${slot.from}`

export const ListingDetailsPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { isAuthenticated, user } = useAuth()
  const [listing, setListing] = useState<PublicListingDetails | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [bookingStatus, setBookingStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle')
  const [bookingMessage, setBookingMessage] = useState<string | null>(null)
  const [selectedDate, setSelectedDate] = useState<string | null>(null)
  const [selectedSlotKey, setSelectedSlotKey] = useState<string | null>(null)
  const [lightboxIndex, setLightboxIndex] = useState<number | null>(null)
  const [carouselIndex, setCarouselIndex] = useState(0)

  const applyListing = useCallback((details: PublicListingDetails) => {
    setListing(details)
    if (details.availableSlots.length > 0) {
      const firstSlot = details.availableSlots[0]
      setSelectedDate(firstSlot.from.split('T')[0])
      setSelectedSlotKey(makeSlotKey(firstSlot))
    } else {
      setSelectedDate(null)
      setSelectedSlotKey(null)
    }
  }, [])

  useEffect(() => {
    let cancelled = false
    if (!id) {
      setError('Skelbimo ID nerastas maršrute.')
      setLoading(false)
      return
    }
    const load = async () => {
      setLoading(true)
      setError(null)
      try {
        const { data } = await publicClient.get<PublicListingDetails>(`/api/Listings/public/${id}`)
        if (!cancelled) {
          applyListing(data)
        }
      } catch (err) {
        console.error(err)
        if (!cancelled) {
          setError('Nepavyko įkelti skelbimo detalių.')
        }
      } finally {
        if (!cancelled) {
          setLoading(false)
        }
      }
    }

    void load()
    return () => {
      cancelled = true
    }
  }, [id, applyListing])

  const slotsData = useMemo(() => {
    const groups = new Map<string, AvailabilitySlot[]>()
    if (listing) {
      listing.availableSlots.forEach((slot) => {
        const dateKey = slot.from.split('T')[0]
        const bucket = groups.get(dateKey)
        if (bucket) {
          bucket.push(slot)
        } else {
          groups.set(dateKey, [slot])
        }
      })
      groups.forEach((bucket) =>
        bucket.sort((a, b) => new Date(a.from).getTime() - new Date(b.from).getTime()),
      )
    }
    const dates = Array.from(groups.keys()).sort(
      (a, b) => new Date(a).getTime() - new Date(b).getTime(),
    )
    return { groups, dates }
  }, [listing])

  const slotsByDate = slotsData.groups
  const dateOptions = slotsData.dates
  const slotsForSelectedDate = selectedDate ? slotsByDate.get(selectedDate) ?? [] : []

  const dateFormatter = useMemo(
    () =>
      new Intl.DateTimeFormat('lt-LT', {
        weekday: 'short',
        month: 'short',
        day: 'numeric',
      }),
    [],
  )

  const formatDateLabel = useCallback((value: string) => dateFormatter.format(new Date(value)), [dateFormatter])

  const handleDateSelect = useCallback(
    (date: string) => {
      setSelectedDate(date)
      const firstSlot = slotsByDate.get(date)?.[0] ?? null
      setSelectedSlotKey(firstSlot ? makeSlotKey(firstSlot) : null)
    },
    [slotsByDate],
  )

  const selectedSlot = useMemo(() => {
    if (!listing || !selectedSlotKey) {
      return null
    }
    return listing.availableSlots.find((slot) => makeSlotKey(slot) === selectedSlotKey) ?? null
  }, [listing, selectedSlotKey])

  const timeFormatter = useMemo(
    () =>
      new Intl.DateTimeFormat('lt-LT', {
        hour: '2-digit',
        minute: '2-digit',
      }),
    [],
  )

  const formatSlotRange = useCallback(
    (slot: AvailabilitySlot) =>
      `${timeFormatter.format(new Date(slot.from))} – ${timeFormatter.format(new Date(slot.to))}`,
    [timeFormatter],
  )

  const heroImage = useMemo(
    () => resolvePictureSrc(listing?.pictureUrl ?? undefined, listing?.pictureId ?? undefined) ?? null,
    [listing?.pictureUrl, listing?.pictureId],
  )

  const resolvedGalleryImages = useMemo(() => {
    if (!listing) return []
    const useUrls = listing.galleryPictureUrls.length > 0
    const sources = useUrls ? listing.galleryPictureUrls : listing.galleryPictureIds
    return sources
      .map((source) =>
        useUrls ? resolvePictureSrc(source, undefined) : resolvePictureSrc(undefined, source),
      )
      .filter((src): src is string => Boolean(src))
  }, [listing])

  const lightboxImages = useMemo(() => {
    const ordered = heroImage ? [heroImage, ...resolvedGalleryImages] : [...resolvedGalleryImages]
    return Array.from(new Set(ordered))
  }, [heroImage, resolvedGalleryImages])

  useEffect(() => {
    if (lightboxImages.length === 0) {
      setCarouselIndex(0)
      return
    }
    setCarouselIndex((prev) => (prev >= lightboxImages.length ? 0 : prev))
  }, [lightboxImages.length])

  const openLightbox = useCallback(
    (index: number) => {
      if (!lightboxImages.length) return
      setLightboxIndex((index + lightboxImages.length) % lightboxImages.length)
    },
    [lightboxImages.length],
  )

  const closeLightbox = useCallback(() => setLightboxIndex(null), [])

  const showNext = useCallback(() => {
    if (!lightboxImages.length) return
    setLightboxIndex((prev) => {
      if (prev === null) return null
      return (prev + 1) % lightboxImages.length
    })
  }, [lightboxImages.length])

  const showPrev = useCallback(() => {
    if (!lightboxImages.length) return
    setLightboxIndex((prev) => {
      if (prev === null) return null
      return (prev - 1 + lightboxImages.length) % lightboxImages.length
    })
  }, [lightboxImages.length])

  const showHeroNext = useCallback(() => {
    if (!lightboxImages.length) return
    setCarouselIndex((prev) => (prev + 1) % lightboxImages.length)
  }, [lightboxImages.length])

  const showHeroPrev = useCallback(() => {
    if (!lightboxImages.length) return
    setCarouselIndex((prev) => (prev - 1 + lightboxImages.length) % lightboxImages.length)
  }, [lightboxImages.length])

  useEffect(() => {
    if (lightboxImages.length === 0 && lightboxIndex !== null) {
      setLightboxIndex(null)
    }
  }, [lightboxImages.length, lightboxIndex])

  useEffect(() => {
    if (lightboxIndex === null) return
    const handler = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setLightboxIndex(null)
      } else if (event.key === 'ArrowRight') {
        showNext()
      } else if (event.key === 'ArrowLeft') {
        showPrev()
      }
    }
    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [lightboxIndex, showNext, showPrev])

  const handleBooking = async () => {
    if (!listing) return
    const slot = selectedSlot
    if (!slot) return

    if (!isAuthenticated || user?.role !== 'Buyer') {
      setBookingStatus('error')
      setBookingMessage('Prisijunkite kaip pirkėjas, kad galėtumėte rezervuoti apžiūrą.')
      return
    }

    setBookingStatus('loading')
    setBookingMessage(null)
    try {
      const confirmationLabel = formatFriendly(slot.from)
      await client.post('/api/Viewings', {
        from: slot.from,
        to: slot.to,
        status: 1,
        fkAvailabilityidAvailability: slot.availabilityId,
        fkListingidListing: listing.id,
      })
      const { data } = await publicClient.get<PublicListingDetails>(`/api/Listings/public/${listing.id}`)
      applyListing(data)
      setBookingStatus('success')
      setBookingMessage(`Prašymas pateiktas (${confirmationLabel}). Brokeris netrukus patvirtins jūsų apžiūrą.`)
    } catch (err) {
      console.error(err)
      setBookingStatus('error')
      setBookingMessage('Rezervacijos pateikti nepavyko. Pabandykite dar kartą.')
    }
  }

  const canBook = Boolean(isAuthenticated && user?.role === 'Buyer' && selectedSlot)

  const heroHasImage = lightboxImages.length > 0
  const heroStyle = heroHasImage
    ? { backgroundImage: `url(${lightboxImages[carouselIndex]})` }
    : buildCoverStyle(listing?.pictureUrl, listing?.pictureId)

  return (
    <div className="page listing-details">
      <button type="button" className="btn btn--ghost" onClick={() => navigate(-1)}>
        ← Grįžti
      </button>

      {loading && <p>Kraunama...</p>}
      {error && <p className="error-banner">{error}</p>}

      {!loading && !error && listing && (
        <>
          <section className="card details-hero">
            <div
              className={heroHasImage ? 'details-hero__media' : 'details-hero__media details-hero__media--empty'}
              style={heroStyle}
            >
              {!heroHasImage && <span>Nuotrauka ruošiama</span>}
              {heroHasImage && (
                <>
                  {lightboxImages.length > 1 && (
                    <>
                      <button
                        type="button"
                        className="details-hero__nav details-hero__nav--prev"
                        onClick={showHeroPrev}
                        aria-label="Ankstesnė nuotrauka"
                      >
                        ‹
                      </button>
                      <button
                        type="button"
                        className="details-hero__nav details-hero__nav--next"
                        onClick={showHeroNext}
                        aria-label="Kita nuotrauka"
                      >
                        ›
                      </button>
                    </>
                  )}
                  <button
                    type="button"
                    className="details-hero__lightbox-trigger"
                    onClick={() => openLightbox(carouselIndex)}
                    aria-label="Padidinti nuotrauką"
                  >
                    <span className="details-hero__zoom">Padidinti</span>
                  </button>
                </>
              )}
            </div>
            <div className="details-hero__content">
              <p className="hero__eyebrow">Skelbimas #{listing.id}</p>
              <h2>{listing.description}</h2>
              <p className="details-price">{formatPrice(listing.askingPrice)}</p>
              <p className="details-chip">{listing.rent ? 'Nuomai' : 'Pardavimui'}</p>
              <ul className="details-meta">
                <li>
                  <span>Kvadratūra</span>
                  <strong>{listing.apartmentArea ? `${listing.apartmentArea} m²` : 'Nenurodyta'}</strong>
                </li>
                <li>
                  <span>Kambariai</span>
                  <strong>{listing.rooms ?? '—'}</strong>
                </li>
                <li>
                  <span>Adresas</span>
                  <strong>
                    {listing.buildingCity}
                    {listing.buildingAddress ? `, ${listing.buildingAddress}` : ''}
                  </strong>
                </li>
              </ul>
              {listing.nextViewingFrom && (
                <p className="details-note">Artimiausia vieša apžiūra {formatFriendly(listing.nextViewingFrom)}</p>
              )}
            </div>
          </section>

          <section className="details-grid">
            <article className="card">
              <h3>Buto informacija</h3>
              <ul className="info-list">
                <li>
                  <span>Butas ID</span>
                  <strong>{listing.apartmentId ?? '—'}</strong>
                </li>
                <li>
                  <span>Pastatas ID</span>
                  <strong>{listing.buildingId ?? '—'}</strong>
                </li>
                <li>
                  <span>Miestas</span>
                  <strong>{listing.buildingCity ?? '—'}</strong>
                </li>
              </ul>
            </article>
            <article className="card">
              <h3>Brokerio kontaktai</h3>
              <p className="details-broker">{listing.brokerName ?? 'Nepriskirtas brokeris'}</p>
              {listing.brokerPhone ? (
                <a className="details-phone" href={`tel:${listing.brokerPhone}`}>
                  {listing.brokerPhone}
                </a>
              ) : (
                <p className="muted">Telefono numeris neprieinamas</p>
              )}
            </article>
          </section>


          <section className="details-booking">
            <article className="card booking-card">
              <div>
                <h3>Rezervuoti privačią apžiūrą</h3>
                <p>Pasirinkite jums tinkamą brokerio laiką ir atsiųskite užklausą.</p>
              </div>
              <div className="calendar">
                {listing.availableSlots.length === 0 ? (
                  <p className="muted">Brokeris dar nepaskelbė savo prieinamumo.</p>
                ) : (
                  <>
                    <div className="calendar__dates">
                      {dateOptions.map((date) => (
                        <button
                          key={date}
                          type="button"
                          className={
                            date === selectedDate ? 'calendar__date calendar__date--active' : 'calendar__date'
                          }
                          onClick={() => handleDateSelect(date)}
                        >
                          {formatDateLabel(date)}
                        </button>
                      ))}
                    </div>
                    <div className="calendar__slots">
                      {slotsForSelectedDate.length === 0 ? (
                        <p className="muted">Šiai dienai nebėra laisvų laikų.</p>
                      ) : (
                        slotsForSelectedDate.map((slot) => {
                          const key = makeSlotKey(slot)
                          const isActive = selectedSlotKey === key
                          return (
                            <button
                              key={key}
                              type="button"
                              className={isActive ? 'calendar__slot calendar__slot--active' : 'calendar__slot'}
                              onClick={() => setSelectedSlotKey(key)}
                            >
                              {formatSlotRange(slot)}
                            </button>
                          )
                        })
                      )}
                    </div>
                  </>
                )}
              </div>
              <div className="booking-actions">
                {!isAuthenticated && (
                  <p className="muted">Prisijunkite, kad tęstumėte rezervaciją.</p>
                )}
                {isAuthenticated && user?.role !== 'Buyer' && (
                  <p className="muted">Rezervacijos prieinamos tik pirkėjų paskyroms.</p>
                )}
                <button
                  type="button"
                  className="btn"
                  disabled={!canBook || bookingStatus === 'loading'}
                  onClick={handleBooking}
                >
                  {bookingStatus === 'loading' ? 'Siunčiama...' : 'Siųsti užklausą'}
                </button>
                {bookingMessage && (
                  <p className={bookingStatus === 'success' ? 'hint' : 'error-banner'}>{bookingMessage}</p>
                )}
                {selectedSlot && (
                  <p className="muted">
                    Patvirtinus vizitą gausite el. laišką su informacija apie {formatFriendly(selectedSlot.from)}.
                  </p>
                )}
              </div>
            </article>
          </section>

          {lightboxIndex !== null && lightboxImages[lightboxIndex] && (
            <div className="lightbox" role="dialog" aria-modal="true">
              <button
                type="button"
                className="lightbox__close"
                onClick={closeLightbox}
                aria-label="Uždaryti galeriją"
              >
                ×
              </button>
              {lightboxImages.length > 1 && (
                <>
                  <button
                    type="button"
                    className="lightbox__nav lightbox__nav--prev"
                    onClick={showPrev}
                    aria-label="Ankstesnė nuotrauka"
                  >
                    ‹
                  </button>
                  <button
                    type="button"
                    className="lightbox__nav lightbox__nav--next"
                    onClick={showNext}
                    aria-label="Kita nuotrauka"
                  >
                    ›
                  </button>
                </>
              )}
              <img
                src={lightboxImages[lightboxIndex]}
                alt="Skelbimo nuotrauka"
                className="lightbox__image"
              />
            </div>
          )}
        </>
      )}
    </div>
  )
}
