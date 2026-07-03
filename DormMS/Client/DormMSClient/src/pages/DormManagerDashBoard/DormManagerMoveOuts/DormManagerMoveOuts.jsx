import { useState, useEffect } from 'react'
import { getPendingMoveOutRequestsForManager, approveMoveOutRequest, rejectMoveOutRequest } from '../../../api/moveOut'
import styles from './DormManagerMoveOuts.module.css'

export default function DormManagerMoveOuts() {
    const [requests, setRequests] = useState([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState('')
    const [actioningId, setActioningId] = useState(null)

    const loadRequests = async () => {
        setLoading(true)
        setError('')
        try {
            const data = await getPendingMoveOutRequestsForManager()
            setRequests(data)
        } catch (err) {
            setError(err.message)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        loadRequests()
    }, [])

    const handleApprove = async (id) => {
        setActioningId(id)
        try {
            await approveMoveOutRequest(id)
            setRequests((prev) => prev.filter((r) => r.id !== id))
        } catch (err) {
            setError(err.message)
        } finally {
            setActioningId(null)
        }
    }

    const handleReject = async (id) => {
        setActioningId(id)
        try {
            await rejectMoveOutRequest(id)
            setRequests((prev) => prev.filter((r) => r.id !== id))
        } catch (err) {
            setError(err.message)
        } finally {
            setActioningId(null)
        }
    }

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h2 className={styles.title}>Zahtevi za iseljenje</h2>
                <p className={styles.subtitle}>
                    {requests.length > 0
                        ? `Zahtevi koji čekaju odobrenje: ${requests.length}`
                        : 'Pregled zahteva za iseljenje koji čekaju vaše odobrenje'}
                </p>
            </div>

            {loading && <div className={styles.loading}>Učitavanje zahteva...</div>}
            {!loading && error && <div className={styles.error}>{error}</div>}
            {!loading && !error && requests.length === 0 && (
                <div className={styles.empty}>Nema zahteva koji čekaju odobrenje.</div>
            )}

            {!loading && !error && requests.length > 0 && (
                <div className={styles.list}>
                    {requests.map((req) => (
                        <MoveOutRequestCard
                            key={req.id}
                            request={req}
                            busy={actioningId === req.id}
                            onApprove={() => handleApprove(req.id)}
                            onReject={() => handleReject(req.id)}
                        />
                    ))}
                </div>
            )}
        </div>
    )
}

function MoveOutRequestCard({ request, busy, onApprove, onReject }) {
    return (
        <div className={styles.card}>
            <div className={styles.studentInfo}>
                <div className={styles.studentBlock}>
                    <p className={styles.studentName}>{request.studentName}</p>
                    <p className={styles.studentMeta}>Br. indeksa: {request.studentIdNumber}</p>
                </div>

                <div className={styles.roomBlock}>
                    <p className={styles.roomInfo}>Soba: {request.roomNumber}</p>
                    <p className={styles.dormInfo}>{request.dormName}</p>
                </div>
            </div>

            <div className={styles.footer}>
                <span className={styles.date}>
                    Datum slanja: {new Date(request.createdAt).toLocaleDateString('sr-RS')}
                </span>

                <div className={styles.actions}>
                    <button
                        className={styles.rejectButton}
                        onClick={onReject}
                        disabled={busy}
                    >
                        Odbij
                    </button>
                    <button
                        className={styles.approveButton}
                        onClick={onApprove}
                        disabled={busy}
                    >
                        Odobri
                    </button>
                </div>
            </div>
        </div>
    )
}
