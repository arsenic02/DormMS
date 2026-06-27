import { useState, useEffect } from 'react'
import styles from './RoomSwapRequest.module.css'
import {
    createSwapRequest,
    respondToSwapRequest,
    getIncomingSwapRequests,
    getOutgoingSwapRequests
} from '../../../../api/roomSwap'

const STATUS_LABELS = {
    0: { text: 'Na čekanju', className: 'statusPending' },
    1: { text: 'Prihvaćeno', className: 'statusAccepted' },
    2: { text: 'Odbijeno', className: 'statusRejected' }
}

function StatusBadge({ status }) {
    const info = STATUS_LABELS[status] ?? { text: 'Nepoznato', className: 'statusPending' }
    return <span className={`${styles.statusBadge} ${styles[info.className]}`}>{info.text}</span>
}

function RoomSwapRequest() {
    const [incoming, setIncoming] = useState([])
    const [outgoing, setOutgoing] = useState([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    // const [receiverId, setReceiverId] = useState('')
    const [receiverIdNumber, setReceiverIdNumber] = useState('')
    const [sending, setSending] = useState(false)
    const [formError, setFormError] = useState(null)
    const [formSuccess, setFormSuccess] = useState(null)

    const [respondingId, setRespondingId] = useState(null)

    const loadRequests = async () => {
        setLoading(true)
        setError(null)
        try {
            const [incomingData, outgoingData] = await Promise.all([
                getIncomingSwapRequests(),
                getOutgoingSwapRequests()
            ])
            setIncoming(incomingData)
            setOutgoing(outgoingData)
        } catch (err) {
            setError(err.message)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        loadRequests()
    }, [])

    const handleSendRequest = async (e) => {
        e.preventDefault()
        setFormError(null)
        setFormSuccess(null)

        const trimmedValue = receiverIdNumber.trim()
        if (!trimmedValue) {
            setFormError('Unesite broj indeksa studenta.')
            return
        }
       
        setSending(true)
        try {
            await createSwapRequest(trimmedValue)
            setFormSuccess('Zahtev je uspešno poslat.')
            setReceiverIdNumber('')
            await loadRequests()
        } catch (err) {
            setFormError(err.message)
        } finally {
            setSending(false)
        }
    }

    const handleRespond = async (requestId, accept) => {
        setRespondingId(requestId)
        try {
            await respondToSwapRequest(requestId, accept)
            await loadRequests()
        } catch (err) {
            setError(err.message)
        } finally {
            setRespondingId(null)
        }
    }

    return (
        <div className={styles.container}>
            <h1 className={styles.title}>Zamena sobe</h1>

            <div className={styles.formCard}>
                <h2 className={styles.subtitle}>Pošalji zahtev za zamenu</h2>
                <form className={styles.form} onSubmit={handleSendRequest}>
                    <input
                        type="text"
                        placeholder="Indeks studenta sa kojim želite da zamenite sobu"
                        value={receiverIdNumber}
                        onChange={(e) => setReceiverIdNumber(e.target.value)}
                        className={styles.input}
                        disabled={sending}
                    />
                    <button type="submit" className={styles.sendButton} disabled={sending}>
                        {sending ? 'Slanje...' : 'Pošalji zahtev'}
                    </button>
                </form>
                {formError && <p className={styles.error}>{formError}</p>}
                {formSuccess && <p className={styles.success}>{formSuccess}</p>}
            </div>

            {loading && <p className={styles.info}>Učitavanje...</p>}
            {!loading && error && <p className={styles.error}>{error}</p>}

            {!loading && !error && (
                <>
                    <h2 className={styles.subtitle}>Primljeni zahtevi</h2>
                    {incoming.length === 0 ? (
                        <p className={styles.info}>Nemate primljenih zahteva za zamenu sobe.</p>
                    ) : (
                        <div className={styles.requestsList}>
                            {incoming.map((req) => (
                                <div key={req.id} className={styles.requestCard}>
                                    <div className={styles.requestInfo}>
                                        <span className={styles.requestName}>{req.senderName}</span>
                                        <span className={styles.requestMeta}>
                                            Nudi sobu {req.senderRoomNumber} za vašu sobu {req.receiverRoomNumber}
                                        </span>
                                        <StatusBadge status={req.status} />
                                    </div>
                                    {req.status === 0 && (
                                        <div className={styles.actions}>
                                            <button
                                                className={styles.acceptButton}
                                                disabled={respondingId === req.id}
                                                onClick={() => handleRespond(req.id, true)}
                                            >
                                                Prihvati
                                            </button>
                                            <button
                                                className={styles.rejectButton}
                                                disabled={respondingId === req.id}
                                                onClick={() => handleRespond(req.id, false)}
                                            >
                                                Odbij
                                            </button>
                                        </div>
                                    )}
                                </div>
                            ))}
                        </div>
                    )}

                    <h2 className={styles.subtitle}>Poslati zahtevi</h2>
                    {outgoing.length === 0 ? (
                        <p className={styles.info}>Niste poslali nijedan zahtev za zamenu sobe.</p>
                    ) : (
                        <div className={styles.requestsList}>
                            {outgoing.map((req) => (
                                <div key={req.id} className={styles.requestCard}>
                                    <div className={styles.requestInfo}>
                                        <span className={styles.requestName}>{req.receiverName}</span>
                                        <span className={styles.requestMeta}>
                                            Vaša soba {req.senderRoomNumber} za sobu {req.receiverRoomNumber}
                                        </span>
                                        <StatusBadge status={req.status} />
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </>
            )}
        </div>
    )
}

export default RoomSwapRequest