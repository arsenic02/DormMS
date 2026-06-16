import { BrowserRouter, Routes, Route } from 'react-router-dom'
import Navbar from './components/Navbar/Navbar'
import Home from './pages/Home/Home'
import Login from './pages/Login/Login'
import Register from './pages/Register/Register'
import StudentDashboard from './pages/StudentDashboard/StudentDashboard'
import AdminDashboard from './pages/AdminDashboard/AdminDashboard'
import DormManagerDashboard from './pages/DormManagerDashboard/DormManagerDashboard'
import StudentRoom from './pages/StudentDashboard/StudentRoom/StudentRoom'
import StudentProfile from './pages/StudentDashboard/StudentProfile/StudentProfile'
import StudentNotifications from './pages/StudentDashboard/StudentNotifications/StudentNotifications'
import ProtectedRoute from './components/ProtectedRoute/ProtectedRoute'
import DormManagerCompetition from './pages/DormManagerDashBoard/DormManagerCompetitions/DormManagerCompetitions'
function App() {
  return (
    <BrowserRouter>
      <Navbar />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

        <Route element={<ProtectedRoute allowedRoles={['Student']} />}>
          <Route path="/student" element={<StudentDashboard />} />
          <Route path="/student/room" element={<StudentRoom />} />
          <Route path="/student/profile" element={<StudentProfile />} />
          <Route path="/student/notifications" element={<StudentNotifications />} />
        </Route>

        <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
          <Route path="/admin" element={<AdminDashboard />} />
        </Route>

        <Route element={<ProtectedRoute allowedRoles={['DormManager']} />}>
          <Route path="/dorm-manager" element={<DormManagerDashboard />} />
          <Route path="/dorm-manager/competitions" element={<DormManagerCompetition />} />
        </Route>

      </Routes>
    </BrowserRouter>
  )
}

export default App