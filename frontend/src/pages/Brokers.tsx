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

type Option = {
  value: number
  label: string
}

const energyClassOptions: Option[] = [
  { value: 1, label: 'A+++' },
  { value: 2, label: 'A++' },
  { value: 3, label: 'A+' },
  { value: 4, label: 'A' },
  { value: 5, label: 'B' },
  { value: 6, label: 'C' },
  { value: 7, label: 'D' },
  { value: 8, label: 'E' },
  { value: 9, label: 'F' },
  { value: 10, label: 'G' },
]

const finishTypeOptions: Option[] = [
  { value: 1, label: 'Pilnai įrengtas' },
  { value: 2, label: 'Dalinė apdaila' },
  { value: 3, label: 'Neįrengtas' },
  { value: 4, label: 'Statomas' },
  { value: 5, label: 'Pamatai' },
  { value: 6, label: 'Kita' },
]

const heatingTypeOptions: Option[] = [
  { value: 1, label: 'Centrinis' },
  { value: 2, label: 'Dujinis' },
  { value: 3, label: 'Kietas kuras' },
  { value: 4, label: 'Elektrinis' },
  { value: 5, label: 'Šilumos siurblys' },
  { value: 6, label: 'Kita' },
]

const getOptionLabel = (options: Option[], value?: number | null) => {
  if (value === null || value === undefined) {
    return 'Nenurodyta'
  }
  return options.find((option) => option.value === value)?.label ?? `ID ${value}`
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

type ViewMode = 'buildings' | 'apartments'
type ApartmentTab = 'units' | 'schedule'

export const BrokersPage = () => {
  const { user } = useAuth()
  const isBroker = user && (user.role === 'Broker' || user.role === 'Administrator')

  const [viewMode, setViewMode] = useState<ViewMode>('buildings')
  const [apartmentTab, setApartmentTab] = useState<ApartmentTab>('units')
  const [buildings, setBuildings] = useState<Building[]>([])
  const [apartments, setApartments] = useState<Apartment[]>([])
  const [pictures, setPictures] = useState<Picture[]>([])
  const [availabilities, setAvailabilities] = useState<Availability[]>([])
  const [viewings, setViewings] = useState<Viewing[]>([])
  const [listings, setListings] = useState<Listing[]>([])

  const [buildingForm, setBuildingForm] = useState(defaultBuilding)
  const [buildingEditForm, setBuildingEditForm] = useState(defaultBuilding)
  const [listingForm, setListingForm] = useState(defaultListing)
  const [availabilityForm, setAvailabilityForm] = useState(defaultAvailability)
  const [apartmentForm, setApartmentForm] = useState(defaultApartmentForm)
  const [apartmentEditForm, setApartmentEditForm] = useState(defaultApartmentForm)
  const [pictureUploadForm, setPictureUploadForm] = useState(defaultPictureUpload)

  const [selectedBuildingIdInput, setSelectedBuildingIdInput] = useState<number | null>(null)
  const [selectedApartmentIdInput, setSelectedApartmentIdInput] = useState<number | null>(null)
  const [selectedPictureIdInput, setSelectedPictureIdInput] = useState<string | null>(null)
  const [editingBuildingId, setEditingBuildingId] = useState<number | null>(null)
  const [editingApartmentId, setEditingApartmentId] = useState<number | null>(null)
  const [activeListingApartmentId, setActiveListingApartmentId] = useState<number | null>(null)
  const [listingPictureId, setListingPictureId] = useState<string | null>(null)
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
    if (viewMode === 'apartments' && buildings.length === 0) {
      setViewMode('buildings')
    }
  }, [viewMode, buildings.length])

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
  const listingPicture = listingPictureId ? pictureDirectory.get(listingPictureId) : undefined
  const listingPictureAlreadyUsed = listingPictureId ? listings.some((listing) => listing.fkPictureid === listingPictureId) : false

  const goToApartmentsView = (buildingId: number) => {
    setSelectedBuildingIdInput(buildingId)
    setSelectedApartmentIdInput(null)
    setSelectedPictureIdInput(null)
    setEditingApartmentId(null)
    setActiveListingApartmentId(null)
    setListingPictureId(null)
    setApartmentTab('units')
    setViewMode('apartments')
  }

  const handleBackToBuildings = () => {
    setViewMode('buildings')
    setEditingApartmentId(null)
    setActiveListingApartmentId(null)
    setListingPictureId(null)
    setApartmentTab('units')
  }

  const startBuildingEdit = (building: Building) => {
    setEditingBuildingId(building.idBuilding)
    setBuildingEditForm({
      city: building.city,
      address: building.address,
      area: building.area,
      year: building.year,
      lastrenovationyear: building.lastrenovationyear ?? building.year,
      floors: building.floors,
      energy: building.energy ?? defaultBuilding.energy,
    })
  }

  const handleBuildingCreate = async () => {
    if (!user) return
    try {
      const payload = { ...buildingForm, fkBrokeridUser: user.id }
      const { data } = await client.post<Building>('/api/Buildings', payload)
      setBuildings((prev) => [...prev, data])
      setBuildingForm(defaultBuilding)
      setSelectedBuildingIdInput(data.idBuilding)
      setViewMode('apartments')
      setFeedback('Pastatas išsaugotas!')
    } catch (error) {
      console.error(error)
      setFeedback('Nepavyko sukurti pastato. Patikrinkite laukus.')
    }
  }

  const handleBuildingUpdate = async () => {
    if (editingBuildingId === null) return
    const existing = buildings.find((building) => building.idBuilding === editingBuildingId)
    if (!existing) return
    try {
      const payload = { ...buildingEditForm, fkBrokeridUser: existing.fkBrokeridUser }
      await client.put(`/api/Buildings/${editingBuildingId}`, payload)
      setBuildings((prev) =>
        prev.map((building) => (building.idBuilding === editingBuildingId ? { ...building, ...payload } : building)),
      )
      setFeedback('Pastato informacija atnaujinta!')
      setEditingBuildingId(null)
    } catch (error) {
      console.error(error)
      setFeedback('Nepavyko atnaujinti pastato.')
    }
  }

  const handleBuildingDelete = async (buildingId: number) => {
    if (typeof window !== 'undefined' && !window.confirm('Ar tikrai norite ištrinti pastatą?')) {
      return
    }
    const buildingApartmentIds = apartments
      .filter((apartment) => apartment.fkBuildingidBuilding === buildingId)
      .map((apartment) => apartment.idApartment)
    try {
      await client.delete(`/api/Buildings/${buildingId}`)
      setBuildings((prev) => prev.filter((building) => building.idBuilding !== buildingId))
      setApartments((prev) => prev.filter((apartment) => apartment.fkBuildingidBuilding !== buildingId))
      setPictures((prev) => prev.filter((picture) => !buildingApartmentIds.includes(picture.fkApartmentidApartment)))
      if (selectedBuildingId === buildingId) {
        setSelectedBuildingIdInput(null)
        setSelectedApartmentIdInput(null)
        setSelectedPictureIdInput(null)
        setViewMode('buildings')
      }
      setFeedback('Pastatas pašalintas.')
    } catch (error) {
      console.error(error)
      setFeedback('Nepavyko pašalinti pastato.')
    }
  }

  const startApartmentEdit = (apartment: Apartment) => {
    const picturesForApartment = pictures.filter((picture) => picture.fkApartmentidApartment === apartment.idApartment)
    setEditingApartmentId(apartment.idApartment)
    setActiveListingApartmentId(null)
    setListingPictureId(null)
    setSelectedApartmentIdInput(apartment.idApartment)
    setSelectedPictureIdInput(picturesForApartment[0]?.id ?? null)
    setPictureUploadForm(defaultPictureUpload)
    if (fileInputRef.current) {
      fileInputRef.current.value = ''
    }
    setApartmentEditForm({
      apartmentnumber: apartment.apartmentnumber?.toString() ?? '',
      rooms: apartment.rooms.toString(),
      area: apartment.area.toString(),
      floor: apartment.floor?.toString() ?? '',
      finish: apartment.finish.toString(),
      heating: apartment.heating?.toString() ?? '',
      notes: apartment.notes ?? '',
      isWholeBuilding: apartment.isWholeBuilding,
    })
  }

  const handleApartmentUpdate = async () => {
    if (editingApartmentId === null) return
    const existing = apartments.find((apartment) => apartment.idApartment === editingApartmentId)
    if (!existing) return
    try {
      const payload = {
        fkBuildingidBuilding: existing.fkBuildingidBuilding,
        apartmentnumber: apartmentEditForm.apartmentnumber ? Number(apartmentEditForm.apartmentnumber) : undefined,
        rooms: Number(apartmentEditForm.rooms),
        area: Number(apartmentEditForm.area),
        floor: apartmentEditForm.floor ? Number(apartmentEditForm.floor) : undefined,
        finish: Number(apartmentEditForm.finish),
        heating: apartmentEditForm.heating ? Number(apartmentEditForm.heating) : undefined,
        notes: apartmentEditForm.notes || undefined,
        isWholeBuilding: apartmentEditForm.isWholeBuilding,
      }
      await client.put(`/api/Apartments/${editingApartmentId}`, payload)
      setApartments((prev) =>
        prev.map((apartment) => (apartment.idApartment === editingApartmentId ? { ...apartment, ...payload } : apartment)),
      )
      setFeedback('Butas atnaujintas!')
      setEditingApartmentId(null)
    } catch (error) {
      console.error(error)
      setFeedback('Nepavyko atnaujinti buto.')
    }
  }

  const handleApartmentDelete = async (apartmentId: number) => {
    if (typeof window !== 'undefined' && !window.confirm('Ar tikrai norite ištrinti butą?')) {
      return
    }
    try {
      await client.delete(`/api/Apartments/${apartmentId}`)
      setApartments((prev) => prev.filter((apartment) => apartment.idApartment !== apartmentId))
      setPictures((prev) => prev.filter((picture) => picture.fkApartmentidApartment !== apartmentId))
      if (selectedApartmentId === apartmentId) {
        setSelectedApartmentIdInput(null)
        setSelectedPictureIdInput(null)
      }
      setFeedback('Butas pašalintas.')
    } catch (error) {
      console.error(error)
      setFeedback('Nepavyko pašalinti buto.')
    }
  }

  const startListingCreation = (apartment: Apartment) => {
    const picturesForApartment = pictures.filter((picture) => picture.fkApartmentidApartment === apartment.idApartment)
    const defaultPicture = picturesForApartment[0]?.id ?? null
    setActiveListingApartmentId(apartment.idApartment)
    setEditingApartmentId(null)
    setSelectedApartmentIdInput(apartment.idApartment)
    setListingPictureId(defaultPicture)
    setSelectedPictureIdInput(defaultPicture)
    setListingForm(defaultListing)
  }

  const closeListingPanel = () => {
    setActiveListingApartmentId(null)
    setListingPictureId(null)
    setListingForm(defaultListing)
  }

  const handleBuildingFormChange = (key: keyof typeof defaultBuilding, value: number | string) => {
    setBuildingForm((prev) => ({ ...prev, [key]: value }))
  }

  const handleBuildingCreateChange = (field: keyof typeof defaultBuilding, rawValue: string) => {
    if (typeof defaultBuilding[field] === 'number') {
      handleBuildingFormChange(field, Number(rawValue) || 0)
    } else {
      handleBuildingFormChange(field, rawValue)
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
        const latestId = data[data.length - 1].id
        setPictures((prev) => [...prev, ...data])
        setSelectedPictureIdInput(latestId)
        if (activeListingApartmentId !== null && activeListingApartmentId === selectedApartmentId) {
          setListingPictureId(latestId)
        }
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

  const handleListingCreate = async (pictureId: string | null) => {
    if (!pictureId) {
      setFeedback('Pasirinkite nuotrauką, kuri bus skelbime.')
      return
    }
    try {
      const payload = { ...listingForm, fkPictureid: pictureId }
      const { data } = await client.post<Listing>('/api/Listings', payload)
      setListings((prev) => [...prev, data])
      setListingForm(defaultListing)
      closeListingPanel()
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

      {viewMode === 'buildings' && (
        <>
        <section className="card">
            <h3>Jūsų pastatai</h3>
            <p className="muted">Pasirinkite pastatą ir pereikite prie jo butų valdymo.</p>
            <div className="management-list">
              {buildings.map((building) => (
                <article key={building.idBuilding} className="management-card">
                  <div className="management-card__header">
                    <div>
                      <h4>{building.city}</h4>
                      <p className="muted">{building.address}</p>
                      <p className="muted">
                        {building.area} m² · {building.floors} aukštai · Pastatyta {building.year}
                      </p>
                    </div>
                    <div className="management-card__actions table__actions table__actions--stacked">
                      <button className="btn" onClick={() => goToApartmentsView(building.idBuilding)}>
                        Peržiūrėti butus
                      </button>
                      <button className="btn btn--ghost" onClick={() => startBuildingEdit(building)}>
                        Redaguoti
                      </button>
                      <button className="btn btn--ghost" onClick={() => handleBuildingDelete(building.idBuilding)}>
                        Pašalinti
                      </button>
                    </div>
                  </div>
                  {editingBuildingId === building.idBuilding && (
                    <div className="management-edit">
                      <div className="form-grid">
                        <label>
                          Miestas
                          <input value={buildingEditForm.city} onChange={(event) => setBuildingEditForm((prev) => ({ ...prev, city: event.target.value }))} />
                        </label>
                        <label>
                          Adresas
                          <input value={buildingEditForm.address} onChange={(event) => setBuildingEditForm((prev) => ({ ...prev, address: event.target.value }))} />
                        </label>
                        <label>
                          Plotas (m²)
                          <input type="number" value={buildingEditForm.area} onChange={(event) => setBuildingEditForm((prev) => ({ ...prev, area: Number(event.target.value) || 0 }))} />
                        </label>
                        <label>
                          Aukštai
                          <input type="number" value={buildingEditForm.floors} onChange={(event) => setBuildingEditForm((prev) => ({ ...prev, floors: Number(event.target.value) || 0 }))} />
                        </label>
                        <label>
                          Metai
                          <input type="number" value={buildingEditForm.year} onChange={(event) => setBuildingEditForm((prev) => ({ ...prev, year: Number(event.target.value) || 0 }))} />
                        </label>
                        <label>
                          Renovacija
                          <input type="number" value={buildingEditForm.lastrenovationyear} onChange={(event) => setBuildingEditForm((prev) => ({ ...prev, lastrenovationyear: Number(event.target.value) || 0 }))} />
                        </label>
                        <label>
                          Energijos klasė
                          <select value={buildingEditForm.energy?.toString() ?? ''} onChange={(event) => setBuildingEditForm((prev) => ({ ...prev, energy: Number(event.target.value) }))}>
                            {energyClassOptions.map((option) => (
                              <option key={option.value} value={option.value}>
                                {option.label}
                              </option>
                            ))}
                          </select>
                        </label>
                      </div>
                      <div className="management-edit__actions">
                        <button className="btn" onClick={handleBuildingUpdate}>
                          Išsaugoti
                        </button>
                        <button className="btn btn--ghost" onClick={() => setEditingBuildingId(null)}>
                          Atšaukti
                        </button>
                      </div>
                    </div>
                  )}
                </article>
              ))}
              {buildings.length === 0 && <p className="muted">Kol kas neturite pastatų. Pridėkite pirmąjį žemiau.</p>}
            </div>
          </section>

        <section className="card">
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
                <input type="number" value={buildingForm.area} onChange={(event) => handleBuildingCreateChange('area', event.target.value)} />
              </label>
              <label>
                Aukštai
                <input type="number" value={buildingForm.floors} onChange={(event) => handleBuildingCreateChange('floors', event.target.value)} />
              </label>
              <label>
                Metai
                <input type="number" value={buildingForm.year} onChange={(event) => handleBuildingCreateChange('year', event.target.value)} />
              </label>
              <label>
                Renovacija
                <input type="number" value={buildingForm.lastrenovationyear} onChange={(event) => handleBuildingCreateChange('lastrenovationyear', event.target.value)} />
              </label>
              <label>
                Energijos klasė
                <select value={buildingForm.energy.toString()} onChange={(event) => handleBuildingCreateChange('energy', event.target.value)}>
                  {energyClassOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </label>
            </div>
            <button className="btn" onClick={handleBuildingCreate}>
              Išsaugoti pastatą
            </button>
          </section>
        </>
      )}

      {viewMode === 'apartments' && (
        <>
          <section className="card card--subtle">
            <div className="apartments-header">
              <button className="btn btn--ghost" onClick={handleBackToBuildings}>
                ← Grįžti į pastatus
              </button>
              <div>
                <p className="muted">Tvarkote pastatą</p>
                <h3>{selectedBuilding ? `${selectedBuilding.city}, ${selectedBuilding.address}` : 'Nepasirinkta'}</h3>
              </div>
            </div>
            <ul className="cascade-panel__summary">
              <li>
                <strong>Pastatas:</strong> {selectedBuilding ? `${selectedBuilding.area} m² · ${selectedBuilding.floors} aukšt.` : 'nepasirinkta'}
              </li>
              <li>
                <strong>Butas:</strong> {selectedApartmentSummary ?? 'nepasirinkta'}
              </li>
              <li>
                <strong>Nuotrauka:</strong> {selectedPictureSummary ?? 'nepasirinkta'}
              </li>
            </ul>
            <div className="tab-strip">
              <button className={apartmentTab === 'units' ? 'tab-strip__tab tab-strip__tab--active' : 'tab-strip__tab'} onClick={() => setApartmentTab('units')}>
                Butai
              </button>
              <button className={apartmentTab === 'schedule' ? 'tab-strip__tab tab-strip__tab--active' : 'tab-strip__tab'} onClick={() => setApartmentTab('schedule')}>
                Grafikas
              </button>
            </div>
          </section>

          {apartmentTab === 'units' && (
            <>
            <section className="card">
            <h3>Butų sąrašas</h3>
            <div className="table">
              <div className="table__row table__row--head table__row--apartments">
                <span>Buto informacija</span>
                <span>Parametrai</span>
                <span>Pastabos</span>
                <span>Veiksmai</span>
              </div>
              {apartmentsInBuilding.map((apartment) => (
                <div key={apartment.idApartment} className="table__group">
                  <div
                    className="table__row table__row--apartments"
                    onClick={() => {
                      setSelectedApartmentIdInput(apartment.idApartment)
                      if (activeListingApartmentId && activeListingApartmentId !== apartment.idApartment) {
                        closeListingPanel()
                      }
                    }}
                  >
                    <span>
                      <strong>{apartment.apartmentnumber ? `Nr. ${apartment.apartmentnumber}` : 'Be numerio'}</strong>
                      <p className="muted">{apartment.floor ? `${apartment.floor} aukštas` : 'Aukštas nenurodytas'}</p>
                    </span>
                    <span>
                      <p>{apartment.rooms} kamb. · {apartment.area} m²</p>
                      <p className="muted">
                        Apdaila {getOptionLabel(finishTypeOptions, apartment.finish)} · Šildymas {getOptionLabel(heatingTypeOptions, apartment.heating ?? null)}
                      </p>
                    </span>
                    <span>
                      <p className="muted">{apartment.notes || 'Pastabų nėra'}</p>
                      <p className="muted">{apartment.isWholeBuilding ? 'Visas pastatas' : 'Individualus butas'}</p>
                    </span>
                    <span className="table__actions table__actions--stacked">
                      <button
                        className="btn btn--ghost"
                        onClick={(event) => {
                          event.stopPropagation()
                          startApartmentEdit(apartment)
                        }}
                      >
                        Redaguoti
                      </button>
                      <button
                        className="btn btn--ghost"
                        onClick={(event) => {
                          event.stopPropagation()
                          handleApartmentDelete(apartment.idApartment)
                        }}
                      >
                        Pašalinti
                      </button>
                      <button
                        className="btn"
                        onClick={(event) => {
                          event.stopPropagation()
                          startListingCreation(apartment)
                        }}
                      >
                        Kurti skelbimą
                      </button>
                    </span>
                  </div>
                  {editingApartmentId === apartment.idApartment && (
                    <div className="management-edit management-edit--split">
                      <div>
                        <div className="form-grid">
                        <label>
                          Buto numeris
                          <input value={apartmentEditForm.apartmentnumber} onChange={(event) => setApartmentEditForm((prev) => ({ ...prev, apartmentnumber: event.target.value }))} />
                        </label>
                        <label>
                          Kambariai
                          <input type="number" value={apartmentEditForm.rooms} onChange={(event) => setApartmentEditForm((prev) => ({ ...prev, rooms: event.target.value }))} />
                        </label>
                        <label>
                          Plotas m²
                          <input type="number" value={apartmentEditForm.area} onChange={(event) => setApartmentEditForm((prev) => ({ ...prev, area: event.target.value }))} />
                        </label>
                        <label>
                          Aukštas
                          <input type="number" value={apartmentEditForm.floor} onChange={(event) => setApartmentEditForm((prev) => ({ ...prev, floor: event.target.value }))} />
                        </label>
                        <label>
                          Apdaila
                          <select value={apartmentEditForm.finish} onChange={(event) => setApartmentEditForm((prev) => ({ ...prev, finish: event.target.value }))}>
                            {finishTypeOptions.map((option) => (
                              <option key={option.value} value={String(option.value)}>
                                {option.label}
                              </option>
                            ))}
                          </select>
                        </label>
                        <label>
                          Šildymas
                          <select value={apartmentEditForm.heating} onChange={(event) => setApartmentEditForm((prev) => ({ ...prev, heating: event.target.value }))}>
                            <option value="">Nepriskirta</option>
                            {heatingTypeOptions.map((option) => (
                              <option key={option.value} value={String(option.value)}>
                                {option.label}
                              </option>
                            ))}
                          </select>
                        </label>
                        <label>
                          Pastabos
                          <input value={apartmentEditForm.notes} onChange={(event) => setApartmentEditForm((prev) => ({ ...prev, notes: event.target.value }))} />
                        </label>
                        <label>
                          <span>Visa pastato dalis?</span>
                          <select value={apartmentEditForm.isWholeBuilding ? 'taip' : 'ne'} onChange={(event) => setApartmentEditForm((prev) => ({ ...prev, isWholeBuilding: event.target.value === 'taip' }))}>
                            <option value="ne">Ne</option>
                            <option value="taip">Taip</option>
                          </select>
                        </label>
                      </div>
                        <div className="management-edit__actions">
                          <button className="btn" onClick={handleApartmentUpdate}>
                            Išsaugoti
                          </button>
                          <button className="btn btn--ghost" onClick={() => setEditingApartmentId(null)}>
                            Atšaukti
                          </button>
                        </div>
                      </div>
                      <div className="management-media-panel">
                        <h4>Nuotraukos</h4>
                        <p className="muted">Tvarkykite šio buto nuotraukas ir matomumą.</p>
                        <div className="picture-grid">
                          {picturesInApartment.map((picture) => {
                            const coverStyle = { backgroundImage: `url(${baseURL}/uploads/${picture.id})` }
                            return (
                              <div
                                key={picture.id}
                                className={selectedPictureId === picture.id ? 'picture-card picture-card--selected' : 'picture-card'}
                                onClick={() => {
                                  setSelectedPictureIdInput(picture.id)
                                  if (activeListingApartmentId === apartment.idApartment) {
                                    setListingPictureId(picture.id)
                                  }
                                }}
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
                      </div>
                    </div>
                  )}
                  {activeListingApartmentId === apartment.idApartment && (
                    <div className="management-edit management-edit--highlight">
                      <div className="management-edit__header">
                        <h4>Skelbimo kūrimas</h4>
                        <button className="btn btn--ghost" onClick={closeListingPanel}>
                          Užverti
                        </button>
                      </div>
                      <p className="muted">
                        Naudojama nuotrauka: <strong>{listingPicture ? (listingPicture.public ? 'Vieša' : 'Privati') : 'nepasirinkta'}</strong>
                      </p>
                      {listingPictureAlreadyUsed && <p className="hint">Ši nuotrauka jau naudojama kitame skelbime.</p>}
                      <div className="picture-grid picture-grid--compact">
                        {picturesInApartment.map((picture) => {
                          const coverStyle = { backgroundImage: `url(${baseURL}/uploads/${picture.id})` }
                          const isSelected = listingPictureId === picture.id
                          return (
                            <div
                              key={picture.id}
                              className={isSelected ? 'picture-card picture-card--selected' : 'picture-card'}
                              onClick={() => {
                                setListingPictureId(picture.id)
                                setSelectedPictureIdInput(picture.id)
                              }}
                            >
                              <div className="picture-card__media" style={coverStyle}>
                                {isSelected && <span className="badge">Viršelis</span>}
                                <span className="badge">{picture.public ? 'Vieša' : 'Privatu'}</span>
                              </div>
                            </div>
                          )
                        })}
                        {picturesInApartment.length === 0 && <p className="muted">Pridėkite nuotrauką, kad sukurtumėte skelbimą.</p>}
                      </div>
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
                      <div className="management-edit__actions">
                        <button className="btn" disabled={!listingPictureId} onClick={() => handleListingCreate(listingPictureId)}>
                          Skelbti
                        </button>
                        <button className="btn btn--ghost" onClick={closeListingPanel}>
                          Atšaukti
                        </button>
                      </div>
                    </div>
                  )}
                </div>
              ))}
              {apartmentsInBuilding.length === 0 && <p className="muted">Šiame pastate dar nėra butų.</p>}
            </div>
          </section>

              <section className="card">
                <h3>Naujas butas šiame pastate</h3>
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
                Apdaila
                <select value={apartmentForm.finish} onChange={(event) => setApartmentForm((prev) => ({ ...prev, finish: event.target.value }))}>
                  {finishTypeOptions.map((option) => (
                    <option key={option.value} value={String(option.value)}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Šildymas
                <select value={apartmentForm.heating} onChange={(event) => setApartmentForm((prev) => ({ ...prev, heating: event.target.value }))}>
                  <option value="">Nepriskirta</option>
                  {heatingTypeOptions.map((option) => (
                    <option key={option.value} value={String(option.value)}>
                      {option.label}
                    </option>
                  ))}
                </select>
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
                </>
              )}

          {apartmentTab === 'schedule' && (
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
          )}
        </>
      )}
    </div>
  )
}
