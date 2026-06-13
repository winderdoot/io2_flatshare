import { useListings } from "./useListings";
import FlatOffer from "../FlatOffer/FlatOffer";
import { useFiltersStore } from "../SearchBar/FiltersStore";
import { useTranslation } from "react-i18next";
import "./OffersList.css"

export default function Listings() {
  const { t } = useTranslation();
  const { data, isLoading, isError, isFetching } = useListings();
  const { setPage, page } = useFiltersStore();

  const nextPage = (maxNumber: number) => {
    if (page < maxNumber - 1) setPage(page + 1);
  }

  const prevPage = () => {
    if (page > 0) setPage(page - 1);
  };

  if (isLoading) return <div className="offers-state">{t("offers.loading")}</div>;
  if (isError) return <div className="offers-state offers-state--error">{t("offers.error")}</div>;

  return (
    <div className="offers-wrapper">
      {isFetching && <div className="offers-state offers-state--muted">{t("offers.refreshing")}</div>}
      <div className="offers-list">
        {data?.content.map((item: any) => (
          <FlatOffer key={item.listing.id} listingId={item.listing.id} vertical={false} title={item.listing.title} area={item.listing.area} description={item.listing.description} mail={item.listing.ownerContact} location={item.listing.location} currency={item.listing.currency} price={item.listing.price}/>
        ))}
      </div>

      <div className="pagination">
        <button className="pagination-btn" onClick={prevPage}>{t("offers.prevPage")}</button>
        <button className="pagination-btn" onClick={() => nextPage(data.page.totalPages)}>{t("offers.nextPage")}</button>
      </div>
    </div>
  );
}
