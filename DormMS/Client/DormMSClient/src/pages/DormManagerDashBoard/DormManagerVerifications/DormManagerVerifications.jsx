import { useState, useEffect } from 'react'
import styles from './DormManagerVerifications.module.css'
import { useAuth } from '../../../context/AuthContext'
import {
    getAllStudents,
    verifyStudentProfile,
    verifyStudentFinancialStatus
} from '../../../api/student'

const PROFILE_STATUS = {
    Verified: 2,
    Rejected: 3,
}

function DormManagerVerifications() {
    const { auth } = useAuth()

    const [students, setStudents] = useState([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    const [actionLoadingKey, setActionLoadingKey] = useState(null)
    const [actionError, setActionError] = useState(null)

    const fetchStudents = async () => {
        setLoading(true)
        setError(null)
        try {
            const data = await getAllStudents(auth.token)
            setStudents(data)
        } catch (err) {
            setError(err.message)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        fetchStudents()
    }, [])

    const pendingProfiles = students.filter(
        (s) => s.profileStatus === 1 // PendingVerification
    )

    const pendingFinancial = students.filter(
        (s) => s.financialStatus != null && s.financialStatus.isVerified === false
    )

    const handleProfileDecision = async (studentId, status) => {
        const key = `profile-${studentId}`
        setActionError(null)
        setActionLoadingKey(key)
        try {
            await verifyStudentProfile(studentId, status, auth.token)
            setStudents((prev) =>
                prev.map((s) =>
                    s.id === studentId ? { ...s, profileStatus: status } : s
                )
            )
        } catch (err) {
            setActionError(err.message)
        } finally {
            setActionLoadingKey(null)
        }
    }

    const handleFinancialDecision = async (studentId) => {
        const key = `financial-${studentId}`
        setActionError(null)
        setActionLoadingKey(key)
        try {
            await verifyStudentFinancialStatus(studentId, true, auth.token)
            setStudents((prev) =>
                prev.map((s) =>
                    s.id === studentId
                        ? { ...s, financialStatus: { ...s.financialStatus, isVerified: true } }
                        : s
                )
            )
        } catch (err) {
            setActionError(err.message)
        } finally {
            setActionLoadingKey(null)
        }
    }

    if (loading) return <p className={styles.info}>Učitavanje...</p>
    if (error) return <p className={styles.error}>{error}</p>

    return (
        <div className={styles.container}>
            <h1 className={styles.title}>Verifikacije</h1>

            {actionError && <p className={styles.error}>{actionError}</p>}

            <div className={styles.columns}>
                {/* LEVA KOLONA - Profili */}
                <div className={styles.column}>
                    <h2 className={styles.columnTitle}>Profili na čekanju</h2>

                    {pendingProfiles.length === 0 && (
                        <p className={styles.info}>Nema profila koji čekaju verifikaciju.</p>
                    )}

                    <div className={styles.list}>
                        {pendingProfiles.map((s) => {
                            const key = `profile-${s.id}`
                            const isLoading = actionLoadingKey === key

                            return (
                                <div key={s.id} className={styles.card}>
                                    <div className={styles.cardHeader}>
                                        <span className={styles.cardName}>
                                            {s.name} {s.surname}
                                        </span>
                                        <span className={styles.badge}>Na čekanju</span>
                                    </div>

                                    <div className={styles.cardDetails}>
                                        <span>Broj indeksa: {s.studentIDNumber}</span>
                                        <span>Fakultet: {s.facultyName ?? '-'}</span>
                                        <span>Prosek: {s.averageGrade}</span>
                                        <span>ESPB: {s.espb}</span>
                                        <span>JMBG: {s.jmbg}</span>
                                        <span>Adresa: {s.homeAddress}</span>
                                    </div>

                                    <div className={styles.actions}>
                                        <button
                                            className={styles.btnApprove}
                                            disabled={isLoading}
                                            onClick={() =>
                                                handleProfileDecision(s.id, PROFILE_STATUS.Verified)
                                            }
                                        >
                                            {isLoading ? 'Čuvanje...' : 'Verifikuj'}
                                        </button>
                                        <button
                                            className={styles.btnReject}
                                            disabled={isLoading}
                                            onClick={() =>
                                                handleProfileDecision(s.id, PROFILE_STATUS.Rejected)
                                            }
                                        >
                                            Odbij
                                        </button>
                                    </div>
                                </div>
                            )
                        })}
                    </div>
                </div>

                {/* DESNA KOLONA - Finansijski statusi */}
                <div className={styles.column}>
                    <h2 className={styles.columnTitle}>Finansijski podaci na čekanju</h2>

                    {pendingFinancial.length === 0 && (
                        <p className={styles.info}>Nema finansijskih podataka koji čekaju verifikaciju.</p>
                    )}

                    <div className={styles.list}>
                        {pendingFinancial.map((s) => {
                            const key = `financial-${s.id}`
                            const isLoading = actionLoadingKey === key
                            const fs = s.financialStatus

                            return (
                                <div key={s.id} className={styles.card}>
                                    <div className={styles.cardHeader}>
                                        <span className={styles.cardName}>
                                            {s.name} {s.surname}
                                        </span>
                                        <span className={styles.badge}>Na čekanju</span>
                                    </div>

                                    <div className={styles.cardDetails}>
                                        <span>Ukupni prihodi domaćinstva: {fs.totalFamilyIncome} RSD</span>
                                        <span>Broj članova porodice: {fs.familyMemberCount}</span>
                                        <span>Prihod po članu: {fs.incomePerMember} RSD</span>
                                        <span>
                                            Podneto: {new Date(fs.submittedAt).toLocaleDateString('sr-RS')}
                                        </span>
                                    </div>

                                    <div className={styles.actions}>
                                        <button
                                            className={styles.btnApprove}
                                            disabled={isLoading}
                                            onClick={() => handleFinancialDecision(s.id)}
                                        >
                                            {isLoading ? 'Čuvanje...' : 'Verifikuj'}
                                        </button>
                                    </div>
                                </div>
                            )
                        })}
                    </div>
                </div>
            </div>
        </div>
    )
}

export default DormManagerVerifications