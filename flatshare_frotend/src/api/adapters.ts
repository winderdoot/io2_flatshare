/**
 * Adaptery danych: projekt_2 backend → io2_flatshare frontend models
 *
 * Gdy BACKEND_TYPE === "team2", wszystkie odpowiedzi API przechodzą przez
 * funkcje z tego pliku, które normalizują różnice w nazwach pól i wartościach
 * enumów między oboma backendami.
 */

import type {
  ListingDTO,
  ListingStatus,
  ListingAttributes,
  ListingTenantProfile,
} from "../models/listing";
import type { Location } from "../models/location";
import type { BookingDTO, BookingStatus } from "../models/booking";
import type { ViolationReportDTO } from "../models/report";
import type { PaymentDTO, GatewayPaymentStatus } from "../models/payment";

// ─── Status: Listing ─────────────────────────────────────────────────────────

/**
 * Mapuje wartości ListingStatus z projektu_2 (SCREAMING_SNAKE_CASE)
 * na wartości projektu io2_flatshare (PascalCase).
 * Wartości w formacie PascalCase są przepuszczane bez zmian (kompatybilność wsteczna).
 */
export function mapTeam2ListingStatus(raw: string): ListingStatus {
  switch (raw) {
    case "DRAFT":
      return "Draft";
    case "UNDER_REVIEW":
    case "AWAITING_REVIEW":
    case "AWAITING_FIXES":
      return "UnderReview";
    case "ACTIVE":
      return "Active";
    case "HIDDEN":
      return "Hidden";
    case "ARCHIVED":
      return "Archived";
    case "HIDDEN_BY_MODERATION":
      return "HiddenByModeration";
    default:
      return raw as ListingStatus;
  }
}

// ─── Status: Booking ─────────────────────────────────────────────────────────

/**
 * Mapuje wartości BookingStatus z projektu_2 (SCREAMING_SNAKE_CASE)
 * na wartości projektu io2_flatshare (PascalCase).
 */
export function mapTeam2BookingStatus(raw: string): BookingStatus {
  switch (raw) {
    case "PENDING_APPROVAL":
      return "PendingApproval";
    case "PENDING_PAYMENT":
      return "PendingPayment";
    case "CONFIRMED":
      return "Confirmed";
    case "REJECTED":
      return "Rejected";
    case "EXPIRED":
      return "Expired";
    case "PAYMENT_FAILED":
      return "PaymentFailed";
    case "CANCELLED":
      return "Cancelled";
    default:
      return raw as BookingStatus;
  }
}

// ─── Status: Payment ─────────────────────────────────────────────────────────

export function mapTeam2PaymentStatus(raw: string): GatewayPaymentStatus {
  switch (raw.toUpperCase()) {
    case "INITIATED":
      return "Initiated";
    case "REDIRECTED":
      return "Redirected";
    case "SUCCEEDED":
    case "SUCCESS":
      return "Succeeded";
    case "FAILED":
    case "FAILURE":
      return "Failed";
    case "CANCELLED":
    case "CANCELED":
      return "Cancelled";
    default:
      return raw as GatewayPaymentStatus;
  }
}

// ─── Adapter: Listing ────────────────────────────────────────────────────────

/**
 * Różnice między backendami dla Listing:
 * - status:          SCREAMING_SNAKE (P2)  vs PascalCase (P1)
 * - availableFrom:   pole startowe (P2)    → availableSince (P1)
 * - availableSince:  pole końcowe (P2)     → availableUntil (P1)
 * - unavailability:  tablica (P2)          → unavailabilities (P1)
 * - attributes.preferredTenantProfile (P2) → attributes.profile (P1)
 * - location.houseNumber, buildingNumber, postalCode istnieją tylko w P2 (ignorowane)
 */
export function adaptTeam2Listing(raw: Record<string, unknown>): ListingDTO {
  const locRaw = (raw.location ?? {}) as Record<string, unknown>;
  const attrsRaw = (raw.attributes ?? {}) as Record<string, unknown>;

  const unavailabilities = (
    (raw.unavailability ?? raw.unavailabilities ?? []) as Array<
      Record<string, unknown>
    >
  ).map((u) => ({
    since: String(u.since ?? "").slice(0, 10),
    until: String(u.until ?? "").slice(0, 10),
    message: String(u.message ?? ""),
  }));

  const location: Location = {
    city: String(locRaw.city ?? ""),
    district: locRaw.district != null ? String(locRaw.district) : "",
    street: locRaw.street != null ? String(locRaw.street) : "",
    aptNumber: locRaw.aptNumber != null ? String(locRaw.aptNumber) : "",
  };

  const attributes: ListingAttributes = {
    petsAllowed: Boolean(attrsRaw.petsAllowed ?? false),
    nonSmokingOnly: Boolean(attrsRaw.nonSmokingOnly ?? false),
    closeToShops: Boolean(attrsRaw.closeToShops ?? false),
    profile: String(
      attrsRaw.preferredTenantProfile ?? attrsRaw.profile ?? "Student"
    ) as ListingTenantProfile,
  };

  return {
    id: String(raw.id ?? ""),
    ownerId: raw.ownerId != null ? String(raw.ownerId) : undefined,
    status: mapTeam2ListingStatus(String(raw.status ?? "")),
    title: String(raw.title ?? ""),
    description: String(raw.description ?? ""),
    price: Number(raw.price ?? 0),
    currency: String(raw.currency ?? ""),
    // P2: availableFrom (start) → P1: availableSince (start)
    // P2: availableSince (end)  → P1: availableUntil (end)
    availableSince: String(raw.availableFrom ?? "").slice(0, 10),
    availableUntil: String(raw.availableSince ?? "").slice(0, 10),
    ownerContact: String(raw.ownerContact ?? raw.contact ?? ""),
    area: Number(raw.area ?? 0),
    location,
    attributes,
    unavailabilities,
    createdAt: raw.createdAt != null ? String(raw.createdAt) : undefined,
  };
}

