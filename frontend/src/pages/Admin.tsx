import { useEffect, useMemo, useState } from 'react'
import { client } from '../api/client'
import { PaginationControls } from '../components/PaginationControls'
import { usePagination } from '../hooks/usePagination'
import type { AppUser, Broker, Buyer, Listing } from '../types/api'
import { useAuth } from '../context/useAuth'
import { formatFriendly } from '../utils/dates'
import { formatPrice } from '../utils/text'

export const AdminPage = () => {
  const { user } = useAuth()
  const isAdmin = user?.role === 'Administrator'
  const [users, setUsers] = useState<AppUser[]>([])
  const [brokers, setBrokers] = useState<Broker[]>([])
  const [buyers, setBuyers] = useState<Buyer[]>([])
  const [listings, setListings] = useState<Listing[]>([])
  const [message, setMessage] = useState<string | null>(null)
  const [previewListingId, setPreviewListingId] = useState<number | null>(null)
  const userDirectory = useMemo(() => new Map(users.map((entry) => [entry.idUser, entry])), [users])
  const userPagination = usePagination(users)
  const listingsPagination = usePagination(listings)

  useEffect(() => {
    const loadAdminData = async () => {
      if (!isAdmin) return
      try {
        const [usersRes, brokersRes, buyersRes, listingsRes] = await Promise.all([
          client.get<AppUser[]>('/api/Users'),
          client.get<Broker[]>('/api/Brokers'),
          client.get<Buyer[]>('/api/Buyers'),
          client.get<Listing[]>('/api/Listings'),
        ])
        setUsers(usersRes.data ?? [])
        setBrokers(brokersRes.data ?? [])
        setBuyers(buyersRes.data ?? [])
        setListings(listingsRes.data ?? [])
      } catch (error) {
        console.error(error)
        setMessage('Nepavyko užkrauti administratoriaus duomenų.')
      }
    }

    void loadAdminData()
  }, [isAdmin])

  const toggleBrokerField = async (id: number, field: 'confirmed' | 'blocked', value: boolean) => {
    try {
      await client.patch(`/api/Brokers/${id}`, { [field]: value })
      setBrokers((prev) => prev.map((broker) => (broker.idUser === id ? { ...broker, [field]: value } : broker)))
    } catch (error) {
      console.error(error)
      setMessage('Brokerio būsena nepakoreguota.')
    }
  }

  const toggleBuyerField = async (id: number, field: 'confirmed' | 'blocked', value: boolean) => {
    try {
      await client.patch(`/api/Buyers/${id}`, { [field]: value })
      setBuyers((prev) => prev.map((buyer) => (buyer.idUser === id ? { ...buyer, [field]: value } : buyer)))
    } catch (error) {
      console.error(error)
      setMessage('Nepavyko pakeisti pirkėjo būsenos.')
    }
  }

  const deleteListing = async (id: number) => {
    try {
      await client.delete(`/api/Listings/${id}`)
      setListings((prev) => prev.filter((listing) => listing.idListing !== id))
    } catch (error) {
      console.error(error)
      setMessage('Skelbimo pašalinti nepavyko.')
    }
  }

  const handlePreview = (listingId: number) => {
    setPreviewListingId(listingId)
  }

  const closePreview = () => setPreviewListingId(null)

  if (!isAdmin) {
    return <p className="muted">Norint pasiekti skiltį, reikalinga administratoriaus rolė.</p>
  }

  return (
    <div className="page admin">
      {message && <p className="hint">{message}</p>}

      <section className="card">
        <h3>Naudotojai</h3>
        <div className="table">
          <div className="table__row table__row--head">
            <span>Naudotojas</span>
            <span>El. paštas</span>
            <span>Telefonas</span>
            <span>Registracija</span>
          </div>
          {userPagination.pageItems.map((item) => (
            <div key={item.idUser} className="table__row">
              <span>
                {item.name} {item.surname}
              </span>
              <span>{item.email}</span>
              <span>{item.phone || '—'}</span>
              <span>{formatFriendly(item.registrationtime)}</span>
            </div>
          ))}
          {userPagination.totalItems === 0 && <p className="muted">Naudotojų nėra.</p>}
        </div>
        {userPagination.totalItems > 0 && (
          <PaginationControls
            page={userPagination.page}
            pageSize={userPagination.pageSize}
            totalItems={userPagination.totalItems}
            totalPages={userPagination.totalPages}
            rangeStart={userPagination.rangeStart}
            rangeEnd={userPagination.rangeEnd}
            pageSizeOptions={userPagination.pageSizeOptions}
            onPageChange={userPagination.goToPage}
            onPageSizeChange={userPagination.setPageSize}
          />
        )}
      </section>

      <section className="grid-two">
        <div className="card">
          <h3>Brokerių patvirtinimai</h3>
          {brokers.map((broker) => {
            const profile = userDirectory.get(broker.idUser)
            return (
              <div key={broker.idUser} className="admin-toggle">
                <div>
                  <p>
                    {profile ? `${profile.name} ${profile.surname}` : 'Neidentifikuotas brokeris'}
                  </p>
                  <p className="muted">
                    {profile?.email ?? 'El. paštas neprieinamas'}
                    {profile?.phone ? ` · ${profile.phone}` : ''}
                  </p>
                  <p className="muted">
                    {broker.confirmed ? 'Patvirtintas' : 'Laukia patvirtinimo'} ·{' '}
                    {broker.blocked ? 'Blokuotas' : 'Aktyvus'}
                  </p>
                </div>
              <div className="admin-toggle__actions">
                <button className="btn btn--ghost" onClick={() => toggleBrokerField(broker.idUser, 'confirmed', !broker.confirmed)}>
                  {broker.confirmed ? 'Atšaukti patvirtinimą' : 'Patvirtinti'}
                </button>
                <button className="btn btn--ghost" onClick={() => toggleBrokerField(broker.idUser, 'blocked', !broker.blocked)}>
                  {broker.blocked ? 'Atblokuoti' : 'Blokuoti'}
                </button>
              </div>
              </div>
            )
          })}
        </div>

        <div className="card">
          <h3>Pirkėjų statusas</h3>
          {buyers.map((buyer) => {
            const profile = userDirectory.get(buyer.idUser)
            return (
              <div key={buyer.idUser} className="admin-toggle">
                <div>
                  <p>{profile ? `${profile.name} ${profile.surname}` : 'Neidentifikuotas pirkėjas'}</p>
                  <p className="muted">
                    {profile?.email ?? 'El. paštas neprieinamas'}
                    {profile?.phone ? ` · ${profile.phone}` : ''}
                  </p>
                  <p className="muted">
                    {buyer.confirmed ? 'Dokumentai patvirtinti' : 'Laukia tapatybės'} ·{' '}
                    {buyer.blocked ? 'Blokuotas' : 'Aktyvus'}
                  </p>
                </div>
              <div className="admin-toggle__actions">
                <button className="btn btn--ghost" onClick={() => toggleBuyerField(buyer.idUser, 'confirmed', !buyer.confirmed)}>
                  {buyer.confirmed ? 'Atšaukti' : 'Patvirtinti'}
                </button>
                <button className="btn btn--ghost" onClick={() => toggleBuyerField(buyer.idUser, 'blocked', !buyer.blocked)}>
                  {buyer.blocked ? 'Atblokuoti' : 'Blokuoti'}
                </button>
              </div>
              </div>
            )
          })}
        </div>
      </section>

      <section className="card">
        <h3>Skelbimų moderavimas</h3>
        <div className="listing-grid">
          {listingsPagination.pageItems.map((listing) => (
            <article key={listing.idListing} className="card card--subtle listing-moderation-card">
              <div className="listing-moderation-card__content">
                <p className="muted listing-moderation-card__badge">
                  {listing.rent ? 'Nuomos pasiūlymas' : 'Pardavimo pasiūlymas'}
                </p>
                <h4 className="listing-moderation-card__title">{listing.description || 'Skelbimas be aprašo'}</h4>
                <p className="listing-moderation-card__price">{formatPrice(listing.askingprice)}</p>
              </div>
              <div className="listing-moderation-card__actions">
                <button className="btn btn--ghost" onClick={() => handlePreview(listing.idListing)}>
                  Peržiūrėti
                </button>
                <button className="btn btn--ghost" onClick={() => deleteListing(listing.idListing)}>
                  Pašalinti
                </button>
              </div>
            </article>
          ))}
          {listingsPagination.totalItems === 0 && <p className="muted">Skelbimų nėra.</p>}
        </div>
        {listingsPagination.totalItems > 0 && (
          <PaginationControls
            page={listingsPagination.page}
            pageSize={listingsPagination.pageSize}
            totalItems={listingsPagination.totalItems}
            totalPages={listingsPagination.totalPages}
            rangeStart={listingsPagination.rangeStart}
            rangeEnd={listingsPagination.rangeEnd}
            pageSizeOptions={listingsPagination.pageSizeOptions}
            onPageChange={listingsPagination.goToPage}
            onPageSizeChange={listingsPagination.setPageSize}
          />
        )}
      </section>

      {previewListingId !== null && (
        <div className="modal-overlay" role="dialog" aria-modal="true" aria-label="Skelbimo peržiūra">
          <div className="modal-frame">
            <div className="modal-frame__header">
              <h4>Skelbimo peržiūra #{previewListingId}</h4>
              <button type="button" className="btn btn--ghost" onClick={closePreview}>
                Uždaryti
              </button>
            </div>
            <iframe
              title={`Skelbimas ${previewListingId}`}
              src={`/skelbimai/${previewListingId}`}
              className="modal-frame__iframe"
            />
          </div>
        </div>
      )}
    </div>
  )
}
