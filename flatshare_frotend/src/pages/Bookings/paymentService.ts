import { API_URL, BACKEND_TYPE } from "../../config";
import { adaptTeam2Payment } from "../../api/adapters";
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

/**
 * P1: /api/payment  (brak prefiksu v1)
 * P2: /api/v1/payments
 */
const paymentBase = () =>
  BACKEND_TYPE === "team2" ? `${API_URL}/api/v1/payments` : `${API_URL}/api/payment`;

function selectNormalizer(raw: Record<string, unknown>): PaymentDTO {
  return BACKEND_TYPE === "team2" ? adaptTeam2Payment(raw) : normalizePayment(raw);
}

export const paymentService = {
  getByBookingId: async (
    token: string,
    bookingId: string
  ): Promise<PaymentDTO> => {
    const q = new URLSearchParams({ bookingId });
    const res = await fetch(`${paymentBase()}?${q}`, {
      method: "GET",
      headers: authHeaders(token),
    });
    if (!res.ok) throw await readError(res);
    const data = (await res.json()) as Record<string, unknown>;
    return selectNormalizer(data);
  },

  getById: async (token: string, paymentId: string): Promise<PaymentDTO> => {
    const res = await fetch(`${paymentBase()}/${paymentId}`, {
      method: "GET",
      headers: authHeaders(token),
    });
    if (!res.ok) throw await readError(res);
    const data = (await res.json()) as Record<string, unknown>;
    return selectNormalizer(data);
  },
};
