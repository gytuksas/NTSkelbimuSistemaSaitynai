import { publicClient } from './client'
import type { RegistrationPayload, RegistrationResponse, RegistrationRole } from '../types/api'

const endpoints: Record<RegistrationRole, string> = {
  buyer: '/api/Authentication/register/buyer',
  broker: '/api/Authentication/register/broker',
}

const registerWithRole = async (
  role: RegistrationRole,
  payload: RegistrationPayload,
): Promise<RegistrationResponse> => {
  const { data } = await publicClient.post<RegistrationResponse>(endpoints[role], payload)
  return data
}

export const registerBuyer = (payload: RegistrationPayload) => registerWithRole('buyer', payload)
export const registerBroker = (payload: RegistrationPayload) => registerWithRole('broker', payload)
export const registerUser = registerWithRole
