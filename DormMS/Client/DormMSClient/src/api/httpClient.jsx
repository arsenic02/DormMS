export async function fetchWithAuth(url, options = {}) {
    const res = await fetch(url, options)
 
    if (res.status === 401) {
        localStorage.removeItem('token')
        localStorage.removeItem('role')
 
        // window.location (ne useNavigate) jer ovaj fajl nije React komponenta
        // i nema pristup React Router hook-ovima.
        window.location.href = '/login?sessionExpired=true'
 
        // Bacamo gresku da prekinemo dalje izvrsavanje pozivajuce funkcije
        // (npr. da ne nastavi na res.json() nad praznim/nevalidnim odgovorom).
        throw new Error('Sesija je istekla, ulogujte se ponovo.')
    }
 
    return res
}
 