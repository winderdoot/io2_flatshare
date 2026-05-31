import type { BookingStatus } from "../../models/booking";

export function formatBookingDate(
  iso: string,
  locale: string
): string {
  const d = new Date(iso + "T12:00:00");
  return new Intl.DateTimeFormat(locale, {
    day: "numeric",
    month: "long",
    year: "numeric",
  }).format(d);
}

export function formatDateTime(
  iso: string,
  locale: string
): string {
  const d = new Date(iso);
  return new Intl.DateTimeFormat(locale, {
    day: "numeric",
    month: "long",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(d);
}

export function statusClassName(status: BookingStatus): string {
  switch (status) {
    case "PendingApproval":
      return "bookings-status bookings-status--pendingApproval";
    case "PendingPayment":
      return "bookings-status bookings-status--pendingPayment";
    case "Confirmed":
      return "bookings-status bookings-status--confirmed";
    case "Rejected":
      return "bookings-status bookings-status--rejected";
    case "Cancelled":
      return "bookings-status bookings-status--cancelled";
    case "Expired":
      return "bookings-status bookings-status--expired";
    case "PaymentFailed":
      return "bookings-status bookings-status--paymentFailed";
    default:
      return "bookings-status";
  }
}

export function canTenantCancel(status: BookingStatus): boolean {
  return (
    status === "PendingApproval" ||
    status === "PendingPayment" ||
    status === "Confirmed"
  );
}

export function canLandlordAcceptReject(status: BookingStatus): boolean {
  return status === "PendingApproval";
}
