import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../auth/AuthContext";
import type { ListingDTO, ListingStatus } from "../../models/listing";
import {
  isListingRequestError,
  landlordListingsService,
} from "./LandlordListingsService";
import "./LandlordListings.css";

// ── Types ─────────────────────────────────────────────────────────────────────

type ActionKey = "submit" | "publish" | "hide" | "archive";
type BusyAction = { id: string; action: ActionKey } | null;
type Toast = { id: number; message: string; kind: "success" | "error" };
type RedirectToast = { id: string; message: string; kind: "success" | "error" };

const shownRedirectToastIds = new Set<string>();

// ── Helpers ───────────────────────────────────────────────────────────────────

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

// ── Component ─────────────────────────────────────────────────────────────────

export const LandlordListings = () => {
  const { t, i18n } = useTranslation();
  const { user, token } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  const [items, setItems] = useState<ListingDTO[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [busyAction, setBusyAction] = useState<BusyAction>(null);
  const [toasts, setToasts] = useState<Toast[]>([]);
  const toastIdRef = useRef(0);

  const sorted = useMemo(() => {
    if (!items) return [];
    return [...items].sort((a, b) => a.title.localeCompare(b.title, i18n.language));
  }, [items, i18n.language]);

  // ── Toast helpers ───────────────────────────────────────────────────────────

  const addToast = useCallback((message: string, kind: "success" | "error") => {
    const id = ++toastIdRef.current;
    setToasts((prev) => [...prev, { id, message, kind }]);
    setTimeout(
      () => setToasts((prev) => prev.filter((toast) => toast.id !== id)),
      4500
    );
  }, []);

  // Show toast if redirected here from ListingEditor after create/edit.
  // Must clear state via React Router (not window.history), otherwise Strict Mode /
  // stale location.state can show the same toast twice.
  useEffect(() => {
    const state = location.state as { toast?: RedirectToast } | null;
    const payload = state?.toast;
    if (!payload?.id) return;
    if (shownRedirectToastIds.has(payload.id)) {
      navigate(
        { pathname: location.pathname, search: location.search, hash: location.hash },
        { replace: true, state: {} }
      );
      return;
    }

    shownRedirectToastIds.add(payload.id);
    addToast(payload.message, payload.kind);
    navigate(
      { pathname: location.pathname, search: location.search, hash: location.hash },
      { replace: true, state: {} }
    );
  }, [
    location.state,
    location.pathname,
    location.search,
    location.hash,
    navigate,
    addToast,
  ]);

  // ── Data fetching ───────────────────────────────────────────────────────────

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

  // ── Action handler ──────────────────────────────────────────────────────────

  const handleAction = useCallback(
    async (id: string, action: ActionKey, successKey: string) => {
      if (!token) {
        addToast(t("landlordListings.error401"), "error");
        return;
      }
      setBusyAction({ id, action });
      try {
        await landlordListingsService[action](token, id);
        const updated = await landlordListingsService.getById(id);
        setItems((prev) =>
          prev ? prev.map((item) => (item.id === id ? updated : item)) : prev
        );
        addToast(t(successKey), "success");
      } catch (e: unknown) {
        const msg = isListingRequestError(e)
          ? e.message
          : t("landlordListings.actionError");
        addToast(msg, "error");
      } finally {
        setBusyAction(null);
      }
    },
    [token, t, addToast]
  );

  // ── Action cell renderer ────────────────────────────────────────────────────

  const renderActionButtons = useCallback(
    (row: ListingDTO) => {
      const anyBusy = busyAction !== null;

      const btn = (
        action: ActionKey,
        labelKey: string,
        variant: string,
        successKey: string
      ) => {
        const isBusy = busyAction?.id === row.id && busyAction.action === action;
        return (
          <button
            key={action}
            className={`ll-action-btn ll-action-btn--${variant}`}
            disabled={anyBusy}
            aria-busy={isBusy}
            onClick={() => handleAction(row.id, action, successKey)}
          >
            {isBusy ? <span className="ll-spinner" aria-hidden="true" /> : t(labelKey)}
          </button>
        );
      };

      switch (row.status) {
        case "Draft":
          return (
            <div className="ll-action-cell">
              {btn(
                "submit",
                "landlordListings.btnSubmit",
                "submit",
                "landlordListings.toastSubmitted"
              )}
              <span className="ll-draft-hint">{t("landlordListings.draftFixesHint")}</span>
            </div>
          );

        case "UnderReview":
          return (
            <div className="ll-action-cell">
              <span className="ll-review-badge">
                ⏳ {t("landlordListings.underReviewHint")}
              </span>
            </div>
          );

        case "Active":
          return (
            <div className="ll-action-cell">
              {btn("hide", "landlordListings.btnHide", "hide", "landlordListings.toastHidden")}
              {btn(
                "archive",
                "landlordListings.btnArchive",
                "archive",
                "landlordListings.toastArchived"
              )}
            </div>
          );

        case "Hidden":
          return (
            <div className="ll-action-cell">
              {btn(
                "publish",
                "landlordListings.btnRepublish",
                "publish",
                "landlordListings.toastPublished"
              )}
              {btn(
                "archive",
                "landlordListings.btnArchive",
                "archive",
                "landlordListings.toastArchived"
              )}
            </div>
          );

        case "HiddenByModeration":
          return (
            <div className="ll-action-cell">
              {btn(
                "archive",
                "landlordListings.btnArchive",
                "archive",
                "landlordListings.toastArchived"
              )}
              <span className="ll-moderation-note">
                ⚠️ {t("landlordListings.moderationHint")}
              </span>
            </div>
          );

        case "Archived":
          return (
            <div className="ll-action-cell">
              <span className="ll-muted-note">{t("landlordListings.archivedNote")}</span>
            </div>
          );

        default:
          return null;
      }
    },
    [busyAction, handleAction, t]
  );

  // ── Role guard ──────────────────────────────────────────────────────────────

  if (user?.role !== "LANDLORD") {
    return (
      <div className="landlord-listings">
        <div className="landlord-listings-inner">
          <p className="landlord-listings-notice">{t("landlordListings.landlordOnly")}</p>
        </div>
      </div>
    );
  }

  // ── Render ──────────────────────────────────────────────────────────────────

  return (
    <>
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
                    <th>{t("landlordListings.colLinks")}</th>
                  </tr>
                </thead>
                <tbody>
                  {sorted.map((row) => {
                    const canEdit =
                      row.status === "Draft" || row.status === "Hidden";
                    const priceStr = new Intl.NumberFormat(
                      i18n.language === "en" ? "en-US" : "pl-PL",
                      { minimumFractionDigits: 2, maximumFractionDigits: 2 }
                    ).format(row.price);
                    const rowBusy = busyAction?.id === row.id;

                    return (
                      <tr
                        key={row.id}
                        className={[
                          rowBusy ? "ll-row--busy" : "",
                          row.status === "HiddenByModeration"
                            ? "ll-row--moderation-hidden"
                            : "",
                        ]
                          .filter(Boolean)
                          .join(" ") || undefined}
                      >
                        <td>{row.title}</td>
                        <td>
                          <span className={statusBadgeClass(row.status)}>
                            {t(`landlordListings.status.${row.status}`)}
                          </span>
                        </td>
                        <td>{row.location.city}</td>
                        <td className="ll-price-td">
                          {priceStr} {row.currency}
                        </td>
                        <td className="ll-actions-td">
                          {renderActionButtons(row)}
                        </td>
                        <td>
                          <div className="ll-links-row">
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
                            <Link
                              className="landlord-listings-link"
                              to={`/my-listings/${row.id}/availability`}
                            >
                              {t("landlordListings.btnCalendar")}
                            </Link>
                            {" · "}
                            <Link
                              className="landlord-listings-link"
                              to={`/offer/${row.id}`}
                              state={{ from: "/my-listings" }}
                            >
                              {t("landlordListings.preview")}
                            </Link>
                          </div>
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

      {/* Toast notifications */}
      {toasts.length > 0 && (
        <div className="ll-toast-wrap" aria-live="polite">
          {toasts.map((toast) => (
            <div key={toast.id} className={`ll-toast ll-toast--${toast.kind}`}>
              {toast.message}
            </div>
          ))}
        </div>
      )}
    </>
  );
};
