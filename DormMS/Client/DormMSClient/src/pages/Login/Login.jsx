import { useState } from 'react'
import { Link } from 'react-router-dom'
import { loginUser } from '../../api/auth'
import { useAuth } from '../../context/AuthContext'
import { useNavigate } from 'react-router-dom'
import styles from './Login.module.css'

function Login() {
    const [formData, setFormData] = useState({
        email: '',
        password: ''
    })
    const { login } = useAuth()
    const navigate = useNavigate()
    const [errors, setErrors] = useState({})

    const validate = () => {
        const newErrors = {}
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

        if (!formData.email) {
            newErrors.email = 'Email je obavezan'
        } else if (!emailRegex.test(formData.email)) {
            newErrors.email = 'Email nije u ispravnom formatu'
        }

        if (!formData.password) {
            newErrors.password = 'Lozinka je obavezna'
        }

        return newErrors
    }

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value })
        setErrors({ ...errors, [e.target.name]: '' })
    }

    const handleSubmit = async (e) => {
        e.preventDefault()
        const newErrors = validate()
        if (Object.keys(newErrors).length > 0) {
            setErrors(newErrors)
            return
        }

        console.log(formData)
        setErrors({})

        try {
            const data = await loginUser(formData.email, formData.password)
            console.log('Uspešna prijava:', data)
            login(data)
            navigate('/dashboard')
        } catch (err) {
            setErrors({ general: err.message })
        }
    }

    return (
        <div className={styles.container}>
            <div className={styles.card}>
                <h2>Prijava</h2>
                <form onSubmit={handleSubmit}>
                    <div className={styles.field}>
                        <label>Email</label>
                        <input
                            type="email"
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
                            placeholder="Unesite lozinku"
                            className={errors.password ? styles.inputError : ''}
                        />
                        {errors.password && <span className={styles.error}>{errors.password}</span>}
                        {errors.general && <p className={styles.error}>{errors.general}</p>}
                    </div>
                    <button type="submit" className={styles.btn}>Prijavi se</button>
                </form>
                <p className={styles.switch}>
                    Nemate nalog? <Link to="/register">Registrujte se</Link>
                </p>
            </div>
        </div>
    )
}

export default Login
