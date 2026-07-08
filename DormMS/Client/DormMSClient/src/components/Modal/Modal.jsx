import styles from './Modal.module.css'

export default function Modal({ title, isOpen, onClose, children, maxWidth = '420px' }) {
    if (!isOpen) return null

    return (
        <div className={styles.modalOverlay} onClick={onClose}>
            <div 
                className={styles.modalContent} 
                onClick={(e) => e.stopPropagation()}
                style={{ maxWidth }}
            >
                <div className={styles.modalHeader}>
                    <h3 className={styles.modalTitle}>{title}</h3>
                    <button className={styles.modalCloseBtn} onClick={onClose} aria-label="Zatvori modal">&times;</button>
                </div>
                <div className={styles.modalBody}>
                    {children}
                </div>
            </div>
        </div>
    )
}
