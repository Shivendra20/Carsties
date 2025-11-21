import React from 'react';
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import AuctionList from './components/AuctionList';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import { AuthProvider, useAuth } from './context/AuthProvider';
import './index.css';

const Header = () => {
  const { user, logout } = useAuth();
  return (
    <header className="app-header">
      <div className="header-content">
        <h1><Link to="/" style={{ textDecoration: 'none', color: 'inherit' }}>Carsties Auctions</Link></h1>
        <nav className="nav-links">
          {user ? (
            <>
              <span className="welcome-msg">Welcome, {user.unique_name || user.username}</span>
              <button onClick={logout} className="btn-link">Logout</button>
            </>
          ) : (
            <>
              <Link to="/login" className="nav-link">Login</Link>
              <Link to="/register" className="nav-link">Register</Link>
            </>
          )}
        </nav>
      </div>
    </header>
  );
};

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <div className="app">
          <Header />
          <main>
            <Routes>
              <Route path="/" element={<AuctionList />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
            </Routes>
          </main>
        </div>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
