import { baseURL } from '../api/client'

const HTTP_PATTERN = /^https?:\/\//i

export const resolvePictureSrc = (pictureUrl?: string | null, pictureId?: string | null) => {
  if (pictureUrl) {
    return HTTP_PATTERN.test(pictureUrl) ? pictureUrl : `${baseURL}${pictureUrl}`
  }
  if (pictureId) {
    return `${baseURL}/uploads/${pictureId}`
  }
  return undefined
}

export const buildCoverStyle = (pictureUrl?: string | null, pictureId?: string | null) => {
  const resolved = resolvePictureSrc(pictureUrl, pictureId)
  return resolved ? { backgroundImage: `url(${resolved})` } : undefined
}
