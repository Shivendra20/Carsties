import React from 'react';
import { motion, useInView } from 'framer-motion';
import { useNavigate } from 'react-router-dom';

const AuctionCard = ({ auction, index = 0 }) => {
    const navigate = useNavigate();
    const ref = React.useRef(null);
    const isInView = useInView(ref, { once: true, margin: "-100px" });

    return (
        <motion.div
            ref={ref}
            className="auction-card"
            initial={{ opacity: 0, y: 50 }}
            animate={isInView ? { opacity: 1, y: 0 } : { opacity: 0, y: 50 }}
            transition={{
                duration: 0.5,
                delay: index * 0.1,
                ease: [0.4, 0, 0.2, 1]
            }}
            whileHover={{
                y: -8,
                transition: { duration: 0.3 }
            }}
            onClick={() => navigate(`/auction/${auction.id}`)}
        >
            <div className="image-container">
                <img src={auction.imageUrl} alt={`${auction.make} ${auction.model}`} />
                <div className="status-badge">{auction.status}</div>
            </div>
            <div className="card-content">
                <h3>{auction.year} {auction.make} {auction.model}</h3>
                <div className="details">
                    <p className="color">Color: {auction.color}</p>
                    <p className="mileage">Mileage: {auction.mileage.toLocaleString()} mi</p>
                </div>
                <div className="price-section">
                    <span className="label">Current Price</span>
                    <span className="price">${(auction.currentPrice || auction.reservePrice).toLocaleString()}</span>
                </div>
                <div className="time-remaining">
                    Ends: {new Date(auction.endedAt).toLocaleDateString()}
                </div>
            </div>
        </motion.div>
    );
};

export default AuctionCard;
