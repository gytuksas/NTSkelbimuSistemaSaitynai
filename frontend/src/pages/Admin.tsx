import { useEffect, useState } from 'react'
import { client } from '../api/client'
import type { AppUser, Broker, Buyer, Listing } from '../types/api'
import { useAuth } from '../context/AuthContext'
import { formatPrice } from '../utils/text'

export const AdminPage = () => {
  const { user } = useAuth()
  const isAdmin = user?.role === 'Administrator'
  const [users, setUsers] = useState<AppUser[]>([])
  const [brokers, setBrokers] = useState<Broker[]>([])
  const [buyers, setBuyers] = useState<Buyer[]>([])
  const [listings, setListings] = useState<Listing[]>([])
  const [message, setMessage] = useState<string | null>(null)

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
            <span>ID</span>
            <span>Vardas</span>
            <span>El. paštas</span>
          </div>
          {users.map((item) => (
            <div key={item.idUser} className="table__row">
              <span>{item.idUser}</span>
              <span>
                {item.name} {item.surname}
              </span>
              <span>{item.email}</span>
            </div>
          ))}
        </div>
      </section>

      <section className="grid-two">
        <div className="card">
          <h3>Brokerių patvirtinimai</h3>
          {brokers.map((broker) => (
            <div key={broker.idUser} className="admin-toggle">
              <div>
                <p>ID {broker.idUser}</p>
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
          ))}
        </div>

        <div className="card">
          <h3>Pirkėjų statusas</h3>
          {buyers.map((buyer) => (
            <div key={buyer.idUser} className="admin-toggle">
              <div>
                <p>ID {buyer.idUser}</p>
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
          ))}
        </div>
      </section>

      <section className="card">
        <h3>Skelbimų moderavimas</h3>
        <div className="listing-grid">
          {listings.map((listing) => (
            <article key={listing.idListing} className="card card--subtle">
              <h4>#{listing.idListing}</h4>
              <p>{listing.description}</p>
              <p>{formatPrice(listing.askingprice)}</p>
              <button className="btn btn--ghost" onClick={() => deleteListing(listing.idListing)}>
                Pašalinti
              </button>
            </article>
          ))}
        </div>
      </section>
    </div>
  )
}
