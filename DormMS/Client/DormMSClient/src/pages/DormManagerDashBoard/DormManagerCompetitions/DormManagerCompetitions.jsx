import { useState, useEffect } from 'react'
import styles from './DormManagerCompetition.module.css'
import { getCompetitions, createCompetition } from '../../../api/competition'

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

const API_URL = 'http://localhost:5153/api/Competition'

function DormManagerCompetition() {
    const [form, setForm] = useState({
        name: '',
        description: '',
        startDate: new Date().toISOString().split('T')[0],
        endDate: '',
    })
    const [loading, setLoading] = useState(false)
    const [error, setError] = useState(null)
    const [success, setSuccess] = useState(false)

    const [competitions, setCompetitions] = useState([])
    const [listLoading, setListLoading] = useState(true)
    const [listError, setListError] = useState(null)

    const fetchCompetitions = async () => {
        setListLoading(true)
        setListError(null)
        try {
            const data = await getCompetitions()
            setCompetitions(data)
        } catch (err) {
            setListError(err.message)
        } finally {
            setListLoading(false)
        }
    }

    useEffect(() => {
        fetchCompetitions()
    }, [])

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

        setLoading(true)

        try {
            await createCompetition(form)
            setSuccess(true)
            setForm({
                name: '',
                description: '',
                startDate: new Date().toISOString().split('T')[0],
                endDate: '',
            })
            fetchCompetitions()
        } catch (err) {
            setError(err.message)
        } finally {
            setLoading(false)
        }
        // try {
        //     const token = localStorage.getItem('token')
        //     const res = await fetch(API_URL, {
        //         method: 'POST',
        //         headers: {
        //             'Content-Type': 'application/json',
        //             Authorization: `Bearer ${token}`,
        //         },
        //         body: JSON.stringify(form),
        //     })

        //     if (!res.ok) {
        //         const data = await res.json().catch(() => null)
        //         //console.log(data)
        //         throw new Error(data?.message || 'Greška prilikom kreiranja konkursa.')
        //     }
        //     const data = await res.json().catch(() => null)
        //     console.log(data)
        //     setSuccess(true)
        //     setForm({ name: '', description: '', startDate: '', endDate: '' })


        // } catch (err) {
        //     setError(err.message)
        // } finally {
        //     setLoading(false)
        // }
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
                    //required jer baca na engleskom greske
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
                </div>
                <div className={styles.row}>
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

            <h2 className={styles.subtitle}>Konkursi</h2>

            {listLoading && <p className={styles.info}>Učitavanje...</p>}
            {listError && <p className={styles.error}>{listError}</p>}

            {!listLoading && !listError && competitions.length === 0 && (
                <p className={styles.info}>Nema kreiranih konkursa.</p>
            )}

            {!listLoading && !listError && competitions.length > 0 && (
                <div className={styles.list}>
                    {competitions.map((comp) => (
                        <div key={comp.id} className={styles.card}>
                            <div className={styles.cardHeader}>
                                <span className={styles.cardName}>{comp.name}</span>
                                    <span className={`${styles.badge} ${styles[comp.status?.toString().toLowerCase() ?? '']}`}>
                                        {statusLabel[comp.status] ?? 'N/A'}
                                </span>
                            </div>
                            <p className={styles.cardDesc}>{comp.description}</p>
                            <div className={styles.cardDates}>
                                <span>Početak: {formatDate(comp.startDate)}</span>
                                <span>Završetak: {formatDate(comp.endDate)}</span>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    )
}

export default DormManagerCompetition
