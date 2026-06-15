import { useState } from 'react'
import styles from './StudentProfile.module.css'
import { useAuth } from '../../../context/AuthContext'
import { updateStudentProfile } from '../../../api/student'

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
        console.log(formData)

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
            const data = await updateStudentProfile(payload, auth.token)
            console.log('Profil sačuvan:', data)
        } catch (err) {
            setErrors({ general: err.message })
        }
    }

    return (
        <div className={styles.container}>
            <div className={styles.card}>
                <h2>Moj profil</h2>
                <form onSubmit={handleSubmit}>

                    <div className={styles.field}>
                        <label>Adresa stanovanja</label>
                        <input
                            type="text"
                            name="homeAddress"
                            value={formData.homeAddress}
                            onChange={handleChange}
                            placeholder="Unesite adresu"
                            className={errors.homeAddress ? styles.inputError : ''}
                        />
                        {errors.homeAddress && <span className={styles.error}>{errors.homeAddress}</span>}
                    </div>

                    <div className={styles.field}>
                        <label>Opština prebivališta</label>
                        <input
                            type="text"
                            name="municipalityOfResidence"
                            value={formData.municipalityOfResidence}
                            onChange={handleChange}
                            onKeyDown={(e) => /\d/.test(e.key) && e.preventDefault()}
                            placeholder="Unesite opštinu"
                            className={errors.municipalityOfResidence ? styles.inputError : ''}
                        />
                        {errors.municipalityOfResidence && <span className={styles.error}>{errors.municipalityOfResidence}</span>}
                    </div>

                    <div className={styles.field}>
                        <label>JMBG</label>
                        <input
                            type="text"
                            name="jmbg"
                            value={formData.jmbg}
                            onChange={(e) => {
                                const value = e.target.value.replace(/\D/g, '').slice(0, 13)
                                setFormData({ ...formData, jmbg: value })
                                setErrors({ ...errors, jmbg: '' })
                            }}
                            placeholder="13 cifara"
                            maxLength={13}
                            className={errors.jmbg ? styles.inputError : ''}
                        />
                        {errors.jmbg && <span className={styles.error}>{errors.jmbg}</span>}
                    </div>

                    <div className={styles.field}>
                        <label>Godine</label>
                        <input
                            type="number"
                            name="age"
                            value={formData.age}
                            onChange={handleChange}
                            min="18"
                            onKeyDown={(e) => ['e', 'E', '+', '-', '.'].includes(e.key) && e.preventDefault()}
                            placeholder="Unesite godine"
                            className={errors.age ? styles.inputError : ''}
                        />
                        {errors.age && <span className={styles.error}>{errors.age}</span>}
                    </div>

                    <div className={styles.field}>
                        <label>Pol</label>
                        <select name="gender" value={formData.gender} onChange={handleChange}>
                            {genderOptions.map(o => (
                                <option key={o.value} value={o.value}>{o.label}</option>
                            ))}
                        </select>
                    </div>

                    <div className={styles.field}>
                        <label>Godina upisa</label>
                        <input
                            type="text"
                            name="yearOfEnrollment"
                            value={formData.yearOfEnrollment}
                            onChange={(e) => {
                                const value = e.target.value.replace(/\D/g, '').slice(0, 4)
                                setFormData({ ...formData, yearOfEnrollment: value })
                                setErrors({ ...errors, yearOfEnrollment: '' })
                            }}
                            placeholder="npr. 2021"
                            maxLength={4}
                            className={errors.yearOfEnrollment ? styles.inputError : ''}
                        />
                        {/* <input
                            type="text"
                            name="yearOfEnrollment"
                            value={formData.yearOfEnrollment}
                            onChange={handleChange}
                            placeholder="npr. 2021"
                            className={errors.yearOfEnrollment ? styles.inputError : ''}
                        /> */}
                        {errors.yearOfEnrollment && <span className={styles.error}>{errors.yearOfEnrollment}</span>}
                    </div>

                    <div className={styles.field}>
                        <label>Nivo studija</label>
                        <select name="studyLevel" value={formData.studyLevel} onChange={handleChange}>
                            {studyLevelOptions.map(o => (
                                <option key={o.value} value={o.value}>{o.label}</option>
                            ))}
                        </select>
                    </div>
                    <div className={styles.field}>
                        <label>Naziv fakulteta</label>
                        <select
                            name="facultyName"
                            value={formData.facultyName}
                            onChange={handleChange}
                            className={errors.facultyName ? styles.inputError : ''}
                        >
                            <option value="">-- Izaberite fakultet --</option>
                            {facultyOptions.map(f => (
                                <option key={f} value={f}>{f}</option>
                            ))}
                        </select>
                        {errors.facultyName && <span className={styles.error}>{errors.facultyName}</span>}
                    </div>

                    <div className={styles.field}>
                        <label>Broj ESPB</label>
                        <input
                            type="number"
                            name="espb"
                            value={formData.espb}
                            onChange={handleChange}
                            min="0"
                            onKeyDown={(e) => ['e', 'E', '+', '-', '.'].includes(e.key) && e.preventDefault()}
                            placeholder="Unesite broj ESPB"
                            className={errors.espb ? styles.inputError : ''}
                        />
                        {errors.espb && <span className={styles.error}>{errors.espb}</span>}
                    </div>

                    <div className={styles.field}>
                        <label>Prosečna ocena</label>
                        <input
                            type="number"
                            name="averageGrade"
                            value={formData.averageGrade}
                            onChange={handleChange}
                            min="6"
                            max="10"
                            step="0.01"
                            onKeyDown={(e) => ['e', 'E', '+', '-'].includes(e.key) && e.preventDefault()}
                            placeholder="npr. 8.50"
                            className={errors.averageGrade ? styles.inputError : ''}
                        />
                        {errors.averageGrade && <span className={styles.error}>{errors.averageGrade}</span>}
                    </div>

                    <div className={styles.checkboxField}>
                        <input
                            type="checkbox"
                            name="isBudgetStudent"
                            checked={formData.isBudgetStudent}
                            onChange={handleChange}
                            id="isBudgetStudent"
                        />
                        <label htmlFor="isBudgetStudent">Budžetski student</label>
                    </div>

                    {errors.general && <p className={styles.error}>{errors.general}</p>}
                    <button type="submit" className={styles.btn}>Sačuvaj profil</button>
                </form>
            </div>
        </div>
    )
}

export default StudentProfile

// function StudentProfile() {
//   return <h1>Moj Profil</h1>
// }

// export default StudentProfile