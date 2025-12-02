import axios from 'axios'
import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { client, publicClient } from '../api/client'
import type { PublicListing, Viewing } from '../types/api'
import { useAuth } from '../context/AuthContext'
import { formatFriendly } from '../utils/dates'

export const ViewingsPage = () => {
  const { user } = useAuth()
  const canSeePrivateSchedule = Boolean(user && (user.role === 'Broker' || user.role === 'Administrator'))
  const [viewings, setViewings] = useState<Viewing[]>([])
  const [publicListings, setPublicListings] = useState<PublicListing[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false

    const fetchPublicListings = async () => {
      const { data } = await publicClient.get<PublicListing[]>('/api/Listings/public')
      if (!cancelled) {
        setPublicListings(data ?? [])
        setViewings([])
      }
    }

    const fetchPrivateViewings = async () => {
      const { data } = await client.get<Viewing[]>('/api/Viewings')
      if (!cancelled) {
        setViewings(data ?? [])
        setPublicListings([])
      }
    }

    const loadData = async () => {
      setLoading(true)
      setError(null)
      try {
        if (canSeePrivateSchedule) {
          await fetchPrivateViewings()
        } else {
          await fetchPublicListings()
        }
      } catch (err) {
        const isForbidden = axios.isAxiosError(err) && err.response?.status === 403
        if (canSeePrivateSchedule && isForbidden) {
          console.warn('Privatus grafikas nepasiekiamas – rodoma vieša versija.')
          await fetchPublicListings()
          setError('Nepavyko parodyti brokerio grafiko – rodoma vieša versija.')
        } else {
          console.error(err)
          setError('Nepavyko gauti apžiūrų grafiko.')
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
  }, [canSeePrivateSchedule])

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
    <div className="page viewings">
      <section className="hero">
        <div>
          <p className="hero__eyebrow">Atvirų durų grafikas</p>
          <h2>Suplanuokite viešus ir privačius vizitus</h2>
          <p>
            Prisijungę brokeriai ir administratoriai mato pilną vidaus grafiką, o svečiai gali naršyti viešai
            skelbiamas apžiūrų datas.
          </p>
        </div>
        <div className="hero__card">
          <h3>Ieškote konkretaus skelbimo?</h3>
          <p>Pasinaudokite galerija ir raskite objektą, kurį norite aplankyti.</p>
          <Link className="btn" to="/skelbimai">
            Peržiūrėti skelbimus
          </Link>
        </div>
      </section>

      {error && <p className="error-banner">{error}</p>}

      <section>
        <div className="section-heading">
          <h3>{canSeePrivateSchedule ? 'Mano suplanuotos apžiūros' : 'Viešos apžiūros'}</h3>
          {loading && <span>Kraunama...</span>}
        </div>
        <div className="timeline">
          {canSeePrivateSchedule
            ? viewings.map((viewing) => (
                <div key={viewing.idViewing} className="timeline__item">
                  <div>
                    <p className="timeline__date">{formatFriendly(viewing.from)}</p>
                    <p className="timeline__subtitle">Trukmė iki {formatFriendly(viewing.to)}</p>
                  </div>
                  <div className={`status status--${viewing.status}`}>Būsena #{viewing.status}</div>
                </div>
              ))
            : publicViewingCards.map((viewing) => (
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

          {!loading && canSeePrivateSchedule && viewings.length === 0 && (
            <p className="muted">Šiuo metu neturite suplanuotų apžiūrų.</p>
          )}
          {!loading && !canSeePrivateSchedule && publicViewingCards.length === 0 && (
            <p className="muted">Šiuo metu neviešinama jokių atvirų durų dienų.</p>
          )}
        </div>
      </section>
    </div>
  )
}
