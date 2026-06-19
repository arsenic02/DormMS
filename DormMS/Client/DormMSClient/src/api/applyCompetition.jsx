const API_URL = 'http://localhost:5153/api/CompetitionApplication'

const getToken = () => localStorage.getItem('token')

export const applyToCompetition = async (competitionId, notes = '', documentUrl = null) => {
    const res = await fetch(`${API_URL}/${competitionId}/apply`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${getToken()}`,
        },
        body: JSON.stringify({ notes, documentUrl }),
    })

    if (!res.ok) {
        const text = await res.text().catch(() => null)
        throw new Error(text || 'Greška prilikom prijave na konkurs.')

        // const data = await res.json().catch(() => null)
        // throw new Error(data?.error || data || 'Greška prilikom prijave na konkurs.')
    }

    return res.json()
}

export const uploadDocument = async (file) => {
    const formData = new FormData()
    formData.append('file', file)

    const res = await fetch(`${API_URL}/upload-document`, {
        method: 'POST',
        headers: {
            Authorization: `Bearer ${getToken()}`,
        },
        body: formData,
    })

    if (!res.ok) {
        const text = await res.text().catch(() => null)
        throw new Error(text || 'Greška prilikom otpremanja dokumenta.')
    }

    return res.json()
}