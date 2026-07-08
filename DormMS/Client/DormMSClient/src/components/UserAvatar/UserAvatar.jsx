import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import styles from './UserAvatar.module.css'

function getInitials(firstName, lastName) {
  const first = firstName?.charAt(0) ?? ''
  const last = lastName?.charAt(0) ?? ''
  return (first + last).toUpperCase()
}

function getRoleLabel(role) {
  switch (role) {
    case 'Admin': return 'Admin'
    case 'DormManager': return 'Referent'
    case 'Student': return 'Student'
    default: return ''
  }
}

function getProfilePath(role) {
  switch (role) {
    case 'Admin': return '/admin/profile'
    case 'DormManager': return '/dorm-manager/profile'
    case 'Student': return '/student/profile'
    default: return '/'
  }
}

export default function UserAvatar() {
  const { auth } = useAuth()
  const navigate = useNavigate()

  if (!auth) return null

  const initials = getInitials(auth.firstName, auth.lastName)
  const fullName = `${auth.firstName} ${auth.lastName}`.trim()
  const roleLabel = getRoleLabel(auth.role)

  return (
    <div
      className={styles.avatarWrapper}
      onClick={() => navigate(getProfilePath(auth.role))}
      title={`${fullName}${roleLabel ? ` (${roleLabel})` : ''}`}
    >
      <div className={styles.avatarCircle}>{initials}</div>
    </div>
  )
}