import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'

function ProtectedRoute({ allowedRoles }) {
  const { auth } = useAuth()

  if (!auth) {
    return <Navigate to="/login" />
  }

  if (allowedRoles && !allowedRoles.includes(auth.role)) {
    return <Navigate to="/" />
  }

  return <Outlet />
}

export default ProtectedRoute

// import { Navigate } from 'react-router-dom'
// import { useAuth } from '../../context/AuthContext'

// function ProtectedRoute({ children, allowedRoles }) {
//   const { auth } = useAuth()

//   if (!auth) {
//     return <Navigate to="/login" />
//   }

//   if (allowedRoles && !allowedRoles.includes(auth.role)) {
//     return <Navigate to="/" />
//   }

//   return children
// }

// export default ProtectedRoute