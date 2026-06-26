import { useState, useEffect } from 'react'
// import { getRoomsPaged } from '../api/room'
import { getRoomsPaged } from '../../../api/dormitoryRoom'
import styles from './DormManagerRooms.module.css'

const STATUS_LABELS = {
    0: 'Dostupna',
    1: 'Popunjena',
    2: 'Održavanje',
    3: 'Zatvorena',
}

const STATUS_CLASS = {
    0: 'Available',
    1: 'FullyOccupied',
    2: 'Maintenance',
    3: 'Closed',
}

const GENDER_LABELS = {
    0: 'Muška',
    1: 'Ženska',
}

const PAGE_SIZE = 10

export default function DormManagerRooms() {
    const [rooms, setRooms] = useState([])
    const [pageNumber, setPageNumber] = useState(1)
    const [totalPages, setTotalPages] = useState(1)
    const [totalCount, setTotalCount] = useState(0)
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState('')

    useEffect(() => {
        let active = true

        const loadRooms = async () => {
            setLoading(true)
            setError('')
            try {
                const data = await getRoomsPaged(pageNumber, PAGE_SIZE)
                console.log("API RESPONSE:", data)

                if (!active) return
                setRooms(data.items)
                console.log(data.items)


                setTotalPages(data.totalPages)
                setTotalCount(data.totalCount)
            } catch (err) {
                if (active) setError(err.message)
            } finally {
                if (active) setLoading(false)
            }
        }

        loadRooms()
        return () => { active = false }
    }, [pageNumber])

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h2 className={styles.title}>Sobe</h2>
                <p className={styles.subtitle}>
                    {totalCount > 0 ? `Ukupno soba: ${totalCount}` : 'Pregled soba i dodeljenih studenata'}
                </p>
            </div>

            {loading && <div className={styles.loading}>Učitavanje soba...</div>}
            {!loading && error && <div className={styles.error}>{error}</div>}
            {!loading && !error && rooms.length === 0 && (
                <div className={styles.empty}>Nema soba za prikaz.</div>
            )}

            {!loading && !error && rooms.length > 0 && (
                <>
                    <div className={styles.grid}>
                        {rooms.map((room) => (
                            <RoomCard key={room.id} room={room} />
                        ))}
                    </div>

                    <div className={styles.pagination}>
                        <button
                            className={styles.pageButton}
                            onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                            disabled={pageNumber <= 1}
                        >
                            Prethodna
                        </button>
                        <span className={styles.pageInfo}>
                            Stranica {pageNumber} od {totalPages}
                        </span>
                        <button
                            className={styles.pageButton}
                            onClick={() => setPageNumber((p) => Math.min(totalPages, p + 1))}
                            disabled={pageNumber >= totalPages}
                        >
                            Sledeća
                        </button>
                    </div>
                </>
            )}
        </div>
    )
}

function RoomCard({ room }) {
    const statusKey = STATUS_CLASS[room.status] ?? 'Available'
    const occupancyPercent = room.capacity > 0
        ? Math.min(100, Math.round((room.currentOccupancy / room.capacity) * 100))
        : 0

    return (
        <div className={`${styles.card} ${styles[`card${statusKey}`]}`}>
            <div className={styles.cardTop}>
                <div>
                    <p className={styles.roomNumber}>Soba {room.roomNumber}</p>
                    {room.dormName && <p className={styles.dormName}>{room.dormName}</p>}
                </div>
                <span className={`${styles.statusBadge} ${styles[`status${statusKey}`]}`}>
                    {STATUS_LABELS[room.status]}
                </span>
            </div>

            <div className={styles.meta}>
                <span>{room.currentOccupancy}/{room.capacity} mesta</span>
                <span>{GENDER_LABELS[room.gender]}</span>
            </div>

            <div className={styles.occupancyBar}>
                <div
                    className={styles.occupancyFill}
                    style={{ width: `${occupancyPercent}%` }}
                />
            </div>

            <div className={styles.studentsList}>
                <p className={styles.studentsLabel}>Studenti</p>
                {room.students.length === 0 ? (
                    <p className={styles.emptyRoom}>Soba je trenutno prazna.</p>
                ) : (
                    room.students.map((student) => (
                        <div key={student.id} className={styles.studentRow}>
                            <span className={styles.studentName}>
                                {student.name} {student.surname}
                                {student.isFreshman && (
                                    <span className={styles.freshmanTag}>brucoš</span>
                                )}
                            </span>
                            <span className={styles.studentFaculty}>{student.facultyName}</span>
                        </div>
                    ))
                )}
            </div>
        </div>
    )
}
