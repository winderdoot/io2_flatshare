import { useCallback, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../auth/AuthContext";
import type { ListingDTO, ListingStatus } from "../../models/listing";
import {
  adminListingsService,
  isAdminRequestError,
} from "./AdminListingsService";
import { AdminReportsSection } from "./AdminReportsSection";
import "./AdminPanel.css";

/* ── Status badge CSS classes ─────────────────────────────────── */

const STATUS_CLASS: Record<ListingStatus, string> = {
  Draft: "badge--draft",
  UnderReview: "badge--review",
  Active: "badge--active",
  Hidden: "badge--hidden",
  HiddenByModeration: "badge--moderation",
  Archived: "badge--archived",
};

/* ── Helpers ──────────────────────────────────────────────────── */

function fmt(iso: string): string {
  return new Intl.DateTimeFormat(undefined, {
    day: "numeric",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(iso));
}

function fmtDate(iso: string): string {
  return new Intl.DateTimeFormat(undefined, {
    day: "numeric",
    month: "long",
    year: "numeric",
  }).format(new Date(iso + "T12:00:00"));
}

/* ── Toast ────────────────────────────────────────────────────── */

type Toast = { id: number; type: "success" | "error"; text: string };
let _toastId = 0;

function useToasts() {
  const [toasts, setToasts] = useState<Toast[]>([]);
  const push = (type: "success" | "error", text: string) => {
    const id = ++_toastId;
    setToasts((prev) => [...prev, { id, type, text }]);
    setTimeout(
      () => setToasts((prev) => prev.filter((t) => t.id !== id)),
      4000
    );
  };
  return { toasts, push };
}

/* ── Detail modal ─────────────────────────────────────────────── */

type ModalProps = {
  listing: ListingDTO;
  pendingId: string | null;
  onApprove: (id: string) => void;
  onRequestFixes: (id: string) => void;
  onModerationHide: (id: string) => void;
  onReinstate: (id: string) => void;
  onArchive: (id: string) => void;
  onClose: () => void;
};

function ListingModal({
  listing,
  pendingId,
  onApprove,
  onRequestFixes,
  onModerationHide,
  onReinstate,
  onArchive,
  onClose,
}: ModalProps) {
  const { t } = useTranslation();
  const overlayRef = useRef<HTMLDivElement>(null);
  const busy = pendingId === listing.id;
  const m = "adminPanel.modal";

  useEffect(() => {
    const handleKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };
    window.addEventListener("keydown", handleKey);
    return () => window.removeEventListener("keydown", handleKey);
  }, [onClose]);

  const addr = [
    listing.location.street,
    listing.location.aptNumber,
    listing.location.district,
    listing.location.city,
  ]
    .filter(Boolean)
    .join(", ");

  return (
    <div
      className="ap-overlay"
      ref={overlayRef}
      onClick={(e) => {
        if (e.target === overlayRef.current) onClose();
      }}
      role="dialog"
      aria-modal="true"
      aria-label={listing.title}
    >
      <div className="ap-modal">
        <button
          className="ap-modal-close"
          onClick={onClose}
          aria-label={t(`${m}.closeLabel`)}
        >
          ✕
        </button>

        <div className="ap-modal-header">
          <h2 className="ap-modal-title">{listing.title}</h2>
          <span className={`ap-badge ${STATUS_CLASS[listing.status]}`}>
            {t(`adminPanel.status.${listing.status}`)}
          </span>
        </div>

        <div className="ap-modal-body">
          <section className="ap-detail-section">
            <h3>{t(`${m}.sectionDescription`)}</h3>
            <p className="ap-description">{listing.description}</p>
          </section>

          <section className="ap-detail-section">
            <h3>{t(`${m}.sectionBasic`)}</h3>
            <div className="ap-facts-grid">
              <div className="ap-fact">
                <span className="ap-fact-label">{t(`${m}.price`)}</span>
                <span className="ap-fact-value">
                  {t(`${m}.priceValue`, {
                    price: listing.price,
                    currency: listing.currency,
                  })}
                </span>
              </div>
              <div className="ap-fact">
                <span className="ap-fact-label">{t(`${m}.area`)}</span>
                <span className="ap-fact-value">
                  {t(`${m}.areaValue`, { area: listing.area })}
                </span>
              </div>
              <div className="ap-fact">
                <span className="ap-fact-label">{t(`${m}.availableSince`)}</span>
                <span className="ap-fact-value">
                  {fmtDate(listing.availableSince)}
                </span>
              </div>
              <div className="ap-fact">
                <span className="ap-fact-label">{t(`${m}.availableUntil`)}</span>
                <span className="ap-fact-value">
                  {fmtDate(listing.availableUntil)}
                </span>
              </div>
              <div className="ap-fact">
                <span className="ap-fact-label">{t(`${m}.ownerContact`)}</span>
                <span className="ap-fact-value">{listing.ownerContact}</span>
              </div>
              {listing.createdAt && (
                <div className="ap-fact">
                  <span className="ap-fact-label">{t(`${m}.createdAt`)}</span>
                  <span className="ap-fact-value">{fmt(listing.createdAt)}</span>
                </div>
              )}
            </div>
          </section>

          <section className="ap-detail-section">
            <h3>{t(`${m}.sectionLocation`)}</h3>
            <p className="ap-address">{addr || t(`${m}.noAddress`)}</p>
          </section>

          <section className="ap-detail-section">
            <h3>{t(`${m}.sectionAttributes`)}</h3>
            <div className="ap-chips">
              <span
                className={`ap-chip ${listing.attributes.profile === "Student" ? "ap-chip--accent" : "ap-chip--neutral"}`}
              >
                {listing.attributes.profile === "Student"
                  ? t(`${m}.profileStudent`)
                  : t(`${m}.profileTourist`)}
              </span>
              <span
                className={`ap-chip ${listing.attributes.petsAllowed ? "ap-chip--ok" : "ap-chip--warn"}`}
              >
                {listing.attributes.petsAllowed
                  ? t(`${m}.petsAllowed`)
                  : t(`${m}.noPets`)}
              </span>
              {listing.attributes.nonSmokingOnly && (
                <span className="ap-chip ap-chip--warn">
                  {t(`${m}.nonSmoking`)}
                </span>
              )}
              {listing.attributes.closeToShops && (
                <span className="ap-chip ap-chip--ok">
                  {t(`${m}.closeToShops`)}
                </span>
              )}
            </div>
          </section>

          {listing.unavailabilities && listing.unavailabilities.length > 0 && (
            <section className="ap-detail-section">
              <h3>{t(`${m}.sectionUnavail`)}</h3>
              <ul className="ap-unavail-list">
                {listing.unavailabilities.map((u, i) => (
                  <li key={i}>
                    {fmtDate(u.since)} – {fmtDate(u.until)}
                    {u.message && (
                      <span className="ap-unavail-msg"> ({u.message})</span>
                    )}
                  </li>
                ))}
              </ul>
            </section>
          )}
        </div>

        <div className="ap-modal-actions">
          {listing.status === "UnderReview" && (
            <>
              <button
                className="ap-btn ap-btn--approve"
                disabled={busy}
                onClick={() => onApprove(listing.id)}
              >
                {busy ? t(`${m}.busy`) : t(`${m}.btnApprove`)}
              </button>
              <button
                className="ap-btn ap-btn--fixes"
                disabled={busy}
                onClick={() => onRequestFixes(listing.id)}
              >
                {busy ? t(`${m}.busy`) : t(`${m}.btnFixes`)}
              </button>
            </>
          )}
          {listing.status === "Active" && (
            <button
              className="ap-btn ap-btn--moderation"
              disabled={busy}
              onClick={() => onModerationHide(listing.id)}
            >
              {busy ? t(`${m}.busy`) : t(`${m}.btnModerationHide`)}
            </button>
          )}
          {listing.status === "HiddenByModeration" && (
            <button
              className="ap-btn ap-btn--reinstate"
              disabled={busy}
              onClick={() => onReinstate(listing.id)}
            >
              {busy ? t(`${m}.busy`) : t(`${m}.btnReinstate`)}
            </button>
          )}
          {["Active", "Hidden", "HiddenByModeration"].includes(
            listing.status
          ) && (
            <button
              className="ap-btn ap-btn--archive"
              disabled={busy}
              onClick={() => onArchive(listing.id)}
            >
              {busy ? t(`${m}.busy`) : t(`${m}.btnArchive`)}
            </button>
          )}
          <button className="ap-btn ap-btn--ghost" onClick={onClose}>
            {t(`${m}.btnClose`)}
          </button>
        </div>
      </div>
    </div>
  );
}

/* ── Main component ───────────────────────────────────────────── */

type StatusFilter = "UnderReview" | "all";
type PanelSection = "listings" | "reports";
type AdminAction =
  | "approve"
  | "requestFixes"
  | "archive"
  | "moderationHide"
  | "reinstate";

export const AdminPanel = () => {
  const { t } = useTranslation();
  const { token } = useAuth();
  const [pendingItems, setPendingItems] = useState<ListingDTO[]>([]);
  const [allItems, setAllItems] = useState<ListingDTO[]>([]);
  const [loading, setLoading] = useState(true);
  const [fetchError, setFetchError] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("UnderReview");
  const [panelSection, setPanelSection] = useState<PanelSection>("listings");
  const [reportsRefreshKey, setReportsRefreshKey] = useState(0);
  const [pendingId, setPendingId] = useState<string | null>(null);
  const [selected, setSelected] = useState<ListingDTO | null>(null);
  const { toasts, push } = useToasts();

  const load = useCallback(() => {
    if (!token) {
      setFetchError(t("adminPanel.noToken"));
      setLoading(false);
      return;
    }
    setLoading(true);
    setFetchError(null);

    Promise.all([
      adminListingsService.listUnderReview(token),
      adminListingsService.listAll(),
    ])
      .then(([pending, all]) => {
        setPendingItems(pending);
        setAllItems(all);
      })
      .catch((e: unknown) => {
        const msg = isAdminRequestError(e)
          ? t("adminPanel.fetchError", { status: e.status, message: e.message })
          : t("adminPanel.networkError");
        setFetchError(msg);
      })
      .finally(() => setLoading(false));
  }, [token, t]);

  useEffect(() => {
    if (panelSection === "listings") {
      load();
    }
  }, [load, panelSection]);

  const handleRefresh = () => {
    if (panelSection === "listings") {
      load();
    } else {
      setReportsRefreshKey((k) => k + 1);
    }
  };

  const removeFromPending = (id: string) => {
    setPendingItems((prev) => prev.filter((l) => l.id !== id));
  };

  const patchListing = (id: string, nextStatus: ListingStatus) => {
    const patch = (list: ListingDTO[]) =>
      list.map((l) => (l.id === id ? { ...l, status: nextStatus } : l));
    setPendingItems(patch);
    setAllItems(patch);
    setSelected((prev) =>
      prev && prev.id === id ? { ...prev, status: nextStatus } : prev
    );
  };

  const items =
    statusFilter === "UnderReview" ? pendingItems : allItems;
  const pendingCount = pendingItems.length;
  const allCount = allItems.length;

  const handleAction = async (action: AdminAction, id: string) => {
    if (!token) {
      push("error", t("adminPanel.noToken"));
      return;
    }
    setPendingId(id);
    try {
      if (action === "approve") {
        await adminListingsService.approve(token, id);
        removeFromPending(id);
        patchListing(id, "Active");
        push("success", t("adminPanel.toastApproved"));
      } else if (action === "requestFixes") {
        await adminListingsService.requestFixes(token, id);
        removeFromPending(id);
        patchListing(id, "Draft");
        push("success", t("adminPanel.toastFixes"));
      } else if (action === "moderationHide") {
        await adminListingsService.moderationHide(token, id);
        patchListing(id, "HiddenByModeration");
        push("success", t("adminPanel.toastModerationHide"));
      } else if (action === "reinstate") {
        await adminListingsService.reinstate(token, id);
        patchListing(id, "Active");
        push("success", t("adminPanel.toastReinstate"));
      } else {
        await adminListingsService.archive(token, id);
        patchListing(id, "Archived");
        push("success", t("adminPanel.toastArchived"));
      }
    } catch (e: unknown) {
      const msg = isAdminRequestError(e)
        ? t("adminPanel.fetchError", { status: e.status, message: e.message })
        : t("adminPanel.actionError");
      push("error", msg);
    } finally {
      setPendingId(null);
    }
  };

  return (
    <div className="ap-root">
      {/* Toast notifications */}
      <div className="ap-toasts" aria-live="polite">
        {toasts.map((t) => (
          <div key={t.id} className={`ap-toast ap-toast--${t.type}`}>
            {t.text}
          </div>
        ))}
      </div>

      <div className="ap-inner">
        {/* Header */}
        <header className="ap-header">
          <div>
            <h1 className="ap-title">{t("adminPanel.title")}</h1>
            <p className="ap-subtitle">
              {panelSection === "listings"
                ? t("adminPanel.subtitle")
                : t("adminPanel.reports.tab")}
            </p>
          </div>
          <button
            className="ap-btn ap-btn--ghost ap-btn--sm"
            onClick={handleRefresh}
            disabled={loading && panelSection === "listings"}
          >
            {loading && panelSection === "listings"
              ? t("adminPanel.refreshing")
              : t("adminPanel.refresh")}
          </button>
        </header>

        <div className="ap-filter-bar ap-filter-bar--sections">
          {(["listings", "reports"] as PanelSection[]).map((section) => (
            <button
              key={section}
              className={`ap-filter-btn ${panelSection === section ? "ap-filter-btn--active" : ""}`}
              onClick={() => setPanelSection(section)}
            >
              {section === "listings"
                ? t("adminPanel.tabListings")
                : t("adminPanel.tabReports")}
            </button>
          ))}
        </div>

        {panelSection === "reports" ? (
          <AdminReportsSection
            key={reportsRefreshKey}
            pushToast={push}
          />
        ) : (
          <>
        {pendingCount > 0 && statusFilter === "UnderReview" && (
          <p className="ap-pending-note">
            {t("adminPanel.pendingBadge", { count: pendingCount })}
          </p>
        )}

        {/* Filter bar */}
        <div className="ap-filter-bar">
          {(["UnderReview", "all"] as StatusFilter[]).map((f) => (
            <button
              key={f}
              className={`ap-filter-btn ${statusFilter === f ? "ap-filter-btn--active" : ""}`}
              onClick={() => setStatusFilter(f)}
            >
              {f === "UnderReview"
                ? t("adminPanel.filterPending", { count: pendingCount })
                : t("adminPanel.filterAll", { count: allCount })}
            </button>
          ))}
        </div>

        {/* Fetch error */}
        {fetchError && (
          <div className="ap-alert ap-alert--error">{fetchError}</div>
        )}

        {/* Loading skeleton */}
        {loading && (
          <div className="ap-skeleton-wrap">
            {[1, 2, 3].map((n) => (
              <div key={n} className="ap-skeleton-row" />
            ))}
          </div>
        )}

        {/* Empty state */}
        {!loading && !fetchError && items.length === 0 && (
          <div className="ap-empty">
            <p>
              {statusFilter === "UnderReview"
                ? t("adminPanel.emptyPending")
                : t("adminPanel.emptyAll")}
            </p>
          </div>
        )}

        {/* Table */}
        {!loading && items.length > 0 && (
          <div className="ap-table-wrap">
            <table className="ap-table">
              <thead>
                <tr>
                  <th>{t("adminPanel.colTitle")}</th>
                  <th>{t("adminPanel.colOwner")}</th>
                  <th>{t("adminPanel.colCity")}</th>
                  <th>{t("adminPanel.colStatus")}</th>
                  <th>{t("adminPanel.colDate")}</th>
                  <th>{t("adminPanel.colActions")}</th>
                </tr>
              </thead>
              <tbody>
                {items.map((row) => {
                  const busy = pendingId === row.id;
                  return (
                    <tr key={row.id} className={busy ? "ap-row--busy" : ""}>
                      <td className="ap-col-title">
                        <button
                          className="ap-link-btn"
                          onClick={() => setSelected(row)}
                          title={t("adminPanel.btnDetails")}
                        >
                          {row.title}
                        </button>
                      </td>
                      <td className="ap-col-owner">{row.ownerContact}</td>
                      <td>{row.location.city}</td>
                      <td>
                        <span className={`ap-badge ${STATUS_CLASS[row.status]}`}>
                          {t(`adminPanel.status.${row.status}`)}
                        </span>
                      </td>
                      <td className="ap-col-date">
                        {row.createdAt
                          ? fmt(row.createdAt)
                          : t("adminPanel.noDate")}
                      </td>
                      <td className="ap-col-actions">
                        <div className="ap-actions-inner">
                          <button
                            className="ap-btn ap-btn--sm ap-btn--ghost"
                            onClick={() => setSelected(row)}
                          >
                            {t("adminPanel.btnDetails")}
                          </button>
                          {row.status === "UnderReview" && (
                            <>
                              <button
                                className="ap-btn ap-btn--sm ap-btn--approve"
                                disabled={busy}
                                onClick={() => handleAction("approve", row.id)}
                              >
                                {t("adminPanel.btnApprove")}
                              </button>
                              <button
                                className="ap-btn ap-btn--sm ap-btn--fixes"
                                disabled={busy}
                                onClick={() =>
                                  handleAction("requestFixes", row.id)
                                }
                              >
                                {t("adminPanel.btnFixes")}
                              </button>
                            </>
                          )}
                          {row.status === "Active" && (
                            <button
                              className="ap-btn ap-btn--sm ap-btn--moderation"
                              disabled={busy}
                              onClick={() =>
                                handleAction("moderationHide", row.id)
                              }
                            >
                              {t("adminPanel.btnModerationHide")}
                            </button>
                          )}
                          {row.status === "HiddenByModeration" && (
                            <button
                              className="ap-btn ap-btn--sm ap-btn--reinstate"
                              disabled={busy}
                              onClick={() => handleAction("reinstate", row.id)}
                            >
                              {t("adminPanel.btnReinstate")}
                            </button>
                          )}
                          {["Active", "Hidden", "HiddenByModeration"].includes(
                            row.status
                          ) && (
                            <button
                              className="ap-btn ap-btn--sm ap-btn--archive"
                              disabled={busy}
                              onClick={() => handleAction("archive", row.id)}
                            >
                              {t("adminPanel.btnArchive")}
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
          </>
        )}
      </div>

      {/* Detail modal */}
      {selected && (
        <ListingModal
          listing={selected}
          pendingId={pendingId}
          onApprove={(id) => handleAction("approve", id)}
          onRequestFixes={(id) => handleAction("requestFixes", id)}
          onModerationHide={(id) => handleAction("moderationHide", id)}
          onReinstate={(id) => handleAction("reinstate", id)}
          onArchive={(id) => handleAction("archive", id)}
          onClose={() => setSelected(null)}
        />
      )}
    </div>
  );
};
