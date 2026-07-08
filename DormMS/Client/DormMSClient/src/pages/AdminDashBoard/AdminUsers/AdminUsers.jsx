import { useState, useEffect } from 'react'
import styles from './AdminUsers.module.css'
import { getUsersPaged, deleteUser, createDormManager } from '../../../api/admin'
import { useAuth } from '../../../context/AuthContext'
import Modal from '../../../components/Modal/Modal'

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
    const { auth } = useAuth()

    const [users, setUsers] = useState([])
    const [pageNumber, setPageNumber] = useState(1)
    const [totalPages, setTotalPages] = useState(1)
    const [totalCount, setTotalCount] = useState(0)
    const [roleFilter, setRoleFilter] = useState('')

    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    const [deletingId, setDeletingId] = useState(null)
    const [deleteError, setDeleteError] = useState(null)

    const [showCreateModal, setShowCreateModal] = useState(false)
    const [createError, setCreateError] = useState(null)
    const [creating, setCreating] = useState(false)
    
    const [formData, setFormData] = useState({
        name: '',
        surname: '',
        email: '',
        phoneNumber: '',
        password: ''
    })

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

    const handleOpenCreateModal = () => {
        setShowCreateModal(true)
        setCreateError(null)
    }

    const handleCreateSubmit = async (e) => {
        e.preventDefault()
        setCreateError(null)
        setCreating(true)

        try {
            await createDormManager(formData)
            setShowCreateModal(false)
            setFormData({
                name: '',
                surname: '',
                email: '',
                phoneNumber: '',
                password: ''
            })
            fetchUsers()
        } catch (err) {
            setCreateError(err.message)
        } finally {
            setCreating(false)
        }
    }

    const handleFormChange = (e) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        })
    }

    const handleRoleFilterChange = (e) => {
        setRoleFilter(e.target.value)
        setPageNumber(1)
    }

    const handleDelete = async (user) => {
        const confirmed = window.confirm(
            `Da li ste sigurni da želite da obrišete korisnika "${user.profileName ?? user.email}"? Ova akcija je nepovratna.`
        )
        if (!confirmed) return

        setDeleteError(null)
        setDeletingId(user.id)

        try {
            await deleteUser(user.id, auth.token)

            // Ako je ovo bio poslednji korisnik na stranici, a nismo na prvoj strani, vrati se za jednu stranicu
            if (users.length === 1 && pageNumber > 1) {
                setPageNumber((p) => p - 1)
            } else {
                fetchUsers()
            }
        } catch (err) {
            setDeleteError(err.message)
        } finally {
            setDeletingId(null)
        }
    }

    return (
        <div className={styles.container}>
            <div className={styles.subtitleRow}>
                <h1 className={styles.title}>Korisnici</h1>

                <div className={styles.filters}>
                    <button 
                        type="button"
                        className={styles.createButton}
                        onClick={handleOpenCreateModal}
                    >
                        + Dodaj referenta
                    </button>
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
            {deleteError && <p className={styles.error}>{deleteError}</p>}

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
                                   
                                    {user.role !== 0 && (
                                        <button
                                            type="button"
                                            className={styles.deleteButton}
                                            onClick={() => handleDelete(user)}
                                            disabled={deletingId === user.id}
                                        >
                                            {deletingId === user.id ? 'Brisanje...' : 'Obriši'}
                                        </button>
                                    )}
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

            <Modal
                title="Dodaj referenta"
                isOpen={showCreateModal}
                onClose={() => setShowCreateModal(false)}
                maxWidth="500px"
            >
                <form onSubmit={handleCreateSubmit} className={styles.modalForm}>
                    {createError && <p className={styles.error}>{createError}</p>}
                    
                    <div className={styles.formField}>
                        <label htmlFor="name">Ime:</label>
                        <input
                            type="text"
                            id="name"
                            name="name"
                            value={formData.name}
                            onChange={handleFormChange}
                            required
                        />
                    </div>
                    
                    <div className={styles.formField}>
                        <label htmlFor="surname">Prezime:</label>
                        <input
                            type="text"
                            id="surname"
                            name="surname"
                            value={formData.surname}
                            onChange={handleFormChange}
                            required
                        />
                    </div>
                    
                    <div className={styles.formField}>
                        <label htmlFor="email">Email:</label>
                        <input
                            type="email"
                            id="email"
                            name="email"
                            value={formData.email}
                            onChange={handleFormChange}
                            required
                        />
                    </div>
                    
                    <div className={styles.formField}>
                        <label htmlFor="phoneNumber">Broj telefona:</label>
                        <input
                            type="text"
                            id="phoneNumber"
                            name="phoneNumber"
                            value={formData.phoneNumber}
                            onChange={handleFormChange}
                            required
                        />
                    </div>
                    
                    <div className={styles.formField}>
                        <label htmlFor="password">Lozinka:</label>
                        <input
                            type="password"
                            id="password"
                            name="password"
                            value={formData.password}
                            onChange={handleFormChange}
                            required
                        />
                    </div>
                    
                    <div className={styles.modalActions}>
                        <button
                            type="button"
                            className={styles.cancelButton}
                            onClick={() => setShowCreateModal(false)}
                            disabled={creating}
                        >
                            Otkaži
                        </button>
                        <button
                            type="submit"
                            className={styles.submitButton}
                            disabled={creating}
                        >
                            {creating ? 'Kreiranje...' : 'Kreiraj'}
                        </button>
                    </div>
                </form>
            </Modal>
        </div>
    )
}

export default AdminUsers