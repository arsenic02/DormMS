import { useState, useEffect } from 'react'
import styles from './StudentCompetitions.module.css'
import { getOpenCompetitions } from '../../../api/competition'
import { applyToCompetition } from '../../../api/applyCompetition'

const statusLabel = {
    0: 'Nacrt',
    1: 'Otvoren',
    2: 'Zatvoren',
    3: 'Završen',
}

function StudentCompetitions() {
    const [competitions, setCompetitions] = useState([])
    const [listLoading, setListLoading] = useState(true)
    const [listError, setListError] = useState(null)

    const [applyingId, setApplyingId] = useState(null)
    const [applyError, setApplyError] = useState(null)
    const [applyErrorId, setApplyErrorId] = useState(null)
    const [applySuccessId, setApplySuccessId] = useState(null)

    const fetchCompetitions = async () => {
        setListLoading(true)
        setListError(null)
        try {
            const data = await getOpenCompetitions()
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

    const handleApply = async (competitionId) => {
        setApplyError(null)
        setApplyErrorId(null)
        setApplySuccessId(null)
        setApplyingId(competitionId)

        try {
            await applyToCompetition(competitionId)
            setCompetitions((prev) =>
                prev.map((c) =>
                    c.id === competitionId ? { ...c, hasApplied: true } : c
                )
            )
            setApplySuccessId(competitionId)
        } catch (err) {
            setApplyError(err.message)
            setApplyErrorId(competitionId)
        } finally {
            setApplyingId(null)
        }
    }

    const formatDate = (dateStr) => {
        if (!dateStr) return '-'
        return new Date(dateStr).toLocaleDateString('sr-RS')
    }

    return (
        <div className={styles.container}>
            <h1 className={styles.title}>Otvoreni konkursi</h1>

            {listLoading && <p className={styles.info}>Učitavanje...</p>}
            {listError && <p className={styles.error}>{listError}</p>}

            {!listLoading && !listError && competitions.length === 0 && (
                <p className={styles.info}>Trenutno nema otvorenih konkursa.</p>
            )}

            {!listLoading && !listError && competitions.length > 0 && (
                <div className={styles.list}>
                    {competitions.map((comp) => {
                        const isApplying = applyingId === comp.id

                        return (
                            <div key={comp.id} className={styles.card}>
                                <div className={styles.cardHeader}>
                                    <span className={styles.cardName}>{comp.name}</span>
                                    <span className={styles.badge}>
                                        {statusLabel[comp.status] ?? 'N/A'}
                                    </span>
                                </div>

                                <p className={styles.cardDesc}>{comp.description}</p>

                                <div className={styles.cardDates}>
                                    <span>Početak: {formatDate(comp.startDate)}</span>
                                    <span>Završetak: {formatDate(comp.endDate)}</span>
                                </div>

                                {applySuccessId === comp.id && (
                                    <p className={styles.success}>Uspešno si se prijavio na konkurs!</p>
                                )}
                                {applyErrorId === comp.id && (
                                    <p className={styles.error}>{applyError}</p>
                                )}

                                <button
                                    className={styles.btn}
                                    disabled={isApplying || comp.hasApplied}
                                    onClick={() => handleApply(comp.id)}
                                >
                                    {comp.hasApplied
                                        ? 'Prijavljen'
                                        : isApplying
                                            ? 'Prijavljivanje...'
                                            : 'Prijavi se'}
                                </button>
                            </div>
                        )
                    })}
                </div>
            )}
        </div>
    )
}

export default StudentCompetitions