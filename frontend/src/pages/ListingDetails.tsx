import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { client, publicClient, baseURL } from '../api/client'
import type { PublicAvailability, PublicListingDetails } from '../types/api'
import { formatPrice } from '../utils/text'
import { formatFriendly } from '../utils/dates'
import { useAuth } from '../context/AuthContext'

const buildCoverStyle = (pictureId?: string | null) =>
  pictureId
    ? {
        backgroundImage: `url(${baseURL}/uploads/${pictureId})`,
      }
    : undefined

export const ListingDetailsPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { isAuthenticated, user } = useAuth()
  const [listing, setListing] = useState<PublicListingDetails | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [selectedAvailability, setSelectedAvailability] = useState<number | null>(null)
  const [bookingStatus, setBookingStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle')
  const [bookingMessage, setBookingMessage] = useState<string | null>(null)

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
          setListing(data)
          setSelectedAvailability(data.availabilities[0]?.id ?? null)
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
  }, [id])

  const handleBooking = async () => {
    if (!listing || !selectedAvailability) return
    const slot = listing.availabilities.find((availability) => availability.id === selectedAvailability)
    if (!slot) return

    if (!isAuthenticated || user?.role !== 'Buyer') {
      setBookingStatus('error')
      setBookingMessage('Prisijunkite kaip pirkėjas, kad galėtumėte rezervuoti apžiūrą.')
      return
    }

    setBookingStatus('loading')
    setBookingMessage(null)
    try {
      await client.post('/api/Viewings', {
        from: slot.from,
        to: slot.to,
        status: 1,
        fkAvailabilityidAvailability: slot.id,
        fkListingidListing: listing.id,
      })
      setBookingStatus('success')
      setBookingMessage('Prašymas pateiktas! Brokeris netrukus patvirtins jūsų apžiūrą.')
    } catch (err) {
      console.error(err)
      setBookingStatus('error')
      setBookingMessage('Rezervacijos pateikti nepavyko. Pabandykite dar kartą.')
    }
  }

  const availabilityChips = useMemo(() => {
    if (!listing) return []
    return listing.availabilities.map((availability) => ({
      id: availability.id,
      label: `${formatFriendly(availability.from)} – ${formatFriendly(availability.to)}`,
    }))
  }, [listing])

  const canBook = Boolean(isAuthenticated && user?.role === 'Buyer' && selectedAvailability)

  const selectedSlot: PublicAvailability | undefined = listing?.availabilities.find(
    (availability) => availability.id === selectedAvailability,
  )

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
            <div className={listing.pictureId ? 'details-hero__media' : 'details-hero__media details-hero__media--empty'} style={buildCoverStyle(listing.pictureId)}>
              {!listing.pictureId && <span>Nuotrauka ruošiama</span>}
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

          {listing.galleryPictureIds.length > 1 && (
            <section>
              <h3>Nuotraukų galerija</h3>
              <div className="gallery-grid">
                {listing.galleryPictureIds.map((pictureId) => (
                  <div key={pictureId} className="gallery-grid__item" style={buildCoverStyle(pictureId)}>
                    {!pictureId && <span>Nuotrauka</span>}
                  </div>
                ))}
              </div>
            </section>
          )}

          <section className="details-booking">
            <article className="card booking-card">
              <div>
                <h3>Rezervuoti privačią apžiūrą</h3>
                <p>Pasirinkite jums tinkamą brokerio laiką ir atsiųskite užklausą.</p>
              </div>
              <div className="availability-list">
                {availabilityChips.length === 0 && <p className="muted">Brokeris dar nepaskelbė savo prieinamumo.</p>}
                {availabilityChips.map((chip) => (
                  <button
                    key={chip.id}
                    type="button"
                    className={chip.id === selectedAvailability ? 'availability-card availability-card--active' : 'availability-card'}
                    onClick={() => setSelectedAvailability(chip.id)}
                  >
                    <p>{chip.label}</p>
                  </button>
                ))}
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
        </>
      )}
    </div>
  )
}
