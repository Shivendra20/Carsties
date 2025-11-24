import React, { useEffect, useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import axios from 'axios';

const BidList = ({ auctionId, refresh }) => {
    const [bids, setBids] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchBids = async () => {
            try {
                const response = await axios.get(`http://localhost:5058/api/bids/auction/${auctionId}`);
                setBids(response.data);
                setLoading(false);
            } catch (err) {
                // If no bids found, that's okay
                if (err.response?.status === 404) {
                    setBids([]);
                    setLoading(false);
                } else {
                    setError('Failed to load bids');
                    setLoading(false);
                }
                console.error(err);
            }
        };

        fetchBids();
    }, [auctionId, refresh]);

    if (loading) return <div className="loading" style={{ padding: '2rem' }}>Loading bids...</div>;

    return (
        <motion.div
            className="glassmorphism"
            style={{ padding: '2rem', borderRadius: '1.5rem' }}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
        >
            <h3 style={{ marginTop: 0, marginBottom: '1.5rem', fontSize: '1.5rem' }}>
                Bid History ({bids.length})
            </h3>

            {error && <div className="error-message">{error}</div>}

            {bids.length === 0 ? (
                <p style={{ color: 'var(--text-tertiary)', textAlign: 'center', padding: '2rem' }}>
                    No bids yet. Be the first to bid!
                </p>
            ) : (
                <div style={{ maxHeight: '500px', overflowY: 'auto' }}>
                    <AnimatePresence>
                        {bids.map((bid, index) => (
                            <motion.div
                                key={bid.id}
                                initial={{ opacity: 0, x: -20 }}
                                animate={{ opacity: 1, x: 0 }}
                                exit={{ opacity: 0, x: 20 }}
                                transition={{ delay: index * 0.05 }}
                                style={{
                                    padding: '1.25rem',
                                    background: index === 0 ? 'var(--gradient-primary)' : 'var(--bg-secondary)',
                                    borderRadius: '0.75rem',
                                    marginBottom: '1rem',
                                    border: index === 0 ? '2px solid var(--primary)' : '1px solid var(--border-color)',
                                    position: 'relative'
                                }}
                            >
                                {index === 0 && (
                                    <div style={{
                                        position: 'absolute',
                                        top: '-10px',
                                        right: '1rem',
                                        background: 'var(--gradient-accent)',
                                        color: 'white',
                                        padding: '0.25rem 0.75rem',
                                        borderRadius: '9999px',
                                        fontSize: '0.75rem',
                                        fontWeight: '700',
                                        textTransform: 'uppercase'
                                    }}>
                                        Highest Bid
                                    </div>
                                )}

                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                    <div>
                                        <p style={{
                                            fontWeight: '700',
                                            fontSize: '1.125rem',
                                            marginBottom: '0.25rem',
                                            color: index === 0 ? 'white' : 'var(--text-primary)'
                                        }}>
                                            {bid.bidder}
                                        </p>
                                        <p style={{
                                            fontSize: '0.875rem',
                                            color: index === 0 ? 'rgba(255,255,255,0.8)' : 'var(--text-tertiary)'
                                        }}>
                                            {new Date(bid.bidTime).toLocaleString()}
                                        </p>
                                    </div>
                                    <div style={{ textAlign: 'right' }}>
                                        <p style={{
                                            fontSize: '1.75rem',
                                            fontWeight: '900',
                                            color: index === 0 ? 'white' : 'var(--primary)'
                                        }}>
                                            ${bid.amount.toLocaleString()}
                                        </p>
                                    </div>
                                </div>
                            </motion.div>
                        ))}
                    </AnimatePresence>
                </div>
            )}
        </motion.div>
    );
};

export default BidList;
