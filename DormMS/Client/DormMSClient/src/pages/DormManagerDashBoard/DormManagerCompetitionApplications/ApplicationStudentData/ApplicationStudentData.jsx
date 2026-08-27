import { useState, useEffect } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import styles from './ApplicationStudentData.module.css'
import { getApplicationStudentData } from '../../../../api/applyCompetition'

const studyLevelLabel = {
    'Undergraduate': 'Osnovne studije',
    'Postgraduate': 'Master studije',
    'Doctoral': 'Doktorske studije',
    'Integrated': 'Integrirane studije',
}

const studentStatusLabel = {
    'Active': 'Aktivan',
    'Inactive': 'Neaktivan',
    'Graduated': 'Diplomirao',
}

const genderLabel = {
    'Male': 'Muški',
    'Female': 'Ženski',
}

function ApplicationStudentData() {
    const { applicationId } = useParams()
    const navigate = useNavigate()

    const [studentData, setStudentData] = useState(null)
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    useEffect(() => {
        let active = true

        const loadStudentData = async () => {
            try {
                const data = await getApplicationStudentData(applicationId)
                if (!active) return
                setStudentData(data)
            } catch (err) {
                if (active) setError(err.message)
            } finally {
                if (active) setLoading(false)
            }
        }

        loadStudentData()
        return () => { active = false }
    }, [applicationId])

    const formatDate = (dateStr) => {
        if (!dateStr) return '-'
        return new Date(dateStr).toLocaleDateString('sr-RS')
    }

    const formatCurrency = (value) => {
        if (value === null || value === undefined) return '-'
        return new Intl.NumberFormat('sr-RS', { style: 'currency', currency: 'RSD' }).format(value)
    }

    if (loading) {
        return (
            <div className={styles.container}>
                <p className={styles.info}>Učitavanje...</p>
            </div>
        )
    }

    if (error) {
        return (
            <div className={styles.container}>
                <button className={styles.backBtn} onClick={() => navigate(-1)}>
                    ← Nazad
                </button>
                <p className={styles.error}>{error}</p>
            </div>
        )
    }

    if (!studentData) {
        return (
            <div className={styles.container}>
                <button className={styles.backBtn} onClick={() => navigate(-1)}>
                    ← Nazad
                </button>
                <p className={styles.info}>Podaci nisu pronađeni.</p>
            </div>
        )
    }

    return (
        <div className={styles.container}>
            <button className={styles.backBtn} onClick={() => navigate(-1)}>
                ← Nazad
            </button>

            <div className={styles.header}>
                <h1 className={styles.title}>Podaci studenta</h1>
                <p className={styles.subtitle}>
                    {studentData.studentName} {studentData.studentSurname} (Broj indeksa: {studentData.studentIDNumber})
                </p>
            </div>

            <div className={styles.section}>
                <h2 className={styles.sectionTitle}>Lični podaci</h2>
                <div className={styles.grid}>
                    <div className={styles.field}>
                        <span className={styles.label}>Ime:</span>
                        <span className={styles.value}>{studentData.studentName}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>Prezime:</span>
                        <span className={styles.value}>{studentData.studentSurname}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>JMBG:</span>
                        <span className={styles.value}>{studentData.jmbg}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>Email:</span>
                        <span className={styles.value}>{studentData.studentEmail}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>Adresa stanovanja:</span>
                        <span className={styles.value}>{studentData.homeAddress}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>Opština stanovanja:</span>
                        <span className={styles.value}>{studentData.municipalityOfResidence}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>Starost:</span>
                        <span className={styles.value}>{studentData.age} godina</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>Pol:</span>
                        <span className={styles.value}>{genderLabel[studentData.gender] || studentData.gender}</span>
                    </div>
                </div>
            </div>

            <div className={styles.section}>
                <h2 className={styles.sectionTitle}>Podaci o studijama</h2>
                <div className={styles.grid}>
                    <div className={styles.field}>
                        <span className={styles.label}>Fakultet:</span>
                        <span className={styles.value}>{studentData.facultyName}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>Nivo studija:</span>
                        <span className={styles.value}>{studyLevelLabel[studentData.studyLevel] || studentData.studyLevel}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>Status studenta:</span>
                        <span className={styles.value}>{studentStatusLabel[studentData.studentStatus] || studentData.studentStatus}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>Godina upisa:</span>
                        <span className={styles.value}>{studentData.yearOfEnrollment}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>Trenutna godina studija:</span>
                        <span className={styles.value}>{studentData.currentYearOfStudy}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>ESPB:</span>
                        <span className={styles.value}>{studentData.espb}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>Prosek ocena:</span>
                        <span className={styles.value}>{studentData.averageGrade !== null ? studentData.averageGrade.toFixed(2) : '-'}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>Prosek iz srednje škole:</span>
                        <span className={styles.value}>{studentData.highSchoolAverageGrade !== null ? studentData.highSchoolAverageGrade.toFixed(2) : '-'}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>Budžetski student:</span>
                        <span className={styles.value}>{studentData.isBudgetStudent ? 'Da' : 'Ne'}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>Brucoš:</span>
                        <span className={styles.value}>{studentData.isFreshman ? 'Da' : 'Ne'}</span>
                    </div>
                </div>
            </div>

            <div className={styles.section}>
                <h2 className={styles.sectionTitle}>Finansijski podaci</h2>
                <div className={styles.grid}>
                    <div className={styles.field}>
                        <span className={styles.label}>Ukupan prihod porodice:</span>
                        <span className={styles.value}>{formatCurrency(studentData.totalFamilyIncome)}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>Broj članova porodice:</span>
                        <span className={styles.value}>{studentData.familyMemberCount !== null ? studentData.familyMemberCount : '-'}</span>
                    </div>
                    <div className={styles.field}>
                        <span className={styles.label}>Prihod po članu:</span>
                        <span className={styles.value}>{formatCurrency(studentData.incomePerMember)}</span>
                    </div>
                </div>
            </div>

            <div className={styles.section}>
                <h2 className={styles.sectionTitle}>Podaci o prijavi</h2>
                <div className={styles.grid}>
                    <div className={styles.field}>
                        <span className={styles.label}>Datum prijave:</span>
                        <span className={styles.value}>{formatDate(studentData.appliedAt)}</span>
                    </div>
                    {studentData.notes && (
                        <div className={styles.fieldFull}>
                            <span className={styles.label}>Napomena:</span>
                            <span className={styles.value}>{studentData.notes}</span>
                        </div>
                    )}
                </div>
            </div>
        </div>
    )
}

export default ApplicationStudentData
