import "./FlatOffer.css";
import { FlatOfferProps } from "./FlatOfferProps";

const FlatOffer = ({
    vertical = true,
    title,
    description,
    phone,
    location,
    price,
    area, // TODO: add area info
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
                    <span className="offer-phone">📞 {phone}</span>
                    <button className="contact-button">Więcej</button>
                </div>
            </div>
        </div>
    );
};

export default FlatOffer;