import { Link, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../auth/AuthContext";
import { AvailabilityCalendar } from "../../components/AvailabilityCalendar/AvailabilityCalendar";
import { UnavailabilityManager } from "../ListingDetail/UnavailabilityManager";
import { useListingDetail } from "../ListingDetail/useListingDetail";
import {
  isListingRequestError,
} from "./LandlordListingsService";
import "./ListingAvailability.css";

function messageForListingFailure(
  e: unknown,
  t: (key: string, opt?: Record<string, string>) => string
): string {
  if (isListingRequestError(e)) {
    if (e.status === 401) return t("landlordListings.error401");
    if (e.status === 403) return t("landlordListings.error403");
    return t("landlordListings.loadErrorDetail", { message: e.message });
  }
  return t("landlordListings.networkError");
}

function formatDate(iso: string): string {
  const d = new Date(iso + "T12:00:00");
  return new Intl.DateTimeFormat("pl-PL", {
    day: "numeric",
    month: "long",
    year: "numeric",
  }).format(d);
}

export const ListingAvailability = () => {
  const { t } = useTranslation();
  const { listingId } = useParams<{ listingId: string }>();
  const { user, token } = useAuth();
  const { data: listing, isLoading, isError, error } = useListingDetail(listingId);

  if (user?.role !== "LANDLORD") {
    return (
      <div className="listing-availability">
        <div className="listing-availability-inner">
          <p className="listing-availability-banner">
            {t("landlordListings.landlordOnly")}
          </p>
          <Link className="listing-availability-back" to="/my-listings">
            {t("landlordListings.backToList")}
          </Link>
        </div>
      </div>
    );
  }

  if (!listingId || !token) {
    return (
      <div className="listing-availability">
        <div className="listing-availability-inner">
          <p className="listing-availability-error">
            {t("landlordListings.unavailMissingId")}
          </p>
          <Link className="listing-availability-back" to="/my-listings">
            {t("landlordListings.backToList")}
          </Link>
        </div>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="listing-availability">
        <div className="listing-availability-inner">
          <p className="listing-availability-lead">{t("landlordListings.loading")}</p>
        </div>
      </div>
    );
  }

  if (isError || !listing) {
    const message =
      error instanceof Error
        ? error.message
        : messageForListingFailure(error, t);
    return (
      <div className="listing-availability">
        <div className="listing-availability-inner">
          <p className="listing-availability-error">{message}</p>
          <Link className="listing-availability-back" to="/my-listings">
            {t("landlordListings.backToList")}
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="listing-availability">
      <div className="listing-availability-inner">
        <header className="listing-availability-head">
          <Link className="listing-availability-back" to="/my-listings">
            ← {t("landlordListings.backToList")}
          </Link>
          <h1 className="listing-availability-title">
            {t("landlordListings.availabilityTitle")}
          </h1>
          <p className="listing-availability-lead">
            {t("landlordListings.availabilityLead", { title: listing.title })}
          </p>
          <div className="listing-availability-links">
            <Link
              className="listing-availability-link"
              to={`/offer/${listingId}`}
              state={{ from: "/my-listings" }}
            >
              {t("landlordListings.preview")}
            </Link>
          </div>
        </header>

        <div className="listing-availability-card">
          <AvailabilityCalendar
            availableSince={listing.availableSince}
            availableUntil={listing.availableUntil}
            unavailabilities={listing.unavailabilities}
          />
        </div>

        <UnavailabilityManager
          listingId={listingId}
          listing={listing}
          token={token}
          formatDate={formatDate}
        />
      </div>
    </div>
  );
};
