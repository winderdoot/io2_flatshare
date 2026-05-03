import FlatOffer from "../../components/FlatOffer/FlatOffer"
import { Location } from "../../models/location"
import rentHouse from "../../assets/rent_house.png"
import "./FlatOffers.css"
import SearchFilters from "../../components/SearchBar/SearchFilters"
import OffersList from "../../components/OffersList/OffersList";

const MOCK_ID_WAW = "550e8400-e29b-41d4-a716-446655440001"
const MOCK_ID_KRK = "550e8400-e29b-41d4-a716-446655440002"

const FlatOffers = () => {
    const location: Location = {
        city: "City",
        district: "District",
        street: "Street",
        aptNumber: "1"
    }

    return <>
        <div className="flat-offers-container">            
            <SearchFilters></SearchFilters>
            <OffersList></OffersList>
            <h1 style={{marginBottom: "0rem"}}>Ciekawe oferty</h1>
            <div className="horizontally-scrollable-offers">
                <FlatOffer listingId={MOCK_ID_WAW} title="Flat" area={15.5} description="Description bardzo długi opis oferty, który nie mieści się w jednej lini, żeby pokazać zawiajanie, jak się będzie wyświetlać, jeśli opis będzie naprawdę długi" phone="+48 123456789" location={location} currency="PLN" price={100} image={rentHouse}/>
                <FlatOffer listingId={MOCK_ID_KRK} title="Flat" area={15.5} description="Description bardzo długi opis oferty, który nie mieści się w jednej lini, żeby pokazać zawiajanie, jak się będzie wyświetlać, jeśli opis będzie naprawdę długi" phone="+48 123456789" location={location} currency="PLN" price={100} image={rentHouse}/>
                <FlatOffer title="Flat"  area={15.5} description="Description bardzo długi opis oferty, który nie mieści się w jednej lini, żeby pokazać zawiajanie, jak się będzie wyświetlać, jeśli opis będzie naprawdę długi" phone="+48 123456789" location={location} currency="PLN" price={100} image={rentHouse}/>
                <FlatOffer title="Flat"  area={15.5} description="Description bardzo długi opis oferty, który nie mieści się w jednej lini, żeby pokazać zawiajanie, jak się będzie wyświetlać, jeśli opis będzie naprawdę długi" phone="+48 123456789" location={location} currency="PLN" price={100} image={rentHouse}/>
                <FlatOffer title="Flat"  area={15.5} description="Description bardzo długi opis oferty, który nie mieści się w jednej lini, żeby pokazać zawiajanie, jak się będzie wyświetlać, jeśli opis będzie naprawdę długi" phone="+48 123456789" location={location} currency="PLN" price={100} image={rentHouse}/>
                <FlatOffer title="Flat"  area={15.5} description="Description bardzo długi opis oferty, który nie mieści się w jednej lini, żeby pokazać zawiajanie, jak się będzie wyświetlać, jeśli opis będzie naprawdę długi" phone="+48 123456789" location={location} currency="PLN" price={100} image={rentHouse}/>
                <FlatOffer title="Flat"  area={15.5} description="Description bardzo długi opis oferty, który nie mieści się w jednej lini, żeby pokazać zawiajanie, jak się będzie wyświetlać, jeśli opis będzie naprawdę długi" phone="+48 123456789" location={location} currency="PLN" price={100} image={rentHouse}/>
            </div>
        </div>
    </>
}

export default FlatOffers