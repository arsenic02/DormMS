import { useState, useEffect } from 'react'
import styles from './AdminDorm.module.css'
import { getDorms, createDorm, updateDorm, generateRooms, deleteAllRooms } from '../../../api/admin'
import { getRoomsPaged, getRoomDetails } from '../../../api/dormitoryRoom'
import { resetFlow } from '../../../api/ranking'
import Modal from '../../../components/Modal/Modal'
import DormInfoCard from '../../../components/DormInfoCard/DormInfoCard'
const genderLabel = {
    0: 'Muška',
    1: 'Ženska'
}

const statusLabel = {
    0: 'Slobodna',
    1: 'Popunjena',
    2: 'Održavanje',
    3: 'Zatvorena'
}

const statusClass = {
    0: 'available',
    1: 'full',
    2: 'maintenance',
    3: 'closed'
}

function AdminDorm() {
    const [dorm, setDorm] = useState(null)
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    const [dormForm, setDormForm] = useState({ name: '', location: '', municipality: '' })
    const [editingDorm, setEditingDorm] = useState(false)
    const [savingDorm, setSavingDorm] = useState(false)
    const [dormFormError, setDormFormError] = useState(null)

    const [roomsForm, setRoomsForm] = useState({ doubleRooms: '', tripleRooms: '', malePercentage: 50 })
    const [generating, setGenerating] = useState(false)
    const [roomsFormError, setRoomsFormError] = useState(null)
    const [roomsFormSuccess, setRoomsFormSuccess] = useState(null)

    const [selectedRoom, setSelectedRoom] = useState(null)
    const [modalOpen, setModalOpen] = useState(false)
    const [modalLoading, setModalLoading] = useState(false)
    const [modalError, setModalError] = useState(null)

    const [deleting, setDeleting] = useState(false)

    const [resettingFlow, setResettingFlow] = useState(false)

    const loadDorm = async () => {
        setLoading(true)
        setError(null)
        try {
            const dorms = await getDorms()
            if (dorms.length > 0) {
                setDorm(dorms[0])
                setDormForm({ name: dorms[0].name, location: dorms[0].location, municipality: dorms[0].municipality })
            } else {
                setDorm(null)
            }
        } catch (err) {
            setError(err.message)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        loadDorm()
    }, [])

    const handleDormFormChange = (e) => {
        setDormForm({ ...dormForm, [e.target.name]: e.target.value })
    }

    const handleCreateOrUpdateDorm = async (e) => {
        e.preventDefault()
        setDormFormError(null)

        if (!dormForm.name.trim()) {
            setDormFormError('Naziv doma je obavezan.')
            return
        }
        if (!dormForm.location.trim()) {
            setDormFormError('Lokacija je obavezna.')
            return
        }
        if (!dormForm.municipality.trim()) {
            setDormFormError('Opština je obavezna.')
            return
        }

        setSavingDorm(true)
        try {
            if (dorm) {
                await updateDorm(dorm.id, dormForm)
            } else {
                await createDorm(dormForm)
            }
            setEditingDorm(false)
            await loadDorm()
        } catch (err) {
            setDormFormError(err.message)
        } finally {
            setSavingDorm(false)
        }
    }

    const handleRoomsFormChange = (e) => {
        setRoomsForm({ ...roomsForm, [e.target.name]: e.target.value })
    }

    const handleGenerateRooms = async (e) => {
        e.preventDefault()
        setRoomsFormError(null)
        setRoomsFormSuccess(null)

        const doubleRooms = Number(roomsForm.doubleRooms) || 0
        const tripleRooms = Number(roomsForm.tripleRooms) || 0
        const malePercentage = Number(roomsForm.malePercentage)

        if (doubleRooms <= 0 && tripleRooms <= 0) {
            setRoomsFormError('Unesite bar jedan tip sobe za generisanje.')
            return
        }
        if (malePercentage < 0 || malePercentage > 100) {
            setRoomsFormError('Procenat muških soba mora biti između 0 i 100.')
            return
        }

        setGenerating(true)
        try {
            await generateRooms({
                dormId: dorm.id,
                doubleRooms,
                tripleRooms,
                malePercentage
            })
            setRoomsFormSuccess('Sobe su uspešno generisane.')
            setRoomsForm({ doubleRooms: '', tripleRooms: '', malePercentage: 50 })
            await loadDorm()
        } catch (err) {
            setRoomsFormError(err.message)
        } finally {
            setGenerating(false)
        }
    }

    const handleRoomClick = async (room) => {
        setModalOpen(true)
        setSelectedRoom(room)
        setModalLoading(true)
        setModalError(null)

        try {
            const fullRoom = await getRoomDetails(room.id)
            setSelectedRoom(fullRoom)
        } catch (err) {
            setModalError(err.message)
        } finally {
            setModalLoading(false)
        }
    }

    const closeModal = () => {
        setModalOpen(false)
        setSelectedRoom(null)
        setModalError(null)
    }

    const handleDeleteAllRooms = async () => {
        if (!window.confirm('Da li ste sigurni da želite da obrišete SVE sobe u ovom domu? Ova akcija se ne može opozvati.')) {
            return
        }

        setDeleting(true)
        try {
            await deleteAllRooms(dorm.id)
            await loadDorm()
        } catch (err) {
            setError(err.message)
        } finally {
            setDeleting(false)
        }
    }

    const handleResetFlow = async () => {
        if (!window.confirm('Da li ste sigurni da želite da iselite SVE studente iz svih soba? Ova akcija resetuje rangiranje i dodelu za sve studente.')) {
            return
        }

        setResettingFlow(true)
        try {
            await resetFlow()
            await loadDorm()
        } catch (err) {
            setError(err.message)
        } finally {
            setResettingFlow(false)
        }
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
                <p className={styles.error}>{error}</p>
            </div>
        )
    }

    return (
        <div className={styles.container}>
            <h1 className={styles.title}>Upravljanje domom</h1>

            {!dorm && !editingDorm && (
                <div className={styles.emptyCard}>
                    <p className={styles.emptyText}>Trenutno nema kreiranog doma.</p>
                    <button className={styles.btn}
                        onClick={() => {
                            setEditingDorm(false)
                            setDormForm({ name: dorm.name, location: dorm.location, municipality: dorm.municipality })
                            setDormFormError(null)
                        }}>

                        Kreiraj dom
                    </button>
                </div>
            )
            }

            {
                (editingDorm || (!dorm && editingDorm)) && (
                    <form onSubmit={handleCreateOrUpdateDorm} className={styles.form}>
                        <div className={styles.field}>
                            <label htmlFor="name">Naziv doma</label>
                            <input
                                id="name"
                                name="name"
                                type="text"
                                value={dormForm.name}
                                onChange={handleDormFormChange}
                                placeholder="Studentski dom Stefan Nemanja"
                            />
                        </div>
                        <div className={styles.field}>
                            <label htmlFor="location">Lokacija</label>
                            <input
                                id="location"
                                name="location"
                                type="text"
                                value={dormForm.location}
                                onChange={handleDormFormChange}
                                placeholder="Bulevar Nemanjića 67, Niš"
                            />
                        </div>
                        <div className={styles.field}>
                            <label htmlFor="municipality">Opština</label>
                            <input
                                id="municipality"
                                name="municipality"
                                type="text"
                                value={dormForm.municipality}
                                onChange={handleDormFormChange}
                                placeholder="Medijana"
                            />
                        </div>

                        {dormFormError && <p className={styles.error}>{dormFormError}</p>}

                        <div className={styles.formActions}>
                            <button type="submit" className={styles.btn} disabled={savingDorm}>
                                {savingDorm ? 'Čuvanje...' : dorm ? 'Sačuvaj izmene' : 'Kreiraj dom'}
                            </button>
                            {dorm && (
                                <button
                                    type="button"
                                    className={styles.cancelBtn}
                                    onClick={() => {
                                        setEditingDorm(false)
                                        setDormForm({ name: dorm.name, location: dorm.location })
                                        setDormFormError(null)
                                    }}
                                >
                                    Otkaži
                                </button>
                            )}
                        </div>
                    </form>
                )
            }

            {
                dorm && !editingDorm && (
                    <>
                        <DormInfoCard
                            dorm={dorm}
                            editable={true}
                            onEdit={() => setEditingDorm(true)}
                        />

                        <h2 className={styles.subtitle}>Generiši sobe</h2>
                        <form onSubmit={handleGenerateRooms} className={styles.form}>
                            <div className={styles.row}>
                                <div className={styles.field}>
                                    <label htmlFor="doubleRooms">Broj dvokrevetnih soba</label>
                                    <input
                                        id="doubleRooms"
                                        name="doubleRooms"
                                        type="number"
                                        min="0"
                                        value={roomsForm.doubleRooms}
                                        onChange={handleRoomsFormChange}
                                        placeholder="0"
                                    />
                                </div>
                                <div className={styles.field}>
                                    <label htmlFor="tripleRooms">Broj trokrevetnih soba</label>
                                    <input
                                        id="tripleRooms"
                                        name="tripleRooms"
                                        type="number"
                                        min="0"
                                        value={roomsForm.tripleRooms}
                                        onChange={handleRoomsFormChange}
                                        placeholder="0"
                                    />
                                </div>
                            </div>
                            <div className={styles.field}>
                                <label htmlFor="malePercentage">Procenat muških soba: {roomsForm.malePercentage}%</label>
                                <input
                                    id="malePercentage"
                                    name="malePercentage"
                                    type="range"
                                    min="0"
                                    max="100"
                                    value={roomsForm.malePercentage}
                                    onChange={handleRoomsFormChange}
                                />
                            </div>

                            {roomsFormError && <p className={styles.error}>{roomsFormError}</p>}
                            {roomsFormSuccess && <p className={styles.success}>{roomsFormSuccess}</p>}

                            <div className={styles.formActions}>
                                <button type="submit" className={styles.btn} disabled={generating}>
                                    {generating ? 'Generisanje...' : 'Generiši sobe'}
                                </button>
                                <button
                                    type="button"
                                    className={styles.deleteBtn}
                                    onClick={handleDeleteAllRooms}
                                    disabled={deleting || dorm.rooms.length === 0}
                                >
                                    {deleting ? 'Brisanje...' : 'Obriši sve sobe'}
                                </button>
                                <button
                                    type="button"
                                    className={styles.deleteBtn}
                                    onClick={handleResetFlow}
                                    disabled={resettingFlow}
                                >
                                    {resettingFlow ? 'Iseljavanje...' : 'Iseli sve studente'}
                                </button>
                            </div>
                        </form>

                        <h2 className={styles.subtitle}>Sobe ({dorm.rooms.length})</h2>
                        {dorm.rooms.length === 0 ? (
                            <p className={styles.info}>Nema generisanih soba.</p>
                        ) : (
                            <div className={styles.roomsList}>
                                {[...dorm.rooms].sort((a, b) => a.roomNumber - b.roomNumber).map((room) => (
                                    <div
                                        key={room.id}
                                        className={styles.roomCard}
                                        onClick={() => handleRoomClick(room)}
                                        style={{ cursor: 'pointer' }}
                                    >
                                        <div className={styles.roomCardHeader}>
                                            <span className={styles.roomNumber}>Soba {room.roomNumber}</span>
                                            <span className={`${styles.badge} ${styles[statusClass[room.status]]}`}>
                                                {statusLabel[room.status]}
                                            </span>
                                        </div>
                                        <div className={styles.roomMeta}>
                                            <span>{genderLabel[room.gender]}</span>
                                            <span>{room.currentOccupancy}/{room.capacity} mesta</span>
                                        </div>
                                    </div>
                                ))}

                                <Modal
                                    title={`Soba ${selectedRoom?.roomNumber}`}
                                    isOpen={modalOpen}
                                    onClose={closeModal}
                                >
                                    {modalLoading && <p className={styles.info}>Učitavanje...</p>}
                                    {modalError && <p className={styles.error}>{modalError}</p>}

                                    {!modalLoading && !modalError && selectedRoom && (
                                        <>
                                            <div className={styles.modalMeta}>
                                                <span>{genderLabel[selectedRoom.gender]}</span>
                                                <span>{selectedRoom.currentOccupancy}/{selectedRoom.capacity} mesta</span>
                                                <span className={`${styles.badge} ${styles[statusClass[selectedRoom.status]]}`}>
                                                    {statusLabel[selectedRoom.status]}
                                                </span>
                                            </div>

                                            <div className={styles.modalStudents}>
                                                <p className={styles.studentsLabel}>Studenti</p>
                                                {(!selectedRoom.students || selectedRoom.students.length === 0) ? (
                                                    <p className={styles.info}>Soba je trenutno prazna.</p>
                                                ) : (
                                                    selectedRoom.students.map((student) => (
                                                        <div key={student.id} className={styles.studentRow}>
                                                            <span className={styles.studentName}>
                                                                {student.name} {student.surname}
                                                                {student.isFreshman && (
                                                                    <span className={styles.freshmanTag}>brucoš</span>
                                                                )}
                                                            </span>
                                                            <span className={styles.studentFaculty}>{student.facultyName}</span>
                                                        </div>
                                                    ))
                                                )}
                                            </div>
                                        </>
                                    )}
                                </Modal>
                            </div>
                        )}
                    </>
                )
            }
        </div >
    )
}

export default AdminDorm