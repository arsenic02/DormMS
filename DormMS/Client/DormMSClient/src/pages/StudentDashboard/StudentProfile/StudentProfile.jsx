import styles from './StudentProfile.module.css'
import { useAuth } from '../../../context/AuthContext'
import { useState, useEffect } from 'react'
import { updateStudentProfile } from '../../../api/student'
import { useStudentProfile } from '../../../hooks/useStudentProfile'
import { useFinancialStatus } from '../../../hooks/useFinancialStatus'
import { submitFinancialStatus } from '../../../api/student'



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
    const [successMessage, setSuccessMessage] = useState('')
    const [submitError, setSubmitError] = useState('')
    const [isEditing, setIsEditing] = useState(false)
    const [isEditingFinancial, setIsEditingFinancial] = useState(false)
    const [financialSuccessMessage, setFinancialSuccessMessage] = useState('')
    const [financialSubmitError, setFinancialSubmitError] = useState('')
    // const [financialError, setFinancialError] = useState('')

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

        if (!formData.facultyName) {
            err.facultyName = 'Fakultet je obavezan'
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

    const handleSubmit = async (e) => {
        e.preventDefault()
        setSubmitError('')

        const validationErrors = validate()
        if (Object.keys(validationErrors).length > 0) {
            setErrors(validationErrors)
            return
        }

        const {
            totalFamilyIncome,
            familyMemberCount,
            ...profileFields
        } = formData

        const payload = {
            ...profileFields,
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
            setSuccessMessage('Profil uspesno azuriran!')
            setIsEditing(false)
            setTimeout(() => setSuccessMessage(''), 3000)
        } catch (err) {
            setSubmitError(err.message)
        }
    }

    const handleFinancialChange = (e) => {
        const { name, value } = e.target
        setFinancialData(prev => ({ ...prev, [name]: value }))
    }

    const handleFinancialSubmit = async (e) => {
        e.preventDefault()
        setFinancialSubmitError('')

        if (!financialData.totalFamilyIncome || Number(financialData.totalFamilyIncome) <= 0) {
            setFinancialSubmitError('Ukupan prihod porodice je obavezan i mora biti veći od 0')
            return
        }

        if (!financialData.familyMemberCount || Number(financialData.familyMemberCount) <= 0) {
            setFinancialSubmitError('Broj članova porodice je obavezan i mora biti veći od 0')
            return
        }

        const payload = {
            totalFamilyIncome: Number(financialData.totalFamilyIncome),
            familyMemberCount: Number(financialData.familyMemberCount)
        }

        try {
            const saved = await submitFinancialStatus(payload, auth.token)
            setFormData(prev => ({
                ...prev,
                totalFamilyIncome: saved.totalFamilyIncome,
                familyMemberCount: saved.familyMemberCount,
                incomePerMember: saved.incomePerMember,
                verificationStatus: saved.verificationStatus
            }))
            // await updateStudentProfile({
            //     ...formData,
            //     totalFamilyIncome: payload.totalFamilyIncome,
            //     familyMemberCount: payload.familyMemberCount
            // }, auth.token)
            setFinancialSuccessMessage('Finansijski podaci uspešno ažurirani!')
            setIsEditingFinancial(false)
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
                </div>

                {profileError && <p className={styles.error}>{profileError.message}</p>}
                {financialSubmitError && <p className={styles.error}>{financialSubmitError}</p>}
                {successMessage && <p className={styles.success}>{successMessage}</p>}

                {!isEditing ? (
                    <div className={styles.viewMode}>
                        <div className={styles.viewGrid}>
                            <ViewItem label="Ime" value={formData.name} />
                            <ViewItem label="Prezime" value={formData.surname} />
                            <ViewItem label="Broj indeksa" value={formData.studentIDNumber} />
                            <ViewItem label="Email" value={formData.email} />
                            <ViewItem label="Adresa" value={formData.homeAddress} />
                            <ViewItem label="Opština" value={formData.municipalityOfResidence} />
                            <ViewItem label="JMBG" value={formData.jmbg} />
                            <ViewItem label="Godine" value={formData.age} />
                            <ViewItem label="Pol" value={selectedGender} />
                            <ViewItem label="Godina upisa" value={formData.yearOfEnrollment} />
                            <ViewItem label="Nivo studija" value={selectedStudyLevel} />
                            <ViewItem label="Fakultet" value={formData.facultyName} />
                            <ViewItem label="ESPB" value={formData.espb} />
                            {isFreshman ? (
                                <ViewItem label="Prosečna ocena (srednja škola)" value={formData.highSchoolAverageGrade} />
                            ) : (
                                <ViewItem label="Prosečna ocena (fakultet)" value={formData.averageGrade} />
                            )}
                            <ViewItem label="Finansiranje" value={formData.isBudgetStudent ? 'Budžet' : 'Samofinansiranje'} />
                        </div>

                        <button type="button" className={styles.editBtn} onClick={() => setIsEditing(true)}>
                            Izmeni profil
                        </button>
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
                                <label htmlFor="municipalityOfResidence">Opština</label>
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
                                <label htmlFor="studentIDNumber">Broj indeksa</label>
                                <input
                                    id="studentIDNumber"
                                    name="studentIDNumber"
                                    value={formData.studentIDNumber}
                                    onChange={handleChange}
                                />
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
                                    <option value="">Izaberi fakultet</option>
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
                                    <label htmlFor="highSchoolAverageGrade">Prosečna ocena (srednja škola)</label>
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
                                    <label htmlFor="averageGrade">Prosečna ocena (fakultet)</label>
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
                            <span>Budžetski student</span>
                        </label>

                        <div className={styles.actions}>
                            <button type="submit" className={styles.btn}>Sačuvaj</button>
                            <button type="button" className={styles.cancelBtn} onClick={() => setIsEditing(false)}>
                                Odustani
                            </button>
                        </div>
                    </form>
                )}
            </div>

            <div className={styles.card}>
                <div className={styles.cardHeader}>
                    <h2>Finansijski podaci</h2>
                </div>

                {financialLoading && <p className={styles.muted}>Učitavanje finansija...</p>}
                {financialSubmitError && <p className={styles.error}>{financialSubmitError}</p>}
                {financialSuccessMessage && <p className={styles.success}>{financialSuccessMessage}</p>}

                {!isEditingFinancial ? (
                    <div className={styles.viewMode}>
                        <div className={styles.viewGrid}>
                            <ViewItem label="Ukupan prihod porodice" value={financialData.totalFamilyIncome ? `${financialData.totalFamilyIncome} RSD` : '-'} />
                            <ViewItem label="Broj članova porodice" value={financialData.familyMemberCount || '-'} />
                            {financialData.totalFamilyIncome && financialData.familyMemberCount && (
                                <ViewItem label="Prihod po članu" value={`${(financialData.totalFamilyIncome / financialData.familyMemberCount).toFixed(2)} RSD`} />
                            )}
                        </div>

                        <button type="button" className={styles.editBtn} onClick={() => setIsEditingFinancial(true)}>
                            Izmeni finansijske podatke
                        </button>
                    </div>
                ) : (
                    <form onSubmit={handleFinancialSubmit} className={styles.form}>
                        <div className={styles.field}>
                            <label htmlFor="totalFamilyIncome">Ukupan prihod porodice (RSD)</label>
                            <input
                                id="totalFamilyIncome"
                                name="totalFamilyIncome"
                                value={financialData.totalFamilyIncome}
                                onChange={handleFinancialChange}
                                type="number"
                                min="0"
                            />
                        </div>

                        <div className={styles.field}>
                            <label htmlFor="familyMemberCount">Broj članova porodice</label>
                            <input
                                id="familyMemberCount"
                                name="familyMemberCount"
                                value={financialData.familyMemberCount}
                                onChange={handleFinancialChange}
                                type="number"
                                min="1"
                            />
                        </div>

                        <div className={styles.actions}>
                            <button type="submit" className={styles.btn}>Sačuvaj</button>
                            <button type="button" className={styles.cancelBtn} onClick={() => setIsEditingFinancial(false)}>
                                Odustani
                            </button>
                        </div>
                    </form>
                )}
            </div>

            {/* <div className={styles.card}>
                <div className={styles.cardHeader}>
                    <h2>Finansijski podaci</h2>
                </div>

                {financialError && <p className={styles.error}>{financialError}</p>}
                {financialSuccessMessage && <p className={styles.success}>{financialSuccessMessage}</p>}

                {!isEditingFinancial ? (
                    <div className={styles.viewMode}>
                        <div className={styles.viewGrid}>
                            <ViewItem label="Ukupan prihod porodice" value={formData.totalFamilyIncome ? `${formData.totalFamilyIncome} RSD` : '-'} />
                            <ViewItem label="Broj članova porodice" value={formData.familyMemberCount || '-'} />
                            {formData.totalFamilyIncome && formData.familyMemberCount && (
                                <ViewItem label="Prihod po članu" value={`${(formData.totalFamilyIncome / formData.familyMemberCount).toFixed(2)} RSD`} />
                            )}
                        </div>

                        <button type="button" className={styles.editBtn} onClick={() => setIsEditingFinancial(true)}>
                            Izmeni finansijske podatke
                        </button>
                    </div>
                ) : (
                    <form onSubmit={handleFinancialSubmit} className={styles.form}>
                        <div className={styles.field}>
                            <label htmlFor="totalFamilyIncome">Ukupan prihod porodice (RSD)</label>
                            <input
                                id="totalFamilyIncome"
                                name="totalFamilyIncome"
                                value={formData.totalFamilyIncome}
                                onChange={handleChange}
                                type="number"
                                min="0"
                            />
                        </div>

                        <div className={styles.field}>
                            <label htmlFor="familyMemberCount">Broj članova porodice</label>
                            <input
                                id="familyMemberCount"
                                name="familyMemberCount"
                                value={formData.familyMemberCount}
                                onChange={handleChange}
                                type="number"
                                min="1"
                            />
                        </div>

                        <div className={styles.actions}>
                            <button type="submit" className={styles.btn}>Sačuvaj</button>
                            <button type="button" className={styles.cancelBtn} onClick={() => setIsEditingFinancial(false)}>
                                Odustani
                            </button>
                        </div>
                    </form>
                )}
            </div> */}
        </div>
    )
}

export default StudentProfile