import { useState, useEffect } from 'react'
import { getPendingSwapRequestsForManager, approveSwapRequest, rejectSwapRequest } from '../../../api/roomSwap'
import styles from './DormManagerRoomSwaps.module.css'

export default function DormManagerRoomSwaps() {
    const [requests, setRequests] = useState([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState('')
    const [actioningId, setActioningId] = useState(null)

    const loadRequests = async () => {
        setLoading(true)
        setError('')
        try {
            const data = await getPendingSwapRequestsForManager()
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
            await approveSwapRequest(id)
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
            await rejectSwapRequest(id)
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
                <h2 className={styles.title}>Zahtevi za zamenu soba</h2>
                <p className={styles.subtitle}>
                    {requests.length > 0
                        ? `Zahtevi koji čekaju odobrenje: ${requests.length}`
                        : 'Pregled zahteva za zamenu soba koji čekaju vaše odobrenje'}
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
                        <SwapRequestCard
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

function SwapRequestCard({ request, busy, onApprove, onReject }) {
    return (
        <div className={styles.card}>
            <div className={styles.swapRow}>
                <div className={styles.studentBlock}>
                    <p className={styles.studentName}>{request.senderName}</p>
                    <p className={styles.roomInfo}>Trenutna soba: {request.senderRoomNumber}</p>
                </div>

                <div className={styles.arrow}>⇄</div>

                <div className={styles.studentBlock}>
                    <p className={styles.studentName}>{request.receiverName}</p>
                    <p className={styles.roomInfo}>Trenutna soba: {request.receiverRoomNumber}</p>
                </div>
            </div>

            <div className={styles.footer}>
                <span className={styles.date}>
                    Prihvaćeno od strane studenata: {new Date(request.respondedAt).toLocaleDateString('sr-RS')}
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