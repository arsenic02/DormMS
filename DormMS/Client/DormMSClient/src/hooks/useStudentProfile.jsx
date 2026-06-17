import { useEffect, useState } from 'react'
import { getStudentProfile } from '../api/student'

export function useStudentProfile(token) {
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

    const [profileStatus, setProfileStatus] = useState(null)
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    useEffect(() => {
        if (!token) return

        getStudentProfile(token)
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
            .catch(err => setError(err))
            .finally(() => setLoading(false))

    }, [token])

    return {
        formData,
        setFormData,
        profileStatus,
        loading,
        error
    }
}
