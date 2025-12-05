import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { client, publicClient } from '../api/client'
import type { AvailabilitySlot, PublicListingDetails } from '../types/api'
import { formatPrice, translateFinishType, translateHeatingType } from '../utils/text'
import { formatFriendly } from '../utils/dates'
import { useAuth } from '../context/useAuth'
import { resolvePictureSrc } from '../utils/pictures'

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

  const publicViewings = useMemo(() => {
    if (!listing) return []
  const baseViewings = Array.isArray(listing.publicViewings) ? listing.publicViewings : []
  const sorted = [...baseViewings].sort(
      (a, b) => new Date(a.from).getTime() - new Date(b.from).getTime(),
    )

    const fallbackFrom = listing.nextViewingFrom
    if (fallbackFrom) {
      const hasFallback = sorted.some((viewing) => viewing.from === fallbackFrom)
      if (!hasFallback) {
        sorted.unshift({
          id: -1,
          from: fallbackFrom,
          to: listing.nextViewingTo ?? fallbackFrom,
        })
      }
    }

    return sorted
  }, [listing])

  const nextPublicViewingLabel = publicViewings[0]?.from ?? null

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
  const bookingDisabled = !canBook || bookingStatus === 'loading'

  const buildingInfoItems = listing
    ? [
        {
          label: 'Energetinė klasė',
          value: listing.buildingEnergyClass ?? 'Nenurodyta',
        },
        {
          label: 'Aukštų skaičius',
          value: listing.buildingFloors ? `${listing.buildingFloors} aukšt.` : 'Nenurodyta',
        },
        {
          label: 'Statybos metai',
          value: listing.buildingYear ? `${listing.buildingYear} m.` : 'Nenurodyta',
        },
        {
          label: 'Renovacija',
          value: listing.buildingLastRenovationYear
            ? `${listing.buildingLastRenovationYear} m.`
            : 'Informacija nepateikta',
        },
      ]
    : []

  const apartmentInfoItems = listing
    ? [
        {
          label: 'Buto numeris',
          value:
            listing.apartmentNumber !== undefined && listing.apartmentNumber !== null
              ? String(listing.apartmentNumber)
              : '—',
        },
        {
          label: 'Aukštas',
          value: listing.apartmentIsWholeBuilding
            ? 'Visas pastatas'
            : listing.apartmentFloor !== undefined && listing.apartmentFloor !== null
              ? `${listing.apartmentFloor}`
              : 'Nenurodyta',
        },
        {
          label: 'Šildymas',
          value: translateHeatingType(listing.apartmentHeating),
        },
        {
          label: 'Apdaila',
          value: translateFinishType(listing.apartmentFinish),
        },
      ]
    : []

  const apartmentDescription = listing?.apartmentNotes?.trim() || null

  const heroHasImage = lightboxImages.length > 0
  const heroImageSrc = heroHasImage ? lightboxImages[carouselIndex] : null
  const heroEyebrow = listing
    ? listing.buildingCity
      ? `${listing.buildingCity}${listing.buildingAddress ? `, ${listing.buildingAddress}` : ''}`
      : listing.rent
        ? 'Nuomos pasiūlymas'
        : 'Pardavimo pasiūlymas'
    : ''

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
              className={heroImageSrc ? 'details-hero__media' : 'details-hero__media details-hero__media--empty'}
            >
              {!heroImageSrc && <span>Nuotrauka ruošiama</span>}
              {heroImageSrc && (
                <>
                  <img
                    src={heroImageSrc}
                    alt={listing.description}
                    className="details-hero__image"
                  />
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
              <p className="hero__eyebrow">{heroEyebrow}</p>
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
              {nextPublicViewingLabel && (
                <p className="details-note">Artimiausia vieša apžiūra {formatFriendly(nextPublicViewingLabel)}</p>
              )}
            </div>
          </section>

          <section className="details-grid">
            <article className="card">
              <h3>Pastato informacija</h3>
              <ul className="info-list">
                {buildingInfoItems.map((item) => (
                  <li key={item.label}>
                    <span>{item.label}</span>
                    <strong>{item.value}</strong>
                  </li>
                ))}
              </ul>
            </article>
            <article className="card">
              <h3>Buto informacija</h3>
              <ul className="info-list">
                {apartmentInfoItems.map((item) => (
                  <li key={item.label}>
                    <span>{item.label}</span>
                    <strong>{item.value}</strong>
                  </li>
                ))}
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

          {apartmentDescription && (
            <section className="card details-description">
              <h3>Aprašymas</h3>
              <p>{apartmentDescription}</p>
            </section>
          )}

          <section className="card details-public-viewings">
            <h3>Viešos apžiūros</h3>
            {publicViewings.length === 0 ? (
              <p className="muted">Šiam skelbimui šiuo metu viešų apžiūrų nenumatyta.</p>
            ) : (
              <ul className="details-public-viewings__list">
                {publicViewings.map((viewing) => {
                  const endLabel = viewing.to ? formatFriendly(viewing.to) : null
                  return (
                    <li key={viewing.id} className="details-public-viewings__item">
                      <strong>{formatFriendly(viewing.from)}</strong>
                      <span className="muted">{endLabel ? `iki ${endLabel}` : 'Trukmė neviešinama'}</span>
                    </li>
                  )
                })}
              </ul>
            )}
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
                  aria-disabled={bookingDisabled}
                  disabled={bookingDisabled}
                  onClick={handleBooking}
                >
                  {bookingStatus === 'loading' ? 'Siunčiama...' : 'Siųsti užklausą'}
                </button>
                {bookingMessage && (
                  <p className={bookingStatus === 'success' ? 'hint' : 'error-banner'}>{bookingMessage}</p>
                )}
                {selectedSlot && (
                  <p className="muted">
                    Patvirtinus vizitą jo statusą rasite savo apžiūrų skiltyje ({formatFriendly(selectedSlot.from)}).
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
