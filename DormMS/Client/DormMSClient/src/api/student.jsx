import { fetchWithAuth } from "./httpClient"

const BASE_URL = 'http://localhost:5153/api'

export async function updateStudentProfile(profileData, token) {
  const response = await fetchWithAuth(`${BASE_URL}/student/me/profile`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(profileData)
  })
  const data = await response.json()
  // console.log('Status:', response.status)
  // const text = await response.text()
  // console.log('Response body:', text)

  if (!response.ok) {
    throw new Error(data.message || 'Greška pri čuvanju profila')
    //throw new Error('Greška pri čuvanju profila')
  }

  return data//text ? JSON.parse(text) : {}
}

export async function getStudentProfile(token) {
  const response = await fetchWithAuth(`${BASE_URL}/student/me`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  })

  const data = await response.json()

  if (!response.ok) {
    throw new Error(data.message || 'Greška pri učitavanju profila')
  }

  return data
}

export async function submitFinancialStatus(financialData, token) {
  const response = await fetchWithAuth(`${BASE_URL}/student/me/financial-status`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(financialData)
  })

  const data = await response.json().catch(() => null)

  if (!response.ok) {
    throw new Error(data?.message || 'Greška pri čuvanju finansijskog statusa')
  }

  return data
}

export async function getFinancialStatus(token) {
  const response = await fetchWithAuth(
    `${BASE_URL}/student/me`,
    {
      headers: {
        Authorization: `Bearer ${token}`
      }
    }
  )

  const data = await response.json()

  return data.financialStatus
}

export async function getAllStudents(token) {
  const response = await fetchWithAuth(`${BASE_URL}/Student`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  })

  const data = await response.json()

  if (!response.ok) {
    throw new Error(data?.message || 'Greška pri učitavanju studenata')
  }

  return data
}

export async function verifyStudentProfile(studentId, status, token) {
  const response = await fetchWithAuth(`${BASE_URL}/Student/${studentId}/verify-profile`, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({ status })
  })

  const data = await response.json().catch(() => null)

  if (!response.ok) {
    throw new Error(data?.message || 'Greška pri verifikaciji profila')
  }

  return data
}

export async function verifyStudentFinancialStatus(studentId, isVerified, token) {
  const response = await fetchWithAuth(`${BASE_URL}/Student/${studentId}/verify-financial-status`, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(isVerified)
  })

  const data = await response.json().catch(() => null)

  if (!response.ok) {
    throw new Error(data?.message || 'Greška pri verifikaciji finansijskog statusa')
  }

  return data
}