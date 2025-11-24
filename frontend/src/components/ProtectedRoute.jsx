import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthProvider';

const ProtectedRoute = ({ children, requiredRole }) => {
    const { user, loading } = useAuth();

    if (loading) {
        return <div className="loading">Loading...</div>;
    }

    if (!user) {
        return <Navigate to="/login" replace />;
    }

    if (requiredRole) {
        const userRole = user.role || user['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

        if (Array.isArray(requiredRole)) {
            if (!requiredRole.includes(userRole)) {
                return <div className="error">Access denied. Required role: {requiredRole.join(' or ')}</div>;
            }
        } else {
            if (userRole !== requiredRole) {
                return <div className="error">Access denied. Required role: {requiredRole}</div>;
            }
        }
    }

    return children;
};

export default ProtectedRoute;
