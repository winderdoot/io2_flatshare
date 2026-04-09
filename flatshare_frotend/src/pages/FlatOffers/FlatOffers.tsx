import FlatOffer from "../../components/FlatOffer/FlatOffer"
import { Location } from "../../models/location"
import "./FlatOffers.css"

const FlatOffers = () => {
    const location: Location = {
        city: "City",
        district: "District",
        street: "Street",
        aptNumber: "1"
    }

    return <>
        <div className="flat-offers-container">
            <h1>Ciekawe oferty</h1>
            <div className="horizontally-scrollable-offers">
                <FlatOffer title="Flat" description="Description bardzo długi opis oferty, który nie mieści się w jednej lini, żeby pokazać zawiajanie, jak się będzie wyświetlać, jeśli opis będzie naprawdę długi" phone="+48 123456789" location={location} currency="PLN" price={100} image="src/assets/rent_house.png"/>
                <FlatOffer title="Flat" description="Description bardzo długi opis oferty, który nie mieści się w jednej lini, żeby pokazać zawiajanie, jak się będzie wyświetlać, jeśli opis będzie naprawdę długi" phone="+48 123456789" location={location} currency="PLN" price={100} image="src/assets/rent_house.png"/>
                <FlatOffer title="Flat" description="Description bardzo długi opis oferty, który nie mieści się w jednej lini, żeby pokazać zawiajanie, jak się będzie wyświetlać, jeśli opis będzie naprawdę długi" phone="+48 123456789" location={location} currency="PLN" price={100} image="src/assets/rent_house.png"/>
                <FlatOffer title="Flat" description="Description bardzo długi opis oferty, który nie mieści się w jednej lini, żeby pokazać zawiajanie, jak się będzie wyświetlać, jeśli opis będzie naprawdę długi" phone="+48 123456789" location={location} currency="PLN" price={100} image="src/assets/rent_house.png"/>
                <FlatOffer title="Flat" description="Description bardzo długi opis oferty, który nie mieści się w jednej lini, żeby pokazać zawiajanie, jak się będzie wyświetlać, jeśli opis będzie naprawdę długi" phone="+48 123456789" location={location} currency="PLN" price={100} image="src/assets/rent_house.png"/>
                <FlatOffer title="Flat" description="Description bardzo długi opis oferty, który nie mieści się w jednej lini, żeby pokazać zawiajanie, jak się będzie wyświetlać, jeśli opis będzie naprawdę długi" phone="+48 123456789" location={location} currency="PLN" price={100} image="src/assets/rent_house.png"/>
                <FlatOffer title="Flat" description="Description bardzo długi opis oferty, który nie mieści się w jednej lini, żeby pokazać zawiajanie, jak się będzie wyświetlać, jeśli opis będzie naprawdę długi" phone="+48 123456789" location={location} currency="PLN" price={100} image="src/assets/rent_house.png"/>
                <FlatOffer title="Flat" description="Description bardzo długi opis oferty, który nie mieści się w jednej lini, żeby pokazać zawiajanie, jak się będzie wyświetlać, jeśli opis będzie naprawdę długi" phone="+48 123456789" location={location} currency="PLN" price={100} image="src/assets/rent_house.png"/>
            </div>
            <FlatOffer vertical={false} title="Flat" description="Description bardzo długi opis oferty, który nie mieści się w jednej lini, żeby pokazać zawiajanie, jak się będzie wyświetlać, jeśli opis będzie naprawdę długi" phone="+48 123456789" location={location} currency="PLN" price={100} image="src/assets/rent_house.png"/>
            <FlatOffer vertical={false} title="Flat" description="Description" phone="+48 123456789" location={location} currency="PLN" price={100} image="src/assets/rent_house.png"/>
            <FlatOffer vertical={false} title="Flat" description="Description" phone="+48 123456789" location={location} currency="PLN" price={100} image="src/assets/rent_house.png"/>
            <FlatOffer vertical={false} title="Flat" description="Description" phone="+48 123456789" location={location} currency="PLN" price={100} image="src/assets/rent_house.png"/>
        </div>
    </>
}

export default FlatOffers