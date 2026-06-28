import styles from './Contact.module.css'

function Contact() {
    return (
        <div className={styles.container}>
            <h1 className={styles.title}>Kontakt</h1>

            <div className={styles.dormCard}>
                <h2 className={styles.cardTitle}>Studentski dom</h2>
                <div className={styles.infoRow}>
                    <span className={styles.label}>Naziv:</span>
                    <span className={styles.value}>Studentski dom Paviljon 4 Niš </span>
                </div>
                <div className={styles.infoRow}>
                    <span className={styles.label}>Adresa:</span>
                    <span className={styles.value}>Bulevar Nikole Tesle 15, Niš</span>
                </div>
                <div className={styles.infoRow}>
                    <span className={styles.label}>Opština:</span>
                    <span className={styles.value}>Crveni Krst (Niš)</span>
                </div>
            </div>

            <div className={styles.contactsGrid}>
                <div className={styles.contactCard}>
                    <h3 className={styles.cardTitle}>Uprava doma</h3>
                    <div className={styles.infoRow}>
                        <span className={styles.label}>Email:</span>
                        <a href="mailto:uprava@dormms.com" className={styles.value}>uprava@dormms.com</a>
                    </div>
                    <div className={styles.infoRow}>
                        <span className={styles.label}>Telefon:</span>
                        <a href="tel:+38100000001" className={styles.value}>+381 00 000 0001</a>
                    </div>
                </div>

                <div className={styles.contactCard}>
                    <h3 className={styles.cardTitle}>Referent doma</h3>
                    <div className={styles.infoRow}>
                        <span className={styles.label}>Email:</span>
                        <a href="referent1@dormms.com" className={styles.value}>referent1@dormms.com</a>
                    </div>
                    <div className={styles.infoRow}>
                        <span className={styles.label}>Telefon:</span>
                        <a href="tel:+38100000000" className={styles.value}>+381 00 000 0000</a>
                    </div>
                </div>
            </div>
        </div>
    )
}

export default Contact