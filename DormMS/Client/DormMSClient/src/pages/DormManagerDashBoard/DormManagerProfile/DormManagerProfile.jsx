import styles from './DormManagerProfile.module.css'
import { useAuth } from '../../../context/AuthContext'
import { useState, useEffect } from 'react'
import { getDormManagerProfile } from '../../../api/admin'

function ViewItem({ label, value }) {
    return (
        <div className={styles.viewItem}>
            <span className={styles.viewLabel}>{label}</span>
            <span className={styles.viewValue}>{value || '-'}</span>
        </div>
    )
}

function DormManagerProfile() {
    const { auth } = useAuth()
    const [profile, setProfile] = useState(null)
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    useEffect(() => {
        const loadProfile = async () => {
            setLoading(true)
            setError(null)

            try {
                const data = await getDormManagerProfile()
                setProfile(data)
            } catch (err) {
                setError(err.message)
            } finally {
                setLoading(false)
            }
        }

        loadProfile()
    }, [])

    if (loading) return <div className={styles.loading}>Učitavanje profila...</div>

    return (
        <div className={styles.container}>
            <div className={styles.card}>
                <div className={styles.cardHeader}>
                    <h2>Moj profil</h2>
                </div>

                {error && <p className={styles.error}>{error}</p>}

                {profile && (
                    <div className={styles.viewMode}>
                        <div className={styles.viewGrid}>
                            <ViewItem label="Ime" value={profile.name} />
                            <ViewItem label="Prezime" value={profile.surname} />
                            <ViewItem label="Email" value={profile.email} />
                            <ViewItem label="Broj telefona" value={profile.phoneNumber} />
                            <ViewItem label="Dom" value={profile.dormName} />
                        </div>
                    </div>
                )}
            </div>
        </div>
    )
}

export default DormManagerProfile
