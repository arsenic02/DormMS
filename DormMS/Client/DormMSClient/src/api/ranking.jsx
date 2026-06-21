const API_URL = 'http://localhost:5153/api/Assignment'

const getToken = () => localStorage.getItem('token')

export const rankFreshmen = async (competitionId) => {
    const res = await fetch(`${API_URL}/rank-freshmen`, {
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
    const res = await fetch(`${API_URL}/rank-older`, {
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