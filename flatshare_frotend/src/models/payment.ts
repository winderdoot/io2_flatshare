export type GatewayPaymentStatus =
  | "Initiated"
  | "Redirected"
  | "Succeeded"
  | "Failed"
  | "Cancelled";

export type PaymentDTO = {
  paymentId: string;
  bookingId: string;
  status: GatewayPaymentStatus;
  totalValue: number;
  currency: string;
};

export type PaymentInitiatedResponse = {
  paymentId: string;
  bookingId: string;
  status: string;
  redirectUrl: string;
  amount: number;
  currency: string;
};
