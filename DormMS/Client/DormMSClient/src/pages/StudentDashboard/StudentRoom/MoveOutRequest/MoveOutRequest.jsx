import { useState, useEffect } from 'react'
import styles from './MoveOutRequest.module.css'
import { createMoveOutRequest, getMyMoveOutRequest } from '../../../../api/moveOut'

const STATUS_LABELS = {
    0: { text: 'Na čekanju', className: 'statusPending' },
    1: { text: 'Odobreno', className: 'statusApproved' },
    2: { text: 'Odbijeno', className: 'statusRejected' }
}

function StatusBadge({ status }) {
    const info = STATUS_LABELS[status] ?? { text: 'Nepoznato', className: 'statusPending' }

    return (
        <span className={`${styles.statusBadge} ${styles[info.className]}`}>
            {info.text}
        </span>
    )
}

function MoveOutRequest() {
    const [request, setRequest] = useState(null)
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    const [sending, setSending] = useState(false)
    const [formError, setFormError] = useState(null)
    const [formSuccess, setFormSuccess] = useState(null)

    const loadRequest = async () => {
        setLoading(true)
        setError(null)

        try {
            const data = await getMyMoveOutRequest()
            setRequest(data) // uvek setuj (pending/approved/rejected)
        } catch (err) {
            // ako backend vrati 404 = nema zahteva
            if (err.message.includes('Nemate aktivan zahtev')) {
                setRequest(null)
            } else {
                setError(err.message)
            }
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        loadRequest()
    }, [])

    const handleSendRequest = async () => {
        setFormError(null)
        setFormSuccess(null)
        setSending(true)

        try {
            await createMoveOutRequest()

            setFormSuccess('Zahtev za iseljenje je uspešno poslat.')

            const data = await getMyMoveOutRequest()
            setRequest(data)

        } catch (err) {
            setFormError(err.message || 'Došlo je do greške prilikom slanja zahteva.')
        } finally {
            setSending(false)
        }
    }

    const canCreateNewRequest =
        !request || request.status === 2 // dozvoli samo ako nema ili je rejected

    return (
        <div className={styles.container}>
            <h1 className={styles.title}>Iseljenje iz sobe</h1>

            {loading && <p className={styles.info}>Učitavanje...</p>}
            {!loading && error && <p className={styles.error}>{error}</p>}

            {/* =======================
                FORM (NEMA REQUEST ili REJECTED)
            ======================= */}
            {!loading && !error && canCreateNewRequest && (
                <div className={styles.formCard}>
                    <h2 className={styles.subtitle}>Pošalji zahtev za iseljenje</h2>

                    <p className={styles.description}>
                        Ako želite da se iselite iz svoje sobe, pošaljite zahtev referentu.
                        Nakon odobrenja, bićete ispisani iz sobe.
                    </p>

                    <button
                        className={styles.sendButton}
                        onClick={handleSendRequest}
                        disabled={sending}
                    >
                        {sending ? 'Slanje...' : 'Pošalji zahtev'}
                    </button>

                    {formError && <p className={styles.error}>{formError}</p>}
                    {formSuccess && <p className={styles.success}>{formSuccess}</p>}
                </div>
            )}

            {/* =======================
                STATUS VIEW (POSTOJI REQUEST)
            ======================= */}
            {!loading && !error && request && (
                <div className={styles.requestCard}>
                    <h2 className={styles.subtitle}>Status zahteva</h2>

                    <div className={styles.requestInfo}>
                        <span className={styles.requestLabel}>Soba:</span>
                        <span className={styles.requestValue}>{request.roomNumber}</span>
                    </div>

                    <div className={styles.requestInfo}>
                        <span className={styles.requestLabel}>Dom:</span>
                        <span className={styles.requestValue}>{request.dormName}</span>
                    </div>

                    <div className={styles.requestInfo}>
                        <span className={styles.requestLabel}>Datum slanja:</span>
                        <span className={styles.requestValue}>
                            {new Date(request.createdAt).toLocaleDateString('sr-RS')}
                        </span>
                    </div>

                    <div className={styles.requestInfo}>
                        <span className={styles.requestLabel}>Status:</span>
                        <StatusBadge status={request.status} />
                    </div>

                    {request.respondedAt && (
                        <div className={styles.requestInfo}>
                            <span className={styles.requestLabel}>Datum odgovora:</span>
                            <span className={styles.requestValue}>
                                {new Date(request.respondedAt).toLocaleDateString('sr-RS')}
                            </span>
                        </div>
                    )}

                    {/* =======================
                        STATUS PORUKE
                    ======================= */}
                    {request.status === 1 && (
                        <div className={styles.success}>
                            Vaš zahtev je odobren. Bićete ispisani iz sobe.
                        </div>
                    )}

                    {request.status === 2 && (
                        <div className={styles.infoBox}>
                            <p>Vaš zahtev je odbijen.</p>

                            <button
                                className={styles.sendButton}
                                onClick={handleSendRequest}
                                disabled={sending}
                            >
                                {sending ? 'Slanje...' : 'Pošalji novi zahtev'}
                            </button>
                        </div>
                    )}

                    {request.status === 0 && (
                        <div className={styles.info}>
                            Vaš zahtev je na čekanju kod referenta.
                        </div>
                    )}
                </div>
            )}
        </div>
    )
}

