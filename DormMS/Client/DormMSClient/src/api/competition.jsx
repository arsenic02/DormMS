const API_URL = 'http://localhost:5153/api/Competition'

const getToken = () => localStorage.getItem('token')

export const getCompetitions = async () => {
    const res = await fetch(API_URL, {
        headers: { Authorization: `Bearer ${getToken()}` },
    })
    if (!res.ok) throw new Error('Greška prilikom učitavanja konkursa.')
    return res.json()
}

export const createCompetition = async (form) => {
    const res = await fetch(API_URL, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${getToken()}`,
        },
        body: JSON.stringify(form),
    })
    if (!res.ok) {
        const data = await res.json().catch(() => null)
        throw new Error(data?.message || 'Greška prilikom kreiranja konkursa.')
    }
    return res.json()
}