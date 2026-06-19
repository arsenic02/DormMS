import { useState, useEffect } from 'react'
import styles from './StudentCompetitions.module.css'
import { getOpenCompetitions } from '../../../api/competition'
import { applyToCompetition, uploadDocument } from '../../../api/applyCompetition'

const statusLabel = {
    0: 'Nacrt',
    1: 'Otvoren',
    2: 'Zatvoren',
    3: 'Završen',
}
const typeLabel = {
    0: 'Stariji studenti',
    1: 'Brucoši',
}

function StudentCompetitions() {
    const [competitions, setCompetitions] = useState([])
    const [listLoading, setListLoading] = useState(true)
    const [listError, setListError] = useState(null)

    const [selectedFiles, setSelectedFiles] = useState({})
    const [fileErrors, setFileErrors] = useState({})

    const [applyingId, setApplyingId] = useState(null)
    const [applyError, setApplyError] = useState(null)
    const [applyErrorId, setApplyErrorId] = useState(null)
    const [applySuccessId, setApplySuccessId] = useState(null)

    const [confirmModalId, setConfirmModalId] = useState(null)

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

    const handleFileChange = (competitionId, e) => {
        const file = e.target.files?.[0] || null

        if (file && file.type !== 'application/pdf') {
            setFileErrors((prev) => ({ ...prev, [competitionId]: 'Dozvoljen je samo PDF format' }))
            return
        }

        const maxSize = 5 * 1024 * 1024
        if (file && file.size > maxSize) {
            setFileErrors((prev) => ({ ...prev, [competitionId]: 'Fajl prevazilazi maksimalnu veličinu od 5MB' }))
            return
        }

        setFileErrors((prev) => ({ ...prev, [competitionId]: '' }))
        setSelectedFiles((prev) => ({ ...prev, [competitionId]: file }))
    }

    const performApply = async (competitionId) => {
        setApplyError(null)
        setApplyErrorId(null)
        setApplySuccessId(null)
        setApplyingId(competitionId)

        try {
            let documentUrl = null
            const file = selectedFiles[competitionId]

            if (file) {
                const uploadResult = await uploadDocument(file)
                documentUrl = uploadResult.documentUrl
            }

            await applyToCompetition(competitionId, '', documentUrl)

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

    const handleApply = (competitionId) => {
        const file = selectedFiles[competitionId]

        if (!file) {
            setConfirmModalId(competitionId)
            return
        }

        performApply(competitionId)
    }

    const handleConfirmWithoutDocument = () => {
        const competitionId = confirmModalId
        setConfirmModalId(null)
        performApply(competitionId)
    }

    const handleCancelConfirm = () => {
        setConfirmModalId(null)
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
                                    <div className={styles.badges}>
                                        <span className={styles.typeBadge}>{typeLabel[comp.type] ?? 'N/A'}</span>
                                        <span className={styles.badge}>{statusLabel[comp.status] ?? 'N/A'}</span>
                                    </div>
                                </div>

                                <p className={styles.cardDesc}>{comp.description}</p>

                                <div className={styles.cardDates}>
                                    <span>Početak: {formatDate(comp.startDate)}</span>
                                    <span>Završetak: {formatDate(comp.endDate)}</span>
                                </div>

                                {!comp.hasApplied && (

                                    <div className={styles.fileField}>
                                        <label htmlFor={`file-${comp.id}`} className={styles.fileLabel}>
                                            Dokument (PDF)
                                        </label>
                                        <div className={styles.fileRow}>
                                            <label htmlFor={`file-${comp.id}`} className={styles.fileButton}>
                                                Izaberi fajl
                                            </label>
                                            <span className={styles.fileName}>
                                                {selectedFiles[comp.id]?.name || 'Nije izabran fajl'}
                                            </span>
                                            <input
                                                id={`file-${comp.id}`}
                                                type="file"
                                                accept=".pdf"
                                                onChange={(e) => handleFileChange(comp.id, e)}
                                                className={styles.fileInputHidden}
                                            />
                                        </div>
                                        {fileErrors[comp.id] && (
                                            <span className={styles.error}>{fileErrors[comp.id]}</span>
                                        )}
                                    </div>

                                    // <div className={styles.fileField}>
                                    //     <label htmlFor={`file-${comp.id}`} className={styles.fileLabel}>
                                    //         Dokument (PDF)
                                    //     </label>
                                    //     <input
                                    //         id={`file-${comp.id}`}
                                    //         type="file"
                                    //         accept=".pdf"
                                    //         onChange={(e) => handleFileChange(comp.id, e)}
                                    //         className={styles.fileInput}
                                    //     />
                                    //     {fileErrors[comp.id] && (
                                    //         <span className={styles.error}>{fileErrors[comp.id]}</span>
                                    //     )}
                                    // </div>
                                )}

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

            {confirmModalId !== null && (
                <div className={styles.modalOverlay}>
                    <div className={styles.modal}>
                        <p className={styles.modalText}>
                            Niste priložili dokument. Da li ste sigurni da želite da se prijavite bez njega?
                        </p>
                        <div className={styles.modalActions}>
                            <button className={styles.modalCancelBtn} onClick={handleCancelConfirm}>
                                Otkaži
                            </button>
                            <button className={styles.modalConfirmBtn} onClick={handleConfirmWithoutDocument}>
                                Prijavi se bez dokumenta
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    )
}

export default StudentCompetitions

