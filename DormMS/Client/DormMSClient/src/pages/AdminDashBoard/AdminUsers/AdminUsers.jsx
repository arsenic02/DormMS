import { useState, useEffect } from 'react'
import styles from './AdminUsers.module.css'
import { getUsersPaged } from '../../../api/admin'

const roleLabel = {
    0: 'Admin',
    1: 'Referent',
    2: 'Student'
}

const roleClass = {
    0: 'admin',
    1: 'manager',
    2: 'student'
}

const PAGE_SIZE = 10

function AdminUsers() {
    const [users, setUsers] = useState([])
    const [pageNumber, setPageNumber] = useState(1)
    const [totalPages, setTotalPages] = useState(1)
    const [totalCount, setTotalCount] = useState(0)
    const [roleFilter, setRoleFilter] = useState('')

    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    const fetchUsers = async () => {
        setLoading(true)
        setError(null)
        try {
            const data = await getUsersPaged(pageNumber, PAGE_SIZE, roleFilter)
            setUsers(data.items)
            setTotalPages(data.totalPages)
            setTotalCount(data.totalCount)
        } catch (err) {
            setError(err.message)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        fetchUsers()
    }, [pageNumber, roleFilter])

    const handleRoleFilterChange = (e) => {
        setRoleFilter(e.target.value)
        setPageNumber(1)
    }

    return (
        <div className={styles.container}>
            <div className={styles.subtitleRow}>
                <h1 className={styles.title}>Korisnici</h1>

                <div className={styles.filters}>
                    <div className={styles.filterField}>
                        <label htmlFor="roleFilter">Tip:</label>
                        <select id="roleFilter" value={roleFilter} onChange={handleRoleFilterChange}>
                            <option value="">Svi</option>
                            <option value="0">Admin</option>
                            <option value="1">Referent</option>
                            <option value="2">Student</option>
                        </select>
                    </div>
                </div>
            </div>

            {loading && <p className={styles.info}>Učitavanje...</p>}
            {error && <p className={styles.error}>{error}</p>}

            {!loading && !error && users.length === 0 && (
                <p className={styles.info}>Nema korisnika za prikaz.</p>
            )}

            {!loading && !error && users.length > 0 && (
                <>
                    <div className={styles.list}>
                        {users.map((user) => (
                            <div key={user.id} className={styles.card}>
                                <div className={styles.cardInfo}>
                                    <span className={styles.cardName}>
                                        {user.profileName ?? '(profil nije pronađen)'}
                                    </span>
                                    <span className={styles.cardEmail}>{user.email}</span>
                                </div>
                                <div className={styles.cardBadges}>
                                    <span className={`${styles.roleBadge} ${styles[roleClass[user.role]]}`}>
                                        {roleLabel[user.role] ?? 'N/A'}
                                    </span>
                                    <span className={`${styles.statusBadge} ${user.isActive ? styles.active : styles.inactive}`}>
                                        {user.isActive ? 'Aktivan' : 'Neaktivan'}
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

export default AdminUsers