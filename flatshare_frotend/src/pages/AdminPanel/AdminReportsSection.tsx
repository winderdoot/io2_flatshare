import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../auth/AuthContext";
import type { ListingStatus } from "../../models/listing";
import type { ReportStatus, ReportType, ViolationReportDTO } from "../../models/report";
import { landlordListingsService } from "../LandlordListings/LandlordListingsService";
import {
  adminReportsService,
  isAdminRequestError,
} from "./AdminReportsService";
import { adminListingsService } from "./AdminListingsService";

const REPORT_STATUS_CLASS: Record<ReportStatus, string> = {
  Open: "badge--open",
  UnderReview: "badge--review",
  ActionTaken: "badge--action",
  ClosedNoAction: "badge--closed",
};

const LISTING_STATUS_CLASS: Record<ListingStatus, string> = {
  Draft: "badge--draft",
  UnderReview: "badge--review",
  Active: "badge--active",
  Hidden: "badge--hidden",
  HiddenByModeration: "badge--moderation",
  Archived: "badge--archived",
};

const REPORT_STATUS_ORDER: ReportStatus[] = [
  "Open",
  "UnderReview",
  "ActionTaken",
  "ClosedNoAction",
];

type ReportGroup = {
  key: string;
  type: ReportType;
  targetId: string;
  reports: ViolationReportDTO[];
};

type ListingMeta = {
  status: ListingStatus;
  title: string;
};

