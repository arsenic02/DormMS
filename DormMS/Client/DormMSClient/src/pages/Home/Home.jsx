import styles from './Home.module.css'

function Home() {
  return (
    <div className={styles.home}>
      <h1>Dobrodošli u Studentski dom Niš</h1>
      <p>Informacije o smeštaju, konkursima i aktuelnostima.</p>
    </div>
  )
}

export default Home