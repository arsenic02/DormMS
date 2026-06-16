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
            <Link to="/" className={styles.logo}>
                Studentski dom Niš
            </Link>

            {!auth && (
                <ul className={styles.navLinks}>
                    <li><Link to="/">Početna</Link></li>
                    <li><Link to="/news">Vesti</Link></li>
                    <li><Link to="/info">Informacije</Link></li>
                    <li><Link to="/competitions">Konkursi</Link></li>
                </ul>
            )}

            {auth?.role === 'Student' && (
                <ul className={styles.navLinks}>
                    <li><Link to="/student">Početna</Link></li>
                    <li><Link to="/student/room">Soba</Link></li>
                    <li><Link to="/student/profile">Profil</Link></li>
                    <li><Link to="/student/notifications">Obaveštenja</Link></li>
                </ul>
            )}

            {auth?.role === 'Admin' && (
                <ul className={styles.navLinks}>
                    <li><Link to="/admin">Početna</Link></li>
                    <li><Link to="/admin/students">Studenti</Link></li>
                    <li><Link to="/admin/rooms">Sobe</Link></li>
                    <li><Link to="/admin/competitions">Konkursi</Link></li>
                </ul>
            )}

            {auth?.role === 'DormManager' && (
                <ul className={styles.navLinks}>
                    <li><Link to="/dorm-manager">Početna</Link></li>
                    <li><Link to="/dorm-manager/rooms">Sobe</Link></li>
                    <li><Link to="/dorm-manager/competitions">Konkursi</Link></li>
                </ul>
            )}

            {auth ? (
                <button onClick={handleLogout} className={styles.loginBtn}>
                    Odjavi se
                </button>
            ) : (
                <Link to="/login" className={styles.loginBtn}>
                    Prijavi se
                </Link>
            )}
        </nav>
    )
}

export default Navbar