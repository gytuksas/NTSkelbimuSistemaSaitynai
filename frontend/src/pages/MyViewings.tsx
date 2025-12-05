import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { client } from '../api/client'
import type { BuyerViewing } from '../types/api'
import { useAuth } from '../context/useAuth'
import { formatFriendly } from '../utils/dates'
import { resolvePictureSrc } from '../utils/pictures'
import { translateViewingStatus } from '../utils/text'

const statusVariantMap: Record<string, 'pending' | 'confirmed' | 'rejected' | 'cancelled' | 'public'> = {
  pending: 'pending',
  confirmed: 'confirmed',
  rejected: 'rejected',
  cancelled: 'cancelled',
  public: 'public',
}

const getStatusVariant = (status: string): string => {
  const normalized = status.trim().toLowerCase()
  return statusVariantMap[normalized] ?? 'pending'
}

export const MyViewingsPage = () => {
  const { isAuthenticated, user } = useAuth()
  const isBuyer = isAuthenticated && user?.role === 'Buyer'
  const [viewings, setViewings] = useState<BuyerViewing[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!isBuyer) {
      setViewings([])
      return
    }

    let cancelled = false
    const load = async () => {
      setLoading(true)
      setError(null)
      try {
        const { data } = await client.get<BuyerViewing[]>('/api/Viewings/my')
        if (!cancelled) {
          setViewings(Array.isArray(data) ? data : [])
        }
      } catch (err) {
        console.error(err)
        if (!cancelled) {
          setError('Nepavyko įkelti jūsų apžiūrų sąrašo.')
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
  }, [isBuyer])

  const { upcomingViewings, pastViewings } = useMemo(() => {
    const now = Date.now()
    const upcoming: BuyerViewing[] = []
    const past: BuyerViewing[] = []

    viewings.forEach((viewing) => {
      const toTime = new Date(viewing.to).getTime()
      if (!Number.isNaN(toTime) && toTime >= now) {
        upcoming.push(viewing)
      } else {
        past.push(viewing)
      }
    })

    upcoming.sort((a, b) => new Date(a.from).getTime() - new Date(b.from).getTime())
    past.sort((a, b) => new Date(b.from).getTime() - new Date(a.from).getTime())

    return { upcomingViewings: upcoming, pastViewings: past }
  }, [viewings])

  const renderViewingCard = (viewing: BuyerViewing) => {
    const image = resolvePictureSrc(viewing.pictureUrl ?? undefined, viewing.pictureId ?? undefined)
    const coverStyle = image ? { backgroundImage: `url(${image})` } : undefined
    const location = [viewing.city, viewing.address].filter(Boolean).join(', ')
    const statusVariant = getStatusVariant(viewing.status)
    const translatedStatus = translateViewingStatus(viewing.status)

    return (
      <article key={viewing.id} className="viewing-card">
        <div className={image ? 'viewing-card__media' : 'viewing-card__media viewing-card__media--empty'} style={coverStyle}>
          {!image && <span>Nuotrauka ruošiama</span>}
        </div>
        <div className="viewing-card__body">
          <div className="viewing-card__header">
            <p className="viewing-card__time">{formatFriendly(viewing.from)}</p>
            <span className={`status-chip status-chip--${statusVariant}`}>{translatedStatus}</span>
          </div>
          <h4>{viewing.listingTitle ?? 'Skelbimas'}</h4>
          <p className="muted">{location || 'Adresas netrukus bus pateiktas'}</p>
          <p className="viewing-card__interval">
            {formatFriendly(viewing.from)}
            {' – '}
            {formatFriendly(viewing.to)}
          </p>
          {viewing.brokerName && (
            <p className="muted">
              Brokeris: <strong>{viewing.brokerName}</strong>{' '}
              {viewing.brokerPhone ? (
                <a href={`tel:${viewing.brokerPhone}`} className="viewing-card__phone">
                  {viewing.brokerPhone}
                </a>
              ) : (
                <span>Telefono numeris nepateiktas</span>
              )}
            </p>
          )}
        </div>
        <div className="viewing-card__actions">
          <Link className="btn btn--ghost" to={`/skelbimai/${viewing.listingId}`}>
            Peržiūrėti skelbimą
          </Link>
        </div>
      </article>
    )
  }

  const EmptyState = ({ label, action }: { label: string; action?: ReactNode }) => (
    <div className="empty-state">
      <p className="muted">{label}</p>
      {action}
    </div>
  )

  if (!isAuthenticated) {
    return (
      <div className="page my-viewings">
        <section className="card">
          <h3>Mano apžiūros</h3>
          <p>Prisijunkite prie paskyros, kad galėtumėte peržiūrėti savo rezervacijas.</p>
        </section>
      </div>
    )
  }

  if (!isBuyer) {
    return (
      <div className="page my-viewings">
        <section className="card">
          <h3>Mano apžiūros</h3>
          <p>Ši skiltis prieinama tik pirkėjams. Jei planuojate vizitą, prisijunkite su pirkėjo paskyra.</p>
        </section>
      </div>
    )
  }

  return (
    <div className="page my-viewings">
      <section className="hero">
        <div>
          <p className="hero__eyebrow">Mano apžiūros</p>
          <h2>Stebėkite privačių vizitų būsenas</h2>
          <p>
            Čia rasite visas jūsų rezervuotas privačias apžiūras. Sekite informaciją apie patvirtinimą,
            brokerio kontaktus ir vizito laiką vienoje vietoje.
          </p>
        </div>
        <div className="hero__card">
          <h3>Naujas vizitas?</h3>
          <p>Peržiūrėkite skelbimus ir išsirinkite jums tinkamą laiką.</p>
          <Link className="btn" to="/skelbimai">
            Naršyti skelbimus
          </Link>
        </div>
      </section>

      {error && <p className="error-banner">{error}</p>}

      <section className="card">
        <div className="card__toolbar">
          <h3>Artėjančios apžiūros</h3>
          {loading && <span>Kraunama…</span>}
        </div>
        {upcomingViewings.length > 0 ? (
          <div className="viewing-list">
            {upcomingViewings.map((viewing) => renderViewingCard(viewing))}
          </div>
        ) : (
          <EmptyState
            label="Šiuo metu neturite artėjančių apžiūrų. Pasirinkite laiką naujam vizitui."
            action={
              <Link className="btn btn--ghost" to="/skelbimai">
                Rasti skelbimą
              </Link>
            }
          />
        )}
      </section>

      <section className="card">
        <div className="card__toolbar">
          <h3>Istorija</h3>
        </div>
        {pastViewings.length > 0 ? (
          <div className="viewing-list viewing-list--compact">
            {pastViewings.map((viewing) => renderViewingCard(viewing))}
          </div>
        ) : (
          <EmptyState label="Dar neturite įvykusių apžiūrų." />
        )}
      </section>
    </div>
  )
}
