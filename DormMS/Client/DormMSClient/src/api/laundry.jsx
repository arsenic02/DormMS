import { fetchWithAuth } from "./httpClient"

const BASE_URL = 'http://localhost:5153/api/laundry'

export async function getAllMachines() {
  const token = localStorage.getItem('token')
  const response = await fetchWithAuth(`${BASE_URL}/machines`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  })

  if (!response.ok) {
    throw new Error('Greška pri učitavanju mašina')
  }

  return await response.json()
}

export async function getMachinesByDorm(dormId) {
  const token = localStorage.getItem('token')
  const response = await fetchWithAuth(`${BASE_URL}/machines/dorm/${dormId}`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  })

  if (!response.ok) {
    throw new Error('Greška pri učitavanju mašina')
  }

  return await response.json()
}

export async function getMachine(id) {
  const token = localStorage.getItem('token')
  const response = await fetchWithAuth(`${BASE_URL}/machines/${id}`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  })

  if (!response.ok) {
    throw new Error('Greška pri učitavanju mašine')
  }

  return await response.json()
}

export async function createMachine(data) {
  const token = localStorage.getItem('token')
  const response = await fetchWithAuth(`${BASE_URL}/machines`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(data)
  })

  if (!response.ok) {
    throw new Error('Greška pri kreiranju mašine')
  }

  return await response.json()
}

export async function updateMachine(id, data) {
  const token = localStorage.getItem('token')
  const response = await fetchWithAuth(`${BASE_URL}/machines/${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(data)
  })

  if (!response.ok) {
    throw new Error('Greška pri ažuriranju mašine')
  }
}

export async function deleteMachine(id) {
  const token = localStorage.getItem('token')
  const response = await fetchWithAuth(`${BASE_URL}/machines/${id}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  })

  if (!response.ok) {
    throw new Error('Greška pri brisanju mašine')
  }
}

export async function toggleMachineStatus(id) {
  const token = localStorage.getItem('token')
  const response = await fetchWithAuth(`${BASE_URL}/machines/${id}/toggle-status`, {
    method: 'PATCH',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  })

  if (!response.ok) {
    throw new Error('Greška pri promeni statusa mašine')
  }
}

export async function getAvailableSlots(machineId, date) {
  const token = localStorage.getItem('token')
  
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  const formattedDate = `${year}-${month}-${day}`
  
  const response = await fetchWithAuth(`${BASE_URL}/machines/${machineId}/slots?date=${formattedDate}`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  })

  if (!response.ok) {
    throw new Error('Greška pri učitavanju slobodnih termina')
  }

  return await response.json()
}

export async function createReservation(data) {
  const token = localStorage.getItem('token')
  
  // Formatiranje startTime kao lokalni datum/vreme bez vremenske zone (YYYY-MM-DDTHH:mm:ss)
  const startTimeDate = new Date(data.startTime)
  const year = startTimeDate.getFullYear()
  const month = String(startTimeDate.getMonth() + 1).padStart(2, '0')
  const day = String(startTimeDate.getDate()).padStart(2, '0')
  const hours = String(startTimeDate.getHours()).padStart(2, '0')
  const minutes = String(startTimeDate.getMinutes()).padStart(2, '0')
  const seconds = String(startTimeDate.getSeconds()).padStart(2, '0')
  
  const formattedData = {
    ...data,
    startTime: `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`
  }
  
  const response = await fetchWithAuth(`${BASE_URL}/reservations`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(formattedData)
  })

  if (!response.ok) {
    
    let errorMessage = 'Greška pri kreiranju rezervacije'
    try {
      const errorData = await response.text()
      if (errorData) {
        errorMessage = errorData
      }
    } catch (e) {
    
    }
    throw new Error(errorMessage)
  }

  return await response.json()
}

export async function cancelReservation(id) {
  const token = localStorage.getItem('token')
  const response = await fetchWithAuth(`${BASE_URL}/reservations/${id}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  })

  if (!response.ok) {
    let errorMessage = 'Greška pri otkazivanju rezervacije'
    try {
      const errorData = await response.text()
      if (errorData) {
        errorMessage = errorData
      }
    } catch (e) {
    }
    throw new Error(errorMessage)
  }
}

export async function getStudentReservations() {
  const token = localStorage.getItem('token')
  const response = await fetchWithAuth(`${BASE_URL}/reservations/student`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  })

  if (!response.ok) {
    throw new Error('Greška pri učitavanju rezervacija')
  }

  return await response.json()
}

export async function getMachineReservations(machineId) {
  const token = localStorage.getItem('token')
  const response = await fetchWithAuth(`${BASE_URL}/reservations/machine/${machineId}`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  })

  if (!response.ok) {
    throw new Error('Greška pri učitavanju rezervacija')
  }

  return await response.json()
}
