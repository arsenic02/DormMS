import styles from './StudentProfile.module.css'
import { useAuth } from '../../../context/AuthContext'
import { useEffect, useState } from 'react'
import { updateStudentProfile, getStudentProfile } from '../../../api/student'

const genderOptions = [
    { value: 0, label: 'Muški' },
    { value: 1, label: 'Ženski' }
]

const studyLevelOptions = [
    { value: 0, label: 'Osnovne studije' },
    { value: 1, label: 'Master studije' },
    { value: 2, label: 'Doktorske studije' },
    { value: 3, label: 'Integrisane studije' }
]

const profileStatusLabel = {
    0: { text: 'Profil nije kompletan', color: '#e53e3e' },
    1: { text: 'Verifikacija u toku', color: '#d69e2e' },
    2: { text: 'Verifikovan', color: '#2f855a' }
}

const facultyOptions = [
    'Elektronski fakultet',
    'Pravni fakultet',
    'Ekonomski fakultet',
    'Medicinski fakultet',
    'Filozofski fakultet',
    'Prirodno-matematički fakultet',
    'Građevinsko-arhitektonski fakultet',
    'Mašinski fakultet',
    'Fakultet umetnosti',
    'Fakultet sporta i fizičkog vaspitanja',
    'Fakultet zaštite na radu'
]