function fmt(iso: string): string {
  return new Intl.DateTimeFormat(undefined, {
    day: "numeric",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(iso));
}

function canModerationHide(status: ListingStatus | null): boolean {
  return status === "Active";
}

function canReinstate(status: ListingStatus | null): boolean {
  return status === "HiddenByModeration";
}

function groupAllowsListingModeration(reports: ViolationReportDTO[]): boolean {
  return reports.some((r) => r.status === "UnderReview" || r.status === "ActionTaken");
}

function groupReports(reports: ViolationReportDTO[]): ReportGroup[] {
  const map = new Map<string, ReportGroup>();
  for (const report of reports) {
    const key = `${report.type}:${report.targetId}`;
    const existing = map.get(key);
    if (existing) {
      existing.reports.push(report);
    } else {
      map.set(key, {
        key,
        type: report.type,
        targetId: report.targetId,
        reports: [report],
      });
    }
  }

  const groups = Array.from(map.values());
  for (const group of groups) {
    group.reports.sort(
      (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    );
  }

  const priority = (group: ReportGroup) => {
    if (group.reports.some((r) => r.status === "Open")) return 0;
    if (group.reports.some((r) => r.status === "UnderReview")) return 1;
    if (group.reports.some((r) => r.status === "ActionTaken")) return 2;
    return 3;
  };

  return groups.sort((a, b) => {
    const p = priority(a) - priority(b);
    if (p !== 0) return p;
    const aLatest = new Date(a.reports[0].createdAt).getTime();
    const bLatest = new Date(b.reports[0].createdAt).getTime();
    return bLatest - aLatest;
  });
}

function countByStatus(reports: ViolationReportDTO[]): Partial<Record<ReportStatus, number>> {
  const counts: Partial<Record<ReportStatus, number>> = {};
  for (const report of reports) {
    counts[report.status] = (counts[report.status] ?? 0) + 1;
  }
  return counts;
}

type AdminReportsSectionProps = {
  pushToast: (type: "success" | "error", text: string) => void;
};

export function AdminReportsSection({ pushToast }: AdminReportsSectionProps) {
  const { t } = useTranslation();
  const { token } = useAuth();
  const [page, setPage] = useState(0);
  const [data, setData] = useState<ViolationReportDTO[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [fetchError, setFetchError] = useState<string | null>(null);
  const [pendingReportId, setPendingReportId] = useState<string | null>(null);
  const [pendingListingId, setPendingListingId] = useState<string | null>(null);
  const [banTarget, setBanTarget] = useState<ViolationReportDTO | null>(null);
  const [banReason, setBanReason] = useState("");
  const [listingMeta, setListingMeta] = useState<Record<string, ListingMeta | null>>({});
  const [expandedGroups, setExpandedGroups] = useState<Set<string>>(new Set());
  const banOverlayRef = useRef<HTMLDivElement>(null);

  const pageSize = 20;

  const groups = useMemo(() => groupReports(data), [data]);

  const loadListingMeta = useCallback(async (reports: ViolationReportDTO[]) => {
    const ids = [
      ...new Set(
        reports.filter((r) => r.type === "LISTING").map((r) => r.targetId)
      ),
    ];
    if (ids.length === 0) {
      setListingMeta({});
      return;
    }
    const entries = await Promise.all(
      ids.map(async (id) => {
        try {
          const listing = await landlordListingsService.getById(id);
          return [id, { status: listing.status, title: listing.title }] as const;
        } catch {
          return [id, null] as const;
        }
      })
    );
    setListingMeta(Object.fromEntries(entries));
  }, []);

  const load = useCallback(() => {
    if (!token) {
      setFetchError(t("adminPanel.noToken"));
      setLoading(false);
      return;
    }
    setLoading(true);
    setFetchError(null);
    adminReportsService
      .list(token, page, pageSize)
      .then(async (res) => {
        setData(res.content);
        setTotalPages(Math.max(1, res.page.totalPages));
        await loadListingMeta(res.content);
      })
      .catch((e: unknown) => {
        const msg = isAdminRequestError(e)
          ? t("adminPanel.fetchError", { status: e.status, message: e.message })
          : t("adminPanel.networkError");
        setFetchError(msg);
      })
      .finally(() => setLoading(false));
  }, [token, page, t, loadListingMeta]);

  useEffect(() => {
    load();
  }, [load]);

  useEffect(() => {
    if (loading) return;
    const urgent = groupReports(data)
      .filter((g) =>
        g.reports.some((r) => r.status === "Open" || r.status === "UnderReview")
      )
      .map((g) => g.key);
    setExpandedGroups(new Set(urgent));
  }, [page, loading]);

  const patchReport = (id: string, status: ReportStatus) => {
    setData((prev) =>
      prev.map((r) => (r.id === id ? { ...r, status } : r))
    );
  };

  const patchReportsInGroup = (
    targetId: string,
    type: ReportType,
    fromStatuses: ReportStatus[],
    toStatus: ReportStatus
  ) => {
    setData((prev) =>
      prev.map((r) =>
        r.targetId === targetId &&
        r.type === type &&
        fromStatuses.includes(r.status)
          ? { ...r, status: toStatus }
          : r
      )
    );
  };

  const patchListingStatus = (id: string, status: ListingStatus) => {
    setListingMeta((prev) => {
      const current = prev[id];
      if (!current) return prev;
      return { ...prev, [id]: { ...current, status } };
    });
  };

  const toggleGroup = (key: string) => {
    setExpandedGroups((prev) => {
      const next = new Set(prev);
      if (next.has(key)) next.delete(key);
      else next.add(key);
      return next;
    });
  };

  const handleOpenCase = async (id: string) => {
    if (!token) return;
    setPendingReportId(id);
    try {
      await adminReportsService.openCase(token, id);
      patchReport(id, "UnderReview");
      pushToast("success", t("adminPanel.reports.toastOpened"));
    } catch (e: unknown) {
      const msg = isAdminRequestError(e)
        ? t("adminPanel.fetchError", { status: e.status, message: e.message })
        : t("adminPanel.actionError");
      pushToast("error", msg);
    } finally {
      setPendingReportId(null);
    }
  };

  const handleDismiss = async (id: string) => {
    if (!token) return;
    setPendingReportId(id);
    try {
      await adminReportsService.dismiss(token, id);
      patchReport(id, "ClosedNoAction");
      pushToast("success", t("adminPanel.reports.toastDismissed"));
    } catch (e: unknown) {
      const msg = isAdminRequestError(e)
        ? t("adminPanel.fetchError", { status: e.status, message: e.message })
        : t("adminPanel.actionError");
      pushToast("error", msg);
    } finally {
      setPendingReportId(null);
    }
  };

  const handleReinstateListing = async (targetId: string) => {
    if (!token) return;
    setPendingListingId(targetId);
    try {
      await adminListingsService.reinstate(token, targetId);
      patchListingStatus(targetId, "Active");
      pushToast("success", t("adminPanel.toastReinstate"));
    } catch (e: unknown) {
      const msg = isAdminRequestError(e)
        ? t("adminPanel.fetchError", { status: e.status, message: e.message })
        : t("adminPanel.actionError");
      pushToast("error", msg);
    } finally {
      setPendingListingId(null);
    }
  };

  const handleHideListing = async (group: ReportGroup) => {
    if (!token) return;
    setPendingListingId(group.targetId);
    try {
      await adminListingsService.moderationHide(token, group.targetId);
      patchListingStatus(group.targetId, "HiddenByModeration");
      patchReportsInGroup(group.targetId, group.type, ["Open", "UnderReview"], "ActionTaken");
      pushToast("success", t("adminPanel.toastModerationHide"));
    } catch (e: unknown) {
      const msg = isAdminRequestError(e)
        ? t("adminPanel.fetchError", { status: e.status, message: e.message })
        : t("adminPanel.actionError");
      pushToast("error", msg);
    } finally {
      setPendingListingId(null);
    }
  };

  const handleBan = async () => {
    if (!token || !banTarget || !banReason.trim()) return;
    setPendingReportId(banTarget.id);
    try {
      await adminReportsService.banUser(
        token,
        banTarget.targetId,
        banTarget.id,
        banReason.trim()
      );
      patchReportsInGroup(
        banTarget.targetId,
        banTarget.type,
        ["Open", "UnderReview"],
        "ActionTaken"
      );
      pushToast("success", t("adminPanel.reports.toastBanned"));
      setBanTarget(null);
      setBanReason("");
    } catch (e: unknown) {
      const msg = isAdminRequestError(e)
        ? t("adminPanel.fetchError", { status: e.status, message: e.message })
        : t("adminPanel.actionError");
      pushToast("error", msg);
    } finally {
      setPendingReportId(null);
    }
  };

  const openCount = data.filter((r) => r.status === "Open").length;

  return (
    <>
      <p className="ap-subtitle ap-subtitle--section">
        {t("adminPanel.reports.subtitle")}
        {openCount > 0 && (
          <span className="ap-badge-count">
            {t("adminPanel.reports.openBadge", { count: openCount })}
          </span>
        )}
      </p>

      {fetchError && <div className="ap-alert ap-alert--error">{fetchError}</div>}

      {loading && (
        <div className="ap-skeleton-wrap">
          {[1, 2, 3].map((n) => (
            <div key={n} className="ap-skeleton-row" />
          ))}
        </div>
      )}

      {!loading && !fetchError && data.length === 0 && (
        <div className="ap-empty">
          <p>{t("adminPanel.reports.empty")}</p>
        </div>
      )}

      {!loading && groups.length > 0 && (
        <>
          <div className="ap-report-groups">
            {groups.map((group) => {
              const expanded = expandedGroups.has(group.key);
              const listing = listingMeta[group.targetId];
              const listingStatus =
                group.type === "LISTING" ? listing?.status ?? null : null;
              const listingBusy = pendingListingId === group.targetId;
              const groupBusy =
                listingBusy ||
                group.reports.some((r) => pendingReportId === r.id);
              const statusCounts = countByStatus(group.reports);
              const showListingModeration =
                group.type === "LISTING" && groupAllowsListingModeration(group.reports);
              const showHide =
                showListingModeration && canModerationHide(listingStatus);
              const showReinstate =
                showListingModeration && canReinstate(listingStatus);
              const hasGroupActions = showHide || showReinstate;

              return (
                <section
                  key={group.key}
                  className={`ap-report-group ${groupBusy ? "ap-report-group--busy" : ""}`}
                >
                  <div className="ap-report-group-header">
                    <button
                      type="button"
                      className="ap-report-group-toggle"
                      onClick={() => toggleGroup(group.key)}
                      aria-expanded={expanded}
                      aria-label={
                        expanded
                          ? t("adminPanel.reports.collapse")
                          : t("adminPanel.reports.expand")
                      }
                    >
                      <span
                        className={`ap-report-group-chevron ${expanded ? "ap-report-group-chevron--open" : ""}`}
                      />
                    </button>

                    <div className="ap-report-group-main">
                      <div className="ap-report-group-title-row">
                        <span className="ap-report-group-type">
                          {t(`adminPanel.reports.type.${group.type}`)}
                        </span>
                        {group.type === "LISTING" ? (
                          <Link
                            to={`/offer/${group.targetId}`}
                            className="ap-link-btn ap-report-group-target"
                          >
                            {listing?.title ?? `${group.targetId.slice(0, 8)}…`}
                          </Link>
                        ) : (
                          <span
                            className="ap-report-group-target ap-col-target"
                            title={group.targetId}
                          >
                            {group.targetId.slice(0, 8)}…
                          </span>
                        )}
                        <span className="ap-report-group-count">
                          {t("adminPanel.reports.groupReports", {
                            count: group.reports.length,
                          })}
                        </span>
                      </div>

                      <div className="ap-report-group-badges">
                        {group.type === "LISTING" && (
                          <span className="ap-report-group-badge-label">
                            {t("adminPanel.reports.colListingStatus")}:
                            {listingStatus ? (
                              <span
                                className={`ap-badge ${LISTING_STATUS_CLASS[listingStatus]}`}
                              >
                                {t(`adminPanel.status.${listingStatus}`)}
                              </span>
                            ) : (
                              <span className="ap-muted">—</span>
                            )}
                          </span>
                        )}
                        <span className="ap-report-group-badge-label">
                          {t("adminPanel.reports.colStatus")}:
                          <span className="ap-report-group-status-chips">
                            {REPORT_STATUS_ORDER.filter(
                              (status) => statusCounts[status]
                            ).map((status) => (
                              <span
                                key={status}
                                className={`ap-badge ${REPORT_STATUS_CLASS[status]}`}
                              >
                                {statusCounts[status]}×{" "}
                                {t(`adminPanel.reports.status.${status}`)}
                              </span>
                            ))}
                          </span>
                        </span>
                      </div>
                    </div>

                    {hasGroupActions && (
                      <div className="ap-report-group-actions">
                        {showHide && (
                          <button
                            className="ap-btn ap-btn--sm ap-btn--moderation"
                            disabled={listingBusy}
                            onClick={() => handleHideListing(group)}
                          >
                            {t("adminPanel.btnModerationHide")}
                          </button>
                        )}
                        {showReinstate && (
                          <button
                            className="ap-btn ap-btn--sm ap-btn--reinstate"
                            disabled={listingBusy}
                            onClick={() => handleReinstateListing(group.targetId)}
                          >
                            {t("adminPanel.btnReinstate")}
                          </button>
                        )}
                      </div>
                    )}
                  </div>

                  {expanded && (
                    <div className="ap-report-group-body">
                      <table className="ap-table ap-table--reports ap-table--nested">
                        <thead>
                          <tr>
                            <th>{t("adminPanel.reports.colReason")}</th>
                            <th>{t("adminPanel.reports.colStatus")}</th>
                            <th>{t("adminPanel.reports.colDate")}</th>
                            <th>{t("adminPanel.colActions")}</th>
                          </tr>
                        </thead>
                        <tbody>
                          {group.reports.map((row) => {
                            const busy = pendingReportId === row.id;
                            const showOpen = row.status === "Open";
                            const showDismiss = row.status === "UnderReview";
                            const showBan =
                              row.status === "UnderReview" &&
                              group.type === "USER";
                            const hasActions = showOpen || showDismiss || showBan;

                            return (
                              <tr
                                key={row.id}
                                className={busy ? "ap-row--busy" : ""}
                              >
                                <td className="ap-col-reason">
                                  <strong>{row.reason}</strong>
                                  {row.details && (
                                    <span className="ap-reason-details">
                                      {row.details}
                                    </span>
                                  )}
                                </td>
                                <td>
                                  <span
                                    className={`ap-badge ${REPORT_STATUS_CLASS[row.status]}`}
                                  >
                                    {t(`adminPanel.reports.status.${row.status}`)}
                                  </span>
                                </td>
                                <td className="ap-col-date">
                                  {fmt(row.createdAt)}
                                </td>
                                <td className="ap-col-actions">
                                  {hasActions ? (
                                    <div className="ap-actions-inner">
                                      {showOpen && (
                                        <button
                                          className="ap-btn ap-btn--sm ap-btn--approve"
                                          disabled={busy || listingBusy}
                                          onClick={() => handleOpenCase(row.id)}
                                        >
                                          {t("adminPanel.reports.btnOpen")}
                                        </button>
                                      )}
                                      {showDismiss && (
                                        <button
                                          className="ap-btn ap-btn--sm ap-btn--ghost"
                                          disabled={busy || listingBusy}
                                          onClick={() => handleDismiss(row.id)}
                                        >
                                          {t("adminPanel.reports.btnDismiss")}
                                        </button>
                                      )}
                                      {showBan && (
                                        <button
                                          className="ap-btn ap-btn--sm ap-btn--ban"
                                          disabled={busy || listingBusy}
                                          onClick={() => {
                                            setBanTarget(row);
                                            setBanReason("");
                                          }}
                                        >
                                          {t("adminPanel.reports.btnBan")}
                                        </button>
                                      )}
                                    </div>
                                  ) : (
                                    <span className="ap-muted">—</span>
                                  )}
                                </td>
                              </tr>
                            );
                          })}
                        </tbody>
                      </table>
                    </div>
                  )}
                </section>
              );
            })}
          </div>

          {totalPages > 1 && (
            <div className="ap-pagination">
              <button
                className="ap-btn ap-btn--sm ap-btn--ghost"
                disabled={page === 0 || loading}
                onClick={() => setPage((p) => Math.max(0, p - 1))}
              >
                {t("adminPanel.reports.prevPage")}
              </button>
              <span className="ap-pagination-label">
                {t("adminPanel.reports.pageInfo", {
                  current: page + 1,
                  total: totalPages,
                })}
              </span>
              <button
                className="ap-btn ap-btn--sm ap-btn--ghost"
                disabled={page >= totalPages - 1 || loading}
                onClick={() => setPage((p) => p + 1)}
              >
                {t("adminPanel.reports.nextPage")}
              </button>
            </div>
          )}
        </>
      )}

      {banTarget && (
        <div
          className="ap-overlay"
          ref={banOverlayRef}
          onClick={(e) => {
            if (e.target === banOverlayRef.current) setBanTarget(null);
          }}
          role="dialog"
          aria-modal="true"
        >
          <div className="ap-modal ap-modal--compact">
            <h3 className="ap-modal-title">
              {t("adminPanel.reports.banModalTitle")}
            </h3>
            <p className="ap-modal-desc">{t("adminPanel.reports.banModalLead")}</p>
            <label className="ap-ban-field">
              <span>{t("adminPanel.reports.banReasonLabel")}</span>
              <textarea
                value={banReason}
                onChange={(e) => setBanReason(e.target.value)}
                rows={4}
                placeholder={t("adminPanel.reports.banReasonPlaceholder")}
              />
            </label>
            <div className="ap-modal-actions">
              <button
                className="ap-btn ap-btn--ghost"
                onClick={() => setBanTarget(null)}
                disabled={pendingReportId === banTarget.id}
              >
                {t("adminPanel.reports.banCancel")}
              </button>
              <button
                className="ap-btn ap-btn--ban"
                disabled={!banReason.trim() || pendingReportId === banTarget.id}
                onClick={handleBan}
              >
                {pendingReportId === banTarget.id
                  ? t("adminPanel.reports.banSubmitting")
                  : t("adminPanel.reports.banConfirm")}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
