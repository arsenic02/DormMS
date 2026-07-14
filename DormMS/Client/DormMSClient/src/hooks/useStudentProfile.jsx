import { useEffect, useState } from 'react'
import { getStudentProfile } from '../api/student'

export function useStudentProfile(token) {
    const [formData, setFormData] = useState({
        name: '',
        surname: '',
        studentIDNumber: '',
        email: '',
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
        highSchoolAverageGrade: '',
        isBudgetStudent: true,
        totalFamilyIncome: '',
        familyMemberCount: ''
    })

    const [profileStatus, setProfileStatus] = useState(null)
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    useEffect(() => {
        if (!token) return

        getStudentProfile(token)
            .then(data => {
                setFormData({
                    name: data.name || '',
                    surname: data.surname || '',
                    studentIDNumber: data.studentIDNumber || '',
                    email: data.email || '',
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
                    highSchoolAverageGrade: data.highSchoolAverageGrade || '',
                    isBudgetStudent: data.isBudgetStudent ?? true,
                    totalFamilyIncome: data.financialStatus?.totalFamilyIncome || '',
                    familyMemberCount: data.financialStatus?.familyMemberCount || ''
                })

                setProfileStatus(data.profileStatus)
            })
            .catch(err => setError(err))
            .finally(() => setLoading(false))

    }, [token])

    return {
        formData,
        setFormData,
        profileStatus,
        setProfileStatus,
        loading,
        error
    }
}