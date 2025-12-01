import { Routes, Route } from 'react-router-dom'
import { AppLayout } from './components/AppLayout'
import { HomePage } from './pages/Home'
import { BuyersPage } from './pages/Buyers'
import { BrokersPage } from './pages/Brokers'
import { AdminPage } from './pages/Admin'

const App = () => {
  return (
    <AppLayout>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/pirkejams" element={<BuyersPage />} />
        <Route path="/brokeriams" element={<BrokersPage />} />
        <Route path="/administratoriams" element={<AdminPage />} />
      </Routes>
    </AppLayout>
  )
}

export default App
