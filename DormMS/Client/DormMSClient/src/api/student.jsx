const BASE_URL = 'http://localhost:5153/api'

export async function updateStudentProfile(profileData, token) {
    const response = await fetch(`${BASE_URL}/student/me/profile`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(profileData)
    })
    const data = await response.json()
    // console.log('Status:', response.status)
    // const text = await response.text()
    // console.log('Response body:', text)

    if (!response.ok) {
        throw new Error(data.message || 'Greška pri čuvanju profila')
        //throw new Error('Greška pri čuvanju profila')
    }

    return data//text ? JSON.parse(text) : {}
}
