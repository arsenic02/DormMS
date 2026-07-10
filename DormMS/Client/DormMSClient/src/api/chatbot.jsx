import { fetchWithAuth } from './httpClient'

const API_URL = 'http://localhost:5153/api'

export async function askChatbot(question) {
    const token = localStorage.getItem('token')
    
    const response = await fetchWithAuth(`${API_URL}/chatbot/ask`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ question })
    })

    if (!response.ok) {
        throw new Error('Greška pri slanju pitanja')
    }

    return response.json()
}
