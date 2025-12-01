export type UserRole = 'Administrator' | 'Broker' | 'Buyer' | 'User'

export interface Listing {
  idListing: number
  description: string
  askingprice: number
  rent: boolean
  fkPictureid: string
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

export interface TokenPayload {
  id: string
  role: UserRole
  exp: number
  iat: number
}
