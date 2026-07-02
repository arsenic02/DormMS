import { useEffect, useState } from 'react'
import { getFinancialStatus } from '../api/student'

export function useFinancialStatus(token) {
    const [financialData, setFinancialData] = useState({
        totalFamilyIncome: '',
        familyMemberCount: '',
        incomePerMember: '',
        verificationStatus: 0
    })

    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    useEffect(() => {
        if (!token) return

        getFinancialStatus(token)
            .then(data => {
                const status = data || {}

                setFinancialData({
                    totalFamilyIncome: status.totalFamilyIncome || '',
                    familyMemberCount: status.familyMemberCount || '',
                    incomePerMember: status.incomePerMember || '',
                    verificationStatus: status.verificationStatus ?? 0
                })
            })
            .catch(err => setError(err))
            .finally(() => setLoading(false))

    }, [token])

    return {
        financialData,
        setFinancialData,
        loading,
        error
    }
}
