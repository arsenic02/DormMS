import { fetchWithAuth } from "./httpClient"

const BASE_URL = 'http://localhost:5153/api/move-out-requests'

function authHeaders() {
    const token = localStorage.getItem('token')
    return {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`
    }
}

export async function createMoveOutRequest() {
    const res = await fetchWithAuth(BASE_URL, {
        method: 'POST',
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom slanja zahteva za iseljenje.')
    }

    return res.json()
}

export async function getMyMoveOutRequest() {
    const res = await fetchWithAuth(`${BASE_URL}/my`, {
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom učitavanja zahteva.')
    }

    return res.json()
}

const MANAGER_BASE_URL = 'http://localhost:5153/api/move-out-requests/manager'

export async function getPendingMoveOutRequestsForManager() {
    const res = await fetchWithAuth(`${MANAGER_BASE_URL}/pending`, {
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom učitavanja zahteva za iseljenje.')
    }

    return res.json()
}

export async function approveMoveOutRequest(requestId) {
    const res = await fetchWithAuth(`${MANAGER_BASE_URL}/${requestId}/approve`, {
        method: 'PUT',
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom odobravanja iseljenja.')
    }

    return res.json()
}

export async function rejectMoveOutRequest(requestId) {
    const res = await fetchWithAuth(`${MANAGER_BASE_URL}/${requestId}/reject`, {
        method: 'PUT',
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom odbijanja iseljenja.')
    }

    return res.json()
}
