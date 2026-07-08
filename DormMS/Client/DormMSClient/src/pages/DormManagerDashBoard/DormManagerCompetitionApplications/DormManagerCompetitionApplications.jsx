import { useState, useEffect } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import styles from './DormManagerCompetitionApplications.module.css'
import { getApplicationsByCompetition, verifyApplication, getApplicationsPaged } from '../../../api/applyCompetition'
import { getCompetitions } from '../../../api/competition'

const statusLabel = {
    0: 'Na čekanju',
    1: 'Odobreno',
    2: 'Odbijeno',
    3: 'Na listi čekanja',
}

const statusClass = {
    0: 'pending',
    1: 'approved',
    2: 'rejected',
    3: 'waitlisted',
}

const competitionStatusLabel = {
    0: 'Nacrt',
    1: 'Otvoren',
    2: 'Zatvoren',
    3: 'Završen',
}

const typeLabel = {
    0: 'Stariji studenti',
    1: 'Brucoši',
}

const PAGE_SIZE = 10
const BACKEND_BASE_URL = 'http://localhost:5153'

const buildDocumentUrl = (documentUrl) => {
    if (!documentUrl) return null
    // ako je vec pun URL (npr. pocinje sa http), ne diraj ga
    if (documentUrl.startsWith('http://') || documentUrl.startsWith('https://')) {
        return documentUrl
    }
    return `${BACKEND_BASE_URL}${documentUrl}`
}

