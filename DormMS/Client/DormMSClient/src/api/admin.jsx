import { fetchWithAuth } from './httpClient'

const DORM_URL = 'http://localhost:5153/api/dorm'

function authHeaders() {
    const token = localStorage.getItem('token')
    return {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`
    }
}

export async function getDorms() {
    const res = await fetchWithAuth(DORM_URL, {
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom učitavanja domova.')
    }

    return res.json()
}

export async function getDorm(id) {
    const res = await fetchWithAuth(`${DORM_URL}/${id}`, {
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom učitavanja doma.')
    }

    return res.json()
}

export async function createDorm(dto) {
    const res = await fetchWithAuth(DORM_URL, {
        method: 'POST',
        headers: authHeaders(),
        body: JSON.stringify(dto)
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom kreiranja doma.')
    }

    return res.json()
}

export async function updateDorm(id, dto) {
    const res = await fetchWithAuth(`${DORM_URL}/${id}`, {
        method: 'PUT',
        headers: authHeaders(),
        body: JSON.stringify(dto)
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom izmene doma.')
    }

    return true
}

export async function generateRooms(dto) {
    const res = await fetchWithAuth(`${DORM_URL}/${dto.dormId}/generate-rooms`, {
        method: 'POST',
        headers: authHeaders(),
        body: JSON.stringify(dto)
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom generisanja soba.')
    }

    return true
}

export async function deleteAllRooms(dormId) {
    const res = await fetchWithAuth(`${DORM_URL}/${dormId}/rooms`, {
        method: 'DELETE',
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom brisanja soba.')
    }

    return res.json()
}

const USERS_URL = 'http://localhost:5153/api/users'

export async function getUsersPaged(pageNumber, pageSize, role) {
    const params = new URLSearchParams({
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString()
    })

    if (role !== '' && role !== null && role !== undefined) {
        params.append('role', role.toString())
    }

    const res = await fetchWithAuth(`${USERS_URL}?${params.toString()}`, {
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom učitavanja korisnika.')
    }

    return res.json()
}


export async function deleteUser(id, token) {
  const response = await fetchWithAuth(`${USERS_URL}/${id}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  })

  if (!response.ok) {
    const data = await response.json().catch(() => null)
    throw new Error(data?.message || 'Greška pri brisanju korisnika')
  }
}

export async function createDormManager(dto) {
  const res = await fetchWithAuth(`${USERS_URL}/dorm-manager`, {
    method: 'POST',
    headers: authHeaders(),
    body: JSON.stringify(dto)
  })

  if (!res.ok) {
    const text = await res.text()
    throw new Error(text || 'Greška prilikom kreiranja referenta.')
  }

  return res.json()
}


const STUDENTS_URL = 'http://localhost:5153/api/student'

export async function getStudentsPaged(pageNumber, pageSize, faculty, isFreshman) {
    const params = new URLSearchParams({
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString()
    })

    if (faculty && faculty.trim()) {
        params.append('faculty', faculty.trim())
    }
    if (isFreshman !== '' && isFreshman !== null && isFreshman !== undefined) {
        params.append('isFreshman', isFreshman.toString())
    }

    const res = await fetchWithAuth(`${STUDENTS_URL}/paged?${params.toString()}`, {
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom učitavanja studenata.')
    }

    return res.json()
}

export async function getAdminProfile() {
    const res = await fetchWithAuth(`${USERS_URL}/admin/me`, {
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom učitavanja profila.')
    }

    return res.json()
}

export async function getDormManagerProfile() {
    const res = await fetchWithAuth(`${USERS_URL}/dorm-manager/me`, {
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom učitavanja profila.')
    }

    return res.json()
}
