import { fetchWithAuth } from "./httpClient"

const API_URL = 'http://localhost:5153/api/CompetitionApplication'

const getToken = () => localStorage.getItem('token')

export const applyToCompetition = async (competitionId, notes = '', documentUrl = null) => {
    const res = await fetchWithAuth(`${API_URL}/${competitionId}/apply`, {
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

    const res = await fetchWithAuth(`${API_URL}/upload-document`, {
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

// Vraca listu prijava (CompetitionApplicationGetDto) za dati konkurs
export const getApplicationsByCompetition = async (competitionId) => {
    const res = await fetchWithAuth(`${API_URL}/${competitionId}/applications`, {
        headers: { Authorization: `Bearer ${getToken()}` },
    })
    if (!res.ok) throw new Error('Greška prilikom učitavanja prijava.')
    return res.json()
}

// Vraca paginiranu listu prijava za dati konkurs, opciono filtriranu po statusu.
// status: broj (0=Pending, 1=Approved, 2=Rejected, 3=Waitlisted) ili null/undefined za sve
export const getApplicationsPaged = async (competitionId, pageNumber, pageSize, status) => {
    const params = new URLSearchParams({
        pageNumber: String(pageNumber),
        pageSize: String(pageSize),
    })
    if (status !== null && status !== undefined && status !== '') {
        params.set('status', String(status))
    }

    const res = await fetchWithAuth(`${API_URL}/${competitionId}/applications-paged?${params.toString()}`, {
        headers: { Authorization: `Bearer ${getToken()}` },
    })
    if (!res.ok) throw new Error('Greška prilikom učitavanja prijava.')
    return res.json()
}

// Verifikuje (odobri/odbije) jednu prijavu preko bulk-verify endpointa,
// koristeci StartId=EndId=applicationId (nema potrebe za posebnim endpointom).
export const verifyApplication = async (applicationId, status) => {
    const res = await fetchWithAuth(`${API_URL}/bulk-verify`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${getToken()}`,
        },
        body: JSON.stringify({
            startId: applicationId,
            endId: applicationId,
            status,
        }),
    })

    if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Greška prilikom verifikacije prijave.')
    }

    return res.json()
}
