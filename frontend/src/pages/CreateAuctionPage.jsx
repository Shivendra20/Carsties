import React, { useState } from 'react';
import { motion } from 'framer-motion';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthProvider';

const CreateAuctionPage = () => {
    const navigate = useNavigate();
    const { user } = useAuth();
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    const [formData, setFormData] = useState({
        make: '',
        model: '',
        year: new Date().getFullYear(),
        color: '',
        mileage: 0,
        imageUrl: '',
        reservePrice: 0,
        endDate: ''
    });

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');
        setLoading(true);

        try {
            const token = user?.token || localStorage.getItem('token');

            const response = await axios.post(
                'http://localhost:5058/api/auctions',
                {
                    ...formData,
                    year: parseInt(formData.year),
                    mileage: parseInt(formData.mileage),
                    reservePrice: parseInt(formData.reservePrice),
                    endDate: new Date(formData.endDate).toISOString()
                },
                {
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    }
                }
            );

            setSuccess('Auction created successfully!');
            setTimeout(() => {
                navigate('/');
            }, 2000);
        } catch (err) {
            setError(err.response?.data?.title || err.response?.data || 'Failed to create auction');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    return (
        <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5 }}
            className="form-container"
        >
            <h2>Create New Auction</h2>

            {error && <div className="error-message">{error}</div>}
            {success && <div className="success-message">{success}</div>}

            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <label>Make *</label>
                    <input
                        type="text"
                        name="make"
                        value={formData.make}
                        onChange={handleChange}
                        required
                        placeholder="e.g., Ford, Toyota, BMW"
                    />
                </div>

                <div className="form-group">
                    <label>Model *</label>
                    <input
                        type="text"
                        name="model"
                        value={formData.model}
                        onChange={handleChange}
                        required
                        placeholder="e.g., Mustang, Camry, M3"
                    />
                </div>

                <div className="form-group">
                    <label>Year *</label>
                    <input
                        type="number"
                        name="year"
                        value={formData.year}
                        onChange={handleChange}
                        required
                        min="1900"
                        max={new Date().getFullYear() + 1}
                    />
                </div>

                <div className="form-group">
                    <label>Color *</label>
                    <input
                        type="text"
                        name="color"
                        value={formData.color}
                        onChange={handleChange}
                        required
                        placeholder="e.g., Red, Blue, Black"
                    />
                </div>

                <div className="form-group">
                    <label>Mileage *</label>
                    <input
                        type="number"
                        name="mileage"
                        value={formData.mileage}
                        onChange={handleChange}
                        required
                        min="0"
                        placeholder="Miles"
                    />
                </div>

                <div className="form-group">
                    <label>Image URL *</label>
                    <input
                        type="url"
                        name="imageUrl"
                        value={formData.imageUrl}
                        onChange={handleChange}
                        required
                        placeholder="https://example.com/car-image.jpg"
                    />
                </div>

                <div className="form-group">
                    <label>Reserve Price ($) *</label>
                    <input
                        type="number"
                        name="reservePrice"
                        value={formData.reservePrice}
                        onChange={handleChange}
                        required
                        min="1"
                        placeholder="Minimum price"
                    />
                </div>

                <div className="form-group">
                    <label>Auction End Date *</label>
                    <input
                        type="datetime-local"
                        name="endDate"
                        value={formData.endDate}
                        onChange={handleChange}
                        required
                        min={new Date().toISOString().slice(0, 16)}
                    />
                </div>

                <motion.button
                    type="submit"
                    className="btn-primary"
                    disabled={loading}
                    whileHover={{ scale: 1.02 }}
                    whileTap={{ scale: 0.98 }}
                    style={{ width: '100%' }}
                >
                    {loading ? 'Creating...' : 'Create Auction'}
                </motion.button>
            </form>
        </motion.div>
    );
};

export default CreateAuctionPage;
