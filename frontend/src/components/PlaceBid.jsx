import React, { useState } from 'react';
import { motion } from 'framer-motion';
import axios from 'axios';
import { useAuth } from '../context/AuthProvider';

const PlaceBid = ({ auctionId, currentPrice, onBidPlaced }) => {
    const { user } = useAuth();
    const [bidAmount, setBidAmount] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setSuccess('');
        setLoading(true);

        try {
            const token = user?.token || localStorage.getItem('token');
            const amount = parseInt(bidAmount);

            if (amount <= currentPrice) {
                setError(`Bid must be higher than current price of $${currentPrice.toLocaleString()}`);
                setLoading(false);
                return;
            }

            await axios.post(
                'http://localhost:5058/api/bids',
                {
                    auctionId,
                    amount
                },
                {
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    }
                }
            );

            setSuccess('Bid placed successfully!');
            setBidAmount('');
            if (onBidPlaced) onBidPlaced();

            setTimeout(() => setSuccess(''), 3000);
        } catch (err) {
            setError(err.response?.data?.title || err.response?.data || 'Failed to place bid');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    return (
        <motion.div
            className="glassmorphism"
            style={{ padding: '2rem', borderRadius: '1.5rem' }}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
        >
            <h3 style={{ marginTop: 0, marginBottom: '1.5rem', fontSize: '1.5rem' }}>Place Your Bid</h3>

            {error && <div className="error-message">{error}</div>}
            {success && <div className="success-message">{success}</div>}

            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <label>Bid Amount ($)</label>
                    <input
                        type="number"
                        value={bidAmount}
                        onChange={(e) => setBidAmount(e.target.value)}
                        required
                        min={currentPrice + 1}
                        placeholder={`Minimum: $${(currentPrice + 1).toLocaleString()}`}
                        disabled={loading}
                    />
                    <p style={{ fontSize: '0.875rem', color: 'var(--text-tertiary)', marginTop: '0.5rem' }}>
                        Current price: ${currentPrice.toLocaleString()}
                    </p>
                </div>

                <motion.button
                    type="submit"
                    className="btn-primary"
                    disabled={loading}
                    whileHover={{ scale: 1.02 }}
                    whileTap={{ scale: 0.98 }}
                    style={{ width: '100%' }}
                >
                    {loading ? 'Placing Bid...' : 'Place Bid'}
                </motion.button>
            </form>
        </motion.div>
    );
};

export default PlaceBid;
