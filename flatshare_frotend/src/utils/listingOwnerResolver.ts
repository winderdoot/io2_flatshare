import type { User } from "../models/user";
import {
  landlordListingsService,
  normalizeListingDto,
} from "../pages/LandlordListings/LandlordListingsService";
import { API_URL, BACKEND_TYPE } from "../config";

function extractOwnerId(raw: Record<string, unknown>): string | undefined {
  const direct = raw.ownerId ?? raw.OwnerId;
  if (direct) return String(direct);

  const owner = (raw.owner ?? raw.Owner) as Record<string, unknown> | undefined;
  if (owner?.id ?? owner?.Id) {
    return String(owner.id ?? owner.Id);
  }

  return undefined;
}

export async function fetchListingOwnerId(listingId: string): Promise<string | undefined> {
  const headers: Record<string, string> = { Accept: "application/json" };
  if (BACKEND_TYPE === "team2") {
    const token = localStorage.getItem("token");
    if (token) headers.Authorization = `Bearer ${token}`;
  }
  const res = await fetch(`${API_URL}/api/v1/listings/${listingId}`, {
    method: "GET",
    headers,
  });
  if (!res.ok) return undefined;

  const raw = (await res.json()) as Record<string, unknown>;
  const normalized = normalizeListingDto(raw);
  return normalized.ownerId ?? extractOwnerId(raw);
}

export async function resolveListingOwnerId(
  listingId: string,
  currentUser?: Pick<User, "id" | "role"> | null
): Promise<string | undefined> {
  if (currentUser?.role === "LANDLORD") {
    try {
      const token = localStorage.getItem("token");
      const own = await landlordListingsService.listByOwner(currentUser.id, token);
      if (own.some((l) => l.id === listingId)) {
        return currentUser.id;
      }
    } catch {
      /* ignore */
    }
  }

  try {
    const listing = await landlordListingsService.getById(listingId);
    if (listing.ownerId) return listing.ownerId;
  } catch {
    /* ignore */
  }

  return fetchListingOwnerId(listingId);
}