function DormManagerCompetitionApplications() {
    const { competitionId } = useParams()
    const navigate = useNavigate()

    const [competition, setCompetition] = useState(null)
    const [applications, setApplications] = useState([])
    const [pageNumber, setPageNumber] = useState(1)
    const [totalPages, setTotalPages] = useState(1)
    const [totalCount, setTotalCount] = useState(0)
    const [statusFilter, setStatusFilter] = useState('') // '' = svi statusi

    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    const [processingId, setProcessingId] = useState(null)
    const [actionError, setActionError] = useState(null)

    // Detalji konkursa se ucitavaju samo jednom (ne zavise od paginacije)
    useEffect(() => {
        let active = true

        const loadCompetition = async () => {
            try {
                const allCompetitions = await getCompetitions()
                if (!active) return
                const comp = allCompetitions.find((c) => String(c.id) === String(competitionId))
                setCompetition(comp ?? null)
            } catch (err) {
                if (active) setError(err.message)
            }
        }

        loadCompetition()
        return () => { active = false }
    }, [competitionId])

    useEffect(() => {
        let active = true

        const loadApplications = async () => {
            setLoading(true)
            setError(null)
            try {
                const data = await getApplicationsPaged(competitionId, pageNumber, PAGE_SIZE, statusFilter)
                if (!active) return
                setApplications(data.items)
                setTotalPages(data.totalPages)
                setTotalCount(data.totalCount)
            } catch (err) {
                if (active) setError(err.message)
            } finally {
                if (active) setLoading(false)
            }
        }

        loadApplications()
        return () => { active = false }
    }, [competitionId, pageNumber, statusFilter])

    const handleStatusFilterChange = (e) => {
        setStatusFilter(e.target.value)
        setPageNumber(1) 
    }

    const handleVerify = async (applicationId, status) => {
        setActionError(null)
        setProcessingId(applicationId)
        try {
            await verifyApplication(applicationId, status)
            // azuriramo samo taj red lokalno, bez ponovnog ucitavanja cele liste
            setApplications((prev) =>
                prev.map((a) => (a.id === applicationId ? { ...a, status } : a))
            )
        } catch (err) {
            setActionError(err.message)
        } finally {
            setProcessingId(null)
        }
    }

    const formatDate = (dateStr) => {
        if (!dateStr) return '-'
        return new Date(dateStr).toLocaleDateString('sr-RS')
    }

    return (
        <div className={styles.container}>
            <button className={styles.backBtn} onClick={() => navigate('/dorm-manager/competitions')}>
                ← Nazad na konkurse
            </button>

            {competition && (
                <div className={styles.competitionHeader}>
                    <div className={styles.competitionTitleRow}>
                        <h1 className={styles.title}>{competition.name}</h1>
                        <div className={styles.badges}>
                            <span className={styles.typeBadge}>{typeLabel[competition.type] ?? 'N/A'}</span>
                            <span className={styles.badge}>{competitionStatusLabel[competition.status] ?? 'N/A'}</span>
                        </div>
                    </div>
                    <p className={styles.competitionDesc}>{competition.description}</p>
                    <div className={styles.competitionMeta}>
                        <span>Početak: {formatDate(competition.startDate)}</span>
                        <span>Završetak: {formatDate(competition.endDate)}</span>
                        <span>Max studenata: {competition.maxStudents}</span>
                        <span>Ukupno prijava: {totalCount}</span>
                    </div>
                </div>
            )}

            <div className={styles.subtitleRow}>
                <h2 className={styles.subtitle}>Zahtevi za konkurs</h2>

                <div className={styles.filterField}>
                    <label htmlFor="statusFilter">Status:</label>
                    <select id="statusFilter" value={statusFilter} onChange={handleStatusFilterChange}>
                        <option value="">Svi</option>
                        <option value="0">Na čekanju</option>
                        <option value="1">Odobreno</option>
                        <option value="2">Odbijeno</option>
                    </select>
                </div>
            </div>

            {actionError && <p className={styles.error}>{actionError}</p>}

            {loading && <p className={styles.info}>Učitavanje...</p>}
            {!loading && error && <p className={styles.error}>{error}</p>}

            {!loading && !error && applications.length === 0 && (
                <p className={styles.info}>Nema prijava za prikaz.</p>
            )}

            {!loading && !error && applications.length > 0 && (
                <>
                    <div className={styles.list}>
                        {applications.map((app) => (
                            <div key={app.id} className={styles.card}>
                                <div className={styles.cardHeader}>
                                    <span className={styles.cardName}>
                                        {app.studentName} {app.studentSurname}
                                    </span>
                                    <span className={`${styles.statusBadge} ${styles[statusClass[app.status]] ?? ''}`}>
                                        <span className={styles.statusDot} />
                                        {statusLabel[app.status] ?? 'N/A'}
                                    </span>
                                </div>

                                <div className={styles.cardMeta}>
                                    <span>Br. indeksa: {app.studentIDNumber}</span>
                                    <span>Email: {app.studentEmail}</span>
                                    <span>Prijavljen: {formatDate(app.appliedAt)}</span>
                                </div>

                                {app.notes && <p className={styles.cardNotes}>Napomena: {app.notes}</p>}

                                <div className={styles.cardActions}>
                                    {app.documentUrl ? (
                                        <a
                                            href={buildDocumentUrl(app.documentUrl)}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className={styles.docLink}
                                        >
                                            Pogledaj dokument
                                        </a>
                                    ) : (
                                        <span className={styles.noDoc}>Nema priloženog dokumenta</span>
                                    )}

                                    <div className={styles.decisionButtons}>
                                        {app.status === 0 ? (
                                            <>
                                                <button
                                                    className={styles.approveBtn}
                                                    disabled={processingId === app.id}
                                                    onClick={() => handleVerify(app.id, 1)}
                                                >
                                                    {processingId === app.id ? '...' : 'Odobri'}
                                                </button>
                                                <button
                                                    className={styles.rejectBtn}
                                                    disabled={processingId === app.id}
                                                    onClick={() => handleVerify(app.id, 2)}
                                                >
                                                    {processingId === app.id ? '...' : 'Odbij'}
                                                </button>
                                            </>
                                        ) : (
                                            <button
                                                className={styles.changeDecisionBtn}
                                                disabled={processingId === app.id}
                                                onClick={() => handleVerify(app.id, 0)}
                                            >
                                                {processingId === app.id ? '...' : 'Promeni odluku'}
                                            </button>
                                        )}
                                    </div>
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
export default DormManagerCompetitionApplications