import { API_URL } from "../../config";
import type { PaymentDTO } from "../../models/payment";
import { isBookingRequestError, type BookingRequestError } from "./bookingService";

const authHeaders = (token: string) => ({
  Accept: "application/json",
  Authorization: `Bearer ${token}`,
});

async function readError(res: Response): Promise<BookingRequestError> {
  try {
    const data = (await res.json()) as {
      error?: string;
      title?: string;
    };
    return {
      status: res.status,
      message:
        typeof data.error === "string"
          ? data.error
          : typeof data.title === "string"
            ? data.title
            : res.statusText || `HTTP ${res.status}`,
    };
  } catch {
    return {
      status: res.status,
      message: res.statusText || `HTTP ${res.status}`,
    };
  }
}

function normalizePayment(raw: Record<string, unknown>): PaymentDTO {
  return {
    paymentId: String(raw.paymentId ?? raw.PaymentId ?? ""),
    bookingId: String(raw.bookingId ?? raw.BookingId ?? ""),
    status: String(raw.status ?? raw.Status ?? "") as PaymentDTO["status"],
    totalValue: Number(raw.totalValue ?? raw.TotalValue ?? 0),
    currency: String(raw.currency ?? raw.Currency ?? ""),
  };
}

export { isBookingRequestError };

export const paymentService = {
  getByBookingId: async (
    token: string,
    bookingId: string
  ): Promise<PaymentDTO> => {
    const q = new URLSearchParams({ bookingId });
    const res = await fetch(`${API_URL}/api/payment?${q}`, {
      method: "GET",
      headers: authHeaders(token),
    });
    if (!res.ok) throw await readError(res);
    const data = (await res.json()) as Record<string, unknown>;
    return normalizePayment(data);
  },

  getById: async (token: string, paymentId: string): Promise<PaymentDTO> => {
    const res = await fetch(`${API_URL}/api/payment/${paymentId}`, {
      method: "GET",
      headers: authHeaders(token),
    });
    if (!res.ok) throw await readError(res);
    const data = (await res.json()) as Record<string, unknown>;
    return normalizePayment(data);
  },
};
