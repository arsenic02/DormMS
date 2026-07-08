import { useState, useEffect, useRef } from 'react'
import styles from './RoomSwapRequest.module.css'
import {
    createSwapRequest,
    respondToSwapRequest,
    getIncomingSwapRequests,
    getOutgoingSwapRequests,
    searchStudentsForSwap
} from '../../../../api/roomSwap'

const STATUS_LABELS = {
    0: { text: 'Na čekanju', className: 'statusPending' },
    1: { text: 'Prihvaćeno', className: 'statusAccepted' },
    2: { text: 'Odbio student', className: 'statusRejected' },
    3: { text: 'Čeka odobrenje referenta', className: 'statusPendingManager' },
    4: { text: 'Odbio referent', className: 'statusRejected' }
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

    // Polja za pretragu (dok student nije izabran)
    const [searchIdNumber, setSearchIdNumber] = useState('')
    const [searchName, setSearchName] = useState('')
    const [searchSurname, setSearchSurname] = useState('')
    const [searchFaculty, setSearchFaculty] = useState('')
    const [searchRoom, setSearchRoom] = useState('')

    // Rezultati pretrage
    const [searchResults, setSearchResults] = useState([])
    const [searching, setSearching] = useState(false)
    const [searchError, setSearchError] = useState(null)
    const [showResults, setShowResults] = useState(false)

    // Izabrani student (nakon klika na rezultat)
    const [selectedStudent, setSelectedStudent] = useState(null)

    const [sending, setSending] = useState(false)
    const [formError, setFormError] = useState(null)
    const [formSuccess, setFormSuccess] = useState(null)

    const [respondingId, setRespondingId] = useState(null)

    const debounceRef = useRef(null)
    const searchBoxRef = useRef(null)

    const loadRequests = async () => {
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
        }
    }

    useEffect(() => {
        let isMounted = true

        const loadRequestsSafe = async () => {
            if (!isMounted) return
            setError(null)
            try {
                const [incomingData, outgoingData] = await Promise.all([
                    getIncomingSwapRequests(),
                    getOutgoingSwapRequests()
                ])
                if (isMounted) {
                    setIncoming(incomingData)
                    setOutgoing(outgoingData)
                }
            } catch (err) {
                if (isMounted) setError(err.message)
            } finally {
                if (isMounted) setLoading(false)
            }
        }

        loadRequestsSafe()

        return () => {
            isMounted = false
        }
    }, [])

    // Debounced pretraga — okida se čim se promeni bilo koje polje za pretragu,
    // sem ako je student već izabran (tada su polja "zaključana")
    useEffect(() => {
        if (selectedStudent) return

        const hasAnyInput =
            searchIdNumber.trim() || searchName.trim() || searchSurname.trim() ||
            searchFaculty.trim() || searchRoom.trim()

        if (!hasAnyInput) {
            setSearchResults([])
            setShowResults(false)
            setSearching(false)
            return
        }

        setSearching(true)
        if (debounceRef.current) clearTimeout(debounceRef.current)

        debounceRef.current = setTimeout(async () => {
            setSearchError(null)
            try {
                const results = await searchStudentsForSwap({
                    idNumber: searchIdNumber.trim(),
                    name: searchName.trim(),
                    surname: searchSurname.trim(),
                    faculty: searchFaculty.trim(),
                    room: searchRoom.trim()
                })
                setSearchResults(results)
                setShowResults(true)
            } catch (err) {
                setSearchError(err.message)
                setSearchResults([])
            } finally {
                setSearching(false)
            }
        }, 300)

        return () => clearTimeout(debounceRef.current)
    }, [searchIdNumber, searchName, searchSurname, searchFaculty, searchRoom, selectedStudent])

    // Zatvori dropdown na klik van njega
    useEffect(() => {
        const handleClickOutside = (e) => {
            if (searchBoxRef.current && !searchBoxRef.current.contains(e.target)) {
                setShowResults(false)
            }
        }
        document.addEventListener('mousedown', handleClickOutside)
        return () => document.removeEventListener('mousedown', handleClickOutside)
    }, [])

    const handleSelectStudent = (student) => {
        setSelectedStudent(student)
        setSearchIdNumber(student.studentIdNumber)
        setSearchName(student.name)
        setSearchSurname(student.surname)
        setSearchFaculty(student.facultyName)
        setSearchRoom(student.roomNumber ?? '')
        setShowResults(false)
        setSearchResults([])
        setFormError(null)
        setFormSuccess(null)
    }

    const handleClearSelection = () => {
        setSelectedStudent(null)
        setSearchIdNumber('')
        setSearchName('')
        setSearchSurname('')
        setSearchFaculty('')
        setSearchRoom('')
        setSearchResults([])
        setShowResults(false)
        setFormError(null)
        setFormSuccess(null)
    }

    const handleSendRequest = async (e) => {
        e.preventDefault()
        setFormError(null)
        setFormSuccess(null)

        if (!selectedStudent) {
            setFormError('Izaberite studenta iz liste rezultata pretrage.')
            return
        }

        setSending(true)
        try {
            await createSwapRequest(selectedStudent.studentIdNumber)
            setFormSuccess('Zahtev je uspešno poslat.')
            handleClearSelection()
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
                    <div className={styles.searchBox} ref={searchBoxRef}>
                        <div className={styles.searchFields}>
                        <input
                            type="text"
                            placeholder="Broj indeksa studenta"
                            value={searchIdNumber}
                            onChange={(e) => setSearchIdNumber(e.target.value)}
                            onFocus={() => searchResults.length > 0 && setShowResults(true)}
                            className={styles.input}
                            disabled={sending || !!selectedStudent}
                        />
                        <input
                            type="text"
                            placeholder="Ime studenta"
                            value={searchName}
                            onChange={(e) => setSearchName(e.target.value)}
                            onFocus={() => searchResults.length > 0 && setShowResults(true)}
                            className={styles.input}
                            disabled={sending || !!selectedStudent}
                        />
                        <input
                            type="text"
                            placeholder="Prezime studenta"
                            value={searchSurname}
                            onChange={(e) => setSearchSurname(e.target.value)}
                            onFocus={() => searchResults.length > 0 && setShowResults(true)}
                            className={styles.input}
                            disabled={sending || !!selectedStudent}
                        />
                        <input
                            type="text"
                            placeholder="Fakultet studenta"
                            value={searchFaculty}
                            onChange={(e) => setSearchFaculty(e.target.value)}
                            onFocus={() => searchResults.length > 0 && setShowResults(true)}
                            className={styles.input}
                            disabled={sending || !!selectedStudent}
                        />
                        <input
                            type="text"
                            placeholder="Naziv sobe studenta"
                            value={searchRoom}
                            onChange={(e) => setSearchRoom(e.target.value)}
                            onFocus={() => searchResults.length > 0 && setShowResults(true)}
                            className={styles.input}
                            disabled={sending || !!selectedStudent}
                        />
                        </div>

                        {searchError && <p className={styles.error}>{searchError}</p>}

                        {searching && !selectedStudent && (
                            <div className={styles.searchResultsDropdown}>
                                <div className={styles.searchLoadingRow}>
                                    <span className={styles.spinner} />
                                    <span>Pretraga studenata...</span>
                                </div>
                            </div>
                        )}

                        {!searching && showResults && !selectedStudent && searchResults.length > 0 && (
                            <div className={styles.searchResultsDropdown}>
                                {searchResults.map((student) => (
                                    <div
                                        key={student.studentIdNumber}
                                        className={styles.searchResultItem}
                                        onClick={() => handleSelectStudent(student)}
                                    >
                                        <span className={styles.searchResultName}>
                                            {student.name} {student.surname}
                                        </span>
                                        <span className={styles.searchResultMeta}>
                                            {student.facultyName}
                                            {student.roomNumber && ` · Soba ${student.roomNumber}`}
                                            {' · '}Indeks: {student.studentIdNumber}
                                        </span>
                                    </div>
                                ))}
                            </div>
                        )}

                        {!searching && showResults && !selectedStudent && searchResults.length === 0 && (
                            <div className={styles.searchResultsDropdown}>
                                <p className={styles.info}>Nema rezultata za unete kriterijume.</p>
                            </div>
                        )}
                    </div>

                    {selectedStudent && (
                        <div className={styles.selectedStudentCard}>
                            <span>
                                Izabran student: <strong>{selectedStudent.name} {selectedStudent.surname}</strong>
                                {' '}(indeks {selectedStudent.studentIdNumber})
                            </span>
                            <button
                                type="button"
                                className={styles.rejectButton}
                                onClick={handleClearSelection}
                                disabled={sending}
                            >
                                Promeni
                            </button>
                        </div>
                    )}

                    <div className={styles.formActions}>
                        <button type="submit" className={styles.sendButton} disabled={sending || !selectedStudent}>
                            {sending ? 'Slanje...' : 'Pošalji zahtev'}
                        </button>
                    </div>
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