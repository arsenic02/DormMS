import { useState } from 'react'
import styles from './Information.module.css'
import { Link } from 'react-router-dom'

const faqItems = [
    {
        question: 'Kako se registrujem na sistem?',
        answer: 'Kliknite na "Registruj se" i unesite svoj email i lozinku. Nakon registracije, potrebno je da popunite svoj profil sa ličnim, akademskim i finansijskim podacima.'
    },
   
    {
        question: 'Da li mogu da se prijavim na konkurs ako nisam budžetski student?',
        answer: 'Ne, prijava na konkurs za smeštaj dostupna je samo budžetskim studentima sa verifikovanim profilom i finansijskim statusom.'
    },
    {
        question: 'Kada se objavljuju rezultati konkursa?',
        answer: 'Rezultati se objavljuju nakon zatvaranja konkursa i završetka rangiranja prijava. Status svoje prijave možete pratiti na svom profilu.'
    },
    {
        question: 'Da li mogu da promenim sobu nakon dodele?',
        answer: 'Da, nakon dobijanja smeštaja možete poslati zahtev za zamenu sobe drugom studentu, uz njegovo odobrenje.'
    },
    {
        question: 'Koga kontaktirati za dodatna pitanja?',
        answer: 'Za sva pitanja vezana za smeštaj možete se obratiti referentu doma putem kontakt podataka dostupnih u domu.'
    }
]

function Information() {
    const [openIndex, setOpenIndex] = useState(null)

    const toggleItem = (index) => {
        setOpenIndex(openIndex === index ? null : index)
    }

    return (
        <div className={styles.container}>
            <h1 className={styles.title}>Informacije o smeštaju</h1>

            <section className={styles.section}>
                <h2 className={styles.sectionTitle}>Kako funkcioniše prijava za smeštaj</h2>
                <p className={styles.text}>
                    Smeštaj u studentskom domu dodeljuje se putem konkursa koji raspisuje studentski dom za
                    svaku akademsku godinu. Konkursi se objavljuju odvojeno za brucoše i odvojeno za
                    starije studente, pošto se kriterijumi rangiranja razlikuju.
                </p>
                <p className={styles.text}>
                    Da bi student konkurisao, potrebno je da se prvo registruje na sistemu, popuni
                    svoj profil sa ličnim i akademskim podacima. Nakon toga, student može da podnese prijavu na otvoreni konkurs i uz prijavu prilaže dokumenta. Referent proverava podatke i dokumenta, i odobrava ili odbija prijavu.
                </p>
            </section>

            <section className={styles.section}>
                <h2 className={styles.sectionTitle}>Uslovi za prijavu</h2>
                <ul className={styles.list}>
                    <li>Status budžetskog studenta na fakultetu</li>
                    <li>Za brucoše: prosečna ocena iz srednje škole</li>
                    <li>Za starije studente: prosečna ocena na fakultetu i broj ostvarenih ESPB bodova</li>
                    <li>Dokument koji dokazuje navedene podatke i podatke o mestu prebivališta</li>
                    <li>Student se uspešno prijavio na konkurs nakon što referent pregleda podatke i dokument i odobri prijavu</li>
                </ul>
            </section>

            <section className={styles.section}>
                <h2 className={styles.sectionTitle}>Kako se rangiranje vrši</h2>
                <p className={styles.text}>
                    Nakon zatvaranja konkursa, sve prijave se rangiraju na osnovu kriterijuma
                    propisanih konkursom (akademski uspeh, socijalno-ekonomski status).
                    Studenti koji se nalaze u okviru raspoloživog broja mesta dobijaju smeštaj u
                    domu, u skladu sa raspoloživim kapacitetom soba.
                </p>
            </section>

            <section className={styles.section}>
                <h2 className={styles.sectionTitle}>Najčešća pitanja</h2>
                <div className={styles.faqList}>
                    {faqItems.map((item, index) => (
                        <div key={index} className={styles.faqItem}>
                            <button
                                className={styles.faqQuestion}
                                onClick={() => toggleItem(index)}
                            >
                                {item.question}
                                <span className={styles.faqIcon}>{openIndex === index ? '−' : '+'}</span>
                            </button>
                            {openIndex === index && (
                                <p className={styles.faqAnswer}>{item.answer}</p>
                            )}
                        </div>
                    ))}
                </div>
            </section>

            <section className={styles.ctaSection}>
                <p className={styles.ctaText}>Spremni da konkurišete za smeštaj?</p>
                <div className={styles.ctaButtons}>
                    <Link to="/register" className={styles.primaryBtn}>Registruj se</Link>
                    <Link to="/login" className={styles.secondaryBtn}>Prijavi se</Link>
                </div>
            </section>
        </div>
    )
}

export default Information