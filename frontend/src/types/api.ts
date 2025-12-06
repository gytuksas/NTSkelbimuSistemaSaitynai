export type UserRole = 'Administrator' | 'Broker' | 'Buyer' | 'User'

export interface Listing {
  idListing: number
  description: string
  askingprice: number
  rent: boolean
  fkPictureid: string
}

export interface PublicListing {
  id: number
  description: string
  askingPrice: number
  rent: boolean
  buildingCity?: string | null
  buildingAddress?: string | null
  pictureId?: string | null
  pictureUrl?: string | null
  nextViewingFrom?: string | null
  nextViewingTo?: string | null
}

export interface PublicAvailability {
  id: number
  from: string
  to: string
}

export interface AvailabilitySlot {
  availabilityId: number
  from: string
  to: string
}

export interface PublicViewing {
  id: number
  from: string
  to: string
}

export interface PublicListingDetails extends PublicListing {
  apartmentArea?: number | null
  rooms?: number | null
  apartmentId?: number | null
  buildingId?: number | null
  buildingEnergyClass?: string | null
  buildingFloors?: number | null
  buildingYear?: number | null
  buildingLastRenovationYear?: number | null
  apartmentNumber?: number | null
  apartmentFloor?: number | null
  apartmentIsWholeBuilding?: boolean | null
  apartmentNotes?: string | null
  apartmentHeating?: string | null
  apartmentFinish?: string | null
  brokerName?: string | null
  brokerPhone?: string | null
  galleryPictureIds: string[]
  galleryPictureUrls: string[]
  availabilities: PublicAvailability[]
  availableSlots: AvailabilitySlot[]
  publicViewings: PublicViewing[]
}

export interface Picture {
  id: string
  public: boolean
  fkApartmentidApartment: number
}

export interface Building {
  idBuilding: number
  city: string
  address: string
  area: number
  year: number
  lastrenovationyear?: number
  floors: number
  energy?: number
  fkBrokeridUser: number
}

export interface Apartment {
  idApartment: number
  apartmentnumber?: number
  area: number
  floor?: number
  rooms: number
  notes?: string
  heating?: number
  finish: number
  fkBuildingidBuilding: number
  isWholeBuilding: boolean
}

export interface Availability {
  idAvailability: number
  from: string
  to: string
  fkBrokeridUser: number
}

export interface Viewing {
  idViewing: number
  from: string
  to: string
  status: number
  fkAvailabilityidAvailability: number
  fkListingidListing: number
  fkBuyeridUser?: number | null
  buyerName?: string | null
  buyerPhone?: string | null
  buyerEmail?: string | null
}

export interface BuyerViewing {
  id: number
  listingId: number
  listingTitle?: string | null
  city?: string | null
  address?: string | null
  from: string
  to: string
  statusId: number
  status: string
  pictureId?: string | null
  pictureUrl?: string | null
  brokerName?: string | null
  brokerPhone?: string | null
}

export interface Buyer {
  idUser: number
  confirmed: boolean
  blocked: boolean
}

export interface Broker {
  idUser: number
  confirmed: boolean
  blocked: boolean
}

export interface AppUser {
  idUser: number
  name: string
  surname: string
  email: string
  phone: string
  registrationtime: string
  profilepicture?: string
}

export type RegistrationRole = 'buyer' | 'broker'

export interface RegistrationPayload {
  name: string
  surname: string
  email: string
  phone: string
  password: string
}

export interface RegistrationResponse {
  userId: number
  role: UserRole
}

export interface TokenPayload {
  id?: string
  sub?: string
  role?: UserRole
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: UserRole
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string
  exp: number
  iat: number
}
