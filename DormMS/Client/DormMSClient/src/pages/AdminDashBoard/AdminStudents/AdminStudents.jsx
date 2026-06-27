import { useState, useEffect } from 'react'
import styles from './AdminStudents.module.css'
import { getStudentsPaged } from '../../../api/admin'

const profileStatusLabel = {
    0: 'Nekompletan',
    1: 'Na čekanju',
    2: 'Verifikovan',
    3: 'Odbijen'
}

const profileStatusClass = {
    0: 'incomplete',
    1: 'pending',
    2: 'verified',
    3: 'rejected'
}

const PAGE_SIZE = 10

function AdminStudents() {
    const [students, setStudents] = useState([])
    const [pageNumber, setPageNumber] = useState(1)
    const [totalPages, setTotalPages] = useState(1)
    const [totalCount, setTotalCount] = useState(0)

    const [profileStatusFilter, setProfileStatusFilter] = useState('')
    const [facultyFilter, setFacultyFilter] = useState('')
    const [freshmanFilter, setFreshmanFilter] = useState('')

    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    const fetchStudents = async () => {
        setLoading(true)
        setError(null)
        try {
            const data = await getStudentsPaged(pageNumber, PAGE_SIZE, profileStatusFilter, facultyFilter, freshmanFilter)
            setStudents(data.items)
            setTotalPages(data.totalPages)
            setTotalCount(data.totalCount)
        } catch (err) {
            setError(err.message)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        fetchStudents()
    }, [pageNumber, profileStatusFilter, freshmanFilter])

    const handleFacultySearch = (e) => {
        e.preventDefault()
        setPageNumber(1)
        fetchStudents()
    }

    const handleProfileStatusChange = (e) => {
        setProfileStatusFilter(e.target.value)
        setPageNumber(1)
    }

    const handleFreshmanChange = (e) => {
        setFreshmanFilter(e.target.value)
        setPageNumber(1)
    }

    return (
        <div className={styles.container}>
            <div className={styles.subtitleRow}>
                <h1 className={styles.title}>Studenti</h1>
            </div>

            <div className={styles.filters}>
                <div className={styles.filterField}>
                    <label htmlFor="profileStatusFilter">Status profila:</label>
                    <select id="profileStatusFilter" value={profileStatusFilter} onChange={handleProfileStatusChange}>
                        <option value="">Svi</option>
                        <option value="0">Nekompletan</option>
                        <option value="1">Na čekanju</option>
                        <option value="2">Verifikovan</option>
                        <option value="3">Odbijen</option>
                    </select>
                </div>

                <div className={styles.filterField}>
                    <label htmlFor="freshmanFilter">Godina:</label>
                    <select id="freshmanFilter" value={freshmanFilter} onChange={handleFreshmanChange}>
                        <option value="">Svi</option>
                        <option value="true">Brucoši</option>
                        <option value="false">Stariji studenti</option>
                    </select>
                </div>

                <form className={styles.facultySearch} onSubmit={handleFacultySearch}>
                    <input
                        type="text"
                        placeholder="Pretraga po fakultetu..."
                        value={facultyFilter}
                        onChange={(e) => setFacultyFilter(e.target.value)}
                    />
                    <button type="submit">Pretraži</button>
                </form>
            </div>

            {loading && <p className={styles.info}>Učitavanje...</p>}
            {error && <p className={styles.error}>{error}</p>}

            {!loading && !error && students.length === 0 && (
                <p className={styles.info}>Nema studenata za prikaz.</p>
            )}

            {!loading && !error && students.length > 0 && (
                <>
                    <div className={styles.list}>
                        {students.map((student) => (
                            <div key={student.id} className={styles.card}>
                                <div className={styles.cardInfo}>
                                    <span className={styles.cardName}>
                                        {student.name} {student.surname}
                                        {student.isFreshman && <span className={styles.freshmanTag}>brucoš</span>}
                                    </span>
                                    <span className={styles.cardMeta}>
                                        {student.facultyName ?? 'Fakultet nepoznat'} · Upis {student.yearOfEnrollment}
                                    </span>
                                </div>
                                <div className={styles.cardBadges}>
                                    {student.isBudgetStudent && (
                                        <span className={styles.budgetBadge}>Budžet</span>
                                    )}
                                    <span className={`${styles.statusBadge} ${styles[profileStatusClass[student.profileStatus]]}`}>
                                        {profileStatusLabel[student.profileStatus] ?? 'N/A'}
                                    </span>
                                </div>
                            </div>
                        ))}
                    </div>

                    <div className={styles.pagination}>
                        <button
                            className={styles.pageButton}
                            onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                            disabled={pageNumber <= 1}
                        >
                            ← Prethodna
                        </button>
                        <span className={styles.pageInfo}>
                            Stranica <strong>{pageNumber}</strong> od <strong>{totalPages}</strong>
                            {totalCount > 0 && ` (ukupno: ${totalCount})`}
                        </span>
                        <button
                            className={styles.pageButton}
                            onClick={() => setPageNumber((p) => Math.min(totalPages, p + 1))}
                            disabled={pageNumber >= totalPages}
                        >
                            Sledeća →
                        </button>
                    </div>
                </>
            )}
        </div>
    )
}

export default AdminStudents