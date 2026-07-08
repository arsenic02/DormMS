import styles from './DormInfoCard.module.css'

export default function DormInfoCard({ dorm, editable = false, onEdit }) {
    if (!dorm) return null

    return (
        <div className={styles.dormCard}>
            <div className={styles.dormHeader}>
                <div>
                    <h2 className={styles.dormName}>{dorm.name}</h2>
                    <p className={styles.dormLocation}>{dorm.location}</p>
                    <p className={styles.dormLocation}>{dorm.municipality}</p>
                </div>
                {editable && onEdit && (
                    <button className={styles.editBtn} onClick={onEdit}>
                        Izmeni
                    </button>
                )}
            </div>
            <div className={styles.dormStats}>
                <div className={styles.statItem}>
                    <span className={styles.statValue}>{dorm.capacity}</span>
                    <span className={styles.statLabel}>Kapacitet</span>
                </div>
                <div className={styles.statItem}>
                    <span className={styles.statValue}>{dorm.currentOccupancy}</span>
                    <span className={styles.statLabel}>Popunjeno</span>
                </div>
                <div className={styles.statItem}>
                    <span className={styles.statValue}>{dorm.availableSpots}</span>
                    <span className={styles.statLabel}>Slobodno</span>
                </div>
                <div className={styles.statItem}>
                    <span className={styles.statValue}>{dorm.rooms.length}</span>
                    <span className={styles.statLabel}>Soba</span>
                </div>
            </div>
        </div>
    )
}
