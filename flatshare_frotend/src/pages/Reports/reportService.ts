import { API_URL } from "../../config";
import type { CreateReportBody, ViolationReportDTO } from "../../models/report";

const jsonHeaders = (token: string) => ({
  "Content-Type": "application/json",
  Accept: "application/json",
  Authorization: `Bearer ${token}`,
});

export type ReportRequestError = {
  status: number;
  message: string;
};

export function isReportRequestError(e: unknown): e is ReportRequestError {
  return (
    typeof e === "object" &&
    e !== null &&
    "status" in e &&
    "message" in e &&
    typeof (e as ReportRequestError).status === "number" &&
    typeof (e as ReportRequestError).message === "string"
  );
}

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

export const reportService = {
  create: async (
    token: string,
    body: CreateReportBody
  ): Promise<ViolationReportDTO> => {
    const res = await fetch(`${API_URL}/api/v1/reports`, {
      method: "POST",
      headers: jsonHeaders(token),
      body: JSON.stringify(body),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies ReportRequestError;
    }
    const data = (await res.json()) as Record<string, unknown>;
    return normalizeReport(data);
  },
};
