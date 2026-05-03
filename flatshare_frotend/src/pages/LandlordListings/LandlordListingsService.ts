import { API_URL } from "../../config";
import type { ListingAttributes, ListingDTO, ListingStatus } from "../../models/listing";
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

export const landlordListingsService = {
  listByOwner: async (ownerId: string): Promise<ListingDTO[]> => {
    const q = new URLSearchParams({ ownerId });
    const res = await fetch(`${API_URL}/api/v1/listings?${q}`, {
      method: "GET",
      headers: { Accept: "application/json" },
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies ListingRequestError;
    }
    return res.json();
  },

  getById: async (id: string): Promise<ListingDTO> => {
    const res = await fetch(`${API_URL}/api/v1/listings/${id}`, {
      method: "GET",
      headers: { Accept: "application/json" },
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies ListingRequestError;
    }
    return res.json();
  },

  create: async (
    token: string,
    body: CreateListingBody
  ): Promise<ListingCreatedResponse> => {
    const res = await fetch(`${API_URL}/api/v1/listings`, {
      method: "POST",
      headers: authHeaders(token),
      body: JSON.stringify(body),
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
    const res = await fetch(`${API_URL}/api/v1/listings/${id}`, {
      method: "PATCH",
      headers: authHeaders(token),
      body: JSON.stringify(body),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies ListingRequestError;
    }
    return res.json();
  },
};