// ─── Adapter: Booking ────────────────────────────────────────────────────────

function deriveBookingPaymentStatus(
  status: BookingStatus
): BookingDTO["paymentStatus"] {
  switch (status) {
    case "Confirmed":
      return "SUCCEEDED";
    case "PendingPayment":
    case "PaymentFailed":
      return "PENDING";
    default:
      return "NOT_APPLICABLE";
  }
}

/**
 * Różnice między backendami dla Booking:
 * - since / until  (P2) → startDate / endDate (P1)
 * - totalCost      (P2) → totalPrice (P1)
 * - status:        SCREAMING_SNAKE (P2) → PascalCase (P1)
 * - paymentStatus: w P2 nie istnieje jako pole — wyznaczany ze statusu
 */
export function adaptTeam2Booking(raw: Record<string, unknown>): BookingDTO {
  const status = mapTeam2BookingStatus(String(raw.status ?? ""));
  return {
    id: String(raw.id ?? ""),
    listingId: String(raw.listingId ?? ""),
    tenantId: String(raw.tenantId ?? ""),
    startDate: String(raw.since ?? raw.startDate ?? "").slice(0, 10),
    endDate: String(raw.until ?? raw.endDate ?? "").slice(0, 10),
    totalPrice: Number(raw.totalCost ?? raw.totalPrice ?? 0),
    currency: String(raw.currency ?? ""),
    status,
    paymentStatus: deriveBookingPaymentStatus(status),
  };
}

// ─── Adapter: Payment ────────────────────────────────────────────────────────

/**
 * Różnice między backendami dla Payment:
 * - trasa: /api/payment (P1) vs /api/v1/payments (P2)
 * - paymentId: może być zwrócone jako "id" lub "paymentId"
 */
export function adaptTeam2Payment(raw: Record<string, unknown>): PaymentDTO {
  return {
    paymentId: String(raw.paymentId ?? raw.id ?? ""),
    bookingId: String(raw.bookingId ?? ""),
    status: mapTeam2PaymentStatus(String(raw.status ?? "")),
    totalValue: Number(raw.totalValue ?? raw.amount ?? raw.total ?? 0),
    currency: String(raw.currency ?? ""),
  };
}

// ─── Adapter: Report ─────────────────────────────────────────────────────────

/**
 * Różnice między backendami dla ViolationReport:
 * - targetType (P2: enum LISTING/USER) → type (P1)
 * - id może być zwrócone jako "reportId"
 */
export function adaptTeam2Report(
  raw: Record<string, unknown>
): ViolationReportDTO {
  return {
    id: String(raw.id ?? raw.reportId ?? ""),
    type: String(
      raw.type ?? raw.targetType ?? ""
    ) as ViolationReportDTO["type"],
    targetId: String(raw.targetId ?? ""),
    reason: String(raw.reason ?? ""),
    details: String(raw.details ?? ""),
    status: String(raw.status ?? "") as ViolationReportDTO["status"],
    createdAt: String(raw.createdAt ?? ""),
  };
}

// ─── Adapter: Request body (P1 → P2) ─────────────────────────────────────────

/**
 * Mapuje wartość profilu najemcy z formatu P1 (PascalCase) na format P2
 * (wartości EnumMember: "student", "tourist", "workingPerson", "none").
 */
function mapProfileToTeam2(profile: string): string {
  switch (profile) {
    case "Student":
      return "student";
    case "Tourist":
      return "tourist";
    default:
      return profile.charAt(0).toLowerCase() + profile.slice(1);
  }
}

/**
 * Przekształca ciało żądania CREATE z formatu P1 na format P2.
 * Różnice:
 * - availableSince (P1 start) → availableFrom (P2 start)
 * - availableUntil (P1 end)   → availableSince (P2 end)
 * - attributes.profile "Student" → "student" (EnumMember lowercase)
 */
export function adaptCreateListingBodyForTeam2(
  body: Record<string, unknown>
): Record<string, unknown> {
  const attrs = (body.attributes ?? {}) as Record<string, unknown>;
  return {
    ...body,
    availableFrom: body.availableSince,
    availableSince: body.availableUntil,
    availableUntil: undefined,
    attributes: {
      ...attrs,
      profile: mapProfileToTeam2(String(attrs.profile ?? "")),
    },
  };
}

// ─── Stałe statusów P2 dla admin ─────────────────────────────────────────────

/**
 * Wartości ViolationReportStatus z backendu projekt_2 (JsonStringEnumConverter → PascalCase).
 * Używane przy wywołaniu PATCH /api/v1/admin/reports/{id}/status.
 */
export const TEAM2_REPORT_STATUS = {
  open: "Open",
  underReview: "UnderReview",
  actionTaken: "ActionTaken",
  closedNoAction: "ClosedNoAction",
} as const;
