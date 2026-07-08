import { useState } from 'react'
import { Link } from 'react-router-dom'
import { registerUser } from '../../api/auth'
import styles from './Register.module.css'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'

function Register() {
    const [formData, setFormData] = useState({
        firstName: '',
        lastName: '',
        email: '',
        password: '',
        confirmPassword: '',
        indexNumber: ''
    })
    const { login } = useAuth()
    const navigate = useNavigate()
    const [errors, setErrors] = useState({})

    const nameRegex = /^[a-zA-ZšđčćžŠĐČĆŽ\s]+$/
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

    const validate = () => {
        const newErrors = {}

        if (!formData.firstName) {
            newErrors.firstName = 'Ime je obavezno'
        } else if (!nameRegex.test(formData.firstName)) {
            newErrors.firstName = 'Ime sme da sadrži samo slova'
        } else if (formData.firstName.length < 2) {
            newErrors.firstName = 'Ime mora imati bar 2 karaktera'
        }

        if (!formData.lastName) {
            newErrors.lastName = 'Prezime je obavezno'
        } else if (!nameRegex.test(formData.lastName)) {
            newErrors.lastName = 'Prezime sme da sadrži samo slova'
        } else if (formData.lastName.length < 2) {
            newErrors.lastName = 'Prezime mora imati bar 2 karaktera'
        }

        if (!formData.email) {
            newErrors.email = 'Email je obavezan'
        } else if (!emailRegex.test(formData.email)) {
            newErrors.email = 'Email nije u ispravnom formatu'
        }

        if (!formData.password) {
            newErrors.password = 'Lozinka je obavezna'
        } else if (formData.password.length < 6) {
            newErrors.password = 'Lozinka mora imati minimum 6 karaktera'
        }

        if (!formData.confirmPassword) {
            newErrors.confirmPassword = 'Potvrda lozinke je obavezna'
        } else if (formData.password !== formData.confirmPassword) {
            newErrors.confirmPassword = 'Lozinke se ne poklapaju'
        }

        if (!formData.indexNumber) {
            newErrors.indexNumber = 'Broj indeksa je obavezan'
        } else if (!/\d{4,}/.test(formData.indexNumber)) {
            newErrors.indexNumber = 'Broj indeksa mora sadržati minimum 4 cifre'
        }

        return newErrors
    }

    const capitalize = (value) => {
        if (!value) return value
        return value.charAt(0).toUpperCase() + value.slice(1)
    }

    const handleChange = (e) => {
        let value = e.target.value
        if (e.target.name === 'firstName' || e.target.name === 'lastName') {
            value = capitalize(value)
        }
        setFormData({ ...formData, [e.target.name]: value })
        setErrors({ ...errors, [e.target.name]: '' })
    }

    const handleSubmit = async(e) => {
        e.preventDefault()
        const newErrors = validate()
        if (Object.keys(newErrors).length > 0) {
            setErrors(newErrors)
            return
        }

        console.log(formData)
        setErrors({}) 

        try {
            const data = await registerUser(formData)
            console.log('Uspešna registracija:', data)
            login(data)
            //navigate('/student')
            navigate('/student/profile')
        } catch (err) {
            setErrors({ general: err.message })
        }
    }

    return (
        <div className={styles.container}>
            <div className={styles.card}>
                <h2>Registracija</h2>
                <form onSubmit={handleSubmit}>
                    <div className={styles.field}>
                        <label>Ime</label>
                        <input
                            type="text"
                            name="firstName"
                            value={formData.firstName}
                            onChange={handleChange}
                            placeholder="Unesite ime"
                            className={errors.firstName ? styles.inputError : ''}
                        />
                        {errors.firstName && <span className={styles.error}>{errors.firstName}</span>}
                    </div>
                    <div className={styles.field}>
                        <label>Prezime</label>
                        <input
                            type="text"
                            name="lastName"
                            value={formData.lastName}
                            onChange={handleChange}
                            placeholder="Unesite prezime"
                            className={errors.lastName ? styles.inputError : ''}
                        />
                        {errors.lastName && <span className={styles.error}>{errors.lastName}</span>}
                    </div>
                    <div className={styles.field}>
                        <label>Email</label>
                        <input
                            type="text"
                            name="email"
                            value={formData.email}
                            onChange={handleChange}
                            placeholder="Unesite email"
                            className={errors.email ? styles.inputError : ''}
                        />
                        {errors.email && <span className={styles.error}>{errors.email}</span>}
                    </div>
                    <div className={styles.field}>
                        <label>Lozinka</label>
                        <input
                            type="password"
                            name="password"
                            value={formData.password}
                            onChange={handleChange}
                            placeholder="Minimum 6 karaktera"
                            className={errors.password ? styles.inputError : ''}
                        />
                        {errors.password && <span className={styles.error}>{errors.password}</span>}
                    </div>
                    <div className={styles.field}>
                        <label>Potvrda lozinke</label>
                        <input
                            type="password"
                            name="confirmPassword"
                            value={formData.confirmPassword}
                            onChange={handleChange}
                            placeholder="Ponovite lozinku"
                            className={errors.confirmPassword ? styles.inputError : ''}
                        />
                        {errors.confirmPassword && <span className={styles.error}>{errors.confirmPassword}</span>}
                    </div>
                    <div className={styles.field}>
                        <label>Broj indeksa</label>
                        <input
                            type="text"
                            name="indexNumber"
                            value={formData.indexNumber}
                            onChange={handleChange}
                            placeholder="Unesite broj indeksa"
                            className={errors.indexNumber ? styles.inputError : ''}
                        />
                        {errors.indexNumber && <span className={styles.error}>{errors.indexNumber}</span>}
                        {errors.general && <p className={styles.error}>{errors.general}</p>}
                    </div>
                    <button type="submit" className={styles.btn}>Registruj se</button>
                </form>
                <p className={styles.switch}>
                    Već imate nalog? <Link to="/login">Prijavite se</Link>
                </p>
            </div>
        </div>
    )
}

export default Register
