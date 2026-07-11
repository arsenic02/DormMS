import { useState, useEffect } from 'react'
import { getMyNotifications, markNotificationAsRead } from '../../../api/notifications'
import styles from './StudentNotifications.module.css'

const PAGE_SIZE = 10

function StudentNotifications() {
  const [notifications, setNotifications] = useState([])
  const [pageNumber, setPageNumber] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [unreadOnly, setUnreadOnly] = useState(false)

  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  const loadNotifications = async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await getMyNotifications(pageNumber, PAGE_SIZE, unreadOnly)
      setNotifications(data?.items || [])
      setTotalPages(data?.totalPages || 1)
      setTotalCount(data?.totalCount || 0)
    } catch (err) {
      setError(err.message)
      setNotifications([])
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadNotifications()
  }, [pageNumber, unreadOnly])

  const handleUnreadFilterChange = (e) => {
    setUnreadOnly(e.target.value === 'true')
    setPageNumber(1)
  }

  const handleMarkAsRead = async (id) => {
    try {
      await markNotificationAsRead(id)
      setNotifications(prev => prev.map(n => 
        n.id === id ? { ...n, isRead: true } : n
      ))
    } catch (err) {
      setError(err.message)
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

      {loading && <p className={styles.info}>Učitavanje...</p>}
      {error && <p className={styles.error}>{error}</p>}

      {!loading && !error && notifications.length === 0 && (
        <p className={styles.info}>Nema obaveštenja za prikaz.</p>
      )}

      {!loading && !error && notifications.length > 0 && (
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

export default StudentNotifications