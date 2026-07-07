import { createContext, useContext, useState } from 'react'

const AuthContext = createContext()

export function AuthProvider({ children }) {
  const [auth, setAuth] = useState(() => {
    const token = localStorage.getItem('token')
    const role = localStorage.getItem('role')
    const firstName = localStorage.getItem('firstName')
    const lastName = localStorage.getItem('lastName')
    return token ? { token, role, firstName, lastName } : null
  })

  const login = (data) => {
    localStorage.setItem('token', data.token)
    localStorage.setItem('role', data.role)
    localStorage.setItem('firstName', data.firstName ?? '')
    localStorage.setItem('lastName', data.lastName ?? '')
    setAuth({
      token: data.token,
      role: data.role,
      firstName: data.firstName,
      lastName: data.lastName
    })
  }

  const logout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('role')
    localStorage.removeItem('firstName')
    localStorage.removeItem('lastName')
    setAuth(null)
  }

  return (
    <AuthContext.Provider value={{ auth, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  return useContext(AuthContext)
}

// import { createContext, useContext, useState } from 'react'

// const AuthContext = createContext()

// export function AuthProvider({ children }) {
//   const [auth, setAuth] = useState(() => {
//     const token = localStorage.getItem('token')
//     const role = localStorage.getItem('role')
//     return token ? { token, role } : null
//   })

//   const login = (data) => {
//     localStorage.setItem('token', data.token)
//     localStorage.setItem('role', data.role)
//     setAuth({ token: data.token, role: data.role })
//   }

//   const logout = () => {
//     localStorage.removeItem('token')
//     localStorage.removeItem('role')
//     setAuth(null)
//   }

//   return (
//     <AuthContext.Provider value={{ auth, login, logout }}>
//       {children}
//     </AuthContext.Provider>
//   )
// }

// export function useAuth() {
//   return useContext(AuthContext)
// }