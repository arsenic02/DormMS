import { useState, useEffect } from 'react'
import styles from './DormManagerVerifications.module.css'
import { useAuth } from '../../../context/AuthContext'
import {
    getStudentsPaged,
    verifyStudentProfile,
    verifyStudentFinancialStatus
} from '../../../api/student'

const PROFILE_STATUS = {
    Incomplete: 0,
    Pending: 1,
    Verified: 2,
    Rejected: 3,
}

const FINANCIAL_STATUS = {
    Pending: 0,
    Verified: 1,
    Rejected: 2,
}

const PROFILE_STATUS_LABELS = {
    0: 'Nije kompletan',
    1: 'Na čekanju',
    2: 'Verifikovan',
    3: 'Odbijen',
}

const FINANCIAL_STATUS_LABELS = {
    0: 'Na čekanju',
    1: 'Verifikovan',
    2: 'Odbijen',
}

function DormManagerVerifications() {
    const { auth } = useAuth()

    const [students, setStudents] = useState([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    const [currentPage, setCurrentPage] = useState(1)
    const [pageSize] = useState(10)
    const [totalCount, setTotalCount] = useState(0)

    const [filterProfileStatus, setFilterProfileStatus] = useState('1') // default: na čekanju
    const [filterFinancialStatus, setFilterFinancialStatus] = useState('0') // default: na čekanju

    const [selectedStudent, setSelectedStudent] = useState(null)
    const [modalType, setModalType] = useState(null)
    const [actionLoading, setActionLoading] = useState(false)
    const [actionError, setActionError] = useState(null)

    const fetchStudents = async (page = currentPage) => {
        setLoading(true)
        setError(null)
        try {
            const filters = {}
            if (filterProfileStatus !== '') {
                filters.profileStatus = parseInt(filterProfileStatus)
            }
            if (filterFinancialStatus !== '') {
                filters.financialStatus = parseInt(filterFinancialStatus)
            }

            const data = await getStudentsPaged(auth.token, page, pageSize, filters)

            setStudents(data.items || [])
            setTotalCount(data.totalCount || 0)
            setCurrentPage(page)
        } catch (err) {
            setError(err.message)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        fetchStudents(1)
    }, [filterProfileStatus, filterFinancialStatus])

    const openProfileModal = (student) => {
        setSelectedStudent(student)
        setModalType('profile')
        setActionError(null)
    }

    const openFinancialModal = (student) => {
        setSelectedStudent(student)
        setModalType('financial')
        setActionError(null)
    }

    const closeModal = () => {
        setSelectedStudent(null)
        setModalType(null)
        setActionError(null)
    }

    const handleProfileDecision = async (status) => {
        setActionLoading(true)
        setActionError(null)
        try {
            await verifyStudentProfile(selectedStudent.id, status, auth.token)
            await fetchStudents(currentPage)
            closeModal()
        } catch (err) {
            setActionError(err.message)
        } finally {
            setActionLoading(false)
        }
    }

    const handleFinancialDecision = async (status) => {
        setActionLoading(true)
        setActionError(null)
        try {
            await verifyStudentFinancialStatus(selectedStudent.id, status, auth.token)
            await fetchStudents(currentPage)
            closeModal()
        } catch (err) {
            setActionError(err.message)
        } finally {
            setActionLoading(false)
        }
    }

    const handlePageChange = (newPage) => {
        if (newPage >= 1 && newPage <= totalPages) {
            fetchStudents(newPage)
        }
    }

    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize))

    if (loading) return <p className={styles.info}>Učitavanje...</p>
    if (error) return <p className={styles.error}>{error}</p>

    return (
        <div className={styles.container}>
            <h1 className={styles.title}>Verifikacije studenata</h1>

            <div className={styles.filters}>
                <div className={styles.filterGroup}>
                    <label htmlFor="profileFilter">Status profila:</label>
                    <select
                        id="profileFilter"
                        className={styles.filterSelect}
                        value={filterProfileStatus}
                        onChange={(e) => setFilterProfileStatus(e.target.value)}
                    >
                        <option value="">Svi</option>
                        <option value="0">Nije kompletan</option>
                        <option value="1">Na čekanju</option>
                        <option value="2">Verifikovan</option>
                        <option value="3">Odbijen</option>
                    </select>
                </div>
                <div className={styles.filterGroup}>
                    <label htmlFor="financialFilter">Status finansijski:</label>
                    <select
                        id="financialFilter"
                        className={styles.filterSelect}
                        value={filterFinancialStatus}
                        onChange={(e) => setFilterFinancialStatus(e.target.value)}
                    >
                        <option value="">Svi</option>
                        <option value="0">Na čekanju</option>
                        <option value="1">Verifikovan</option>
                        <option value="2">Odbijen</option>
                    </select>
                </div>
            </div>

            {students.length === 0 ? (
                <p className={styles.info}>Nema studenata za prikaz.</p>
            ) : (
                <div className={styles.tableContainer}>
                    <table className={styles.table}>
                        <thead>
                            <tr>
                                <th>Ime i prezime</th>
                                <th>Broj indeksa</th>
                                <th>Fakultet</th>
                                <th>Status profila</th>
                                <th>Status finansijski</th>
                            </tr>
                        </thead>
                        <tbody>
                            {students.map((s) => (
                                <tr key={s.id}>
                                    <td>{s.name} {s.surname}</td>
                                    <td>{s.studentIDNumber}</td>
                                    <td>{s.facultyName ?? '-'}</td>
                                    <td>
                                        <div className={styles.statusCell}>
                                            <span className={`${styles.statusBadge} ${styles[`profileStatus${s.profileStatus}`]}`}>
                                                {PROFILE_STATUS_LABELS[s.profileStatus]}
                                            </span>
                                            {s.profileStatus === PROFILE_STATUS.Pending && (
                                                <button
                                                    className={styles.verifyBtn}
                                                    onClick={() => openProfileModal(s)}
                                                >
                                                    Verifikuj
                                                </button>
                                            )}
                                        </div>
                                    </td>
                                    <td>
                                        {s.financialStatus ? (
                                            <div className={styles.statusCell}>
                                                <span className={`${styles.statusBadge} ${styles[`financialStatus${s.financialStatus.verificationStatus}`]}`}>
                                                    {FINANCIAL_STATUS_LABELS[s.financialStatus.verificationStatus]}
                                                </span>
                                                {s.financialStatus.verificationStatus === FINANCIAL_STATUS.Pending && (
                                                    <button
                                                        className={styles.verifyBtn}
                                                        onClick={() => openFinancialModal(s)}
                                                    >
                                                        Verifikuj
                                                    </button>
                                                )}
                                            </div>
                                        ) : (
                                            <span className={styles.noData}>Nema podataka</span>
                                        )}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {totalPages > 1 && (
                <div className={styles.pagination}>
                    <button
                        className={styles.pageBtn}
                        disabled={currentPage === 1}
                        onClick={() => handlePageChange(currentPage - 1)}
                    >
                        Prethodna
                    </button>
                    <span className={styles.pageInfo}>
                        Strana {currentPage} od {totalPages} (ukupno {totalCount} studenata)
                    </span>
                    <button
                        className={styles.pageBtn}
                        disabled={currentPage === totalPages}
                        onClick={() => handlePageChange(currentPage + 1)}
                    >
                        Sledeća
                    </button>
                </div>
            )}

            {modalType === 'profile' && selectedStudent && (
                <div className={styles.modalOverlay} onClick={closeModal}>
                    <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
                        <div className={styles.modalHeader}>
                            <h2>Verifikacija profila</h2>
                            <button className={styles.closeBtn} onClick={closeModal}>×</button>
                        </div>
                        <div className={styles.modalContent}>
                            <h3>{selectedStudent.name} {selectedStudent.surname}</h3>
                            <p><strong>Broj indeksa:</strong> {selectedStudent.studentIDNumber}</p>
                            <p><strong>Email:</strong> {selectedStudent.email}</p>
                            <p><strong>JMBG:</strong> {selectedStudent.jmbg}</p>
                            <p><strong>Adresa:</strong> {selectedStudent.homeAddress}</p>
                            <p><strong>Opština:</strong> {selectedStudent.municipalityOfResidence}</p>
                            <p><strong>Godine:</strong> {selectedStudent.age}</p>
                            <p><strong>Pol:</strong> {selectedStudent.gender === 0 ? 'Muški' : 'Ženski'}</p>
                            <p><strong>Godina upisa:</strong> {selectedStudent.yearOfEnrollment}</p>
                            <p><strong>Fakultet:</strong> {selectedStudent.facultyName ?? '-'}</p>
                            <p><strong>ESPB:</strong> {selectedStudent.espb}</p>
                            {selectedStudent.isFreshman ? (
                                <p><strong>Prosečna ocena (srednja škola):</strong> {selectedStudent.highSchoolAverageGrade ?? '-'}</p>
                            ) : (
                                <p><strong>Prosečna ocena (fakultet):</strong> {selectedStudent.averageGrade ?? '-'}</p>
                            )}
                            <p><strong>Finansiranje:</strong> {selectedStudent.isBudgetStudent ? 'Budžet' : 'Samofinansiranje'}</p>

                            {actionError && <p className={styles.error}>{actionError}</p>}

                            <div className={styles.modalActions}>
                                <button
                                    className={styles.btnApprove}
                                    disabled={actionLoading}
                                    onClick={() => handleProfileDecision(PROFILE_STATUS.Verified)}
                                >
                                    {actionLoading ? 'Čuvanje...' : 'Verifikuj'}
                                </button>
                                <button
                                    className={styles.btnReject}
                                    disabled={actionLoading}
                                    onClick={() => handleProfileDecision(PROFILE_STATUS.Rejected)}
                                >
                                    Odbij
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {modalType === 'financial' && selectedStudent && selectedStudent.financialStatus && (
                <div className={styles.modalOverlay} onClick={closeModal}>
                    <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
                        <div className={styles.modalHeader}>
                            <h2>Verifikacija finansijskog statusa</h2>
                            <button className={styles.closeBtn} onClick={closeModal}>×</button>
                        </div>
                        <div className={styles.modalContent}>
                            <h3>{selectedStudent.name} {selectedStudent.surname}</h3>
                            <p><strong>Broj indeksa:</strong> {selectedStudent.studentIDNumber}</p>
                            <p><strong>Ukupni prihodi domaćinstva:</strong> {selectedStudent.financialStatus.totalFamilyIncome} RSD</p>
                            <p><strong>Broj članova porodice:</strong> {selectedStudent.financialStatus.familyMemberCount}</p>
                            <p><strong>Prihod po članu:</strong> {selectedStudent.financialStatus.incomePerMember} RSD</p>
                            <p><strong>Podneto:</strong> {new Date(selectedStudent.financialStatus.submittedAt).toLocaleDateString('sr-RS')}</p>

                            {actionError && <p className={styles.error}>{actionError}</p>}

                            <div className={styles.modalActions}>
                                <button
                                    className={styles.btnApprove}
                                    disabled={actionLoading}
                                    onClick={() => handleFinancialDecision(FINANCIAL_STATUS.Verified)}
                                >
                                    {actionLoading ? 'Čuvanje...' : 'Verifikuj'}
                                </button>
                                <button
                                    className={styles.btnReject}
                                    disabled={actionLoading}
                                    onClick={() => handleFinancialDecision(FINANCIAL_STATUS.Rejected)}
                                >
                                    Odbij
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    )
}

export default DormManagerVerifications