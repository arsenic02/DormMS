import { useState, useEffect } from 'react'
import styles from './DormManagerCompetition.module.css'
import { getCompetitionsPaged, createCompetition, toggleCompetition } from '../../../api/competition'
import { useNavigate } from 'react-router-dom'

const statusLabel = {
    0: 'Nacrt',
    1: 'Otvoren',
    2: 'Zatvoren',
    3: 'Završen',
}

const statusClass = {
    0: 'draft',
    1: 'open',
    2: 'closed',
    3: 'completed',
}

const typeLabel = {
    0: 'Stariji studenti',
    1: 'Brucoši',
}

const PAGE_SIZE = 10

function DormManagerCompetition() {
    const navigate = useNavigate()

    const [form, setForm] = useState({
        name: '',
        description: '',
        startDate: new Date().toISOString().split('T')[0],
        endDate: '',
        type: 0,
        maxStudents: '',
    })
    const [loading, setLoading] = useState(false)
    const [error, setError] = useState(null)
    const [success, setSuccess] = useState(false)

    const [competitions, setCompetitions] = useState([])
    const [pageNumber, setPageNumber] = useState(1)
    const [totalPages, setTotalPages] = useState(1)
    const [totalCount, setTotalCount] = useState(0)
    const [typeFilter, setTypeFilter] = useState('') // '' = svi tipovi
    const [statusFilter, setStatusFilter] = useState('') // '' = svi statusi

    const [listLoading, setListLoading] = useState(true)
    const [listError, setListError] = useState(null)

    const fetchCompetitions = async () => {
        setListLoading(true)
        setListError(null)
        try {
            const data = await getCompetitionsPaged(pageNumber, PAGE_SIZE, typeFilter, statusFilter)
            setCompetitions(data.items)
            setTotalPages(data.totalPages)
            setTotalCount(data.totalCount)
        } catch (err) {
            setListError(err.message)
        } finally {
            setListLoading(false)
        }
    }

    useEffect(() => {
        fetchCompetitions()
    }, [pageNumber, typeFilter, statusFilter])

    const handleTypeFilterChange = (e) => {
        setTypeFilter(e.target.value)
        setPageNumber(1) // vracamo na prvu stranicu kad se promeni filter
    }

    const handleStatusFilterChange = (e) => {
        setStatusFilter(e.target.value)
        setPageNumber(1)
    }

    const handleChange = (e) => {
        setForm({ ...form, [e.target.name]: e.target.value })
    }

    const handleSubmit = async (e) => {
        e.preventDefault()
        setError(null)
        setSuccess(false)

        if (!form.name.trim()) {
            setError('Naziv konkursa je obavezan.')
            return
        }
        if (!form.description.trim()) {
            setError('Opis je obavezan.')
            return
        }
        if (!form.endDate) {
            setError('Datum završetka je obavezan.')
            return
        }
        if (new Date(form.endDate) <= new Date(form.startDate)) {
            setError('Datum završetka mora biti posle datuma početka.')
            return
        }
        if (form.maxStudents !== '' && Number(form.maxStudents) <= 0) {
            setError('Maksimalan broj studenata mora biti veći od 0.')
            return
        }

        setLoading(true)

        const payload = {
            ...form,
            type: Number(form.type),
            maxStudents: form.maxStudents === '' ? null : Number(form.maxStudents),
        }

        try {
            await createCompetition(payload)
            setSuccess(true)
            setForm({
                name: '',
                description: '',
                startDate: new Date().toISOString().split('T')[0],
                endDate: '',
                type: 0,
                maxStudents: '',
            })
            setPageNumber(1) // novi konkurs je najnoviji, vracamo na prvu stranicu da se vidi
            fetchCompetitions()
        } catch (err) {
            setError(err.message)
        } finally {
            setLoading(false)
        }
    }

    const handleToggleCompetition = async (id) => {
        try {
            await toggleCompetition(id)
            fetchCompetitions()
        } catch (err) {
            setError(err.message)
        }
    }

    const formatDate = (dateStr) => {
        if (!dateStr) return '-'
        return new Date(dateStr).toLocaleDateString('sr-RS')
    }

    return (
        <div className={styles.container}>
            <h1 className={styles.title}>Novi konkurs</h1>

            <form onSubmit={handleSubmit} className={styles.form}>
                <div className={styles.field}>
                    <label htmlFor="name">Naziv konkursa</label>
                    <input
                        id="name"
                        name="name"
                        type="text"
                        value={form.name}
                        onChange={handleChange}
                        placeholder="Konkurs za smeštaj 2026"
                    />
                </div>

                <div className={styles.field}>
                    <label htmlFor="description">Opis</label>
                    <textarea
                        id="description"
                        name="description"
                        value={form.description}
                        onChange={handleChange}
                        placeholder="Kratak opis uslova konkursa"
                        rows={4}
                    />
                </div>

                <div className={styles.row}>
                    <div className={styles.field}>
                        <label htmlFor="type">Tip konkursa</label>
                        <select id="type" name="type" value={form.type} onChange={handleChange}>
                            <option value={0}>Stariji studenti</option>
                            <option value={1}>Brucoši</option>
                        </select>
                    </div>

                    <div className={styles.field}>
                        <label htmlFor="maxStudents">Maksimalan broj studenata</label>
                        <input
                            id="maxStudents"
                            name="maxStudents"
                            type="number"
                            min="1"
                            value={form.maxStudents}
                            onChange={handleChange}
                            placeholder="Neograničeno"
                        />
                    </div>
                </div>

                <div className={styles.row}>
                    <div className={styles.field}>
                        <label htmlFor="startDate">Datum početka</label>
                        <input
                            id="startDate"
                            name="startDate"
                            type="date"
                            value={form.startDate}
                            readOnly
                            className={styles.readonly}
                        />
                    </div>

                    <div className={styles.field}>
                        <label htmlFor="endDate">Datum završetka</label>
                        <input
                            id="endDate"
                            name="endDate"
                            type="date"
                            value={form.endDate}
                            onChange={handleChange}
                        />
                    </div>
                </div>

                {error && <p className={styles.error}>{error}</p>}
                {success && <p className={styles.success}>Konkurs je uspešno kreiran!</p>}

                <button type="submit" className={styles.btn} disabled={loading}>
                    {loading ? 'Kreiranje...' : 'Kreiraj konkurs'}
                </button>
            </form>

            <div className={styles.subtitleRow}>
                <h2 className={styles.subtitle}>Konkursi</h2>

                <div className={styles.filters}>
                    <div className={styles.filterField}>
                        <label htmlFor="typeFilter">Tip:</label>
                        <select id="typeFilter" value={typeFilter} onChange={handleTypeFilterChange}>
                            <option value="">Svi</option>
                            <option value="0">Stariji studenti</option>
                            <option value="1">Brucoši</option>
                        </select>
                    </div>

                    <div className={styles.filterField}>
                        <label htmlFor="statusFilter">Status:</label>
                        <select id="statusFilter" value={statusFilter} onChange={handleStatusFilterChange}>
                            <option value="">Svi</option>
                            <option value="0">Nacrt</option>
                            <option value="1">Otvoren</option>
                            <option value="2">Zatvoren</option>
                            <option value="3">Završen</option>
                        </select>
                    </div>
                </div>
            </div>

            {listLoading && <p className={styles.info}>Učitavanje...</p>}
            {listError && <p className={styles.error}>{listError}</p>}

            {!listLoading && !listError && competitions.length === 0 && (
                <p className={styles.info}>Nema konkursa za prikaz.</p>
            )}

            {!listLoading && !listError && competitions.length > 0 && (
                <>
                    <div className={styles.list}>
                        {competitions.map((comp) => (
                            <div key={comp.id} className={styles.card}>
                                <div className={styles.cardHeader}>
                                    <span className={styles.cardName}>{comp.name}</span>
                                    <div className={styles.badges}>
                                        <span className={styles.typeBadge}>{typeLabel[comp.type] ?? 'N/A'}</span>
                                        <span className={`${styles.badge} ${styles[comp.status?.toString().toLowerCase() ?? '']}`}>
                                            {statusLabel[comp.status] ?? 'N/A'}
                                        </span>
                                    </div>
                                </div>
                                <p className={styles.cardDesc}>{comp.description}</p>
                                <div className={styles.cardDates}>
                                    <span>Početak: {formatDate(comp.startDate)}</span>
                                    <span>Završetak: {formatDate(comp.endDate)}</span>
                                </div>
                                <div className={styles.cardMeta}>
                                    <span>Prijave: {comp.applicationCount}</span>
                                    <span>Max studenata: {comp.maxStudents ?? 'Neograničeno'}</span>
                                </div>
                                <div className={styles.cardActions}>
                                    {comp.status === 0 && (
                                        <button
                                            className={styles.openBtn}
                                            onClick={() => handleToggleCompetition(comp.id)}
                                        >
                                            Otvori konkurs
                                        </button>
                                    )}

                                    {comp.status === 1 && (
                                        <button
                                            className={styles.closeBtn}
                                            onClick={() => handleToggleCompetition(comp.id)}
                                        >
                                            Zatvori konkurs
                                        </button>
                                    )}

                                    <button
                                        className={styles.applicationsBtn}
                                        onClick={() => navigate(`/dorm-manager/competitions/${comp.id}/applications`)}
                                    >
                                        Zahtevi za konkurs
                                    </button>
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

export default DormManagerCompetition