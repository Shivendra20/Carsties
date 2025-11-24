import React from 'react';
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import AuctionList from './components/AuctionList';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import CreateAuctionPage from './pages/CreateAuctionPage';
import AuctionDetailPage from './pages/AuctionDetailPage';
import { AuthProvider, useAuth } from './context/AuthProvider';
import { ThemeProvider } from './context/ThemeProvider';
import ThemeToggle from './components/ThemeToggle';
import ProtectedRoute from './components/ProtectedRoute';
import './index.css';

const Header = () => {
  const { user, logout } = useAuth();

  const userRole = user?.role || user?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
  const canCreateAuction = userRole === 'Auctioneer' || userRole === 'Both';

  return (
    <header className="app-header">
      <div className="header-content">
        <h1><Link to="/" style={{ textDecoration: 'none', color: 'inherit' }}>Carsties Auctions</Link></h1>
        <nav className="nav-links">
          {user ? (
            <>
              <span className="welcome-msg">Welcome, {user.unique_name || user.username}</span>
              {canCreateAuction && (
                <Link to="/create-auction" className="nav-link">Create Auction</Link>
              )}
              <button onClick={logout} className="btn-link">Logout</button>
            </>
          ) : (
            <>
              <Link to="/login" className="nav-link">Login</Link>
              <Link to="/register" className="nav-link">Register</Link>
            </>
          )}
          <ThemeToggle />
        </nav>
      </div>
    </header>
  );
};

function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <BrowserRouter>
          <div className="app">
            <Header />
            <main>
              <Routes>
                <Route path="/" element={<AuctionList />} />
                <Route path="/login" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />
                <Route path="/auction/:id" element={<AuctionDetailPage />} />
                <Route
                  path="/create-auction"
                  element={
                    <ProtectedRoute requiredRole={['Auctioneer', 'Both']}>
                      <CreateAuctionPage />
                    </ProtectedRoute>
                  }
                />
              </Routes>
            </main>
          </div>
        </BrowserRouter>
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;
