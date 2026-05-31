export type BookingStatus =
  | "PendingApproval"
  | "PendingPayment"
  | "Confirmed"
  | "Rejected"
  | "Expired"
  | "PaymentFailed"
  | "Cancelled";

export type BookingPaymentStatus = "SUCCEEDED" | "PENDING";

export type BookingDTO = {
  id: string;
  listingId: string;
  tenantId: string;
  startDate: string;
  endDate: string;
  totalPrice: number;
  currency: string;
  status: BookingStatus;
  paymentStatus: BookingPaymentStatus;
};

export type BookingCreatedResponse = {
  bookingId: string;
  status: string;
  createdAt: string;
  totalPrice: number;
  currency: string;
  resourceLink: string;
};

export type AcceptBookingResponse = {
  bookingId: string;
  status: string;
  acceptedAt: string;
  paymentRequiredUntil: string;
};

export type RejectBookingResponse = {
  bookingId: string;
  status: string;
  rejectedAt: string;
  reason: string;
};

export type CancelBookingResponse = {
  bookingId: string;
  status: string;
  cancelledAt: string;
  refundStatus: string;
};

export type CreateBookingBody = {
  listingId: string;
  startDate: string;
  endDate: string;
};

export type ReasonBody = {
  reason: string;
};
