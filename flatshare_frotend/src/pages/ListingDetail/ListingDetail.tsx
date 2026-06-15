import { Link, useLocation, useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import type { ListingDTO } from "../../models/listing";
import { AvailabilityCalendar } from "../../components/AvailabilityCalendar/AvailabilityCalendar";
import { useAuth } from "../../auth/AuthContext";
import { useListingDetail } from "./useListingDetail";
import { BookingForm } from "./BookingForm";
import rentHouse from "../../assets/rent_house.png";
import "./ListingDetail.css";
import ListingGallery from "../../components/ListingGallery/ListingGallery";
import { useCurrency } from "../../context/CurrencyContext";
import {
  formatLocationCity,
  formatLocationDistrict,
} from "../../components/SearchBar/locationConfig";
import { ReportForm } from "../../components/ReportForm/ReportForm";
import "../../components/ReportForm/ReportForm.css";
import { landlordListingsService } from "../LandlordListings/LandlordListingsService";

function formatDate(iso: string, locale: string): string {
  const d = new Date(iso + "T12:00:00");
  return new Intl.DateTimeFormat(locale, {
    day: "numeric",
    month: "long",
    year: "numeric",
  }).format(d);
}

function contactIsPhone(contact: string): boolean {
  return /^\+?[\d\s-]{9,}$/.test(contact.replace(/\s/g, ""));
}

function attributeChips(
  listing: ListingDTO,
  t: (key: string) => string
) {
  const { attributes } = listing;
  const chips: { key: string; label: string; variant?: "accent" | "warn" }[] = [
    {
      key: "profile",
      label:
        attributes.profile === "Student"
          ? t("listingDetail.profileStudent")
          : t("listingDetail.profileTourist"),
      variant: "accent",
    },
  ];
  if (attributes.petsAllowed) {
    chips.push({ key: "pets", label: t("listingDetail.petsAllowed") });
  } else {
    chips.push({ key: "pets", label: t("listingDetail.petsNotAllowed"), variant: "warn" });
  }
  if (attributes.nonSmokingOnly) {
    chips.push({ key: "smoke", label: t("listingDetail.nonSmokingOnly") });
  }
  if (attributes.closeToShops) {
    chips.push({ key: "shops", label: t("listingDetail.closeToShops") });
  }
  return chips;
}

export const ListingDetail = () => {
  const { listingId } = useParams<{ listingId: string }>();
  const location = useLocation();
  const { user, token } = useAuth();
  const { t, i18n } = useTranslation();
  const { formatListingPrice } = useCurrency();
  const { data: listing, isLoading, isError, error } = useListingDetail(listingId);
  const [reportOpen, setReportOpen] = useState(false);
  const [ownsListing, setOwnsListing] = useState(false);

  const fromMyListings =
    (location.state as { from?: string } | null)?.from === "/my-listings";

  useEffect(() => {
    if (fromMyListings || !listingId || user?.role !== "LANDLORD" || !user.id) {
      setOwnsListing(false);
      return;
    }
    let cancelled = false;
    void landlordListingsService.listByOwner(user.id).then((items) => {
      if (!cancelled) {
        setOwnsListing(items.some((item) => item.id === listingId));
      }
    });
    return () => {
      cancelled = true;
    };
  }, [fromMyListings, listingId, user?.id, user?.role]);

  const isOwnListing = fromMyListings || ownsListing;
  const canReport =
    !!user && !!token && !!listing && !isOwnListing && listing.status === "Active";

  if (!listingId) {
    return (
      <div className="listing-detail">
        <div className="listing-detail-empty">
          <p>{t("listingDetail.missingId")}</p>
          <p>
            <Link to={fromMyListings ? "/my-listings" : "/offer"}>
              {fromMyListings
                ? t("listingDetail.backToMyListings")
                : t("listingDetail.backToOffers")}
            </Link>
          </p>
        </div>
      </div>
    );
  }

  const backTo = fromMyListings ? "/my-listings" : "/offer";
  const backLabel = fromMyListings
    ? t("listingDetail.backToMyListings")
    : t("listingDetail.backToOffers");

  if (isLoading) {
    return (
      <div className="listing-detail">
        <div className="listing-detail-empty">
          <p>{t("listingDetail.loading")}</p>
        </div>
      </div>
    );
  }

  if (isError || !listing) {
    const message =
      error instanceof Error ? error.message : t("listingDetail.notFound");
    return (
      <div className="listing-detail">
        <div className="listing-detail-empty">
          <p>{message}</p>
          <p>
            <Link to={backTo}>{backLabel}</Link>
          </p>
        </div>
      </div>
    );
  }

  const addressLine = [
    listing.location.street,
    listing.location.aptNumber,
    formatLocationDistrict(listing.location.city, listing.location.district, i18n.language),
    formatLocationCity(listing.location.city, i18n.language),
  ].join(", ");

  const chips = attributeChips(listing, t);
  const priceLabel = formatListingPrice(
    listing.price,
    listing.currency,
    i18n.language
  );

  return (
    <article className="listing-detail">
      <Link className="listing-detail-back" to={backTo}>
        {backLabel}
      </Link>

      {listing.status === "HiddenByModeration" && (
        <div className="listing-moderation-banner listing-moderation-banner--hidden">
          <strong>
            {isOwnListing
              ? t("listingDetail.hiddenByModerationOwner")
              : t("listingDetail.hiddenByModeration")}
          </strong>
          {!isOwnListing && (
            <p>{t("listingDetail.hiddenByModerationDesc")}</p>
          )}
          {isOwnListing && (
            <p>{t("listingDetail.hiddenByModerationOwnerDesc")}</p>
          )}
        </div>
      )}

      <div className="listing-detail-hero-wrap">
        <ListingGallery
          listingId={listingId}
          fallbackSrc={rentHouse}
          fallbackAlt={listing.title}
        />
        <div className="listing-detail-price-pill">
          {priceLabel} {t("listingDetail.perMonth")}
        </div>
      </div>

      <header className="listing-detail-head">
        <h1>{listing.title}</h1>
        <p className="listing-detail-address">📍 {addressLine}</p>
      </header>

      <div className="listing-detail-grid">
        <section className="listing-detail-panel">
          <h2>{t("listingDetail.description")}</h2>
          <p className="listing-detail-description">{listing.description}</p>
          <div className="listing-detail-attributes" aria-label={t("listingDetail.attributesAria")}>
            {chips.map((c) => (
              <span
                key={c.key}
                className={[
                  "listing-detail-chip",
                  c.variant === "accent" ? "listing-detail-chip--accent" : "",
                  c.variant === "warn" ? "listing-detail-chip--warn" : "",
                ]
                  .filter(Boolean)
                  .join(" ")}
              >
                {c.label}
              </span>
            ))}
          </div>
          {canReport && (
            <div className="listing-detail-report-actions">
              <button
                type="button"
                className="report-trigger"
                onClick={() => setReportOpen(true)}
              >
                {t("report.reportListing")}
              </button>
              <ReportForm
                open={reportOpen}
                onClose={() => setReportOpen(false)}
                token={token!}
                type="LISTING"
                targetId={listingId}
                title={t("report.reportListingTitle")}
              />
            </div>
          )}
          {!user && listing.status === "Active" && !isOwnListing && (
            <div className="report-login-hint">
              {t("report.loginToReport")}{" "}
              <Link to="/login" state={{ from: location.pathname }}>
                {t("report.loginLink")}
              </Link>
            </div>
          )}
        </section>

        <aside className="listing-detail-panel">
          <h2>{t("listingDetail.highlights")}</h2>
          <div className="listing-detail-facts">
            <div className="listing-detail-fact">
              <span className="listing-detail-fact-label">{t("listingDetail.area")}</span>
              <p className="listing-detail-fact-value">
                {listing.area} m<sup>2</sup>
              </p>
            </div>
            <div className="listing-detail-fact">
              <span className="listing-detail-fact-label">{t("listingDetail.availableSince")}</span>
              <p className="listing-detail-fact-value">
                {formatDate(listing.availableSince, i18n.language)}
              </p>
            </div>
            <div className="listing-detail-fact">
              <span className="listing-detail-fact-label">{t("listingDetail.availableUntil")}</span>
              <p className="listing-detail-fact-value">
                {formatDate(listing.availableUntil, i18n.language)}
              </p>
            </div>
          </div>

          <AvailabilityCalendar
            availableSince={listing.availableSince}
            availableUntil={listing.availableUntil}
            unavailabilities={listing.unavailabilities}
            compact
          />

          <div className="listing-detail-contact">
            <div className="listing-detail-facts">
              <div className="listing-detail-fact">
                <span className="listing-detail-fact-label">
                  {t("listingDetail.ownerContact")}
                </span>
                <p className="listing-detail-fact-value">
                  {contactIsPhone(listing.ownerContact) ? (
                    <a href={`tel:${listing.ownerContact.replace(/\s/g, "")}`}>
                      {listing.ownerContact}
                    </a>
                  ) : (
                    <a href={`mailto:${listing.ownerContact}`}>
                      {listing.ownerContact}
                    </a>
                  )}
                </p>
              </div>
            </div>
          </div>

          {user?.role === "TENANT" && token && listing.status === "Active" && (
            <BookingForm listingId={listingId} listing={listing} token={token} />
          )}
        </aside>
      </div>
    </article>
  );
};
