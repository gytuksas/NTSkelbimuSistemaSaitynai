import { Routes, Route } from 'react-router-dom'
import { AppLayout } from './components/AppLayout'
import { HomePage } from './pages/Home'
import { ListingsPage } from './pages/Listings'
import { ViewingsPage } from './pages/Viewings'
import { ProfilePage } from './pages/Profile'
import { BrokersPage } from './pages/Brokers'
import { BrokerSchedulePage } from './pages/BrokerSchedule'
import { AdminPage } from './pages/Admin'
import { ListingDetailsPage } from './pages/ListingDetails'

const App = () => {
  return (
    <AppLayout>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/skelbimai" element={<ListingsPage />} />
        <Route path="/apziuros" element={<ViewingsPage />} />
        <Route path="/profilis" element={<ProfilePage />} />
        <Route path="/brokeriams" element={<BrokersPage />} />
  <Route path="/brokeriams/grafikas" element={<BrokerSchedulePage />} />
        <Route path="/administratoriams" element={<AdminPage />} />
        <Route path="/skelbimai/:id" element={<ListingDetailsPage />} />
      </Routes>
    </AppLayout>
  )
}

export default App
