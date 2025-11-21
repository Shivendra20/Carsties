import React from 'react';
import { motion } from 'framer-motion';

const AuctionCard = ({ auction }) => {
    return (
        <motion.div
            className="auction-card"
            whileHover={{
                scale: 1.02,
                y: -5,
                boxShadow: "0 20px 25px -5px rgba(0, 0, 0, 0.2), 0 10px 10px -5px rgba(0, 0, 0, 0.1)"
            }}
            transition={{ type: "spring", stiffness: 300 }}
        >
            <div className="image-container">
                <img src={auction.imageUrl} alt={`${auction.make} ${auction.model}`} />
                <div className="status-badge">{auction.status}</div>
            </div>
            <div className="card-content">
                <h3>{auction.year} {auction.make} {auction.model}</h3>
                <div className="details">
                    <p className="color">Color: {auction.color}</p>
                    <p className="mileage">Mileage: {auction.mileage} mi</p>
                </div>
                <div className="price-section">
                    <span className="label">Current Price</span>
                    <span className="price">${auction.currentPrice || auction.reservePrice}</span>
                </div>
                <div className="time-remaining">
                    Ends: {new Date(auction.endedAt).toLocaleDateString()}
                </div>
            </div>
        </motion.div>
    );
};

export default AuctionCard;
