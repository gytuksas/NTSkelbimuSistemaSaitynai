import { useEffect, useMemo, useRef, useState, type ChangeEvent, type FormEvent } from 'react'
import { client, baseURL } from '../api/client'
import type { Apartment, Availability, Building, Listing, Picture, Viewing } from '../types/api'
import { useAuth } from '../context/useAuth'
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
  files: File[]
  public: boolean
}

const defaultPictureUpload: PictureUploadFormState = {
  files: [],
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

  const [selectedBuildingIdInput, setSelectedBuildingIdInput] = useState<number | null>(null)
  const [selectedApartmentIdInput, setSelectedApartmentIdInput] = useState<number | null>(null)
  const [selectedPictureIdInput, setSelectedPictureIdInput] = useState<string | null>(null)
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

  const selectedBuildingId = useMemo(() => {
    if (buildings.length === 0) {
      return null
    }
    if (
      selectedBuildingIdInput !== null &&
      buildings.some((building) => building.idBuilding === selectedBuildingIdInput)
    ) {
      return selectedBuildingIdInput
    }
    return buildings[0].idBuilding
  }, [buildings, selectedBuildingIdInput])

  const apartmentsInBuilding = useMemo(
    () => (selectedBuildingId ? apartments.filter((apartment) => apartment.fkBuildingidBuilding === selectedBuildingId) : []),
    [apartments, selectedBuildingId],
  )

  const selectedApartmentId = useMemo(() => {
    if (!selectedBuildingId || apartmentsInBuilding.length === 0) {
      return null
    }
    if (
      selectedApartmentIdInput !== null &&
      apartmentsInBuilding.some((apartment) => apartment.idApartment === selectedApartmentIdInput)
    ) {
      return selectedApartmentIdInput
    }
    return apartmentsInBuilding[0].idApartment
  }, [selectedApartmentIdInput, apartmentsInBuilding, selectedBuildingId])

  const picturesInApartment = useMemo(
    () => (selectedApartmentId ? pictures.filter((picture) => picture.fkApartmentidApartment === selectedApartmentId) : []),
    [pictures, selectedApartmentId],
  )

  const selectedPictureId = useMemo(() => {
    if (!selectedApartmentId || picturesInApartment.length === 0) {
      return null
    }
    if (selectedPictureIdInput !== null && picturesInApartment.some((picture) => picture.id === selectedPictureIdInput)) {
      return selectedPictureIdInput
    }
    return picturesInApartment[0]?.id ?? null
  }, [selectedPictureIdInput, picturesInApartment, selectedApartmentId])

  const selectedBuilding = buildings.find((building) => building.idBuilding === selectedBuildingId)
  const selectedPicture = pictures.find((picture) => picture.id === selectedPictureId)
  const selectedApartment = apartments.find((apartment) => apartment.idApartment === selectedApartmentId)
  const pictureDirectory = useMemo(() => new Map(pictures.map((picture) => [picture.id, picture])), [pictures])
  const selectedListing = listings.find((listing) => listing.fkPictureid === selectedPictureId)
  const selectedPicturePosition = selectedPictureId ? picturesInApartment.findIndex((picture) => picture.id === selectedPictureId) : -1
  const selectedApartmentSummary = selectedApartment
    ? [
        selectedApartment.apartmentnumber ? `Nr. ${selectedApartment.apartmentnumber}` : null,
        `${selectedApartment.rooms} kamb.`,
        `${selectedApartment.area} m²`,
      ]
        .filter(Boolean)
        .join(' · ')
    : null
  const selectedPictureSummary = selectedPicture
    ? selectedPicturePosition >= 0 && picturesInApartment.length > 0
      ? `Nuotrauka ${selectedPicturePosition + 1}/${picturesInApartment.length} · ${selectedPicture.public ? 'vieša' : 'privati'}`
      : selectedPicture.public
        ? 'Vieša nuotrauka'
        : 'Privati nuotrauka'
    : null

  const handleBuildingCreate = async () => {
    if (!user) return
    try {
      const payload = { ...buildingForm, fkBrokeridUser: user.id }
      const { data } = await client.post<Building>('/api/Buildings', payload)
  setBuildings((prev) => [...prev, data])
  setBuildingForm(defaultBuilding)
  setSelectedBuildingIdInput(data.idBuilding)
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
  setSelectedApartmentIdInput(data.idApartment)
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
    if (pictureUploadForm.files.length === 0) {
      setFeedback('Pasirinkite bent vieną failą prieš įkeldami.')
      return
    }
    try {
      const formData = new FormData()
      pictureUploadForm.files.forEach((file) => formData.append('Files', file))
      formData.append('apartmentId', String(selectedApartmentId))
      formData.append('public', String(pictureUploadForm.public))
      const { data } = await client.post<Picture[]>('/api/Pictures/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      if (Array.isArray(data) && data.length > 0) {
  setPictures((prev) => [...prev, ...data])
  setSelectedPictureIdInput(data[data.length - 1].id)
        setFeedback(data.length > 1 ? `Įkeltos ${data.length} nuotraukos!` : 'Nuotrauka įkelta!')
      } else {
        setFeedback('Įkėlimas atliktas, bet negrįžo nuotraukų duomenys.')
      }
      setPictureUploadForm(defaultPictureUpload)
      if (fileInputRef.current) {
        fileInputRef.current.value = ''
      }
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
    const files = event.target.files ? Array.from(event.target.files) : []
    setPictureUploadForm((prev) => ({ ...prev, files }))
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
              onClick={() => setSelectedBuildingIdInput(building.idBuilding)}
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
            <strong>Butas:</strong> {selectedApartmentSummary ?? 'nepasirinkta'}
          </li>
          <li>
            <strong>Nuotrauka:</strong> {selectedPictureSummary ?? 'nepasirinkta'}
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
              <span>Nr.</span>
              <span>Kamb.</span>
              <span>Plotas</span>
              <span>Aukštas</span>
            </div>
            {apartmentsInBuilding.map((apartment) => (
              <div
                key={apartment.idApartment}
                className="table__row"
                role="button"
                tabIndex={0}
                onClick={() => setSelectedApartmentIdInput(apartment.idApartment)}
                onKeyDown={(event) => {
                  if (event.key === 'Enter' || event.key === ' ') {
                    event.preventDefault()
                    setSelectedApartmentIdInput(apartment.idApartment)
                  }
                }}
              >
                <span>{apartment.apartmentnumber ?? '—'}</span>
                <span>{apartment.rooms}</span>
                <span>{apartment.area} m²</span>
                <span>{apartment.floor ?? '—'}</span>
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
                      onClick={() => setSelectedPictureIdInput(picture.id)}
                    >
                      <div className="picture-card__media" style={coverStyle}>
                        {selectedPictureId === picture.id && <span className="badge">Pasirinkta</span>}
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
                  Nuotraukų failai
                  <input ref={fileInputRef} type="file" accept="image/png,image/jpeg,image/webp" multiple onChange={handleFileChange} />
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
            Nuotrauka: <strong>{selectedPictureSummary ?? 'nepasirinkta'}</strong>
          </p>
          {selectedPicture && (
            <p className="muted">Matomumas: {selectedPicture.public ? 'Vieša' : 'Privatu'}</p>
          )}
          {selectedListing && (
            <p className="hint">
              Ši nuotrauka jau naudojama skelbime „{selectedListing.description || formatPrice(selectedListing.askingprice)}“
            </p>
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
              <span>Skelbimas</span>
              <span>Laikas</span>
              <span>Veiksmas</span>
            </div>
            {viewings.map((viewing) => {
              const relatedListing = listings.find((listing) => listing.idListing === viewing.fkListingidListing)
              return (
                <div key={viewing.idViewing} className="table__row">
                  <span>
                    <strong>{relatedListing?.description ?? 'Skelbimo duomenys nepasiekiami'}</strong>
                    <p className="muted">
                      {relatedListing
                        ? relatedListing.rent
                          ? 'Nuomos pasiūlymas'
                          : 'Pardavimo pasiūlymas'
                        : 'Patikrinkite ar skelbimas dar egzistuoja'}
                    </p>
                  </span>
                  <span>
                    <p>{formatFriendly(viewing.from)}</p>
                    <p className="muted">iki {formatFriendly(viewing.to)}</p>
                  </span>
                  <span className="table__actions">
                    <button className="btn btn--ghost" onClick={() => handleViewingDecision(viewing.idViewing, 2)}>
                      Patvirtinti
                    </button>
                    <button className="btn btn--ghost" onClick={() => handleViewingDecision(viewing.idViewing, 3)}>
                      Atmesti
                    </button>
                  </span>
                </div>
              )
            })}
            {viewings.length === 0 && <p className="muted">Šiuo metu neturite užklausų.</p>}
          </div>
        </div>
      </section>

      <section className="card">
        <h3>Skelbimų suvestinė</h3>
        <div className="listing-grid">
          {listings.map((listing) => {
            const coverPicture = listing.fkPictureid ? pictureDirectory.get(listing.fkPictureid) : undefined
            return (
              <article key={listing.idListing} className="card card--subtle">
                <p className="muted">{listing.rent ? 'Nuomos pasiūlymas' : 'Pardavimo pasiūlymas'}</p>
                <h4>{listing.description || 'Skelbimas be aprašo'}</h4>
                <p>{formatPrice(listing.askingprice)}</p>
                <p className="muted">
                  {!listing.fkPictureid
                    ? 'Viršelio nuotrauka nepasirinkta'
                    : coverPicture
                      ? coverPicture.public
                        ? 'Viršelio nuotrauka vieša'
                        : 'Viršelio nuotrauka privati'
                      : 'Viršelio nuotraukos duomenys nepasiekiami'}
                </p>
              </article>
            )
          })}
          {listings.length === 0 && <p className="muted">Dar nesukūrėte skelbimų.</p>}
        </div>
      </section>
    </div>
  )
}
