import { fetchWithAuth } from './httpClient'

const BASE_URL = 'http://localhost:5153/api/notifications'

function authHeaders() {
    const token = localStorage.getItem('token')
    return {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`
    }
}

export async function getMyDormNotificationsPaged(pageNumber, pageSize, unreadOnly) {
    const params = new URLSearchParams({
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString(),
        unreadOnly: unreadOnly.toString()
    })

    const res = await fetchWithAuth(`${BASE_URL}/my-dorm?${params.toString()}`, {
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom učitavanja notifikacija.')
    }

    return res.json()
}

export async function markNotificationAsRead(id) {
    const res = await fetchWithAuth(`${BASE_URL}/${id}/read`, {
        method: 'PATCH',
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom ažuriranja notifikacije.')
    }

    return true
}