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
        // throw new Error(text || 'Greška prilikom učitavanja sačuvanih rezultata.')
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
        //throw new Error(text || 'Greška prilikom raspodele soba.')
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
        // throw new Error(text || 'Greška prilikom raspodele soba.')
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

// Backend nekad vrati ceo AssignmentResultDto kao JSON (BadRequest(result)) umesto
// plain teksta. Ova funkcija pokusava da izvuce citljivu poruku iz "errors" niza,
// a ako ne uspe (npr. ako JE plain tekst), vraca originalni string.
function extractErrorMessage(text) {
    if (!text) return null

    try {
        const parsed = JSON.parse(text)
        if (Array.isArray(parsed?.errors) && parsed.errors.length > 0) {
            return parsed.errors.join(' ')
        }
    } catch {
        // nije JSON, vrati kao plain tekst
    }

    return text
}