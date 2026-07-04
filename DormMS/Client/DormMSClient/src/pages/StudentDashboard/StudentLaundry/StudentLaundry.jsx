import { useState, useEffect } from 'react'
import { getAllMachines, getAvailableSlots, createReservation, getStudentReservations, cancelReservation } from '../../../api/laundry'
import styles from './StudentLaundry.module.css'

function StudentLaundry() {
  const [machines, setMachines] = useState([])
  const [selectedMachine, setSelectedMachine] = useState(null)
  const [selectedDate, setSelectedDate] = useState(new Date())
  const [slots, setSlots] = useState([])
  const [reservations, setReservations] = useState([])
  const [loading, setLoading] = useState(true)
  const [message, setMessage] = useState('')
  const [messageType, setMessageType] = useState('') // 'success' or 'error'

  // Auto-skrivanje poruka nakon 3 sekundi
  useEffect(() => {
    if (message) {
      const timer = setTimeout(() => {
        setMessage('')
        setMessageType('')
      }, 3000)
      return () => clearTimeout(timer)
    }
  }, [message])

  useEffect(() => {
    loadMachines()
    loadReservations()
  }, [])

  useEffect(() => {
    if (selectedMachine) {
      loadSlots(selectedMachine.id, selectedDate)
    }
  }, [selectedMachine, selectedDate])

  // Formatiranje statusa rezervacije u prikazljiv string
  const formatReservationStatus = (status) => {
    const statusMap = {
      'Upcoming': 'Predstojeća',
      'Active': 'Aktivna',
      'Completed': 'Završena',
      'Cancelled': 'Otkazana'
    }
    return statusMap[status] || status
  }

  // Pomoćna funkcija za postavljanje poruka
  const showMessage = (text, type) => {
    setMessage(text)
    setMessageType(type)
  }

  async function loadMachines() {
    try {
      const data = await getAllMachines()
      setMachines(data.filter(m => m.isActive))
      if (data.filter(m => m.isActive).length > 0) {
        setSelectedMachine(data.filter(m => m.isActive)[0])
      }
    } catch (error) {
      showMessage('Greška pri učitavanju mašina', 'error')
    } finally {
      setLoading(false)
    }
  }

  // Konvertuje datum iz backend formata (YYYY-MM-DDTHH:mm:ss) u lokalni Date
  const parseBackendDate = (dateString) => {
    if (!dateString) return null
    // Ako je format YYYY-MM-DDTHH:mm:ss, parsiramo ga kao lokalni datum
    const [datePart, timePart] = dateString.split('T')
    if (!datePart || !timePart) return new Date(dateString)
    const [year, month, day] = datePart.split('-').map(Number)
    const [hours, minutes, seconds] = timePart.split(':').map(Number)
    return new Date(year, month - 1, day, hours, minutes, seconds)
  }

  async function loadSlots(machineId, date) {
    try {
      const data = await getAvailableSlots(machineId, date)
      // Konvertujemo sve datume u slotovima u lokalne
      const processedSlots = data.map(slot => ({
        ...slot,
        startTime: parseBackendDate(slot.startTime),
        endTime: parseBackendDate(slot.endTime),
        reservation: slot.reservation ? {
          ...slot.reservation,
          startTime: parseBackendDate(slot.reservation.startTime),
          endTime: parseBackendDate(slot.reservation.endTime)
        } : null
      }))
      setSlots(processedSlots)
    } catch (error) {
      showMessage('Greška pri učitavanju termina', 'error')
    }
  }

  async function loadReservations() {
    try {
      const data = await getStudentReservations()
      // Konvertujemo datume rezervacija u lokalne
      const processedReservations = data.map(res => ({
        ...res,
        startTime: parseBackendDate(res.startTime),
        endTime: parseBackendDate(res.endTime)
      }))
      setReservations(processedReservations)
    } catch (error) {
      console.error('Greška pri učitavanju rezervacija:', error)
    }
  }

  async function handleReserve(slot) {
    if (!selectedMachine) return

    try {
      await createReservation({
        laundryMachineId: selectedMachine.id,
        startTime: slot.startTime
      })
      showMessage('Rezervacija uspešna!', 'success')
      loadSlots(selectedMachine.id, selectedDate)
      loadReservations()
    } catch (error) {
      showMessage(error.message || 'Greška pri rezervaciji', 'error')
    }
  }

  async function handleCancel(reservationId) {
    try {
      // Optimističko ažuriranje: odmah uklanjamo rezervaciju iz liste
      setReservations(prevReservations => 
        prevReservations.filter(r => r.id !== reservationId)
      )
      
      await cancelReservation(reservationId)
      showMessage('Rezervacija otkazana!', 'success')
      
      // Učitavamo ponovo podatke da potvrdimo stanje
      await loadReservations()
      if (selectedMachine) {
        await loadSlots(selectedMachine.id, selectedDate)
      }
    } catch (error) {
      // Ako dođe do greške, vraćamo podatke sa backend-a
      await loadReservations()
      showMessage(error.message || 'Greška pri otkazivanju rezervacije', 'error')
    }
  }

  const formatTime = (dateString) => {
    const date = new Date(dateString)
    return date.toLocaleTimeString('sr-RS', { hour: '2-digit', minute: '2-digit' })
  }

  const formatDate = (dateString) => {
    const date = new Date(dateString)
    return date.toLocaleDateString('sr-RS', { day: '2-digit', month: '2-digit', year: 'numeric' })
  }

  if (loading) return <div className={styles.container}>Učitavanje...</div>

  return (
    <div className={styles.container}>
      <h1>Veš mašine</h1>

      {message && (
        <div className={`${styles.message} ${messageType === 'success' ? styles.messageSuccess : styles.messageError}`}>
          {message}
        </div>
      )}

      <div className={styles.section}>
        <h2>Moje rezervacije</h2>
        <div className={styles.reservations}>
          {reservations.filter(r => r.status !== 'Cancelled').length === 0 ? (
            <p>Nemate rezervacija</p>
          ) : (
            reservations
              .filter(r => r.status !== 'Cancelled')
              .map(reservation => (
                <div key={reservation.id} className={styles.reservationCard}>
                  <div className={styles.reservationInfo}>
                    <h3>{reservation.laundryMachineName}</h3>
                    <p>{formatDate(reservation.startTime)} {formatTime(reservation.startTime)} - {formatTime(reservation.endTime)}</p>
                    <p className={styles.status}>{formatReservationStatus(reservation.status)}</p>
                  </div>
                  {reservation.startTime > new Date() && (
                    <button onClick={() => handleCancel(reservation.id)} className={styles.cancelBtn}>
                      Otkaži
                    </button>
                  )}
                </div>
              ))
          )}
        </div>
      </div>

      <div className={styles.section}>
        <h2>Rezerviši termin</h2>

        <div className={styles.machineSelect}>
          <label>Odaberi mašinu:</label>
          <select
            value={selectedMachine?.id || ''}
            onChange={(e) => setSelectedMachine(machines.find(m => m.id === parseInt(e.target.value)))}
          >
            <option value="">-- Izaberi mašinu --</option>
            {machines.map(machine => (
              <option key={machine.id} value={machine.id}>
                {machine.name} ({machine.dormName})
              </option>
            ))}
          </select>
        </div>

        {selectedMachine && (
          <>
            <div className={styles.dateSelect}>
              <label>Odaberi datum:</label>
              <input
                type="date"
                value={selectedDate.toISOString().split('T')[0]}
                onChange={(e) => {
                  const [year, month, day] = e.target.value.split('-').map(Number)
                  setSelectedDate(new Date(year, month - 1, day))
                }}
                min={new Date().toISOString().split('T')[0]}
              />
            </div>

            <div className={styles.slots}>
              <h3>Dostupni termini za {selectedMachine.name}</h3>
              <div className={styles.slotsGrid}>
                {slots.map((slot, index) => (
                  <div
                    key={index}
                    className={`${styles.slot} ${slot.isAvailable ? styles.available : styles.booked}`}
                  >
                    <div className={styles.slotTime}>
                      {formatTime(slot.startTime)} - {formatTime(slot.endTime)}
                    </div>
                    {slot.isAvailable ? (
                      <button onClick={() => handleReserve(slot)} className={styles.reserveBtn}>
                        Rezerviši
                      </button>
                    ) : (
                      <div className={styles.bookedInfo}>
                        <span>Zauzeto</span>
                        {slot.reservation && (
                          <small>{slot.reservation.studentName}</small>
                        )}
                      </div>
                    )}
                  </div>
                ))}
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  )
}

export default StudentLaundry
