import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import styles from './Navbar.module.css'

function Navbar() {
    const { auth, logout } = useAuth()
    const navigate = useNavigate()

    const handleLogout = () => {
        logout()
        navigate('/')
    }

    return (
        <nav className={styles.navbar}>
            <div className={styles.logo}>
                Studentski dom Niš
            </div>
            <ul className={styles.navLinks}>
                <li><Link to="/">Početna</Link></li>
                <li><Link to="/news">Vesti</Link></li>
                <li><Link to="/informations">Informacije</Link></li>
                <li><Link to="/competitions">Konkursi</Link></li>
            </ul>
            {auth ? (
                <button onClick={handleLogout} className={styles.loginBtn}>
                    Odjavi se
                </button>
            ) : (
                <Link to="/login" className={styles.loginBtn}>
                    Prijavi se
                </Link>
            )}
            {/* <Link to="/login" className={styles.loginBtn}>
                Prijavi se
            </Link> */}
        </nav>
    )
}

export default Navbar