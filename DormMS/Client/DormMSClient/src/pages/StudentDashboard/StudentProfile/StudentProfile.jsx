import styles from './StudentProfile.module.css'
import { useAuth } from '../../../context/AuthContext'
import { useState, useEffect } from 'react'
import { submitFinancialStatus, updateStudentProfile } from '../../../api/student'

import { useStudentProfile } from '../../../hooks/useStudentProfile'
import { useFinancialStatus } from '../../../hooks/useFinancialStatus'



const genderOptions = [
    { value: 0, label: 'Muski' },
    { value: 1, label: 'Zenski' }
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
    2: { text: 'Verifikovan', color: '#2f855a' },
    3: { text: 'Odbijen', color: '#c53030' }
}

const financialStatusLabel = {
    0: { text: 'Na čekanju', color: '#d69e2e' },
    1: { text: 'Verifikovan', color: '#2f855a' },
    2: { text: 'Odbijen', color: '#c53030' }
}

const facultyOptions = [
    'Elektronski fakultet',
    'Pravni fakultet',
    'Ekonomski fakultet',
    'Medicinski fakultet',
    'Filozofski fakultet',
    'Prirodno-matematicki fakultet',
    'Gradjevinsko-arhitektonski fakultet',
    'Masinski fakultet',
    'Fakultet umetnosti',
    'Fakultet sporta i fizickog vaspitanja',
    'Fakultet zastite na radu'
]

const formatValue = (value) => {
    if (value === null || value === undefined || value === '') return '-'
    return value
}

function ViewItem({ label, value }) {
    return (
        <div className={styles.viewItem}>
            <span className={styles.viewLabel}>{label}</span>
            <span className={styles.viewValue}>{formatValue(value)}</span>
        </div>
    )
}

