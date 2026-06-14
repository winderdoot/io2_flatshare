import { API_URL } from "../../config";
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
    type: String(raw.type ?? raw.Type ?? "") as ViolationReportDTO["type"],
    targetId: String(raw.targetId ?? raw.TargetId ?? ""),
    reason: String(raw.reason ?? raw.Reason ?? ""),
    details: String(raw.details ?? raw.Details ?? ""),
    status: String(raw.status ?? raw.Status ?? "") as ViolationReportDTO["status"],
    createdAt: String(raw.createdAt ?? raw.CreatedAt ?? ""),
  };
}

function normalizePage(raw: Record<string, unknown>): PageResponse<ViolationReportDTO> {
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
    const data = (await res.json()) as Record<string, unknown>;
    return normalizePage(data);
  },

  openCase: async (token: string, id: string): Promise<void> => {
    const res = await fetch(`${API_URL}/api/v1/admin/reports/${id}/open`, {
      method: "PATCH",
      headers: jsonHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies AdminRequestError;
    }
  },

  dismiss: async (token: string, id: string): Promise<void> => {
    const res = await fetch(`${API_URL}/api/v1/admin/reports/${id}/dismiss`, {
      method: "PATCH",
      headers: jsonHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies AdminRequestError;
    }
  },

  banUser: async (
    token: string,
    userId: string,
    reportId: string,
    reason: string
  ): Promise<void> => {
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
