import { useState, useEffect } from 'react'
import styles from './DormManagerNotifications.module.css'
import { getMyDormNotificationsPaged, markNotificationAsRead } from '../../../api/notifications'

const PAGE_SIZE = 10

function DormManagerNotifications() {
    const [notifications, setNotifications] = useState([])
    const [pageNumber, setPageNumber] = useState(1)
    const [totalPages, setTotalPages] = useState(1)
    const [totalCount, setTotalCount] = useState(0)
    const [unreadOnly, setUnreadOnly] = useState(false)

    const [listLoading, setListLoading] = useState(true)
    const [listError, setListError] = useState(null)

    const fetchNotifications = async () => {
        setListLoading(true)
        setListError(null)
        try {
            const data = await getMyDormNotificationsPaged(pageNumber, PAGE_SIZE, unreadOnly)
            setNotifications(data.items)
            setTotalPages(data.totalPages)
            setTotalCount(data.totalCount)
        } catch (err) {
            setListError(err.message)
        } finally {
            setListLoading(false)
        }
    }

    useEffect(() => {
        fetchNotifications()
    }, [pageNumber, unreadOnly])

    const handleUnreadFilterChange = (e) => {
        setUnreadOnly(e.target.value === 'true')
        setPageNumber(1)
    }

    const handleMarkAsRead = async (id) => {
        try {
            await markNotificationAsRead(id)
            setNotifications((prev) =>
                prev.map((n) => (n.id === id ? { ...n, isRead: true } : n))
            )
        } catch (err) {
            setListError(err.message)
        }
    }

    const formatDateTime = (dateStr) => {
        if (!dateStr) return '-'
        return new Date(dateStr).toLocaleString('sr-RS')
    }

    return (
        <div className={styles.container}>
            <div className={styles.subtitleRow}>
                <h1 className={styles.title}>Obaveštenja</h1>

                <div className={styles.filters}>
                    <div className={styles.filterField}>
                        <label htmlFor="unreadFilter">Prikaz:</label>
                        <select id="unreadFilter" value={unreadOnly.toString()} onChange={handleUnreadFilterChange}>
                            <option value="false">Sva obaveštenja</option>
                            <option value="true">Samo nepročitana</option>
                        </select>
                    </div>
                </div>
            </div>

            {listLoading && <p className={styles.info}>Učitavanje...</p>}
            {listError && <p className={styles.error}>{listError}</p>}

            {!listLoading && !listError && notifications.length === 0 && (
                <p className={styles.info}>Nema obaveštenja za prikaz.</p>
            )}

            {!listLoading && !listError && notifications.length > 0 && (
                <>
                    <div className={styles.list}>
                        {notifications.map((n) => (
                            <div key={n.id} className={`${styles.card} ${!n.isRead ? styles.unread : ''}`}>
                                <div className={styles.cardHeader}>
                                    <span className={styles.cardDate}>{formatDateTime(n.createdAt)}</span>
                                    {!n.isRead && <span className={styles.badge}>Novo</span>}
                                </div>
                                <p className={styles.cardMessage}>{n.message}</p>
                                {!n.isRead && (
                                    <button
                                        className={styles.markReadBtn}
                                        onClick={() => handleMarkAsRead(n.id)}
                                    >
                                        Označi kao pročitano
                                    </button>
                                )}
                            </div>
                        ))}
                    </div>

                    <div className={styles.pagination}>
                        <button
                            className={styles.pageButton}
                            onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                            disabled={pageNumber <= 1}
                        >
                            ← Prethodna
                        </button>
                        <span className={styles.pageInfo}>
                            Stranica <strong>{pageNumber}</strong> od <strong>{totalPages}</strong>
                            {totalCount > 0 && ` (ukupno: ${totalCount})`}
                        </span>
                        <button
                            className={styles.pageButton}
                            onClick={() => setPageNumber((p) => Math.min(totalPages, p + 1))}
                            disabled={pageNumber >= totalPages}
                        >
                            Sledeća →
                        </button>
                    </div>
                </>
            )}
        </div>
    )
}

export default DormManagerNotifications