import { fetchWithAuth } from "./httpClient"

const API_URL = 'http://localhost:5153/api/Assignment'

const getToken = () => localStorage.getItem('token')

export const rankFreshmen = async (competitionId) => {
    const res = await fetchWithAuth(`${API_URL}/rank-freshmen`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${getToken()}`,
        },
        body: JSON.stringify({ competitionId }),
    })

    if (!res.ok) {
        const text = await res.text().catch(() => null)
        throw new Error(text || 'Greška prilikom rangiranja brucoša.')
    }

    return res.json()
}

export const rankOlderStudents = async (competitionId) => {
    const res = await fetchWithAuth(`${API_URL}/rank-older`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${getToken()}`,
        },
        body: JSON.stringify({ competitionId }),
    })

    if (!res.ok) {
        const text = await res.text().catch(() => null)
        throw new Error(text || 'Greška prilikom rangiranja starijih studenata.')
    }

    return res.json()
}

export const getSavedRankingResults = async (competitionId, isFreshmanView) => {
    const res = await fetchWithAuth(
        `${API_URL}/results/${competitionId}?isFreshmanView=${isFreshmanView}`,
        {
            headers: {
                Authorization: `Bearer ${getToken()}`,
            },
        }
    )

    if (!res.ok) {
        const text = await res.text().catch(() => null)
        throw new Error(extractErrorMessage(text) || 'Greška prilikom učitavanja sačuvanih rezultata.')
    }

    return res.json()
}

export const assignOlderStudentsToDorms = async (competitionId) => {
    const res = await fetchWithAuth(`${API_URL}/assign-older`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${getToken()}`,
        },
        body: JSON.stringify({ competitionId }),
    })

    if (!res.ok) {
        const text = await res.text().catch(() => null)
        throw new Error(extractErrorMessage(text) || 'Greška prilikom raspodele soba.')
    }

    return res.json()
}

export const assignFreshmenToDorms = async (competitionId) => {
    const res = await fetchWithAuth(`${API_URL}/assign-freshmen`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${getToken()}`,
        },
        body: JSON.stringify({ competitionId }),
    })

    if (!res.ok) {
        const text = await res.text().catch(() => null)
        throw new Error(extractErrorMessage(text) || 'Greška prilikom raspodele soba.')
    }

    return res.json()
}

export const assignSingleStudent = async (rankingResultId) => {
    const res = await fetchWithAuth(`${API_URL}/assign-single/${rankingResultId}`, {
        method: 'POST',
        headers: {
            Authorization: `Bearer ${getToken()}`,
        },
    })

    if (!res.ok) {
        const text = await res.text().catch(() => null)
        throw new Error(extractErrorMessage(text) || 'Greška prilikom dodele sobe studentu.')
    }

    return res.json()
}

export const removeStudentFromRoom = async (studentId) => {
    const res = await fetchWithAuth(`${API_URL}/remove-from-room/${studentId}`, {
        method: 'POST',
        headers: {
            Authorization: `Bearer ${getToken()}`,
        },
    })

    if (!res.ok) {
        const text = await res.text().catch(() => null)
        throw new Error(text || 'Greška prilikom uklanjanja studenta iz sobe.')
    }

    return res.json()
}

export const resetFlow = async () => {
    const response = await fetch(`${API_URL}/reset-flow`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${localStorage.getItem('token')}` }
    })
    if (!response.ok) throw new Error('Greška prilikom iseljavanja studenata.')
    return response.json()
}

function extractErrorMessage(text) {
    if (!text) return null

    try {
        const parsed = JSON.parse(text)
        if (Array.isArray(parsed?.errors) && parsed.errors.length > 0) {
            return parsed.errors.join(' ')
        }
    } catch {
    }

    return text
}