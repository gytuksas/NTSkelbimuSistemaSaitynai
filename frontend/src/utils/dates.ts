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

export const toDateTimeLocalInput = (value: string | Date) => {
  const date = typeof value === 'string' ? new Date(value) : value
  if (Number.isNaN(date.getTime())) {
    return ''
  }
  const year = date.getFullYear()
  const month = pad(date.getMonth() + 1)
  const day = pad(date.getDate())
  const hours = pad(date.getHours())
  const minutes = pad(date.getMinutes())
  return `${year}-${month}-${day}T${hours}:${minutes}`
}

export const toUtcDateTimeString = (value: string) => {
  if (!value) {
    return value
  }
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }
  const year = date.getUTCFullYear()
  const month = pad(date.getUTCMonth() + 1)
  const day = pad(date.getUTCDate())
  const hours = pad(date.getUTCHours())
  const minutes = pad(date.getUTCMinutes())
  return `${year}-${month}-${day} ${hours}:${minutes}`
}

const dateFormatter = new Intl.DateTimeFormat('lt-LT', {
  year: 'numeric',
  month: 'short',
  day: 'numeric',
})

export const formatFriendly = (value?: string) => {
  if (!value) return 'Nenurodyta'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }
  const datePart = dateFormatter.format(date)
  const timePart = `${pad(date.getHours())}:${pad(date.getMinutes())}`
  return `${datePart} ${timePart}`
}
