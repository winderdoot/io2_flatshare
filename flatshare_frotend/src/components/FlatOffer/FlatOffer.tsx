import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import "./FlatOffer.css";
import { FlatOfferProps } from "./FlatOfferProps";
import rentHouse from "../../assets/rent_house.png";
import { useListingThumbnail } from "../../hooks/useListingThumbnail.ts";
import { useCurrency } from "../../context/CurrencyContext";
import {
  formatLocationCity,
  formatLocationDistrict,
} from "../SearchBar/locationConfig";

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
    const { t, i18n } = useTranslation();
    const { formatListingPrice } = useCurrency();
    const { imageUrl, isLoading } = useListingThumbnail(listingId);
    const priceLabel = formatListingPrice(price, currency, i18n.language);
    const cityLabel = formatLocationCity(location.city, i18n.language);
    const districtLabel = formatLocationDistrict(
      location.city,
      location.district,
      i18n.language
    );

    return (
        <div className={`offer-card ${vertical ? "vertical" : "horizontal"}`}>
            <div className="offer-image-wrapper">
                <img 
                    src={isLoading ? rentHouse : (imageUrl || rentHouse)} 
                    alt={title} 
                    className={`offer-image ${isLoading ? "loading-pulse" : ""}`} 
                />
                <div className="offer-price">
                    {priceLabel} {t("listingDetail.perMonth")}
                </div>
            </div>

            <div className="offer-content">
                <h2 className="offer-title">{title}</h2>

                <div className="offer-mini-info">
                    <p className="offer-location">
                        📍 {location.street}, {location.aptNumber}, {districtLabel}, {cityLabel}
                    </p>

                    <p className="offer-area">
                        📐 {area} m<sup>2</sup>
                    </p>
                </div>

                <p className="offer-description">{description}</p>

                <div className="offer-footer">
                    <span className="offer-phone">📧 {mail}</span>
                    <Link className="contact-button" to={`/offer/${listingId}`}>
                        {t("offers.more")}
                    </Link>
                </div>
            </div>
        </div>
    );
};

export default FlatOffer;
