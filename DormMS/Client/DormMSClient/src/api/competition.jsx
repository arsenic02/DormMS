const API_URL = 'http://localhost:5153/api/Competition'

const getToken = () => localStorage.getItem('token')

export const getCompetitions = async () => {
    const res = await fetch(API_URL, {
        headers: { Authorization: `Bearer ${getToken()}` },
    })
    if (!res.ok) throw new Error('Greška prilikom učitavanja konkursa.')
    return res.json()
}

export const getOpenCompetitions = async () => {
    const res = await fetch(`${API_URL}/open`, {
        headers: { Authorization: `Bearer ${getToken()}` },
    })
    if (!res.ok) throw new Error('Greška prilikom učitavanja otvorenih konkursa.')
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
        const text = await res.text()

        console.log('STATUS:', res.status)
        console.log('RESPONSE:', text)

        throw new Error(text || 'Greška prilikom kreiranja konkursa.')
    }

    return res.json()
}


//verzija 2
// export const createCompetition = async (form) => {
//     const res = await fetch(API_URL, {
//         method: 'POST',
//         headers: {
//             'Content-Type': 'application/json',
//             Authorization: `Bearer ${getToken()}`,
//         },
//         body: JSON.stringify(form),
//     })
//     if (!res.ok) {
//         const text = await res.text().catch(() => null)
//         throw new Error(text || 'Greška prilikom kreiranja konkursa.')
//     }
//     return res.json()
// }

//inicijalna verzija
// export const createCompetition = async (form) => {
//     const res = await fetch(API_URL, {
//         method: 'POST',
//         headers: {
//             'Content-Type': 'application/json',
//             Authorization: `Bearer ${getToken()}`,
//         },
//         body: JSON.stringify(form),
//     })
//     if (!res.ok) {
//         const data = await res.json().catch(() => null)
//         throw new Error(data?.message || 'Greška prilikom kreiranja konkursa.')
//     }
//     return res.json()
// }

///////////
export const toggleCompetition = async (id) => {
    const response = await fetch(
        `${API_URL}/${id}/toggle-status`,
        {
            method: 'PUT',
            headers: {
                Authorization: `Bearer ${getToken()}`,
            },
        }
    )

    if (!response.ok) {
        throw new Error('Greška prilikom promene statusa konkursa.')
    }

    return response.json()
}
