import { useParams } from 'react-router-dom'
import { useEffect, useState } from 'react'
// import DormManagerRanking from './DormManagerRanking'
import DormManagerRanking from '../../../../components/Ranking/DormManagerRanking'
import styles from './DormManagerRankingPage.module.css'

const API_URL = 'http://localhost:5153/api/Competition'

export default function DormManagerRankingPage() {
  const { id } = useParams()

  const competitionId = Number(id)
  console.log('ID iz URL-a:', id, '-> konvertovan u:', competitionId)

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

  if (loading) return <div>Loading...</div>

  if (!competition) return <div>Konkurs ne postoji</div>

  return (
    <div className={styles.container}>
      <h1>{competition.name}</h1>

      <p>Status: {competition.status}</p>

      <DormManagerRanking
        competitionId={competitionId}
        isFreshmanView={competition.type === 1}
        isOpen={competition.isOpen}
      />
    </div>
  )
}