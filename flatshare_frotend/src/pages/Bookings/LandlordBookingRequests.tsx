import { useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../../auth/AuthContext";
import type { ListingDTO } from "../../models/listing";
import type { BookingDTO } from "../../models/booking";
import { landlordListingsService } from "../LandlordListings/LandlordListingsService";
import { bookingService } from "./bookingService";
import { messageForBookingFailure } from "./bookingErrorUtils";
import { formatBookingDate, statusClassName, bookingStatusLabel } from "./bookingUtils";
import { useCurrency } from "../../context/CurrencyContext";
import "./Bookings.css";

type ListingWithBookings = {
  listing: ListingDTO;
  bookings: BookingDTO[];
};

export const LandlordBookingRequests = () => {
  const { t, i18n } = useTranslation();
  const { formatListingPrice } = useCurrency();
  const { user, token } = useAuth();
  const [listingFilter, setListingFilter] = useState<string>("all");

  const listingsQuery = useQuery({
    queryKey: ["listings", "owner", user?.id],
    queryFn: () => landlordListingsService.listByOwner(user!.id),
    enabled: !!user && user.role === "LANDLORD",
  });

  const bookingsQuery = useQuery({
    queryKey: ["bookings", "me", user?.id],
    queryFn: () => bookingService.listForCurrentUser(token!),
    enabled: !!token && !!user && user.role === "LANDLORD",
    staleTime: 0,
  });

  const groupedByListing = useMemo((): ListingWithBookings[] => {
    if (!listingsQuery.data) return [];
    const bookingsByListing = new Map<string, BookingDTO[]>();
    for (const booking of bookingsQuery.data ?? []) {
      const existing = bookingsByListing.get(booking.listingId) ?? [];
      existing.push(booking);
      bookingsByListing.set(booking.listingId, existing);
    }
    return listingsQuery.data.map((listing) => ({
      listing,
      bookings: bookingsByListing.get(listing.id) ?? [],
    }));
  }, [bookingsQuery.data, listingsQuery.data]);

  const filteredGroups = useMemo(() => {
    if (listingFilter === "all") return groupedByListing;
    return groupedByListing.filter((g) => g.listing.id === listingFilter);
  }, [groupedByListing, listingFilter]);

  const totalBookings = useMemo(() => {
    return filteredGroups.reduce((sum, g) => sum + g.bookings.length, 0);
  }, [filteredGroups]);

  if (!user || user.role !== "LANDLORD") {
    return (
      <div className="bookings-page">
        <div className="bookings-inner">
          <p className="bookings-banner">{t("booking.landlordOnly")}</p>
        </div>
      </div>
    );
  }

  const isLoading = listingsQuery.isLoading || bookingsQuery.isLoading;
  const isError = listingsQuery.isError || bookingsQuery.isError;
  const error = listingsQuery.error ?? bookingsQuery.error;

  return (
    <div className="bookings-page">
      <div className="bookings-inner">
        <header className="bookings-head">
          <h1 className="bookings-title">{t("booking.landlordRequestsTitle")}</h1>
          <p className="bookings-lead">{t("booking.landlordRequestsLead")}</p>
        </header>

        {listingsQuery.data && listingsQuery.data.length > 0 && (
          <div className="bookings-filter">
            <label>
              {t("booking.filterListing")}
              <select
                value={listingFilter}
                onChange={(e) => setListingFilter(e.target.value)}
              >
                <option value="all">{t("booking.filterAllListings")}</option>
                {listingsQuery.data.map((l) => (
                  <option key={l.id} value={l.id}>
                    {l.title}
                  </option>
                ))}
              </select>
            </label>
          </div>
        )}

        {isLoading && <p>{t("booking.loading")}</p>}

        {isError && (
          <p className="bookings-error" role="alert">
            {messageForBookingFailure(error, t)}
          </p>
        )}

        {!isLoading && !isError && listingsQuery.data?.length === 0 && (
          <div className="bookings-table-wrap">
            <p className="bookings-empty">{t("booking.landlordNoListings")}</p>
          </div>
        )}

        {!isLoading && !isError && totalBookings === 0 && listingsQuery.data && listingsQuery.data.length > 0 && (
          <div className="bookings-table-wrap">
            <p className="bookings-empty">{t("booking.landlordEmpty")}</p>
          </div>
        )}

        {!isLoading &&
          !isError &&
          filteredGroups.map(({ listing, bookings }) => {
            if (bookings.length === 0) return null;
            return (
              <section key={listing.id} className="bookings-listing-group">
                <h2 className="bookings-listing-group-title">{listing.title}</h2>
                <div className="bookings-table-wrap">
                  <table className="bookings-table">
                    <thead>
                      <tr>
                        <th>{t("booking.colDates")}</th>
                        <th>{t("booking.colPrice")}</th>
                        <th>{t("booking.colStatus")}</th>
                        <th>{t("booking.fieldTenantId")}</th>
                        <th>{t("booking.colActions")}</th>
                      </tr>
                    </thead>
                    <tbody>
                      {bookings.map((b) => (
                        <tr key={b.id}>
                          <td>
                            {formatBookingDate(b.startDate, i18n.language)} –{" "}
                            {formatBookingDate(b.endDate, i18n.language)}
                          </td>
                          <td>
                            {formatListingPrice(b.totalPrice, b.currency, i18n.language)}
                          </td>
                          <td>
                            <span className={statusClassName(b.status)}>
                              {bookingStatusLabel(b.status, t)}
                            </span>
                          </td>
                          <td>{b.tenantId}</td>
                          <td>
                            <Link
                              to={`/booking-requests/${b.id}`}
                              className="bookings-link"
                            >
                              {t("booking.viewDetails")}
                            </Link>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </section>
            );
          })}
      </div>
    </div>
  );
};