function StudentProfile() {
    const { auth } = useAuth()
    const [formData, setFormData] = useState({
        homeAddress: '',
        municipalityOfResidence: '',
        jmbg: '',
        age: '',
        gender: 0,
        yearOfEnrollment: '',
        studyLevel: 0,
        facultyName: '',
        espb: '',
        averageGrade: '',
        isBudgetStudent: true
    })
    const [errors, setErrors] = useState({})
    const [successMessage, setSuccessMessage] = useState('')
    const [profileStatus, setProfileStatus] = useState(null)
    const [isEditing, setIsEditing] = useState(false)

    useEffect(() => {
        getStudentProfile(auth.token)
            .then(data => {
                setFormData({
                    homeAddress: data.homeAddress || '',
                    municipalityOfResidence: data.municipalityOfResidence || '',
                    jmbg: data.jmbg || '',
                    age: data.age || '',
                    gender: data.gender ?? 0,
                    yearOfEnrollment: data.yearOfEnrollment || '',
                    studyLevel: data.studyLevel ?? 0,
                    facultyName: data.facultyName || '',
                    espb: data.espb || '',
                    averageGrade: data.averageGrade || '',
                    isBudgetStudent: data.isBudgetStudent ?? true
                })
                setProfileStatus(data.profileStatus)
            })
            .catch(err => console.error('Greška pri učitavanju profila:', err))
    }, [])

    const validate = () => {
        const newErrors = {}
        if (!formData.homeAddress) newErrors.homeAddress = 'Adresa je obavezna'
        if (!formData.municipalityOfResidence) newErrors.municipalityOfResidence = 'Opština je obavezna'
        if (!formData.jmbg) {
            newErrors.jmbg = 'JMBG je obavezan'
        } else if (!/^\d{13}$/.test(formData.jmbg)) {
            newErrors.jmbg = 'JMBG mora imati tačno 13 cifara'
        }
        if (!formData.age) {
            newErrors.age = 'Godine su obavezne'
        } else if (isNaN(formData.age) || formData.age < 16 || formData.age > 100) {
            newErrors.age = 'Unesite ispravne godine'
        }
        if (!formData.yearOfEnrollment) {
            newErrors.yearOfEnrollment = 'Godina upisa je obavezna'
        } else if (!/^\d{4}$/.test(formData.yearOfEnrollment)) {
            newErrors.yearOfEnrollment = 'Unesite ispravnu godinu (npr. 2021)'
        }
        if (!formData.facultyName) newErrors.facultyName = 'Naziv fakulteta je obavezan'
        if (!formData.espb) {
            newErrors.espb = 'ESPB je obavezan'
        } else if (isNaN(formData.espb) || formData.espb < 0) {
            newErrors.espb = 'Unesite ispravan broj ESPB'
        }
        if (!formData.averageGrade) {
            newErrors.averageGrade = 'Prosečna ocena je obavezna'
        } else if (isNaN(formData.averageGrade) || formData.averageGrade < 6 || formData.averageGrade > 10) {
            newErrors.averageGrade = 'Ocena mora biti između 6 i 10'
        }
        return newErrors
    }

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target
        setFormData({ ...formData, [name]: type === 'checkbox' ? checked : value })
        setErrors({ ...errors, [name]: '' })
    }

    const handleSubmit = async (e) => {
        e.preventDefault()
        const newErrors = validate()
        if (Object.keys(newErrors).length > 0) {
            setErrors(newErrors)
            return
        }
        setErrors({})
        try {
            const payload = {
                ...formData,
                age: Number(formData.age),
                espb: Number(formData.espb),
                averageGrade: Number(formData.averageGrade),
                yearOfEnrollment: Number(formData.yearOfEnrollment),
                gender: Number(formData.gender),
                studyLevel: Number(formData.studyLevel)
            }
            await updateStudentProfile(payload, auth.token)
            setSuccessMessage('Profil uspešno ažuriran!')
            setTimeout(() => setSuccessMessage(''), 3000)
            setProfileStatus(1)
            setIsEditing(false)
        } catch (err) {
            setErrors({ general: err.message })
        }
    }

    const renderViewMode = () => (
        <div className={styles.viewMode}>
            <div className={styles.statusBadge} style={{ color: profileStatusLabel[profileStatus]?.color }}>
                {profileStatusLabel[profileStatus]?.text}
            </div>
            <div className={styles.viewGrid}>
                <div className={styles.viewItem}>
                    <span className={styles.viewLabel}>Adresa stanovanja</span>
                    <span className={styles.viewValue}>{formData.homeAddress}</span>
                </div>
                <div className={styles.viewItem}>
                    <span className={styles.viewLabel}>Opština</span>
                    <span className={styles.viewValue}>{formData.municipalityOfResidence}</span>
                </div>
                <div className={styles.viewItem}>
                    <span className={styles.viewLabel}>JMBG</span>
                    <span className={styles.viewValue}>{formData.jmbg}</span>
                </div>
                <div className={styles.viewItem}>
                    <span className={styles.viewLabel}>Godine</span>
                    <span className={styles.viewValue}>{formData.age}</span>
                </div>
                <div className={styles.viewItem}>
                    <span className={styles.viewLabel}>Pol</span>
                    <span className={styles.viewValue}>{genderOptions.find(g => g.value === Number(formData.gender))?.label}</span>
                </div>
                <div className={styles.viewItem}>
                    <span className={styles.viewLabel}>Godina upisa</span>
                    <span className={styles.viewValue}>{formData.yearOfEnrollment}</span>
                </div>
                <div className={styles.viewItem}>
                    <span className={styles.viewLabel}>Nivo studija</span>
                    <span className={styles.viewValue}>{studyLevelOptions.find(s => s.value === Number(formData.studyLevel))?.label}</span>
                </div>
                <div className={styles.viewItem}>
                    <span className={styles.viewLabel}>Fakultet</span>
                    <span className={styles.viewValue}>{formData.facultyName}</span>
                </div>
                <div className={styles.viewItem}>
                    <span className={styles.viewLabel}>ESPB</span>
                    <span className={styles.viewValue}>{formData.espb}</span>
                </div>
                <div className={styles.viewItem}>
                    <span className={styles.viewLabel}>Prosečna ocena</span>
                    <span className={styles.viewValue}>{formData.averageGrade}</span>
                </div>
                <div className={styles.viewItem}>
                    <span className={styles.viewLabel}>Budžetski student</span>
                    <span className={styles.viewValue}>{formData.isBudgetStudent ? 'Da' : 'Ne'}</span>
                </div>
            </div>
            <button onClick={() => setIsEditing(true)} className={styles.editBtn}>
                ✏️ Izmeni profil
            </button>
        </div>
    )

    return (
        <div className={styles.container}>
            <div className={styles.card}>
                <h2>Moj profil</h2>
                {profileStatus === 0 || profileStatus === null || isEditing ? (
                    <form onSubmit={handleSubmit}>
                        <div className={styles.field}>
                            <label>Adresa stanovanja</label>
                            <input type="text" name="homeAddress" value={formData.homeAddress} onChange={handleChange} placeholder="Unesite adresu" className={errors.homeAddress ? styles.inputError : ''} />
                            {errors.homeAddress && <span className={styles.error}>{errors.homeAddress}</span>}
                        </div>
                        <div className={styles.field}>
                            <label>Opština prebivališta</label>
                            <input type="text" name="municipalityOfResidence" value={formData.municipalityOfResidence} onChange={handleChange} onKeyDown={(e) => /\d/.test(e.key) && e.preventDefault()} placeholder="Unesite opštinu" className={errors.municipalityOfResidence ? styles.inputError : ''} />
                            {errors.municipalityOfResidence && <span className={styles.error}>{errors.municipalityOfResidence}</span>}
                        </div>
                        <div className={styles.field}>
                            <label>JMBG</label>
                            <input type="text" name="jmbg" value={formData.jmbg} onChange={(e) => { const value = e.target.value.replace(/\D/g, '').slice(0, 13); setFormData({ ...formData, jmbg: value }); setErrors({ ...errors, jmbg: '' }) }} placeholder="13 cifara" maxLength={13} className={errors.jmbg ? styles.inputError : ''} />
                            {errors.jmbg && <span className={styles.error}>{errors.jmbg}</span>}
                        </div>
                        <div className={styles.field}>
                            <label>Godine</label>
                            <input type="number" name="age" value={formData.age} onChange={handleChange} min="18" onKeyDown={(e) => ['e', 'E', '+', '-', '.'].includes(e.key) && e.preventDefault()} placeholder="Unesite godine" className={errors.age ? styles.inputError : ''} />
                            {errors.age && <span className={styles.error}>{errors.age}</span>}
                        </div>
                        <div className={styles.field}>
                            <label>Pol</label>
                            <select name="gender" value={formData.gender} onChange={handleChange}>
                                {genderOptions.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
                            </select>
                        </div>
                        <div className={styles.field}>
                            <label>Godina upisa</label>
                            <input type="text" name="yearOfEnrollment" value={formData.yearOfEnrollment} onChange={(e) => { const value = e.target.value.replace(/\D/g, '').slice(0, 4); setFormData({ ...formData, yearOfEnrollment: value }); setErrors({ ...errors, yearOfEnrollment: '' }) }} placeholder="npr. 2021" maxLength={4} className={errors.yearOfEnrollment ? styles.inputError : ''} />
                            {errors.yearOfEnrollment && <span className={styles.error}>{errors.yearOfEnrollment}</span>}
                        </div>
                        <div className={styles.field}>
                            <label>Nivo studija</label>
                            <select name="studyLevel" value={formData.studyLevel} onChange={handleChange}>
                                {studyLevelOptions.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
                            </select>
                        </div>
                        <div className={styles.field}>
                            <label>Naziv fakulteta</label>
                            <select name="facultyName" value={formData.facultyName} onChange={handleChange} className={errors.facultyName ? styles.inputError : ''}>
                                <option value="">-- Izaberite fakultet --</option>
                                {facultyOptions.map(f => <option key={f} value={f}>{f}</option>)}
                            </select>
                            {errors.facultyName && <span className={styles.error}>{errors.facultyName}</span>}
                        </div>
                        <div className={styles.field}>
                            <label>Broj ESPB</label>
                            <input type="number" name="espb" value={formData.espb} onChange={handleChange} min="0" onKeyDown={(e) => ['e', 'E', '+', '-', '.'].includes(e.key) && e.preventDefault()} placeholder="Unesite broj ESPB" className={errors.espb ? styles.inputError : ''} />
                            {errors.espb && <span className={styles.error}>{errors.espb}</span>}
                        </div>
                        <div className={styles.field}>
                            <label>Prosečna ocena</label>
                            <input type="number" name="averageGrade" value={formData.averageGrade} onChange={handleChange} min="6" max="10" step="0.01" onKeyDown={(e) => ['e', 'E', '+', '-'].includes(e.key) && e.preventDefault()} placeholder="npr. 8.50" className={errors.averageGrade ? styles.inputError : ''} />
                            {errors.averageGrade && <span className={styles.error}>{errors.averageGrade}</span>}
                        </div>
                        <div className={styles.checkboxField}>
                            <input type="checkbox" name="isBudgetStudent" checked={formData.isBudgetStudent} onChange={handleChange} id="isBudgetStudent" />
                            <label htmlFor="isBudgetStudent">Budžetski student</label>
                        </div>
                        {errors.general && <p className={styles.error}>{errors.general}</p>}
                        {successMessage && <p className={styles.success}>{successMessage}</p>}
                        <button type="submit" className={styles.btn}>Sačuvaj profil</button>
                        {isEditing && (
                            <button type="button" onClick={() => setIsEditing(false)} className={styles.cancelBtn}>
                                Otkaži
                            </button>
                        )}
                    </form>
                ) : (
                    renderViewMode()
                )}
            </div>
        </div>
    )
}

export default StudentProfile