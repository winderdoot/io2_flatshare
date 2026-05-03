import { useListings } from "./useListings";

export default function Listings() {
  const { data, isLoading, isError, isFetching } = useListings();

  console.log(data);

  if (isLoading) return <div>Loading...</div>;
  if (isError) return <div>Błąd</div>;

  return (
    <div>
      {isFetching && <div>Odświeżanie...</div>}
    </div>
  );
}
{/* {data?.items.map((item: any) => (
  <div key={item.id}>
    {item.title} - {item.price} zł
  </div>
))} */}