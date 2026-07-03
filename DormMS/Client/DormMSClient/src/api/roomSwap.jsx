import { fetchWithAuth } from "./httpClient"

const BASE_URL = 'http://localhost:5153/api/room-swap-requests'

function authHeaders() {
    const token = localStorage.getItem('token')
    return {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`
    }
}

export async function createSwapRequest(receiverStudentIdNumber) {
    const res = await fetchWithAuth(BASE_URL, {
        method: 'POST',
        headers: authHeaders(),
        body: JSON.stringify({ receiverStudentIdNumber: receiverStudentIdNumber })
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom slanja zahteva za zamenu sobe.')
    }

    return res.json()
}

export async function respondToSwapRequest(requestId, accept) {
    const res = await fetchWithAuth(`${BASE_URL}/${requestId}/respond`, {
        method: 'POST',
        headers: authHeaders(),
        body: JSON.stringify({ accept })
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom odgovaranja na zahtev.')
    }

    return res.json()
}

export async function getIncomingSwapRequests() {
    const res = await fetchWithAuth(`${BASE_URL}/incoming`, {
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom učitavanja zahteva.')
    }

    return res.json()
}

export async function getOutgoingSwapRequests() {
    const res = await fetchWithAuth(`${BASE_URL}/outgoing`, {
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom učitavanja zahteva.')
    }

    return res.json()
}

const MANAGER_BASE_URL = 'http://localhost:5153/api/room-swap-requests/manager'

export async function getPendingSwapRequestsForManager() {
    const res = await fetchWithAuth(`${MANAGER_BASE_URL}/pending`, {
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom učitavanja zahteva za zamenu soba.')
    }

    return res.json()
}

export async function approveSwapRequest(requestId) {
    const res = await fetchWithAuth(`${MANAGER_BASE_URL}/${requestId}/approve`, {
        method: 'PUT',
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom odobravanja zamene sobe.')
    }

    return res.json()
}

export async function rejectSwapRequest(requestId) {
    const res = await fetchWithAuth(`${MANAGER_BASE_URL}/${requestId}/reject`, {
        method: 'PUT',
        headers: authHeaders()
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom odbijanja zamene sobe.')
    }

    return res.json()
}