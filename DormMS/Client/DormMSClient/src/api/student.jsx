import { fetchWithAuth } from "./httpClient"

const BASE_URL = 'http://localhost:5153/api/Student'
const getToken = () => localStorage.getItem('token')

export async function updateStudentProfile(profileData, token) {
  const response = await fetchWithAuth(`${BASE_URL}/me/profile`, {
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
  const response = await fetchWithAuth(`${BASE_URL}/me`, {
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
  const response = await fetchWithAuth(`${BASE_URL}/me/financial-status`, {
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
    `${BASE_URL}/me`,
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
  const response = await fetchWithAuth(`${BASE_URL}`, {
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

export async function getStudentsPaged(token, pageNumber = 1, pageSize = 10, filters = {}) {
  const queryParams = new URLSearchParams({
    pageNumber: pageNumber.toString(),
    pageSize: pageSize.toString(),
  })

  if (filters.profileStatus !== undefined && filters.profileStatus !== null) {
    queryParams.append('profileStatus', filters.profileStatus.toString())
  }
  if (filters.financialStatus !== undefined && filters.financialStatus !== null) {
    queryParams.append('financialStatus', filters.financialStatus.toString())
  }
  if (filters.faculty) {
    queryParams.append('faculty', filters.faculty)
  }
  if (filters.isFreshman !== undefined && filters.isFreshman !== null) {
    queryParams.append('isFreshman', filters.isFreshman.toString())
  }

  const response = await fetchWithAuth(`${BASE_URL}/paged?${queryParams.toString()}`, {
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
  const response = await fetchWithAuth(`${BASE_URL}/${studentId}/verify-profile`, {
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

export async function verifyStudentFinancialStatus(studentId, status, token) {
  const response = await fetchWithAuth(`${BASE_URL}/${studentId}/verify-financial-status`, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(status)
  })

  const data = await response.json().catch(() => null)

  if (!response.ok) {
    throw new Error(data?.message || 'Greška pri verifikaciji finansijskog statusa')
  }

  return data
}

// Vraca informacije o sobi ulogovanog studenta (broj sobe, dom, cimeri).
// Ako student jos nema dodeljenu sobu, vraceni objekat ima hasRoom: false.
export const getMyRoom = async () => {
  const res = await fetchWithAuth(`${BASE_URL}/me/room`, {
    headers: { Authorization: `Bearer ${getToken()}` },
  })
  if (!res.ok) throw new Error('Greška prilikom učitavanja podataka o sobi.')
  return res.json()
}