import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import axios from 'axios';
import { useAuth } from '../context/AuthProvider';
import PlaceBid from '../components/PlaceBid';
import BidList from '../components/BidList';

const AuctionDetailPage = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const { user } = useAuth();
    const [auction, setAuction] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [refreshBids, setRefreshBids] = useState(0);

    useEffect(() => {
        const fetchAuction = async () => {
            try {
                const response = await axios.get(`http://localhost:5058/api/auctions/${id}`);
                setAuction(response.data);
                setLoading(false);
            } catch (err) {
                setError('Failed to load auction details');
                setLoading(false);
                console.error(err);
            }
        };

        fetchAuction();
    }, [id]);

    const handleBidPlaced = () => {
        setRefreshBids(prev => prev + 1);
        // Refresh auction data to get updated current price
        axios.get(`http://localhost:5058/api/auctions/${id}`)
            .then(response => setAuction(response.data))
            .catch(err => console.error(err));
    };

    if (loading) return <div className="loading">Loading auction...</div>;
    if (error) return <div className="error">{error}</div>;
    if (!auction) return <div className="error">Auction not found</div>;

    const isAuctionEnded = new Date(auction.endedAt) < new Date();
    const userRole = user?.role || user?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    const canBid = user && (userRole === 'Bidder' || userRole === 'Both') && !isAuctionEnded;

    return (
        <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ duration: 0.5 }}
            style={{ maxWidth: '1200px', margin: '0 auto' }}
        >
            <motion.button
                className="btn-secondary"
                onClick={() => navigate('/')}
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                style={{ marginBottom: '2rem' }}
            >
                ← Back to Auctions
            </motion.button>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem', marginBottom: '3rem' }}>
                {/* Auction Details */}
                <motion.div
                    className="glassmorphism"
                    style={{ padding: '2rem', borderRadius: '1.5rem' }}
                    initial={{ opacity: 0, x: -20 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: 0.2 }}
                >
                    <div style={{ marginBottom: '1.5rem' }}>
                        <img
                            src={auction.imageUrl}
                            alt={`${auction.make} ${auction.model}`}
                            style={{
                                width: '100%',
                                height: '300px',
                                objectFit: 'cover',
                                borderRadius: '1rem'
                            }}
                        />
                    </div>

                    <h1 className="gradient-text" style={{ fontSize: '2.5rem', marginBottom: '1rem' }}>
                        {auction.make} {auction.model}
                    </h1>

                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem', marginBottom: '1.5rem' }}>
                        <div>
                            <p style={{ color: 'var(--text-tertiary)', fontSize: '0.875rem', marginBottom: '0.25rem' }}>Year</p>
                            <p style={{ fontSize: '1.25rem', fontWeight: '700' }}>{auction.year}</p>
                        </div>
                        <div>
                            <p style={{ color: 'var(--text-tertiary)', fontSize: '0.875rem', marginBottom: '0.25rem' }}>Color</p>
                            <p style={{ fontSize: '1.25rem', fontWeight: '700' }}>{auction.color}</p>
                        </div>
                        <div>
                            <p style={{ color: 'var(--text-tertiary)', fontSize: '0.875rem', marginBottom: '0.25rem' }}>Mileage</p>
                            <p style={{ fontSize: '1.25rem', fontWeight: '700' }}>{auction.mileage.toLocaleString()} mi</p>
                        </div>
                        <div>
                            <p style={{ color: 'var(--text-tertiary)', fontSize: '0.875rem', marginBottom: '0.25rem' }}>Seller</p>
                            <p style={{ fontSize: '1.25rem', fontWeight: '700' }}>{auction.seller}</p>
                        </div>
                    </div>

                    <div style={{ padding: '1.5rem', background: 'var(--bg-secondary)', borderRadius: '1rem', marginBottom: '1rem' }}>
                        <p style={{ color: 'var(--text-tertiary)', fontSize: '0.875rem', marginBottom: '0.5rem' }}>Current Price</p>
                        <p className="gradient-text" style={{ fontSize: '3rem', fontWeight: '900' }}>
                            ${auction.currentPrice?.toLocaleString() || auction.reservePrice.toLocaleString()}
                        </p>
                    </div>

                    <div style={{ padding: '1rem', background: 'var(--bg-secondary)', borderRadius: '0.75rem' }}>
                        <p style={{ color: 'var(--text-tertiary)', fontSize: '0.875rem', marginBottom: '0.25rem' }}>Auction Ends</p>
                        <p style={{ fontSize: '1.125rem', fontWeight: '600' }}>
                            {new Date(auction.endedAt).toLocaleString()}
                        </p>
                        {isAuctionEnded && (
                            <p style={{ color: '#ef4444', fontWeight: '700', marginTop: '0.5rem' }}>AUCTION ENDED</p>
                        )}
                    </div>
                </motion.div>

                {/* Bidding Section */}
                <motion.div
                    initial={{ opacity: 0, x: 20 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: 0.3 }}
                >
                    {canBid && (
                        <div style={{ marginBottom: '2rem' }}>
                            <PlaceBid
                                auctionId={auction.id}
                                currentPrice={auction.currentPrice || auction.reservePrice}
                                onBidPlaced={handleBidPlaced}
                            />
                        </div>
                    )}

                    {!user && (
                        <div className="glassmorphism" style={{ padding: '2rem', borderRadius: '1.5rem', marginBottom: '2rem', textAlign: 'center' }}>
                            <p style={{ color: 'var(--text-secondary)', marginBottom: '1rem' }}>
                                Please log in to place a bid
                            </p>
                            <motion.button
                                className="btn-primary"
                                onClick={() => navigate('/login')}
                                whileHover={{ scale: 1.05 }}
                                whileTap={{ scale: 0.95 }}
                            >
                                Log In
                            </motion.button>
                        </div>
                    )}

                    <BidList auctionId={auction.id} refresh={refreshBids} />
                </motion.div>
            </div>
        </motion.div>
    );
};

export default AuctionDetailPage;
