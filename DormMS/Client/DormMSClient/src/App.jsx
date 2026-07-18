import { BrowserRouter, Routes, Route } from 'react-router-dom'
import Navbar from './components/Navbar/Navbar'
import Home from './pages/Home/Home'
import Login from './pages/Login/Login'
import Register from './pages/Register/Register'
import StudentRoom from './pages/StudentDashboard/StudentRoom/StudentRoom'
import StudentProfile from './pages/StudentDashboard/StudentProfile/StudentProfile'
import StudentNotifications from './pages/StudentDashboard/StudentNotifications/StudentNotifications'
import ProtectedRoute from './components/ProtectedRoute/ProtectedRoute'
import DormManagerCompetition from './pages/DormManagerDashBoard/DormManagerCompetitions/DormManagerCompetitions'
import StudentCompetitions from './pages/StudentDashboard/StudentCompetitions/StudentCompetitions'
import DormManagerRankingPage from './pages/DormManagerDashBoard/DormManagerRankings/DormManagerRankingPage/DormManagerRankingPage'
import DormManagerRankings from './pages/DormManagerDashBoard/DormManagerRankings/DormManagerRankings'
import DormManagerRanking from './components/Ranking/DormManagerRanking'
import DormManagerRooms from './pages/DormManagerDashBoard/DormManagerRooms/DormManagerRooms'
import DormManagerCompetitionApplications from './pages/DormManagerDashBoard/DormManagerCompetitionApplications/DormManagerCompetitionApplications'
import DormManagerRoomSwaps from './pages/DormManagerDashBoard/DormManagerRoomSwaps/DormManagerRoomSwaps'
import RoomSwapRequest from './pages/StudentDashboard/StudentRoom/RoomSwapRequest/RoomSwapRequest'
import MoveOutRequest from './pages/StudentDashboard/StudentRoom/MoveOutRequest/MoveOutRequest'
import DormManagerMoveOuts from './pages/DormManagerDashBoard/DormManagerMoveOuts/DormManagerMoveOuts'
import DormManagerNotifications from './pages/DormManagerDashBoard/DormManagerNotifications/DormManagerNotifications'
import AdminDorm from './pages/AdminDashBoard/AdminDorm/AdminDorm'
import AdminUsers from './pages/AdminDashBoard/AdminUsers/AdminUsers'
import AdminStudents from './pages/AdminDashBoard/AdminStudents/AdminStudents'
import AdminLaundry from './pages/AdminDashBoard/AdminLaundry/AdminLaundry'
import AdminProfile from './pages/AdminDashBoard/AdminProfile/AdminProfile'
import DormManagerProfile from './pages/DormManagerDashBoard/DormManagerProfile/DormManagerProfile'
import StudentLaundry from './pages/StudentDashboard/StudentLaundry/StudentLaundry'
import Information from './pages/Information/Information'
import Contact from './pages/Contact/Contact'
import ChatWidget from './components/ChatWidget/ChatWidget'


function App() {
  return (
    <BrowserRouter>
      <Navbar />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/info" element={<Information />} />
        <Route path="/contact" element={<Contact />} />

        <Route element={<ProtectedRoute allowedRoles={['Student']} />}>
          <Route path="/student/room" element={<StudentRoom />} />
          <Route path="/student/profile" element={<StudentProfile />} />
          <Route path="/student/notifications" element={<StudentNotifications />} />
          <Route path="/student/competitions" element={<StudentCompetitions />} />
          <Route path="/student/room-swap" element={<RoomSwapRequest />} />
          <Route path="/student/move-out" element={<MoveOutRequest />} />
          <Route path="/student/laundry" element={<StudentLaundry />} />
        </Route>

        <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
          <Route path="/admin/dorm" element={<AdminDorm />} />
          <Route path="/admin/users" element={<AdminUsers />} />
          <Route path="/admin/students" element={<AdminStudents />} />
          <Route path="/admin/laundry" element={<AdminLaundry />} />
          <Route path="/admin/profile" element={<AdminProfile />} />
        </Route>

        <Route element={<ProtectedRoute allowedRoles={['DormManager']} />}>
          <Route path="/dorm-manager/competitions" element={<DormManagerCompetition />} />
          <Route path="/dorm-manager/rankings" element={<DormManagerRankings />} />
          <Route path="/dorm-manager/rankings/:id" element={<DormManagerRankingPage />} />
          <Route path="/dorm-manager/rooms" element={<DormManagerRooms />} />
          <Route path="/dorm-manager/competitions/:competitionId/applications" element={<DormManagerCompetitionApplications />} />
          <Route path="/dorm-manager/notifications" element={<DormManagerNotifications />} />
          <Route path="/dorm-manager/room-swap-requests" element={<DormManagerRoomSwaps />} />
          <Route path="/dorm-manager/move-out-requests" element={<DormManagerMoveOuts />} />
          <Route path="/dorm-manager/profile" element={<DormManagerProfile />} />
        </Route>

      </Routes>
      <ChatWidget />
    </BrowserRouter>
  )
}

export default App