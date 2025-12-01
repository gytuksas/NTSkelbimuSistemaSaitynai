import { useEffect, useState } from 'react'
import { client } from '../api/client'
import type { Apartment, Availability, Building, Listing, Viewing } from '../types/api'
import { useAuth } from '../context/AuthContext'
import { toDateTimeLocal, formatFriendly } from '../utils/dates'
import { formatPrice } from '../utils/text'

const defaultBuilding = {
  city: 'Vilnius',
  address: '',
  area: 80,
  year: 2010,
  lastrenovationyear: 2020,
  floors: 5,
  energy: 1,
}

const defaultListing = {
  description: '',
  askingprice: 100000,
  rent: false,
  fkPictureid: '',
}

const defaultAvailability = {
  from: toDateTimeLocal(new Date(Date.now() + 1000 * 60 * 60 * 24)),
  to: toDateTimeLocal(new Date(Date.now() + 1000 * 60 * 60 * 25)),
}

export const BrokersPage = () => {
  const { user } = useAuth()
  const isBroker = user && (user.role === 'Broker' || user.role === 'Administrator')

  const [buildings, setBuildings] = useState<Building[]>([])
  const [apartments, setApartments] = useState<Apartment[]>([])
  const [availabilities, setAvailabilities] = useState<Availability[]>([])
  const [viewings, setViewings] = useState<Viewing[]>([])
  const [listings, setListings] = useState<Listing[]>([])

  const [buildingForm, setBuildingForm] = useState(defaultBuilding)
  const [listingForm, setListingForm] = useState(defaultListing)
  const [availabilityForm, setAvailabilityForm] = useState(defaultAvailability)
  const [feedback, setFeedback] = useState<string | null>(null)

  useEffect(() => {
    const loadBrokerData = async () => {
      if (!isBroker) return
      try {
        const [buildingsRes, apartmentsRes, availabilitiesRes, viewingsRes, listingsRes] = await Promise.all([
          client.get<Building[]>('/api/Buildings'),
          client.get<Apartment[]>('/api/Apartments'),
          client.get<Availability[]>('/api/Availabilities'),
          client.get<Viewing[]>('/api/Viewings'),
          client.get<Listing[]>('/api/Listings'),
        ])
        setBuildings(buildingsRes.data ?? [])
        setApartments(apartmentsRes.data ?? [])
        setAvailabilities(availabilitiesRes.data ?? [])
        setViewings(viewingsRes.data ?? [])
        setListings(listingsRes.data ?? [])
      } catch (error) {
        console.error(error)
        setFeedback('Nepavyko gauti duomenų. Patikrinkite ar turite brokerio rolę.')
      }
    }

    void loadBrokerData()
  }, [isBroker])

  const handleBuildingCreate = async () => {
    if (!user) return
    try {
      const payload = { ...buildingForm, fkBrokeridUser: user.id }
      const { data } = await client.post<Building>('/api/Buildings', payload)
      setBuildings((prev) => [...prev, data])
      setBuildingForm(defaultBuilding)
      setFeedback('Pastatas išsaugotas!')
    } catch (error) {
      console.error(error)
      setFeedback('Nepavyko sukurti pastato. Patikrinkite laukus.')
    }
  }

  const handleAvailabilityCreate = async () => {
    if (!user) return
    try {
      const { data } = await client.post<Availability>('/api/Availabilities', {
        ...availabilityForm,
        fkBrokeridUser: user.id,
      })
      setAvailabilities((prev) => [...prev, data])
      setAvailabilityForm(defaultAvailability)
      setFeedback('Prieinamumo langas išsaugotas!')
    } catch (error) {
      console.error(error)
      setFeedback('Nepavyko užregistruoti laiko lango.')
    }
  }

  const handleListingCreate = async () => {
    try {
      const { data } = await client.post<Listing>('/api/Listings', listingForm)
      setListings((prev) => [...prev, data])
      setListingForm(defaultListing)
      setFeedback('Skelbimas sukurtas!')
    } catch (error) {
      console.error(error)
      setFeedback('Nepavyko sukurti skelbimo. Įsitikinkite, kad nuotrauka priklauso jums.')
    }
  }

  const handleViewingDecision = async (id: number, status: number) => {
    try {
      await client.patch(`/api/Viewings/${id}`, { status })
      setViewings((prev) => prev.map((viewing) => (viewing.idViewing === id ? { ...viewing, status } : viewing)))
    } catch (error) {
      console.error(error)
      setFeedback('Nepavyko atnaujinti peržiūros būsenos.')
    }
  }

  if (!isBroker) {
    return <p className="muted">Ši skiltis pasiekiama tik brokeriams ir administratoriams.</p>
  }

  return (
    <div className="page brokers">
      {feedback && <p className="hint">{feedback}</p>}

      <section className="grid-two">
        <div className="card">
          <h3>Naujas pastatas</h3>
          <div className="form-grid">
            <label>
              Miestas
              <input
                value={buildingForm.city}
                onChange={(event) => setBuildingForm((prev) => ({ ...prev, city: event.target.value }))}
              />
            </label>
            <label>
              Adresas
              <input
                value={buildingForm.address}
                onChange={(event) => setBuildingForm((prev) => ({ ...prev, address: event.target.value }))}
              />
            </label>
            <label>
              Plotas (m²)
              <input
                type="number"
                value={buildingForm.area}
                onChange={(event) => setBuildingForm((prev) => ({ ...prev, area: Number(event.target.value) }))}
              />
            </label>
            <label>
              Metai
              <input
                type="number"
                value={buildingForm.year}
                onChange={(event) => setBuildingForm((prev) => ({ ...prev, year: Number(event.target.value) }))}
              />
            </label>
          </div>
          <button className="btn" onClick={handleBuildingCreate}>
            Išsaugoti pastatą
          </button>
        </div>

        <div className="card">
          <h3>Naujas skelbimas</h3>
          <label>
            Aprašymas
            <textarea
              value={listingForm.description}
              onChange={(event) => setListingForm((prev) => ({ ...prev, description: event.target.value }))}
            />
          </label>
          <div className="form-grid">
            <label>
              Kaina
              <input
                type="number"
                value={listingForm.askingprice}
                onChange={(event) =>
                  setListingForm((prev) => ({ ...prev, askingprice: Number(event.target.value) }))
                }
              />
            </label>
            <label>
              Nuoma?
              <select
                value={listingForm.rent ? 'taip' : 'ne'}
                onChange={(event) => setListingForm((prev) => ({ ...prev, rent: event.target.value === 'taip' }))}
              >
                <option value="ne">Ne</option>
                <option value="taip">Taip</option>
              </select>
            </label>
          </div>
          <label>
            Nuotraukos ID
            <input
              value={listingForm.fkPictureid}
              onChange={(event) => setListingForm((prev) => ({ ...prev, fkPictureid: event.target.value }))}
            />
          </label>
          <button className="btn" onClick={handleListingCreate}>
            Skelbti
          </button>
        </div>
      </section>

      <section className="card">
        <h3>Pastatų portfelis</h3>
        <div className="listing-grid">
          {buildings.map((building) => (
            <article key={building.idBuilding} className="card card--subtle">
              <h4>{building.city}</h4>
              <p>{building.address}</p>
              <p>
                {building.area} m² · {building.floors} aukšt.
              </p>
            </article>
          ))}
        </div>
      </section>

      <section className="card">
        <h3>Butai</h3>
        <div className="table">
          <div className="table__row table__row--head">
            <span>ID</span>
            <span>Nr.</span>
            <span>Kamb.</span>
            <span>Pastato ID</span>
          </div>
          {apartments.map((apartment) => (
            <div key={apartment.idApartment} className="table__row">
              <span>{apartment.idApartment}</span>
              <span>{apartment.apartmentnumber ?? '—'}</span>
              <span>{apartment.rooms}</span>
              <span>{apartment.fkBuildingidBuilding}</span>
            </div>
          ))}
        </div>
      </section>

      <section className="grid-two">
        <div className="card">
          <h3>Laisvi laikai</h3>
          <div className="timeline">
            {availabilities.map((slot) => (
              <div key={slot.idAvailability} className="timeline__item">
                <div>
                  <p>{formatFriendly(slot.from)}</p>
                  <p className="muted">iki {formatFriendly(slot.to)}</p>
                </div>
                <span># {slot.idAvailability}</span>
              </div>
            ))}
          </div>
          <div className="form-grid">
            <label>
              Pradžia
              <input
                value={availabilityForm.from}
                onChange={(event) => setAvailabilityForm((prev) => ({ ...prev, from: event.target.value }))}
              />
            </label>
            <label>
              Pabaiga
              <input
                value={availabilityForm.to}
                onChange={(event) => setAvailabilityForm((prev) => ({ ...prev, to: event.target.value }))}
              />
            </label>
          </div>
          <button className="btn" onClick={handleAvailabilityCreate}>
            Pridėti laiką
          </button>
        </div>

        <div className="card">
          <h3>Peržiūrų užklausos</h3>
          <div className="table">
            <div className="table__row table__row--head">
              <span>ID</span>
              <span>Skelbimas</span>
              <span>Laikas</span>
              <span>Veiksmas</span>
            </div>
            {viewings.map((viewing) => (
              <div key={viewing.idViewing} className="table__row">
                <span>{viewing.idViewing}</span>
                <span>{viewing.fkListingidListing}</span>
                <span>{formatFriendly(viewing.from)}</span>
                <span className="table__actions">
                  <button className="btn btn--ghost" onClick={() => handleViewingDecision(viewing.idViewing, 2)}>
                    Patvirtinti
                  </button>
                  <button className="btn btn--ghost" onClick={() => handleViewingDecision(viewing.idViewing, 3)}>
                    Atmesti
                  </button>
                </span>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="card">
        <h3>Skelbimų suvestinė</h3>
        <div className="listing-grid">
          {listings.map((listing) => (
            <article key={listing.idListing} className="card card--subtle">
              <h4>{listing.description}</h4>
              <p>{formatPrice(listing.askingprice)}</p>
              <p>{listing.rent ? 'Nuoma' : 'Pardavimas'}</p>
            </article>
          ))}
        </div>
      </section>
    </div>
  )
}