function StudentProfile() {
    const { auth } = useAuth()

    const {
        formData,
        setFormData,
        profileStatus,
        setProfileStatus,
        loading: profileLoading,
        error: profileError
    } = useStudentProfile(auth?.token)

    const {
        financialData,
        setFinancialData,
        loading: financialLoading,
        error: financialError
    } = useFinancialStatus(auth?.token)

    const [errors, setErrors] = useState({})
    const [financialErrors, setFinancialErrors] = useState({})
    const [successMessage, setSuccessMessage] = useState('')
    const [financialSuccessMessage, setFinancialSuccessMessage] = useState('')
    const [submitError, setSubmitError] = useState('')
    const [financialSubmitError, setFinancialSubmitError] = useState('')
    const [isEditing, setIsEditing] = useState(false)
    const [isFinancialEditing, setIsFinancialEditing] = useState(false)

    const selectedGender = genderOptions.find(g => Number(g.value) === Number(formData.gender))?.label
    const selectedStudyLevel = studyLevelOptions.find(s => Number(s.value) === Number(formData.studyLevel))?.label

    const currentYear = new Date().getFullYear()
    const currentYearOfStudy = formData.yearOfEnrollment
        ? (currentYear - Number(formData.yearOfEnrollment) + 1)
        : null
    const isFreshman = currentYearOfStudy === 1

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target

        let nextValue = value

        if (type !== 'checkbox') {
            if (name === 'jmbg') {
                nextValue = value.replace(/\D/g, '').slice(0, 13)
            } else if (name === 'municipalityOfResidence') {
                nextValue = value.replace(/[^A-Za-zšđčćžŠĐČĆŽ\s-]/g, '')
            } else if (name === 'yearOfEnrollment') {
                nextValue = value.replace(/\D/g, '').slice(0, 4)
            }
        }

        setFormData(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : nextValue
        }))

        setErrors(prev => ({
            ...prev,
            [name]: ''
        }))
    }

    const handleFinancialChange = (e) => {
        const { name, value } = e.target

        setFinancialData(prev => ({
            ...prev,
            [name]: value
        }))

        setFinancialErrors(prev => ({
            ...prev,
            [name]: ''
        }))
    }

    const validate = () => {
        const err = {}

        if (!formData.homeAddress) err.homeAddress = 'Adresa je obavezna'
        if (!formData.municipalityOfResidence) err.municipalityOfResidence = 'Opstina je obavezna'

        if (!/^\d{13}$/.test(formData.jmbg)) {
            err.jmbg = 'JMBG mora imati 13 cifara'
        }

        if (!formData.yearOfEnrollment || !/^\d{4}$/.test(formData.yearOfEnrollment)) {
            err.yearOfEnrollment = 'Godina mora biti 4 cifre'
        }

        const age = Number(formData.age)
        if (!formData.age || age < 17 || age > 100) {
            err.age = 'Godine moraju biti izmedju 17 i 100'
        }

        const espb = Number(formData.espb)
        if (formData.espb === '' || espb < 0) {
            err.espb = 'ESPB ne moze biti negativan broj'
        }

        if (isFreshman) {
            const hsGrade = Number(formData.highSchoolAverageGrade)
            if (!formData.highSchoolAverageGrade || hsGrade < 2.0 || hsGrade > 5.0) {
                err.highSchoolAverageGrade = 'Prosecna ocena iz srednje skole mora biti izmedju 2.00 i 5.00'
            }
        } else {
            const avgGrade = Number(formData.averageGrade)
            if (!formData.averageGrade || avgGrade < 6.0 || avgGrade > 10.0) {
                err.averageGrade = 'Prosecna ocena na fakultetu mora biti izmedju 6.00 i 10.00'
            }
        }

        return err
    }

    const validateFinancial = () => {
        const err = {}

        if (!financialData.totalFamilyIncome && financialData.totalFamilyIncome !== 0) {
            err.totalFamilyIncome = 'Ukupan prihod je obavezan'
        }

        if (!financialData.familyMemberCount && financialData.familyMemberCount !== 0) {
            err.familyMemberCount = 'Broj clanova porodice je obavezan'
        }

        if (Number(financialData.familyMemberCount) <= 0) {
            err.familyMemberCount = 'Broj clanova mora biti veci od 0'
        }

        return err
    }

    const handleSubmit = async (e) => {
        e.preventDefault()
        setSubmitError('')

        const validationErrors = validate()
        if (Object.keys(validationErrors).length > 0) {
            setErrors(validationErrors)
            return
        }

        const payload = {
            ...formData,
            age: Number(formData.age),
            espb: Number(formData.espb),
            averageGrade: isFreshman ? null : Number(formData.averageGrade),
            highSchoolAverageGrade: isFreshman ? Number(formData.highSchoolAverageGrade) : null,
            yearOfEnrollment: Number(formData.yearOfEnrollment),
            gender: Number(formData.gender),
            studyLevel: Number(formData.studyLevel)
        }

        try {
            const updatedProfile = await updateStudentProfile(payload, auth.token)
            setProfileStatus(updatedProfile?.profileStatus ?? 1)
            setSuccessMessage('Profil uspesno azuriran!')
            setIsEditing(false)
            setTimeout(() => setSuccessMessage(''), 3000)
        } catch (err) {
            setSubmitError(err.message)
        }
    }

    const handleFinancialSubmit = async (e) => {
        e.preventDefault()
        setFinancialSubmitError('')

        const validationErrors = validateFinancial()
        if (Object.keys(validationErrors).length > 0) {
            setFinancialErrors(validationErrors)
            return
        }

        const payload = {
            totalFamilyIncome: Number(financialData.totalFamilyIncome),
            familyMemberCount: Number(financialData.familyMemberCount)
        }

        try {
            const savedFinancialStatus = await submitFinancialStatus(payload, auth.token)
            const nextFinancialData = savedFinancialStatus || {
                ...financialData,
                incomePerMember: payload.totalFamilyIncome / payload.familyMemberCount
            }

            setFinancialData(nextFinancialData)
            setFinancialSuccessMessage('Finansijski status uspesno azuriran!')
            setIsFinancialEditing(false)
            setTimeout(() => setFinancialSuccessMessage(''), 3000)
        } catch (err) {
            setFinancialSubmitError(err.message)
        }
    }

    if (profileLoading) return <div className={styles.loading}>Ucitavanje profila...</div>

    return (
        <div className={styles.container}>
            <div className={styles.card}>
                <div className={styles.cardHeader}>
                    <h2>Moj profil</h2>
                    <div
                        className={styles.statusBadge}
                        style={{ color: profileStatusLabel[profileStatus]?.color }}
                    >
                        {profileStatusLabel[profileStatus]?.text || 'Status nije poznat'}
                    </div>
                </div>

                {profileError && <p className={styles.error}>{profileError.message}</p>}
                {submitError && <p className={styles.error}>{submitError}</p>}
                {successMessage && <p className={styles.success}>{successMessage}</p>}

                {!isEditing ? (
                    <div className={styles.viewMode}>
                        <div className={styles.viewGrid}>
                            <ViewItem label="Ime" value={formData.name} />
                            <ViewItem label="Prezime" value={formData.surname} />
                            <ViewItem label="Broj indeksa" value={formData.studentIDNumber} />
                            <ViewItem label="Email" value={formData.email} />
                            <ViewItem label="Adresa" value={formData.homeAddress} />
                            <ViewItem label="Opstina" value={formData.municipalityOfResidence} />
                            <ViewItem label="JMBG" value={formData.jmbg} />
                            <ViewItem label="Godine" value={formData.age} />
                            <ViewItem label="Pol" value={selectedGender} />
                            <ViewItem label="Godina upisa" value={formData.yearOfEnrollment} />
                            <ViewItem label="Nivo studija" value={selectedStudyLevel} />
                            <ViewItem label="Fakultet" value={formData.facultyName} />
                            <ViewItem label="ESPB" value={formData.espb} />
                            {isFreshman ? (
                                <ViewItem label="Prosecna ocena (srednja skola)" value={formData.highSchoolAverageGrade} />
                            ) : (
                                <ViewItem label="Prosecna ocena (fakultet)" value={formData.averageGrade} />
                            )}
                            <ViewItem label="Finansiranje" value={formData.isBudgetStudent ? 'Budzet' : 'Samofinansiranje'} />
                        </div>

                        {profileStatus !== 2 ? (
                            <button type="button" className={styles.editBtn} onClick={() => setIsEditing(true)}>
                                Izmeni profil
                            </button>
                        ) : (
                            <p className={styles.muted}>
                                Profil je verifikovan i ne može se menjati.
                            </p>
                        )}
                    </div>
                ) : (
                    <form onSubmit={handleSubmit} className={styles.form}>
                        <div className={styles.formGrid}>
                            <div className={styles.field}>
                                <label htmlFor="homeAddress">Adresa</label>
                                <input
                                    id="homeAddress"
                                    name="homeAddress"
                                    value={formData.homeAddress}
                                    onChange={handleChange}
                                    className={errors.homeAddress ? styles.inputError : ''}
                                />
                                {errors.homeAddress && <span className={styles.error}>{errors.homeAddress}</span>}
                            </div>

                            <div className={styles.field}>
                                <label htmlFor="municipalityOfResidence">Opstina</label>
                                <input
                                    id="municipalityOfResidence"
                                    name="municipalityOfResidence"
                                    value={formData.municipalityOfResidence}
                                    onChange={handleChange}
                                    className={errors.municipalityOfResidence ? styles.inputError : ''}
                                />
                                {errors.municipalityOfResidence && <span className={styles.error}>{errors.municipalityOfResidence}</span>}
                            </div>

                            <div className={styles.field}>
                                <label htmlFor="jmbg">JMBG</label>
                                <input
                                    id="jmbg"
                                    name="jmbg"
                                    value={formData.jmbg}
                                    onChange={handleChange}
                                    inputMode="numeric"
                                    className={errors.jmbg ? styles.inputError : ''}
                                />
                                {errors.jmbg && <span className={styles.error}>{errors.jmbg}</span>}
                            </div>

                            <div className={styles.field}>
                                <label htmlFor="age">Godine</label>
                                <input
                                    id="age"
                                    name="age"
                                    value={formData.age}
                                    onChange={handleChange}
                                    type="number"
                                    min="17"
                                    max="100"
                                    className={errors.age ? styles.inputError : ''}
                                />
                                {errors.age && <span className={styles.error}>{errors.age}</span>}
                            </div>

                            <div className={styles.field}>
                                <label htmlFor="gender">Pol</label>
                                <select id="gender" name="gender" value={formData.gender} onChange={handleChange}>
                                    {genderOptions.map(g => (
                                        <option key={g.value} value={g.value}>{g.label}</option>
                                    ))}
                                </select>
                            </div>

                            <div className={styles.field}>
                                <label htmlFor="yearOfEnrollment">Godina upisa</label>
                                <input
                                    id="yearOfEnrollment"
                                    name="yearOfEnrollment"
                                    value={formData.yearOfEnrollment}
                                    onChange={handleChange}
                                    inputMode="numeric"
                                    className={errors.yearOfEnrollment ? styles.inputError : ''}
                                />
                                {errors.yearOfEnrollment && <span className={styles.error}>{errors.yearOfEnrollment}</span>}
                            </div>

                            <div className={styles.field}>
                                <label htmlFor="studyLevel">Nivo studija</label>
                                <select id="studyLevel" name="studyLevel" value={formData.studyLevel} onChange={handleChange}>
                                    {studyLevelOptions.map(s => (
                                        <option key={s.value} value={s.value}>{s.label}</option>
                                    ))}
                                </select>
                            </div>

                            <div className={styles.field}>
                                <label htmlFor="facultyName">Fakultet</label>
                                <select id="facultyName" name="facultyName" value={formData.facultyName} onChange={handleChange}>
                                    {/* <option value="">Izaberi fakultet</option> */}
                                    {facultyOptions.map(f => (
                                        <option key={f} value={f}>{f}</option>
                                    ))}
                                </select>
                            </div>

                            <div className={styles.field}>
                                <label htmlFor="espb">ESPB</label>
                                <input
                                    id="espb"
                                    name="espb"
                                    value={formData.espb}
                                    onChange={handleChange}
                                    type="number"
                                    min="0"
                                    className={errors.espb ? styles.inputError : ''}
                                />
                                {errors.espb && <span className={styles.error}>{errors.espb}</span>}
                            </div>

                            {isFreshman ? (
                                <div className={styles.field}>
                                    <label htmlFor="highSchoolAverageGrade">Prosecna ocena (srednja skola)</label>
                                    <input
                                        id="highSchoolAverageGrade"
                                        name="highSchoolAverageGrade"
                                        value={formData.highSchoolAverageGrade}
                                        onChange={handleChange}
                                        type="number"
                                        step="0.01"
                                        min="2"
                                        max="5"
                                        className={errors.highSchoolAverageGrade ? styles.inputError : ''}
                                    />
                                    {errors.highSchoolAverageGrade && <span className={styles.error}>{errors.highSchoolAverageGrade}</span>}
                                </div>
                            ) : (
                                <div className={styles.field}>
                                    <label htmlFor="averageGrade">Prosecna ocena (fakultet)</label>
                                    <input
                                        id="averageGrade"
                                        name="averageGrade"
                                        value={formData.averageGrade}
                                        onChange={handleChange}
                                        type="number"
                                        step="0.01"
                                        min="6"
                                        max="10"
                                        className={errors.averageGrade ? styles.inputError : ''}
                                    />
                                    {errors.averageGrade && <span className={styles.error}>{errors.averageGrade}</span>}
                                </div>
                            )}
                        </div>

                        <label className={styles.checkboxField}>
                            <input
                                type="checkbox"
                                name="isBudgetStudent"
                                checked={formData.isBudgetStudent}
                                onChange={handleChange}
                            />
                            <span>Budzet student</span>
                        </label>

                        <div className={styles.actions}>
                            <button type="submit" className={styles.btn}>Sacuvaj</button>
                            <button type="button" className={styles.cancelBtn} onClick={() => setIsEditing(false)}>
                                Odustani
                            </button>
                        </div>
                    </form>
                )}
            </div>

            <div className={styles.card}>
                <div className={styles.cardHeader}>
                    <h2>Finansijski status</h2>
                    <div
                        className={styles.statusBadge}
                        style={{ color: financialStatusLabel[financialData.verificationStatus]?.color }}
                    >
                        {financialStatusLabel[financialData.verificationStatus]?.text || 'Status nije poznat'}
                    </div>
                </div>

                {financialLoading && <p className={styles.muted}>Ucitavanje finansija...</p>}
                {financialError && <p className={styles.error}>{financialError.message}</p>}
                {financialSubmitError && <p className={styles.error}>{financialSubmitError}</p>}
                {financialSuccessMessage && <p className={styles.success}>{financialSuccessMessage}</p>}

                {!isFinancialEditing ? (
                    <div className={styles.viewMode}>
                        <div className={styles.viewGrid}>
                            <ViewItem label="Ukupan prihod" value={financialData.totalFamilyIncome} />
                            <ViewItem label="Clanovi porodice" value={financialData.familyMemberCount} />
                            <ViewItem label="Po clanu" value={financialData.incomePerMember} />
                            <ViewItem label="Status" value={financialStatusLabel[financialData.verificationStatus]?.text || 'Status nije poznat'} />
                        </div>

                        {financialData.verificationStatus !== 1 ? (
                            <button type="button" className={styles.editBtn} onClick={() => setIsFinancialEditing(true)}>
                                Izmeni finansije
                            </button>
                        ) : (
                            <p className={styles.muted}>
                                Finansijski status je verifikovan i ne može se menjati.
                            </p>
                        )}

                    </div>
                ) : (
                    <form onSubmit={handleFinancialSubmit} className={styles.form}>
                        <div className={styles.formGrid}>
                            <div className={styles.field}>
                                <label htmlFor="totalFamilyIncome">Ukupan prihod</label>
                                <input
                                    id="totalFamilyIncome"
                                    name="totalFamilyIncome"
                                    value={financialData.totalFamilyIncome}
                                    onChange={handleFinancialChange}
                                    type="number"
                                    min="0"
                                    className={financialErrors.totalFamilyIncome ? styles.inputError : ''}
                                />
                                {financialErrors.totalFamilyIncome && <span className={styles.error}>{financialErrors.totalFamilyIncome}</span>}
                            </div>

                            <div className={styles.field}>
                                <label htmlFor="familyMemberCount">Clanovi porodice</label>
                                <input
                                    id="familyMemberCount"
                                    name="familyMemberCount"
                                    value={financialData.familyMemberCount}
                                    onChange={handleFinancialChange}
                                    type="number"
                                    min="1"
                                    className={financialErrors.familyMemberCount ? styles.inputError : ''}
                                />
                                {financialErrors.familyMemberCount && <span className={styles.error}>{financialErrors.familyMemberCount}</span>}
                            </div>
                        </div>

                        <div className={styles.actions}>
                            <button type="submit" className={styles.btn}>Sacuvaj</button>
                            <button type="button" className={styles.cancelBtn} onClick={() => setIsFinancialEditing(false)}>
                                Odustani
                            </button>
                        </div>
                    </form>
                )}
            </div>
        </div>
    )
}

export default StudentProfile