import { fetchWithAuth } from "./httpClient"

const BASE_URL = 'http://localhost:5153/api'

export async function loginUser(email, password) {
  const response = await fetch(`${BASE_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  })

  const data = await response.json()

  if (!response.ok) {
    throw new Error(data.message || 'Greška pri prijavi')
  }

  return data
}

export async function registerUser(formData) {
  const response = await fetch(`${BASE_URL}/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      name: formData.firstName,
      surname: formData.lastName,
      email: formData.email,
      password: formData.password,
      studentIDNumber: formData.indexNumber
    })
  })

  const data = await response.json()

  if (!response.ok) {
    throw new Error(data.message || 'Greška pri registraciji')
  }

  return data
}