import "./FlatOffer.css";
import { FlatOfferProps } from "./FlatOfferProps";

const FlatOffer = ({
    vertical = true,
    title,
    description,
    phone,
    location,
    price,
    currency,
    image
}: FlatOfferProps) => {
    return (
        <div className={`offer-card ${vertical ? "vertical" : "horizontal"}`}>
            <div className="offer-image-wrapper">
                <img src={image} alt={title} className="offer-image" />
                <div className="offer-price">
                    {price} {currency}
                </div>
            </div>

            <div className="offer-content">
                <h2 className="offer-title">{title}</h2>

                <p className="offer-location">
                    📍 {location.street}, {location.aptNumber}, {location.district}, {location.city}
                </p>

                <p className="offer-description">{description}</p>

                <div className="offer-footer">
                    <span className="offer-phone">📞 {phone}</span>
                    <button className="contact-button">Kontakt</button>
                </div>
            </div>
        </div>
    );
};

export default FlatOffer;