import { Link } from "react-router-dom";
import "./FlatOffer.css";
import { FlatOfferProps } from "./FlatOfferProps";
import rentHouse from "../../assets/rent_house.png";
import { useListingThumbnail } from "../../hooks/useListingThumbnail.ts";

const FlatOffer = ({
    vertical = true,
    listingId,
    description,
    mail,
    location,
    price,
    area,
    currency,
    title
}: FlatOfferProps) => {
    const { imageUrl, isLoading } = useListingThumbnail(listingId);

    return (
        <div className={`offer-card ${vertical ? "vertical" : "horizontal"}`}>
            <div className="offer-image-wrapper">
                <img 
                    src={isLoading ? rentHouse : (imageUrl || rentHouse)} 
                    alt={title} 
                    className={`offer-image ${isLoading ? "loading-pulse" : ""}`} 
                />
                <div className="offer-price">
                    {price} {currency}
                </div>
            </div>

            <div className="offer-content">
                <h2 className="offer-title">{title}</h2>

                <div className="offer-mini-info">
                    <p className="offer-location">
                        📍 {location.street}, {location.aptNumber}, {location.district}, {location.city}
                    </p>

                    <p className="offer-area">
                        📐 {area} m<sup>2</sup>
                    </p>
                </div>

                <p className="offer-description">{description}</p>

                <div className="offer-footer">
                    <span className="offer-phone">📧 {mail}</span>
                    <Link className="contact-button" to={`/offer/${listingId}`}>
                        Więcej
                    </Link>
                </div>
            </div>
        </div>
    );
};

export default FlatOffer;
