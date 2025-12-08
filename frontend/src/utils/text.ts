const priceFormatter = new Intl.NumberFormat('lt-LT', {
  style: 'currency',
  currency: 'EUR',
  maximumFractionDigits: 0,
})

const finishTypeTranslations: Record<string, string> = {
  complete: 'Pilnai įrengtas',
  partial: 'Dalinė apdaila',
  unfinished: 'Neįrengtas',
  'not fully built': 'Ne iki galo pastatytas',
  foundations: 'Pamatai',
  other: 'Kita',
}

const heatingTypeTranslations: Record<string, string> = {
  central: 'Centrinis',
  gas: 'Dujinis',
  'solid fuel': 'Kietojo kuro',
  electric: 'Elektra',
  'heat pump': 'Šilumos siurblys',
  other: 'Kita',
}

const viewingStatusTranslations: Record<string, string> = {
  pending: 'Laukia patvirtinimo',
  confirmed: 'Patvirtinta',
  rejected: 'Atmesta',
  cancelled: 'Atšaukta',
  public: 'Vieša apžiūra',
}

type PriceFormatOptions = {
  rent?: boolean
}

export const formatPrice = (value?: number, options?: PriceFormatOptions) => {
  if (value === undefined || value === null) {
    return '—'
  }
  const formatted = priceFormatter.format(value)
  return options?.rent ? `${formatted} / mėn.` : formatted
}

export const translateFinishType = (value?: string | null) => {
  if (!value) {
    return 'Nenurodyta'
  }
  const normalized = value.trim().toLowerCase()
  return finishTypeTranslations[normalized] ?? value
}

export const translateHeatingType = (value?: string | null) => {
  if (!value) {
    return 'Nenurodyta'
  }
  const normalized = value.trim().toLowerCase()
  return heatingTypeTranslations[normalized] ?? value
}

export const translateViewingStatus = (value?: string | null) => {
  if (!value) {
    return 'Nežinoma'
  }
  const normalized = value.trim().toLowerCase()
  return viewingStatusTranslations[normalized] ?? value
}
