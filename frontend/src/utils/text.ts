const priceFormatter = new Intl.NumberFormat('lt-LT', {
  style: 'currency',
  currency: 'EUR',
  maximumFractionDigits: 0,
})

export const formatPrice = (value?: number) => {
  if (value === undefined || value === null) {
    return '—'
  }
  return priceFormatter.format(value)
}
