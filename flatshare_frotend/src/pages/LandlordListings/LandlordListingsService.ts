import { API_URL, BACKEND_TYPE } from "../../config";
import { adaptTeam2Listing, adaptCreateListingBodyForTeam2 } from "../../api/adapters";
import type {
  ListingAttributes,
  ListingDTO,
  ListingStatus,
  Unavailability,
} from "../../models/listing";

type RawUnavailability = {
  since?: string;
  until?: string;
  Since?: string;
  Until?: string;
  message?: string;
  Message?: string;
};

function normalizeUnavailability(raw: RawUnavailability): Unavailability {
  return {
    since: (raw.since ?? raw.Since ?? "").slice(0, 10),
    until: (raw.until ?? raw.Until ?? "").slice(0, 10),
    message: raw.message ?? raw.Message ?? "",
  };
}

/**
 * Normalizuje odpowiedź API do formatu ListingDTO.
 * Obsługuje:
 * - team1: camelCase / PascalCase, statusy PascalCase
 * - team2: pola availableFrom/availableSince, statusy SCREAMING_SNAKE
 */
export function normalizeListingDto(raw: Record<string, unknown>): ListingDTO {
  if (BACKEND_TYPE === "team2") {
    return adaptTeam2Listing(raw);
  }

  const periods = (raw.unavailabilities ?? raw.Unavailabilities) as
    | RawUnavailability[]
    | undefined;

  const base = raw as unknown as ListingDTO;
  const ownerRaw = raw.ownerId ?? raw.OwnerId;
  return {
    ...base,
    id: String(raw.id ?? raw.Id ?? base.id),
    ownerId: ownerRaw != null ? String(ownerRaw) : base.ownerId,
    unavailabilities: periods?.map(normalizeUnavailability) ?? [],
  };
}
import type { Location } from "../../models/location";

const authHeaders = (token: string) => ({
  "Content-Type": "application/json",
  Accept: "application/json",
  Authorization: `Bearer ${token}`,
});

export type ListingRequestError = {
  status: number;
  message: string;
};

export function isListingRequestError(e: unknown): e is ListingRequestError {
  return (
    typeof e === "object" &&
    e !== null &&
    "status" in e &&
    "message" in e &&
    typeof (e as ListingRequestError).status === "number" &&
    typeof (e as ListingRequestError).message === "string"
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

export type CreateListingBody = {
  title: string;
  description: string;
  price: number;
  currency: string;
  availableSince: string;
  availableUntil: string;
  ownerContact: string;
  area: number;
  location: Location;
  attributes: ListingAttributes;
};

export type UnavailabilityRangeBody = Pick<Unavailability, "since" | "until">;

export type UpdateListingBody = {
  title?: string;
  description?: string;
  price?: number;
  currency?: string;
  availableSince?: string;
  availableUntil?: string;
  ownerContact?: string;
  area?: number;
  location?: Location;
  attributes?: ListingAttributes;
};

export type ListingCreatedResponse = {
  id: string;
  status: ListingStatus;
  createdAt: string;
};

/**
 * Dla team2 wszystkie endpointy GET wymagają autoryzacji.
 * Jeśli token nie jest podany jawnie, próbujemy odczytać go z localStorage.
 */
const optionalAuthHeaders = (token?: string | null): Record<string, string> => {
  const headers: Record<string, string> = { Accept: "application/json" };
  const effectiveToken =
    token ?? (BACKEND_TYPE === "team2" ? localStorage.getItem("token") : null);
  if (effectiveToken) headers.Authorization = `Bearer ${effectiveToken}`;
  return headers;
};

export const landlordListingsService = {
  listByOwner: async (ownerId: string, token?: string | null): Promise<ListingDTO[]> => {
    const q = new URLSearchParams({ ownerId });
    const res = await fetch(`${API_URL}/api/v1/listings?${q}`, {
      method: "GET",
      headers: optionalAuthHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies ListingRequestError;
    }
    const data = (await res.json()) as Record<string, unknown>[];
    return data.map((row) => normalizeListingDto(row));
  },

  getById: async (id: string, token?: string | null): Promise<ListingDTO> => {
    const res = await fetch(`${API_URL}/api/v1/listings/${id}`, {
      method: "GET",
      headers: optionalAuthHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies ListingRequestError;
    }
    const data = (await res.json()) as Record<string, unknown>;
    return normalizeListingDto(data);
  },

  create: async (
    token: string,
    body: CreateListingBody
  ): Promise<ListingCreatedResponse> => {
    const payload =
      BACKEND_TYPE === "team2"
        ? adaptCreateListingBodyForTeam2(body as unknown as Record<string, unknown>)
        : body;
    const res = await fetch(`${API_URL}/api/v1/listings`, {
      method: "POST",
      headers: authHeaders(token),
      body: JSON.stringify(payload),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies ListingRequestError;
    }
    return res.json();
  },

  update: async (
    token: string,
    id: string,
    body: UpdateListingBody
  ): Promise<ListingDTO> => {
    const payload =
      BACKEND_TYPE === "team2"
        ? adaptCreateListingBodyForTeam2(body as unknown as Record<string, unknown>)
        : body;
    const res = await fetch(`${API_URL}/api/v1/listings/${id}`, {
      method: "PATCH",
      headers: authHeaders(token),
      body: JSON.stringify(payload),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies ListingRequestError;
    }
    return res.json();
  },

  submit: async (token: string, id: string): Promise<void> => {
    const res = await fetch(`${API_URL}/api/v1/listings/${id}/submit`, {
      method: "PATCH",
      headers: authHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies ListingRequestError;
    }
  },

  publish: async (token: string, id: string): Promise<void> => {
    const res = await fetch(`${API_URL}/api/v1/listings/${id}/publish`, {
      method: "PATCH",
      headers: authHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies ListingRequestError;
    }
  },

  hide: async (token: string, id: string): Promise<void> => {
    const res = await fetch(`${API_URL}/api/v1/listings/${id}/hide`, {
      method: "PATCH",
      headers: authHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies ListingRequestError;
    }
  },

  archive: async (token: string, id: string): Promise<void> => {
    const res = await fetch(`${API_URL}/api/v1/listings/${id}/archive`, {
      method: "PATCH",
      headers: authHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies ListingRequestError;
    }
  },

  addUnavailability: async (
    token: string,
    id: string,
    body: Unavailability
  ): Promise<void> => {
    const res = await fetch(`${API_URL}/api/v1/listings/${id}/unavailability`, {
      method: "POST",
      headers: authHeaders(token),
      body: JSON.stringify(body),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies ListingRequestError;
    }
  },

  removeUnavailability: async (
    token: string,
    id: string,
    body: UnavailabilityRangeBody
  ): Promise<void> => {
    if (BACKEND_TYPE === "team2") {
      throw {
        status: 501,
        message:
          "Usuwanie okresu niedostępności nie jest obsługiwane przez backend drugiego zespołu.",
      } satisfies ListingRequestError;
    }
    const res = await fetch(`${API_URL}/api/v1/listings/${id}/unavailability`, {
      method: "DELETE",
      headers: authHeaders(token),
      body: JSON.stringify(body),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies ListingRequestError;
    }
  },
};
