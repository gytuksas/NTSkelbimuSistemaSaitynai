const pad = (value: number) => value.toString().padStart(2, '0')

export const toDateTimeLocal = (value: string | Date) => {
  const date = typeof value === 'string' ? new Date(value) : value
  if (Number.isNaN(date.getTime())) {
    return ''
  }
  const year = date.getFullYear()
  const month = pad(date.getMonth() + 1)
  const day = pad(date.getDate())
  const hours = pad(date.getHours())
  const minutes = pad(date.getMinutes())
  return `${year}-${month}-${day} ${hours}:${minutes}`
}

export const formatFriendly = (value?: string) => {
  if (!value) return 'Nenurodyta'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }
  return new Intl.DateTimeFormat('lt-LT', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(date)
}
