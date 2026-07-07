export async function fetchWithAuth(url, options = {}) {
    const res = await fetch(url, options)
 
    if (res.status === 401) {
        localStorage.removeItem('token')
        localStorage.removeItem('role')
 
        window.location.href = '/login?sessionExpired=true'
 
        throw new Error('Sesija je istekla, ulogujte se ponovo.')
    }
 
    return res
}
 