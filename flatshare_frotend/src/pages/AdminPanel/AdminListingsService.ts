import { API_URL, BACKEND_TYPE } from "../../config";
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
  /**
   * P1: dedykowany endpoint /listings/under-review
   * P2: brak dedykowanego endpointu — pobieramy wszystkie i filtrujemy po statusach recenzji
   */
  listUnderReview: async (token: string): Promise<ListingDTO[]> => {
    if (BACKEND_TYPE === "team2") {
      const res = await fetch(`${API_URL}/api/v1/listings`, {
        method: "GET",
        headers: jsonHeaders(token),
      });
      if (!res.ok) {
        const message = await readErrorMessage(res);
        throw { status: res.status, message } satisfies AdminRequestError;
      }
      const data = (await res.json()) as Record<string, unknown>[];
      const normalized = normalizeListings(data);
      return normalized.filter((l) => l.status === "UnderReview");
    }

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

  listAll: async (token?: string | null): Promise<ListingDTO[]> => {
    const headers: Record<string, string> = { Accept: "application/json" };
    const effectiveToken =
      token ?? (BACKEND_TYPE === "team2" ? localStorage.getItem("token") : null);
    if (effectiveToken) headers.Authorization = `Bearer ${effectiveToken}`;
    const res = await fetch(`${API_URL}/api/v1/listings`, {
      method: "GET",
      headers,
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies AdminRequestError;
    }
    const data = (await res.json()) as Record<string, unknown>[];
    return normalizeListings(data);
  },

  /**
   * P1: PATCH /listings/{id}/approve
   * P2: endpoint nie istnieje — adapter używa /publish (ADMIN ma do niego dostęp),
   *     co ustawia ogłoszenie od razu w stan ACTIVE.
   */
  approve: async (token: string, id: string): Promise<void> => {
    const route =
      BACKEND_TYPE === "team2"
        ? `${API_URL}/api/v1/listings/${id}/publish`
        : `${API_URL}/api/v1/listings/${id}/approve`;
    const res = await fetch(route, {
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

  /**
   * P1: PATCH /listings/{id}/moderation-hide
   * P2: /hide wymaga roli LANDLORD — niedostępny dla admina.
   *     Adapter używa /archive (dozwolone dla LANDLORD i ADMIN),
   *     co skutecznie usuwa ogłoszenie z widoku publicznego.
   */
  moderationHide: async (token: string, id: string): Promise<void> => {
    const route =
      BACKEND_TYPE === "team2"
        ? `${API_URL}/api/v1/listings/${id}/archive`
        : `${API_URL}/api/v1/listings/${id}/moderation-hide`;
    const res = await fetch(route, {
      method: "PATCH",
      headers: jsonHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies AdminRequestError;
    }
  },

  /**
   * P1: PATCH /listings/{id}/reinstate
   * P2: endpoint nie istnieje — adapter używa /publish, które przywraca widoczność
   *     (ustawia stan ACTIVE), co jest semantycznie równoważne z przywróceniem.
   */
  reinstate: async (token: string, id: string): Promise<void> => {
    const route =
      BACKEND_TYPE === "team2"
        ? `${API_URL}/api/v1/listings/${id}/publish`
        : `${API_URL}/api/v1/listings/${id}/reinstate`;
    const res = await fetch(route, {
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