export default MoveOutRequest

// import { useState, useEffect } from 'react'
// import styles from './MoveOutRequest.module.css'
// import { createMoveOutRequest, getMyMoveOutRequest } from '../../../../api/moveOut'

// const STATUS_LABELS = {
//     0: { text: 'Na čekanju', className: 'statusPending' },
//     1: { text: 'Odobreno', className: 'statusApproved' },
//     2: { text: 'Odbijeno', className: 'statusRejected' }
// }

// function StatusBadge({ status }) {
//     const info = STATUS_LABELS[status] ?? { text: 'Nepoznato', className: 'statusPending' }
//     return <span className={`${styles.statusBadge} ${styles[info.className]}`}>{info.text}</span>
// }

// function MoveOutRequest() {
//     const [request, setRequest] = useState(null)
//     const [loading, setLoading] = useState(true)
//     const [error, setError] = useState(null)
//     const [sending, setSending] = useState(false)
//     const [formError, setFormError] = useState(null)
//     const [formSuccess, setFormSuccess] = useState(null)

//     const loadRequest = async () => {
//         setLoading(true)
//         setError(null)

//         try {
//             const data = await getMyMoveOutRequest()

//             // VAŽNO: uvek setuj data (čak i ako je rejected/approved)
//             setRequest(data)

//         } catch (err) {
//             setError(err.message)
//         } finally {
//             setLoading(false)
//         }
//     }

//     useEffect(() => {
//         loadRequest()
//     }, [])

//     const handleSendRequest = async () => {
//         setFormError(null)
//         setFormSuccess(null)
//         setSending(true)
//         // setRequest(null)

//         try {
//             await createMoveOutRequest()
//             setFormSuccess('Zahtev za iseljenje je uspešno poslat.')

//             const data = await getMyMoveOutRequest()
//             setRequest(data)

//         } catch (err) {
//             setFormError(err.message || 'Došlo je do greške prilikom slanja zahteva. Pokušajte ponovo.')
//         } finally {
//             setSending(false)
//         }
//     }

//     return (
//         <div className={styles.container}>
//             <h1 className={styles.title}>Iseljenje iz sobe</h1>

//             {loading && <p className={styles.info}>Učitavanje...</p>}
//             {!loading && error && <p className={styles.error}>{error}</p>}

//             {!loading && !error && !request && (
//                 <div className={styles.formCard}>
//                     <h2 className={styles.subtitle}>Pošalji zahtev za iseljenje</h2>
//                     <p className={styles.description}>
//                         Ako želite da se iselite iz svoje sobe, pošaljite zahtev referentu.
//                         Nakon odobrenja, vaša soba će biti oslobođena.
//                     </p>
//                     <button
//                         className={styles.sendButton}
//                         onClick={handleSendRequest}
//                         disabled={sending}
//                     >
//                         {sending ? 'Slanje...' : 'Pošalji zahtev za iseljenje'}
//                     </button>
//                     {formError && <p className={styles.error}>{formError}</p>}
//                     {formSuccess && <p className={styles.success}>{formSuccess}</p>}
//                 </div>
//             )}

//             {!loading && !error && request && (
//                 <div className={styles.requestCard}>
//                     <h2 className={styles.subtitle}>Status vašeg zahteva</h2>
//                     <div className={styles.requestInfo}>
//                         <span className={styles.requestLabel}>Soba:</span>
//                         <span className={styles.requestValue}>{request.roomNumber}</span>
//                     </div>
//                     <div className={styles.requestInfo}>
//                         <span className={styles.requestLabel}>Dom:</span>
//                         <span className={styles.requestValue}>{request.dormName}</span>
//                     </div>
//                     <div className={styles.requestInfo}>
//                         <span className={styles.requestLabel}>Datum slanja:</span>
//                         <span className={styles.requestValue}>
//                             {new Date(request.createdAt).toLocaleDateString('sr-RS')}
//                         </span>
//                     </div>
//                     <div className={styles.requestInfo}>
//                         <span className={styles.requestLabel}>Status:</span>
//                         <StatusBadge status={request.status} />
//                     </div>
//                     {request.respondedAt && (
//                         <div className={styles.requestInfo}>
//                             <span className={styles.requestLabel}>Datum odgovora:</span>
//                             <span className={styles.requestValue}>
//                                 {new Date(request.respondedAt).toLocaleDateString('sr-RS')}
//                             </span>
//                         </div>
//                     )}
//                     {request.status === 2 && (
//                         <div className={styles.infoBox}>
//                             <p>Vaš zahtev je odbijen. Možete poslati novi zahtev.</p>
//                             <button
//                                 className={styles.sendButton}
//                                 onClick={handleSendRequest}
//                                 disabled={sending}
//                             >
//                                 {sending ? 'Slanje...' : 'Pošalji novi zahtev'}
//                             </button>
//                         </div>
//                     )}
//                 </div>
//             )}
//         </div>
//     )
// }

// export default MoveOutRequest
