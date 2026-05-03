import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../auth/AuthContext";
import type { ListingDTO, ListingStatus } from "../../models/listing";
import {
  isListingRequestError,
  landlordListingsService,
} from "./LandlordListingsService";
import "./LandlordListings.css";

function statusBadgeClass(status: ListingStatus): string {
  switch (status) {
    case "Draft":
      return "landlord-listings-status landlord-listings-status--draft";
    case "UnderReview":
      return "landlord-listings-status landlord-listings-status--underReview";
    case "Active":
      return "landlord-listings-status landlord-listings-status--active";
    case "Hidden":
      return "landlord-listings-status landlord-listings-status--hidden";
    case "HiddenByModeration":
      return "landlord-listings-status landlord-listings-status--hiddenByModeration";
    case "Archived":
      return "landlord-listings-status landlord-listings-status--archived";
    default:
      return "landlord-listings-status landlord-listings-status--draft";
  }
}

function messageForListFailure(
  e: unknown,
  t: (key: string, opt?: Record<string, string>) => string
): string {
  if (isListingRequestError(e)) {
    if (e.status === 401) return t("landlordListings.error401");
    return t("landlordListings.loadErrorDetail", { message: e.message });
  }
  return t("landlordListings.networkError");
}

export const LandlordListings = () => {
  const { t, i18n } = useTranslation();
  const { user } = useAuth();
  const [items, setItems] = useState<ListingDTO[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const sorted = useMemo(() => {
    if (!items) return [];
    return [...items].sort((a, b) => a.title.localeCompare(b.title, i18n.language));
  }, [items, i18n.language]);

  useEffect(() => {
    if (user?.role !== "LANDLORD" || !user.id) {
      setLoading(false);
      setItems(null);
      return;
    }

    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const list = await landlordListingsService.listByOwner(user.id);
        if (!cancelled) setItems(list);
      } catch (e: unknown) {
        if (!cancelled) setError(messageForListFailure(e, t));
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [user?.role, user?.id, t]);

  if (user?.role !== "LANDLORD") {
    return (
      <div className="landlord-listings">
        <div className="landlord-listings-inner">
          <p className="landlord-listings-notice">{t("landlordListings.landlordOnly")}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="landlord-listings">
      <div className="landlord-listings-inner">
        <header className="landlord-listings-head">
          <div>
            <h1 className="landlord-listings-title">{t("landlordListings.title")}</h1>
            <p className="landlord-listings-subtitle">{t("landlordListings.subtitle")}</p>
          </div>
          <div className="landlord-listings-actions">
            <Link
              className="landlord-listings-btn landlord-listings-btn--primary"
              to="/my-listings/new"
            >
              {t("landlordListings.newListing")}
            </Link>
          </div>
        </header>

        {error && <div className="landlord-listings-error">{error}</div>}

        {loading && (
          <p className="landlord-listings-muted">{t("landlordListings.loading")}</p>
        )}

        {!loading && !error && sorted.length === 0 && (
          <div className="landlord-listings-table-wrap">
            <p className="landlord-listings-empty">{t("landlordListings.empty")}</p>
          </div>
        )}

        {!loading && sorted.length > 0 && (
          <div className="landlord-listings-table-wrap">
            <table className="landlord-listings-table">
              <thead>
                <tr>
                  <th>{t("landlordListings.colTitle")}</th>
                  <th>{t("landlordListings.colStatus")}</th>
                  <th>{t("landlordListings.colCity")}</th>
                  <th>{t("landlordListings.colPrice")}</th>
                  <th>{t("landlordListings.colActions")}</th>
                </tr>
              </thead>
              <tbody>
                {sorted.map((row) => {
                  const canEdit = row.status === "Draft" || row.status === "Hidden";
                  const priceStr = new Intl.NumberFormat(
                    i18n.language === "en" ? "en-US" : "pl-PL",
                    { maximumFractionDigits: 2 }
                  ).format(row.price);
                  return (
                    <tr key={row.id}>
                      <td>{row.title}</td>
                      <td>
                        <span className={statusBadgeClass(row.status)}>
                          {t(`landlordListings.status.${row.status}`)}
                        </span>
                      </td>
                      <td>{row.location.city}</td>
                      <td>
                        {priceStr} {row.currency}
                      </td>
                      <td>
                        {canEdit ? (
                          <Link
                            className="landlord-listings-link"
                            to={`/my-listings/${row.id}/edit`}
                          >
                            {t("landlordListings.edit")}
                          </Link>
                        ) : (
                          <span className="landlord-listings-muted">
                            {t("landlordListings.editLocked")}
                          </span>
                        )}
                        {" · "}
                        <Link className="landlord-listings-link" to={`/offer/${row.id}`}>
                          {t("landlordListings.preview")}
                        </Link>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};
