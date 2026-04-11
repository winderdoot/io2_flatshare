import { Location } from "../../models/location";

export type FlatOfferProps = {
    vertical?: boolean;
    title: string;
    description: string;
    phone: string;
    location: Location;
    price: number;
    area: number;
    currency: string;
    image: string;
};