import { useListings } from "./useListings";
import rentHouse from "../../assets/rent_house.png"
import FlatOffer from "../FlatOffer/FlatOffer";
import { Location } from "../../models/location";
import { useFiltersStore } from "../SearchBar/FiltersStore";

export default function Listings() {
  const { data, isLoading, isError, isFetching } = useListings();

  const { setPage, page } = useFiltersStore();

  const nextPage = (maxNumber: number) => {
    if (page < maxNumber - 1) setPage(page + 1);
  }

  const prevPage = () => {
    if (page > 0) setPage(page - 1);
  };

  const location: Location = {
          city: "City",
          district: "District",
          street: "Street",
          aptNumber: "1"
      }

  console.log(data)

  if (isLoading) return <div>Loading...</div>;
  if (isError) return <div>Błąd</div>;

  return (
    <div>
      <FlatOffer vertical={false} title="Flat" area={15.5}  description="Description" phone="+48 123456789" location={location} currency="PLN" price={100} image={rentHouse}/>
      {isFetching && <div>Odświeżanie...</div>}
      <button onClick={prevPage}>Prev</button>
      <button onClick={() => nextPage(data.totalPages)}>Next</button>
    </div>
  );
}
{/* {data?.items.map((item: any) => (
  <div key={item.id}>
    {item.title} - {item.price} zł
  </div>
))} */}