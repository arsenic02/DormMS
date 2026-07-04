import { useState, useEffect } from 'react'
import { getAllMachines, createMachine, updateMachine, deleteMachine, toggleMachineStatus } from '../../../api/laundry'
import styles from './AdminLaundry.module.css'

function AdminLaundry() {
  const [machines, setMachines] = useState([])
  const [loading, setLoading] = useState(true)
  const [message, setMessage] = useState('')
  const [showModal, setShowModal] = useState(false)
  const [editingMachine, setEditingMachine] = useState(null)
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    dormId: 1
  })

  useEffect(() => {
    loadMachines()
  }, [])

  async function loadMachines() {
    try {
      const data = await getAllMachines()
      setMachines(data)
    } catch (error) {
      setMessage('Greška pri učitavanju mašina')
    } finally {
      setLoading(false)
    }
  }

  function handleOpenModal(machine = null) {
    if (machine) {
      setEditingMachine(machine)
      setFormData({
        name: machine.name,
        description: machine.description || '',
        dormId: machine.dormId
      })
    } else {
      setEditingMachine(null)
      setFormData({
        name: '',
        description: '',
        dormId: 1
      })
    }
    setShowModal(true)
  }

  async function handleSubmit(e) {
    e.preventDefault()
    try {
      if (editingMachine) {
        await updateMachine(editingMachine.id, formData)
        setMessage('Mašina uspešno ažurirana!')
      } else {
        await createMachine(formData)
        setMessage('Mašina uspešno kreirana!')
      }
      setShowModal(false)
      loadMachines()
    } catch (error) {
      setMessage('Greška pri čuvanju mašine')
    }
  }

  async function handleDelete(id) {
    if (window.confirm('Da li ste sigurni da želite da obrišete ovu mašinu?')) {
      try {
        await deleteMachine(id)
        setMessage('Mašina uspešno obrisana!')
        loadMachines()
      } catch (error) {
        setMessage('Greška pri brisanju mašine')
      }
    }
  }

  async function handleToggleStatus(id) {
    try {
      await toggleMachineStatus(id)
      loadMachines()
    } catch (error) {
      setMessage('Greška pri promeni statusa')
    }
  }

  if (loading) return <div className={styles.container}>Učitavanje...</div>

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h1>Upravljanje veš mašinama</h1>
        <button onClick={() => handleOpenModal()} className={styles.addBtn}>
          Dodaj mašinu
        </button>
      </div>

      {message && <div className={styles.message}>{message}</div>}

      <div className={styles.machinesGrid}>
        {machines.map(machine => (
          <div key={machine.id} className={styles.machineCard}>
            <div className={styles.machineInfo}>
              <h3>{machine.name}</h3>
              {machine.description && <p>{machine.description}</p>}
              <p className={styles.dorm}>{machine.dormName}</p>
              <span className={`${styles.status} ${machine.isActive ? styles.active : styles.inactive}`}>
                {machine.isActive ? 'Aktivna' : 'Neaktivna'}
              </span>
            </div>
            <div className={styles.actions}>
              <button onClick={() => handleToggleStatus(machine.id)} className={styles.toggleBtn}>
                {machine.isActive ? 'Deaktiviraj' : 'Aktiviraj'}
              </button>
              <button onClick={() => handleOpenModal(machine)} className={styles.editBtn}>
                Izmeni
              </button>
              <button onClick={() => handleDelete(machine.id)} className={styles.deleteBtn}>
                Obriši
              </button>
            </div>
          </div>
        ))}
      </div>

      {showModal && (
        <div className={styles.modal}>
          <div className={styles.modalContent}>
            <div className={styles.modalHeader}>
              <h2>{editingMachine ? 'Izmeni mašinu' : 'Dodaj mašinu'}</h2>
              <button onClick={() => setShowModal(false)} className={styles.closeBtn}>
                ×
              </button>
            </div>
            <form onSubmit={handleSubmit}>
              <div className={styles.formGroup}>
                <label>Naziv mašine:</label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  required
                />
              </div>
              <div className={styles.formGroup}>
                <label>Opis (opciono):</label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                />
              </div>
              <div className={styles.formGroup}>
                <label>ID doma:</label>
                <input
                  type="number"
                  value={formData.dormId}
                  onChange={(e) => setFormData({ ...formData, dormId: parseInt(e.target.value) })}
                  required
                />
              </div>
              <div className={styles.modalActions}>
                <button type="button" onClick={() => setShowModal(false)} className={styles.cancelBtn}>
                  Otkaži
                </button>
                <button type="submit" className={styles.saveBtn}>
                  Sačuvaj
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}

export default AdminLaundry
