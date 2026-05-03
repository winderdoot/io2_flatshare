import { Location } from "../../models/location";

export type FlatOfferProps = {
    vertical?: boolean;
    /** Gdy ustawione, przycisk „Więcej” prowadzi do szczegółów ogłoszenia. */
    listingId?: string;
    title: string;
    description: string;
    mail: string;
    location: Location;
    price: number;
    area: number;
    currency: string;
    image: string;
};