import { useState, useEffect } from 'react'
import styles from './StudentRoom.module.css'
import { getMyRoom } from '../../../api/student'
import { Link } from 'react-router-dom'

function StudentRoom() {
    const [room, setRoom] = useState(null)
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    useEffect(() => {
        let active = true

        const loadRoom = async () => {
            setLoading(true)
            setError(null)
            try {
                const data = await getMyRoom()
                if (active) setRoom(data)
            } catch (err) {
                if (active) setError(err.message)
            } finally {
                if (active) setLoading(false)
            }
        }

        loadRoom()
        return () => { active = false }
    }, [])

    return (
        <div className={styles.container}>
            <h1 className={styles.title}>Moja soba</h1>

            {loading && <p className={styles.info}>Učitavanje...</p>}
            {!loading && error && <p className={styles.error}>{error}</p>}

            {!loading && !error && room && !room.hasRoom && (
                <div className={styles.emptyCard}>
                    <p className={styles.emptyText}>
                        Trenutno nemate dodeljenu sobu. Soba se dodeljuje nakon što konkurs za smeštaj
                        bude rangiran i raspoređen.
                    </p>
                </div>
            )}

            {!loading && !error && room && room.hasRoom && (
                <>
                    <div className={styles.roomCard}>
                        <div className={styles.roomHeader}>
                            <span className={styles.roomNumber}>Soba {room.roomNumber}</span>
                            {room.dormName && <span className={styles.dormName}>{room.dormName}</span>}
                        </div>
                        <div className={styles.occupancyRow}>
                            <span>{room.currentOccupancy}/{room.capacity} mesta popunjeno</span>
                        </div>
                        <div className={styles.occupancyBar}>
                            <div
                                className={styles.occupancyFill}
                                style={{ width: `${room.capacity > 0 ? (room.currentOccupancy / room.capacity) * 100 : 0}%` }}
                            />
                        </div>
                        <Link to="/student/room-swap" className={styles.swapLink}>
                            Zameni sobu →
                        </Link>
                        <Link to="/student/move-out" className={styles.swapLink}>
                            Iseli se →
                        </Link>
                    </div>

                    <h2 className={styles.subtitle}>Cimeri</h2>

                    {room.roommates.length === 0 ? (
                        <p className={styles.info}>Trenutno nemate cimera - soba je samo vaša.</p>
                    ) : (
                        <div className={styles.roommatesList}>
                            {room.roommates.map((mate) => (
                                <div key={mate.id} className={styles.roommateCard}>
                                    <div className={styles.roommateAvatar}>
                                        {mate.name[0]}{mate.surname[0]}
                                    </div>
                                    <div className={styles.roommateInfo}>
                                        <span className={styles.roommateName}>
                                            {mate.name} {mate.surname}
                                            {mate.isFreshman && <span className={styles.freshmanTag}>brucoš</span>}
                                        </span>
                                        <span className={styles.roommateMeta}>
                                            Br. indeksa: {mate.studentIDNumber}
                                            {mate.facultyName && ` · ${mate.facultyName}`}
                                        </span>
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

export default StudentRoom