import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import styles from './DormManagerRankings.module.css'
import { getCompetitions } from '../../../api/competition'

const CLOSED_STATUS = 2
const COMPLETED_STATUS = 3

const typeLabel = {
    0: 'Stariji studenti',
    1: 'Brucoši',
}

const statusLabel = {
    2: 'Zatvoren',
    3: 'Završen',
}

function DormManagerRankings() {
    const navigate = useNavigate()

    const [competitions, setCompetitions] = useState([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    useEffect(() => {
        async function fetchRankableCompetitions() {
            setLoading(true)
            setError(null)
            try {
                const data = await getCompetitions()
                const rankable = data.filter(
                    (comp) => comp.status === CLOSED_STATUS || comp.status === COMPLETED_STATUS
                )
                setCompetitions(rankable)
            } catch (err) {
                setError(err.message)
            } finally {
                setLoading(false)
            }
        }

        fetchRankableCompetitions()
    }, [])

    const formatDate = (dateStr) => {
        if (!dateStr) return '-'
        return new Date(dateStr).toLocaleDateString('sr-RS')
    }

    return (
        <div className={styles.container}>
            <h1 className={styles.title}>Rang-liste</h1>
            <p className={styles.subtitle}>
                Izaberite konkurs da pokrenete ili pregledate rangiranje studenata.
            </p>

            {loading && <p className={styles.info}>Učitavanje...</p>}
            {error && <p className={styles.error}>{error}</p>}

            {!loading && !error && competitions.length === 0 && (
                <p className={styles.info}>Trenutno nema konkursa za rangiranje.</p>
            )}

            {!loading && !error && competitions.length > 0 && (
                <div className={styles.list}>
                    {competitions.map((comp) => (
                        <button
                            key={comp.id}
                            type="button"
                            className={styles.card}
                            onClick={() => navigate(`/dorm-manager/rankings/${comp.id}`)}
                        >
                            <div className={styles.cardHeader}>
                                <span className={styles.cardName}>{comp.name}</span>
                                <div className={styles.badges}>
                                    <span className={styles.typeBadge}>{typeLabel[comp.type] ?? 'N/A'}</span>
                                    <span
                                        className={
                                            comp.status === COMPLETED_STATUS
                                                ? styles.statusBadgeCompleted
                                                : styles.statusBadgeClosed
                                        }
                                    >
                                        {statusLabel[comp.status] ?? 'N/A'}
                                    </span>
                                </div>
                            </div>
                            <p className={styles.cardDesc}>{comp.description}</p>
                            <div className={styles.cardMeta}>
                                <span>Završetak: {formatDate(comp.endDate)}</span>
                                <span>Prijave: {comp.applicationCount}</span>
                                <span>Max studenata: {comp.maxStudents}</span>
                            </div>
                            <span className={styles.cardCta}>
                                {comp.status === COMPLETED_STATUS ? 'Pregledaj rang-listu →' : 'Otvori rangiranje →'}
                            </span>
                        </button>
                    ))}
                </div>
            )}
        </div>
    )
}

export default DormManagerRankings