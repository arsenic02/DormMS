import { fetchWithAuth } from "./httpClient"

const API_URL = 'http://localhost:5153/api/DormitoryRoom'

const getToken = () => localStorage.getItem('token')

export const getRoomsPaged = async (pageNumber = 1, pageSize = 12) => {
    const res = await fetchWithAuth(`${API_URL}/rooms-paged?pageNumber=${pageNumber}&pageSize=${pageSize}`, {
        headers: { Authorization: `Bearer ${getToken()}` },
    })
    if (!res.ok) throw new Error('Greška prilikom učitavanja soba.')
    return res.json()
}