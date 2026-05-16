import type { Location } from "./location";

/** Wartości zgodne z serializacją enum `Listing.ListingStatus` (JsonStringEnumConverter). */
export type ListingStatus =
  | "Draft"
  | "UnderReview"
  | "Active"
  | "Hidden"
  | "HiddenByModeration"
  | "Archived";

/** Odpowiada `ListingAttributes.TenantProfile` z API. */
export type ListingTenantProfile = "Student" | "Tourist";

/**
 * Odpowiada `flatshare_server.Infrastructure.Model.Listings.ListingAttributes`
 * (serializacja JSON camelCase).
 */
export type ListingAttributes = {
  petsAllowed: boolean;
  nonSmokingOnly: boolean;
  closeToShops: boolean;
  profile: ListingTenantProfile;
};

/** Odpowiada `Unavailability` z API. */
export type Unavailability = {
  since: string;
  until: string;
  message: string;
};

/**
 * Odpowiada `ListingDTO` z API (`GET api/v1/Listings/{id}`).
 * Zdjęcia: encja ma `Photos`, ale DTO na razie ich nie zwraca — na froncie
 * używamy opcjonalnego `coverImageUrl` do czasu rozszerzenia API.
 */
export type ListingDTO = {
  id: string;
  status: ListingStatus;
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
  unavailabilities?: Unavailability[];
  coverImageUrl?: string;
};
