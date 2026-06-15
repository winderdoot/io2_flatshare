import { API_URL, BACKEND_TYPE } from "../../config";
import { TEAM2_REPORT_STATUS } from "../../api/adapters";
import type { PageResponse, ViolationReportDTO } from "../../models/report";
import { isAdminRequestError, type AdminRequestError } from "./AdminListingsService";

const jsonHeaders = (token: string) => ({
  "Content-Type": "application/json",
  Accept: "application/json",
  Authorization: `Bearer ${token}`,
});

async function readErrorMessage(res: Response): Promise<string> {
  try {
    const data = (await res.json()) as { error?: string; title?: string };
    if (typeof data.error === "string") return data.error;
    if (typeof data.title === "string") return data.title;
  } catch {
    /* ignore */
  }
  return res.statusText || `HTTP ${res.status}`;
}

function normalizeReport(raw: Record<string, unknown>): ViolationReportDTO {
  return {
    id: String(raw.id ?? raw.Id ?? ""),
    // team2 używa "targetType" zamiast "type"
    type: String(
      raw.type ?? raw.Type ?? raw.targetType ?? raw.TargetType ?? ""
    ) as ViolationReportDTO["type"],
    targetId: String(raw.targetId ?? raw.TargetId ?? ""),
    reason: String(raw.reason ?? raw.Reason ?? ""),
    details: String(raw.details ?? raw.Details ?? ""),
    status: String(raw.status ?? raw.Status ?? "") as ViolationReportDTO["status"],
    createdAt: String(raw.createdAt ?? raw.CreatedAt ?? ""),
  };
}

function normalizePage(
  raw: Record<string, unknown> | Record<string, unknown>[]
): PageResponse<ViolationReportDTO> {
  // team2 zwraca tablicę zamiast obiektu paginowanego
  if (Array.isArray(raw)) {
    return {
      content: raw.map(normalizeReport),
      page: {
        size: raw.length,
        number: 0,
        totalElements: raw.length,
        totalPages: 1,
      },
    };
  }
  const contentRaw = (raw.content ?? raw.Content ?? []) as Record<string, unknown>[];
  const pageRaw = (raw.page ?? raw.Page ?? {}) as Record<string, unknown>;
  return {
    content: contentRaw.map(normalizeReport),
    page: {
      size: Number(pageRaw.size ?? pageRaw.Size ?? 0),
      number: Number(pageRaw.number ?? pageRaw.Number ?? 0),
      totalElements: Number(pageRaw.totalElements ?? pageRaw.TotalElements ?? 0),
      totalPages: Number(pageRaw.totalPages ?? pageRaw.TotalPages ?? 0),
    },
  };
}

export const adminReportsService = {
  list: async (
    token: string,
    page = 0,
    size = 20
  ): Promise<PageResponse<ViolationReportDTO>> => {
    const q = new URLSearchParams({
      page: String(page),
      size: String(size),
    });
    const res = await fetch(`${API_URL}/api/v1/admin/reports?${q}`, {
      method: "GET",
      headers: jsonHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies AdminRequestError;
    }
    const data = (await res.json()) as
      | Record<string, unknown>
      | Record<string, unknown>[];
    return normalizePage(data);
  },

  /**
   * P1: PATCH /admin/reports/{id}/open
   * P2: PATCH /admin/reports/{id}/status z body "UnderReview"
   */
  openCase: async (token: string, id: string): Promise<void> => {
    if (BACKEND_TYPE === "team2") {
      const res = await fetch(`${API_URL}/api/v1/admin/reports/${id}/status`, {
        method: "PATCH",
        headers: jsonHeaders(token),
        body: JSON.stringify(TEAM2_REPORT_STATUS.underReview),
      });
      if (!res.ok) {
        const message = await readErrorMessage(res);
        throw { status: res.status, message } satisfies AdminRequestError;
      }
      return;
    }
    const res = await fetch(`${API_URL}/api/v1/admin/reports/${id}/open`, {
      method: "PATCH",
      headers: jsonHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies AdminRequestError;
    }
  },

  /**
   * P1: PATCH /admin/reports/{id}/dismiss
   * P2: PATCH /admin/reports/{id}/status z body "ClosedNoAction"
   */
  dismiss: async (token: string, id: string): Promise<void> => {
    if (BACKEND_TYPE === "team2") {
      const res = await fetch(`${API_URL}/api/v1/admin/reports/${id}/status`, {
        method: "PATCH",
        headers: jsonHeaders(token),
        body: JSON.stringify(TEAM2_REPORT_STATUS.closedNoAction),
      });
      if (!res.ok) {
        const message = await readErrorMessage(res);
        throw { status: res.status, message } satisfies AdminRequestError;
      }
      return;
    }
    const res = await fetch(`${API_URL}/api/v1/admin/reports/${id}/dismiss`, {
      method: "PATCH",
      headers: jsonHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies AdminRequestError;
    }
  },

  /**
   * P1: POST /admin/users/{userId}/ban?reportId={reportId}  (reportId wymagany)
   * P2: POST /admin/users/{userId}/ban  z body { reason }   (brak reportId)
   */
  banUser: async (
    token: string,
    userId: string,
    reportId: string,
    reason: string
  ): Promise<void> => {
    if (BACKEND_TYPE === "team2") {
      const res = await fetch(`${API_URL}/api/v1/admin/users/${userId}/ban`, {
        method: "POST",
        headers: jsonHeaders(token),
        body: JSON.stringify({ reason }),
      });
      if (!res.ok) {
        const message = await readErrorMessage(res);
        throw { status: res.status, message } satisfies AdminRequestError;
      }
      return;
    }
    const q = new URLSearchParams({ reportId });
    const res = await fetch(
      `${API_URL}/api/v1/admin/users/${userId}/ban?${q}`,
      {
        method: "POST",
        headers: jsonHeaders(token),
        body: JSON.stringify({ reason }),
      }
    );
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies AdminRequestError;
    }
  },
};

export { isAdminRequestError };
