import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { motion } from 'framer-motion';
import AuctionCard from './AuctionCard';

const container = {
    hidden: { opacity: 0 },
    show: {
        opacity: 1,
        transition: {
            staggerChildren: 0.1
        }
    }
};

const item = {
    hidden: { opacity: 0, y: 20 },
    show: { opacity: 1, y: 0 }
};

const AuctionList = () => {
    const [auctions, setAuctions] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        axios.get('http://localhost:5058/api/auctions')
            .then(response => {
                setAuctions(response.data);
                setLoading(false);
            })
            .catch(err => {
                setError('Failed to fetch auctions');
                setLoading(false);
                console.error(err);
            });
    }, []);

    if (loading) return <div className="loading">Loading auctions...</div>;
    if (error) return <div className="error">{error}</div>;

    return (
        <motion.div
            className="auction-list"
            variants={container}
            initial="hidden"
            animate="show"
        >
            {auctions.map((auction, index) => (
                <AuctionCard key={auction.id} auction={auction} index={index} />
            ))}
        </motion.div>
    );
};

export default AuctionList;
