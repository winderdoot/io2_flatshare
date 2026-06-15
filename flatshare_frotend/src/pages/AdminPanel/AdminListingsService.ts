import { API_URL } from "../../config";
import type { ListingDTO } from "../../models/listing";
import { normalizeListingDto } from "../LandlordListings/LandlordListingsService";

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

function normalizeListings(data: Record<string, unknown>[]): ListingDTO[] {
  return data.map((row) => normalizeListingDto(row));
}

export type AdminRequestError = {
  status: number;
  message: string;
};

export function isAdminRequestError(e: unknown): e is AdminRequestError {
  return (
    typeof e === "object" &&
    e !== null &&
    "status" in e &&
    "message" in e &&
    typeof (e as AdminRequestError).status === "number" &&
    typeof (e as AdminRequestError).message === "string"
  );
}

export const adminListingsService = {
  listUnderReview: async (token: string): Promise<ListingDTO[]> => {
    const res = await fetch(`${API_URL}/api/v1/listings/under-review`, {
      method: "GET",
      headers: jsonHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies AdminRequestError;
    }
    const data = (await res.json()) as Record<string, unknown>[];
    return normalizeListings(data);
  },

  listAll: async (): Promise<ListingDTO[]> => {
    const res = await fetch(`${API_URL}/api/v1/listings`, {
      method: "GET",
      headers: { Accept: "application/json" },
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies AdminRequestError;
    }
    const data = (await res.json()) as Record<string, unknown>[];
    return normalizeListings(data);
  },

  approve: async (token: string, id: string): Promise<void> => {
    const res = await fetch(`${API_URL}/api/v1/listings/${id}/approve`, {
      method: "PATCH",
      headers: jsonHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies AdminRequestError;
    }
  },

  requestFixes: async (token: string, id: string): Promise<void> => {
    const res = await fetch(`${API_URL}/api/v1/listings/${id}/request-fixes`, {
      method: "PATCH",
      headers: jsonHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies AdminRequestError;
    }
  },

  moderationHide: async (token: string, id: string): Promise<void> => {
    const res = await fetch(`${API_URL}/api/v1/listings/${id}/moderation-hide`, {
      method: "PATCH",
      headers: jsonHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies AdminRequestError;
    }
  },

  reinstate: async (token: string, id: string): Promise<void> => {
    const res = await fetch(`${API_URL}/api/v1/listings/${id}/reinstate`, {
      method: "PATCH",
      headers: jsonHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies AdminRequestError;
    }
  },

  archive: async (token: string, id: string): Promise<void> => {
    const res = await fetch(`${API_URL}/api/v1/listings/${id}/archive`, {
      method: "PATCH",
      headers: jsonHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies AdminRequestError;
    }
  },
};
