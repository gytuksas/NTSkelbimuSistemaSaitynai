import { useEffect, useMemo, useRef, useState, type ChangeEvent, type FormEvent } from 'react'
import { client, baseURL } from '../api/client'
import type { Apartment, Availability, Building, Listing, Picture, Viewing } from '../types/api'
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
}

const defaultAvailability = {
  from: toDateTimeLocal(new Date(Date.now() + 1000 * 60 * 60 * 24)),
  to: toDateTimeLocal(new Date(Date.now() + 1000 * 60 * 60 * 25)),
}

type ApartmentFormState = {
  apartmentnumber: string
  rooms: string
  area: string
  floor: string
  finish: string
  heating: string
  notes: string
  isWholeBuilding: boolean
}

const defaultApartmentForm: ApartmentFormState = {
  apartmentnumber: '',
  rooms: '2',
  area: '55',
  floor: '1',
  finish: '1',
  heating: '1',
  notes: '',
  isWholeBuilding: false,
}

type PictureUploadFormState = {
  file: File | null
  public: boolean
}

const defaultPictureUpload: PictureUploadFormState = {
  file: null,
  public: true,
}

export const BrokersPage = () => {
  const { user } = useAuth()
  const isBroker = user && (user.role === 'Broker' || user.role === 'Administrator')

  const [buildings, setBuildings] = useState<Building[]>([])
  const [apartments, setApartments] = useState<Apartment[]>([])
  const [pictures, setPictures] = useState<Picture[]>([])
  const [availabilities, setAvailabilities] = useState<Availability[]>([])
  const [viewings, setViewings] = useState<Viewing[]>([])
  const [listings, setListings] = useState<Listing[]>([])

  const [buildingForm, setBuildingForm] = useState(defaultBuilding)
  const [listingForm, setListingForm] = useState(defaultListing)
  const [availabilityForm, setAvailabilityForm] = useState(defaultAvailability)
  const [apartmentForm, setApartmentForm] = useState(defaultApartmentForm)
  const [pictureUploadForm, setPictureUploadForm] = useState(defaultPictureUpload)

  const [selectedBuildingId, setSelectedBuildingId] = useState<number | null>(null)
  const [selectedApartmentId, setSelectedApartmentId] = useState<number | null>(null)
  const [selectedPictureId, setSelectedPictureId] = useState<string | null>(null)
  const [feedback, setFeedback] = useState<string | null>(null)
  const fileInputRef = useRef<HTMLInputElement | null>(null)

  useEffect(() => {
    const loadBrokerData = async () => {
      if (!isBroker) return
      try {
        const [buildingsRes, apartmentsRes, picturesRes, availabilitiesRes, viewingsRes, listingsRes] = await Promise.all([
          client.get<Building[]>('/api/Buildings'),
          client.get<Apartment[]>('/api/Apartments'),
          client.get<Picture[]>('/api/Pictures'),
          client.get<Availability[]>('/api/Availabilities'),
          client.get<Viewing[]>('/api/Viewings'),
          client.get<Listing[]>('/api/Listings'),
        ])
        setBuildings(buildingsRes.data ?? [])
        setApartments(apartmentsRes.data ?? [])
        setPictures(picturesRes.data ?? [])
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

  useEffect(() => {
    if (buildings.length === 0) {
      setSelectedBuildingId(null)
      return
    }
    if (selectedBuildingId === null || !buildings.some((building) => building.idBuilding === selectedBuildingId)) {
      setSelectedBuildingId(buildings[0].idBuilding)
    }
  }, [buildings, selectedBuildingId])

  useEffect(() => {
    if (selectedBuildingId === null) {
      setSelectedApartmentId(null)
      return
    }
    const scopedApartments = apartments.filter((apartment) => apartment.fkBuildingidBuilding === selectedBuildingId)
    if (scopedApartments.length === 0) {
      setSelectedApartmentId(null)
      return
    }
    if (!selectedApartmentId || !scopedApartments.some((apartment) => apartment.idApartment === selectedApartmentId)) {
      setSelectedApartmentId(scopedApartments[0].idApartment)
    }
  }, [apartments, selectedBuildingId, selectedApartmentId])

  useEffect(() => {
    if (selectedApartmentId === null) {
      setSelectedPictureId(null)
      return
    }
    const scopedPictures = pictures.filter((picture) => picture.fkApartmentidApartment === selectedApartmentId)
    if (scopedPictures.length === 0) {
      setSelectedPictureId(null)
      return
    }
    if (!selectedPictureId || !scopedPictures.some((picture) => picture.id === selectedPictureId)) {
      setSelectedPictureId(scopedPictures[0].id)
    }
  }, [pictures, selectedApartmentId, selectedPictureId])

  const selectedBuilding = buildings.find((building) => building.idBuilding === selectedBuildingId)
  const apartmentsInBuilding = useMemo(
    () => (selectedBuildingId ? apartments.filter((apartment) => apartment.fkBuildingidBuilding === selectedBuildingId) : []),
    [apartments, selectedBuildingId],
  )
  const picturesInApartment = useMemo(
    () => (selectedApartmentId ? pictures.filter((picture) => picture.fkApartmentidApartment === selectedApartmentId) : []),
    [pictures, selectedApartmentId],
  )
  const selectedPicture = pictures.find((picture) => picture.id === selectedPictureId)
  const selectedListing = listings.find((listing) => listing.fkPictureid === selectedPictureId)

  const handleBuildingCreate = async () => {
    if (!user) return
    try {
      const payload = { ...buildingForm, fkBrokeridUser: user.id }
      const { data } = await client.post<Building>('/api/Buildings', payload)
      setBuildings((prev) => [...prev, data])
      setBuildingForm(defaultBuilding)
      setSelectedBuildingId(data.idBuilding)
      setFeedback('Pastatas išsaugotas!')
    } catch (error) {
      console.error(error)
      setFeedback('Nepavyko sukurti pastato. Patikrinkite laukus.')
    }
  }

  const handleApartmentCreate = async () => {
    if (!selectedBuildingId) {
      setFeedback('Pasirinkite pastatą prieš kuriant butą.')
      return
    }
    try {
      const payload = {
        fkBuildingidBuilding: selectedBuildingId,
        apartmentnumber: apartmentForm.apartmentnumber ? Number(apartmentForm.apartmentnumber) : undefined,
        rooms: Number(apartmentForm.rooms),
        area: Number(apartmentForm.area),
        floor: apartmentForm.floor ? Number(apartmentForm.floor) : undefined,
        finish: Number(apartmentForm.finish),
        heating: apartmentForm.heating ? Number(apartmentForm.heating) : undefined,
        notes: apartmentForm.notes || undefined,
        isWholeBuilding: apartmentForm.isWholeBuilding,
      }
      const { data } = await client.post<Apartment>('/api/Apartments', payload)
      setApartments((prev) => [...prev, data])
      setApartmentForm(defaultApartmentForm)
      setSelectedApartmentId(data.idApartment)
      setFeedback('Butas pridėtas!')
    } catch (error) {
      console.error(error)
      setFeedback('Nepavyko sukurti buto. Patikrinkite laukus.')
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

  const handlePictureVisibilityChange = async (pictureId: string, isPublic: boolean) => {
    try {
      await client.patch(`/api/Pictures/${pictureId}`, { public: isPublic })
      setPictures((prev) => prev.map((picture) => (picture.id === pictureId ? { ...picture, public: isPublic } : picture)))
      setFeedback(isPublic ? 'Nuotrauka paskelbta viešai!' : 'Nuotrauka paslėpta nuo viešų sąrašų.')
    } catch (error) {
      console.error(error)
      setFeedback('Nepavyko atnaujinti nuotraukos matomumo.')
    }
  }

  const handleUploadPicture = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!selectedApartmentId) {
      setFeedback('Pasirinkite butą, kuriam priklausys nuotrauka.')
      return
    }
    if (!pictureUploadForm.file) {
      setFeedback('Pasirinkite failą prieš įkeldami.')
      return
    }
    try {
      const formData = new FormData()
      formData.append('file', pictureUploadForm.file)
      formData.append('apartmentId', String(selectedApartmentId))
      formData.append('public', String(pictureUploadForm.public))
      const { data } = await client.post<Picture>('/api/Pictures/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      setPictures((prev) => [...prev, data])
      setPictureUploadForm(defaultPictureUpload)
      if (fileInputRef.current) {
        fileInputRef.current.value = ''
      }
      setSelectedPictureId(data.id)
      setFeedback('Nuotrauka įkelta!')
    } catch (error) {
      console.error(error)
      setFeedback('Nepavyko įkelti nuotraukos. Patikrinkite formatą ir dydį.')
    }
  }

  const handleListingCreate = async () => {
    if (!selectedPictureId) {
      setFeedback('Pasirinkite nuotrauką, kuri bus skelbime.')
      return
    }
    try {
      const payload = { ...listingForm, fkPictureid: selectedPictureId }
      const { data } = await client.post<Listing>('/api/Listings', payload)
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

  const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0] ?? null
    setPictureUploadForm((prev) => ({ ...prev, file }))
  }

  if (!isBroker) {
    return <p className="muted">Ši skiltis pasiekiama tik brokeriams ir administratoriams.</p>
  }

  return (
    <div className="page brokers">
      {feedback && <p className="hint">{feedback}</p>}

      <section className="card">
        <h3>Pastatų portfelis</h3>
        <div className="selector-list">
          {buildings.map((building) => (
            <button
              type="button"
              key={building.idBuilding}
              className={selectedBuildingId === building.idBuilding ? 'selector-list__item selector-list__item--active' : 'selector-list__item'}
              onClick={() => setSelectedBuildingId(building.idBuilding)}
            >
              <strong>{building.city}</strong>
              <span>{building.address}</span>
              <span className="muted">{building.area} m² · {building.floors} aukšt.</span>
            </button>
          ))}
          {buildings.length === 0 && <p className="muted">Kol kas neturite pastatų.</p>}
        </div>
        <ul className="cascade-panel__summary">
          <li>
            <strong>Pasirinktas pastatas:</strong> {selectedBuilding ? `${selectedBuilding.city}, ${selectedBuilding.address}` : 'nepasirinkta'}
          </li>
          <li>
            <strong>Butas:</strong> {selectedApartmentId ?? 'nepasirinkta'}
          </li>
          <li>
            <strong>Nuotrauka:</strong> {selectedPictureId ?? 'nepasirinkta'}
          </li>
        </ul>
      </section>

      <section className="grid-two">
        <div className="card">
          <h3>Naujas pastatas</h3>
          <div className="form-grid">
            <label>
              Miestas
              <input value={buildingForm.city} onChange={(event) => setBuildingForm((prev) => ({ ...prev, city: event.target.value }))} />
            </label>
            <label>
              Adresas
              <input value={buildingForm.address} onChange={(event) => setBuildingForm((prev) => ({ ...prev, address: event.target.value }))} />
            </label>
            <label>
              Plotas (m²)
              <input type="number" value={buildingForm.area} onChange={(event) => setBuildingForm((prev) => ({ ...prev, area: Number(event.target.value) }))} />
            </label>
            <label>
              Metai
              <input type="number" value={buildingForm.year} onChange={(event) => setBuildingForm((prev) => ({ ...prev, year: Number(event.target.value) }))} />
            </label>
          </div>
          <button className="btn" onClick={handleBuildingCreate}>
            Išsaugoti pastatą
          </button>
        </div>

        <div className="card">
          <h3>Butai pasirinktame pastate</h3>
          <div className="table">
            <div className="table__row table__row--head">
              <span>ID</span>
              <span>Nr.</span>
              <span>Kamb.</span>
              <span>Plotas</span>
            </div>
            {apartmentsInBuilding.map((apartment) => (
              <div
                key={apartment.idApartment}
                className="table__row"
                role="button"
                tabIndex={0}
                onClick={() => setSelectedApartmentId(apartment.idApartment)}
                onKeyDown={(event) => {
                  if (event.key === 'Enter' || event.key === ' ') {
                    event.preventDefault()
                    setSelectedApartmentId(apartment.idApartment)
                  }
                }}
              >
                <span>{apartment.idApartment}</span>
                <span>{apartment.apartmentnumber ?? '—'}</span>
                <span>{apartment.rooms}</span>
                <span>{apartment.area} m²</span>
              </div>
            ))}
            {apartmentsInBuilding.length === 0 && <p className="muted">Šiame pastate dar nėra butų.</p>}
          </div>
          <div className="form-grid">
            <label>
              Buto numeris
              <input value={apartmentForm.apartmentnumber} onChange={(event) => setApartmentForm((prev) => ({ ...prev, apartmentnumber: event.target.value }))} />
            </label>
            <label>
              Kambariai
              <input type="number" value={apartmentForm.rooms} onChange={(event) => setApartmentForm((prev) => ({ ...prev, rooms: event.target.value }))} />
            </label>
            <label>
              Plotas m²
              <input type="number" value={apartmentForm.area} onChange={(event) => setApartmentForm((prev) => ({ ...prev, area: event.target.value }))} />
            </label>
            <label>
              Aukštas
              <input type="number" value={apartmentForm.floor} onChange={(event) => setApartmentForm((prev) => ({ ...prev, floor: event.target.value }))} />
            </label>
            <label>
              Apdaila ID
              <input type="number" value={apartmentForm.finish} onChange={(event) => setApartmentForm((prev) => ({ ...prev, finish: event.target.value }))} />
            </label>
            <label>
              Šildymas ID
              <input type="number" value={apartmentForm.heating} onChange={(event) => setApartmentForm((prev) => ({ ...prev, heating: event.target.value }))} />
            </label>
            <label>
              Pastabos
              <input value={apartmentForm.notes} onChange={(event) => setApartmentForm((prev) => ({ ...prev, notes: event.target.value }))} />
            </label>
            <label>
              <span>Visa pastato dalis?</span>
              <select value={apartmentForm.isWholeBuilding ? 'taip' : 'ne'} onChange={(event) => setApartmentForm((prev) => ({ ...prev, isWholeBuilding: event.target.value === 'taip' }))}>
                <option value="ne">Ne</option>
                <option value="taip">Taip</option>
              </select>
            </label>
          </div>
          <button className="btn" onClick={handleApartmentCreate}>
            Pridėti butą
          </button>
        </div>
      </section>

      <section className="grid-two">
        <div className="card">
          <h3>Nuotraukos ir įkėlimas</h3>
          {selectedApartmentId ? (
            <>
              <div className="picture-grid">
                {picturesInApartment.map((picture) => {
                  const coverStyle = { backgroundImage: `url(${baseURL}/uploads/${picture.id})` }
                  return (
                    <div
                      key={picture.id}
                      className={selectedPictureId === picture.id ? 'picture-card picture-card--selected' : 'picture-card'}
                      onClick={() => setSelectedPictureId(picture.id)}
                    >
                      <div className="picture-card__media" style={coverStyle}>
                        <span className="badge">ID {picture.id.slice(0, 6)}…</span>
                        <span className="badge">{picture.public ? 'Vieša' : 'Privatu'}</span>
                      </div>
                      <div className="picture-card__actions">
                        <button
                          type="button"
                          className="btn btn--ghost"
                          onClick={(event) => {
                            event.stopPropagation()
                            handlePictureVisibilityChange(picture.id, !picture.public)
                          }}
                        >
                          {picture.public ? 'Slėpti viešai' : 'Skelbti viešai'}
                        </button>
                      </div>
                    </div>
                  )
                })}
                {picturesInApartment.length === 0 && <p className="muted">Šiam butui dar nėra nuotraukų.</p>}
              </div>
              <form className="upload-form" onSubmit={handleUploadPicture}>
                <label>
                  Nuotraukos failas
                  <input ref={fileInputRef} type="file" accept="image/png,image/jpeg,image/webp" onChange={handleFileChange} />
                </label>
                <label>
                  Rodoma viešai?
                  <select value={pictureUploadForm.public ? 'taip' : 'ne'} onChange={(event) => setPictureUploadForm((prev) => ({ ...prev, public: event.target.value === 'taip' }))}>
                    <option value="ne">Ne</option>
                    <option value="taip">Taip</option>
                  </select>
                </label>
                <button type="submit" className="btn">
                  Įkelti nuotrauką
                </button>
              </form>
            </>
          ) : (
            <p className="muted">Pasirinkite butą, kad pamatytumėte nuotraukas.</p>
          )}
        </div>

        <div className="card">
          <h3>Skelbimo kūrimas</h3>
          <p className="muted">
            Nuotrauka: <strong>{selectedPictureId ?? 'nepasirinkta'}</strong>
          </p>
          {selectedPicture && (
            <p className="muted">Matomumas: {selectedPicture.public ? 'Vieša' : 'Privatu'}</p>
          )}
          {selectedListing && (
            <p className="hint">Šiai nuotraukai jau priskirtas skelbimas #{selectedListing.idListing}</p>
          )}
          <label>
            Aprašymas
            <textarea value={listingForm.description} onChange={(event) => setListingForm((prev) => ({ ...prev, description: event.target.value }))} />
          </label>
          <div className="form-grid">
            <label>
              Kaina
              <input type="number" value={listingForm.askingprice} onChange={(event) => setListingForm((prev) => ({ ...prev, askingprice: Number(event.target.value) }))} />
            </label>
            <label>
              Nuoma?
              <select value={listingForm.rent ? 'taip' : 'ne'} onChange={(event) => setListingForm((prev) => ({ ...prev, rent: event.target.value === 'taip' }))}>
                <option value="ne">Ne</option>
                <option value="taip">Taip</option>
              </select>
            </label>
          </div>
          <button className="btn" onClick={handleListingCreate}>
            Skelbti
          </button>
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
            {availabilities.length === 0 && <p className="muted">Dar neregistravote prieinamumo.</p>}
          </div>
          <div className="form-grid">
            <label>
              Pradžia
              <input value={availabilityForm.from} onChange={(event) => setAvailabilityForm((prev) => ({ ...prev, from: event.target.value }))} />
            </label>
            <label>
              Pabaiga
              <input value={availabilityForm.to} onChange={(event) => setAvailabilityForm((prev) => ({ ...prev, to: event.target.value }))} />
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
            {viewings.length === 0 && <p className="muted">Šiuo metu neturite užklausų.</p>}
          </div>
        </div>
      </section>

      <section className="card">
        <h3>Skelbimų suvestinė</h3>
        <div className="listing-grid">
          {listings.map((listing) => (
            <article key={listing.idListing} className="card card--subtle">
              <h4>#{listing.idListing}</h4>
              <p>{listing.description}</p>
              <p>{formatPrice(listing.askingprice)}</p>
              <p>{listing.rent ? 'Nuoma' : 'Pardavimas'}</p>
              <p className="muted">Nuotrauka {listing.fkPictureid}</p>
            </article>
          ))}
          {listings.length === 0 && <p className="muted">Dar nesukūrėte skelbimų.</p>}
        </div>
      </section>
    </div>
  )
}
