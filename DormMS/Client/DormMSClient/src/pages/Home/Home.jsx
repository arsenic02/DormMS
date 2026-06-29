import { Link } from 'react-router-dom'
import styles from './Home.module.css'

function Home() {
    return (
        <div className={styles.home}>
            <section className={styles.hero}>
                <h1 className={styles.heroTitle}>Studentski dom Niš</h1>
                <p className={styles.heroText}>
                    Sistem za prijavu, rangiranje i dodelu smeštaja u studentskom domu.
                    Sve na jednom mestu — od registracije do dodele sobe.
                </p>
                <div className={styles.heroButtons}>
                    <Link to="/register" className={styles.primaryBtn}>Registruj se</Link>
                    <Link to="/login" className={styles.secondaryBtn}>Prijavi se</Link>
                </div>
            </section>

            <section className={styles.steps}>
                <div className={styles.stepCard}>
                    <span className={styles.stepNumber}>1</span>
                    <h3 className={styles.stepTitle}>Registracija i profil</h3>
                    <p className={styles.stepText}>
                        Kreirajte nalog i popunite svoj profil sa ličnim i akademskim podacima.
                    </p>
                </div>
                <div className={styles.stepCard}>
                    <span className={styles.stepNumber}>2</span>
                    <h3 className={styles.stepTitle}>Prijava na konkurs</h3>
                    <p className={styles.stepText}>
                        Nakon verifikacije profila, prijavite se na otvoreni konkurs za smeštaj.
                    </p>
                </div>
                <div className={styles.stepCard}>
                    <span className={styles.stepNumber}>3</span>
                    <h3 className={styles.stepTitle}>Dodela smeštaja</h3>
                    <p className={styles.stepText}>
                        Po zatvaranju konkursa i rangiranju, dobijate informaciju o dodeljenoj sobi.
                    </p>
                </div>
            </section>

            <section className={styles.moreInfo}>
                <p className={styles.moreInfoText}>
                    Želite detaljnije informacije o uslovima i procesu prijave?
                </p>
                <div className={styles.moreInfoLinks}>
                    <Link to="/info" className={styles.linkBtn}>Pogledajte informacije</Link>
                    <Link to="/contact" className={styles.linkBtn}>Kontaktirajte nas</Link>
                </div>
            </section>
        </div>
    )
}

export default Home

// import styles from './Home.module.css'

// function Home() {
//   return (
//     <div className={styles.home}>
//       <h1>Dobrodošli u Studentski dom Niš</h1>
//       <p>Informacije o smeštaju, konkursima i aktuelnostima.</p>
//     </div>
//   )
// }

// export default Home