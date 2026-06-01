import { useEffect, useState } from "react";
import FlatOffer from "../../components/FlatOffer/FlatOffer"
import rentHouse from "../../assets/rent_house.png"
import "./FlatOffers.css"
import SearchFilters from "../../components/SearchBar/SearchFilters"
import OffersList from "../../components/OffersList/OffersList";
import { API_URL } from "../../config";
import type { ListingDTO } from "../../models/listing";

const FEATURED_LIMIT = 8;

const FlatOffers = () => {
    const [featured, setFeatured] = useState<ListingDTO[]>([]);
    const [featuredLoading, setFeaturedLoading] = useState(true);
    const [featuredError, setFeaturedError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        (async () => {
            setFeaturedLoading(true);
            setFeaturedError(null);
            try {
                const res = await fetch(`${API_URL}/api/v1/listings`, {
                    method: "GET",
                    headers: { Accept: "application/json" },
                });
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                const all = (await res.json()) as ListingDTO[];
                if (cancelled) return;
                const active = all
                    .filter((item) => item.status === "Active")
                    .slice(0, FEATURED_LIMIT);
                setFeatured(active);
            } catch (e) {
                if (!cancelled) {
                    setFeaturedError(e instanceof Error ? e.message : "Błąd");
                }
            } finally {
                if (!cancelled) setFeaturedLoading(false);
            }
        })();
        return () => {
            cancelled = true;
        };
    }, []);

    return <>
        <div className="flat-offers-container">            
            <SearchFilters></SearchFilters>
            <OffersList></OffersList>
            {/* <h1 style={{marginBottom: "0rem"}}>Ciekawe oferty</h1>
            {featuredLoading && (
                <div className="featured-state">Ładowanie ciekawych ofert…</div>
            )}
            {featuredError && (
                <div className="featured-state featured-state--error">
                    Nie udało się pobrać ciekawych ofert ({featuredError}).
                </div>
            )}
            {!featuredLoading && !featuredError && featured.length === 0 && (
                <div className="featured-state">Brak aktywnych ogłoszeń do pokazania.</div>
            )}
            {!featuredLoading && featured.length > 0 && (
                <div className="horizontally-scrollable-offers">
                    {featured.map((item) => (
                        <FlatOffer
                            key={item.id}
                            listingId={item.id}
                            title={item.title}
                            area={item.area}
                            description={item.description}
                            mail={item.ownerContact}
                            location={item.location}
                            currency={item.currency}
                            price={item.price}
                            image={item.coverImageUrl || rentHouse}
                        />
                    ))}
                </div>
            )} */}
        </div>
    </>
}

export default FlatOffers
