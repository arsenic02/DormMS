import { useParams, Link } from 'react-router-dom'
import { useEffect, useState } from 'react'
import DormManagerRanking from '../../../../components/Ranking/DormManagerRanking'
import styles from './DormManagerRankingPage.module.css'

const API_URL = 'http://localhost:5153/api/Competition'
const COMPLETED_STATUS = 3

const statusLabel = {
  0: 'Nacrt',
  1: 'Otvoren',
  2: 'Zatvoren',
  3: 'Završen',
}

const statusClass = {
  0: 'statusDraft',
  1: 'statusOpen',
  2: 'statusClosed',
  3: 'statusCompleted',
}

const typeLabel = {
  0: 'Stariji studenti',
  1: 'Brucoši',
}

function formatDate(dateStr) {
  if (!dateStr) return '-'
  return new Date(dateStr).toLocaleDateString('sr-RS')
}

export default function DormManagerRankingPage() {
  const { id } = useParams()

  const competitionId = Number(id)

  const [competition, setCompetition] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function fetchCompetition() {
      try {
        const res = await fetch(`${API_URL}/${competitionId}`)
        const data = await res.json()
        setCompetition(data)
      } catch (err) {
        console.error(err)
      } finally {
        setLoading(false)
      }
    }

    if (competitionId) fetchCompetition()
  }, [competitionId])

  if (loading) return <div className={styles.container}>Učitavanje...</div>

  if (!competition) return <div className={styles.container}>Konkurs ne postoji</div>

  return (
    <div className={styles.container}>
      <Link to="/dorm-manager/rankings" className={styles.backLink}>
        ← Sve rang-liste
      </Link>

      <div className={styles.header}>
        <h1 className={styles.title}>{competition.name}</h1>
        <span className={styles[statusClass[competition.status]] ?? styles.statusNeutral}>
          {statusLabel[competition.status] ?? 'N/A'}
        </span>
      </div>

      {competition.description && (
        <p className={styles.description}>{competition.description}</p>
      )}

      <div className={styles.metaCard}>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Tip konkursa</span>
          <span className={styles.metaValue}>{typeLabel[competition.type] ?? 'N/A'}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Završetak</span>
          <span className={styles.metaValue}>{formatDate(competition.endDate)}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Prijave</span>
          <span className={styles.metaValue}>{competition.applicationCount ?? '-'}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Max studenata</span>
          <span className={styles.metaValue}>{competition.maxStudents ?? 'Neograničeno'}</span>
        </div>
      </div>

      <DormManagerRanking
        competitionId={competitionId}
        isFreshmanView={competition.type === 1}
        isCompleted={competition.status === COMPLETED_STATUS}
      />
    </div>
  )
}