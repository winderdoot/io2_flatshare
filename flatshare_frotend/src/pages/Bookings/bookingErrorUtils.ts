import { isBookingRequestError } from "./bookingService";

const ERROR_KEYS: Record<string, string> = {
  InvalidDateRange: "booking.errors.invalidDateRange",
  "Booking Error": "booking.errors.invalidDateRange",
  RoomNotAvailable: "booking.errors.roomNotAvailable",
  "Room Occupied": "booking.errors.roomOccupied",
  CancellationNotAllowed: "booking.errors.cancellationNotAllowed",
  Forbidden: "booking.errors.forbidden",
  "Booking not found": "booking.errors.notFound",
};

export function messageForBookingFailure(
  e: unknown,
  t: (key: string, opt?: Record<string, string>) => string
): string {
  if (isBookingRequestError(e)) {
    if (e.status === 401) return t("booking.errors.sessionExpired");
    if (e.status === 403) return t("booking.errors.forbidden");

    if (e.fieldErrors?.length) {
      return e.fieldErrors.map((fe) => fe.message).join(" ");
    }

    const key = ERROR_KEYS[e.message];
    if (key) return t(key);

    if (e.message.startsWith("Room Unavailable between")) {
      return t("booking.errors.roomUnavailable", { detail: e.message });
    }
    if (e.message.startsWith("Room no longer available between")) {
      return t("booking.errors.roomNoLongerAvailable", { detail: e.message });
    }
    if (e.message.startsWith("Cannot accept booking from status")) {
      return t("booking.errors.cannotAccept", { detail: e.message });
    }
    if (e.message.startsWith("Cannot reject booking from status")) {
      return t("booking.errors.cannotReject", { detail: e.message });
    }
    if (e.message.startsWith("Booking: Cannot cancel from status")) {
      return t("booking.errors.cannotCancel", { detail: e.message });
    }

    return t("booking.errors.detail", { message: e.message });
  }
  return t("booking.errors.network");
}
