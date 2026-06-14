import { API_URL, BACKEND_TYPE } from "../../config";
import { adaptTeam2Booking } from "../../api/adapters";
import type {
  AcceptBookingResponse,
  BookingCreatedResponse,
  BookingDTO,
  CancelBookingResponse,
  CreateBookingBody,
  PayBookingBody,
  ReasonBody,
  RejectBookingResponse,
} from "../../models/booking";
import type { PaymentInitiatedResponse } from "../../models/payment";

const authHeaders = (token: string) => ({
  "Content-Type": "application/json",
  Accept: "application/json",
  Authorization: `Bearer ${token}`,
});

/** Backend ignores this segment; any GUID satisfies the route. */
const ME_BOOKING_ID_PLACEHOLDER = "00000000-0000-0000-0000-000000000000";

export type BookingRequestError = {
  status: number;
  message: string;
  fieldErrors?: { field: string; message: string }[];
};

export function isBookingRequestError(e: unknown): e is BookingRequestError {
  return (
    typeof e === "object" &&
    e !== null &&
    "status" in e &&
    "message" in e &&
    typeof (e as BookingRequestError).status === "number" &&
    typeof (e as BookingRequestError).message === "string"
  );
}

async function readError(res: Response): Promise<BookingRequestError> {
  try {
    const data = (await res.json()) as {
      error?: string;
      title?: string;
      fieldErrors?: { field: string; message: string }[];
    };
    return {
      status: res.status,
      message:
        typeof data.error === "string"
          ? data.error
          : typeof data.title === "string"
            ? data.title
            : res.statusText || `HTTP ${res.status}`,
      fieldErrors: data.fieldErrors,
    };
  } catch {
    return {
      status: res.status,
      message: res.statusText || `HTTP ${res.status}`,
    };
  }
}

function derivePaymentStatus(status: string): BookingDTO["paymentStatus"] {
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

function normalizeBooking(raw: Record<string, unknown>): BookingDTO {
  if (BACKEND_TYPE === "team2") {
    return adaptTeam2Booking(raw);
  }

  const status = String(raw.status ?? raw.Status ?? "") as BookingDTO["status"];
  const explicitPaymentStatus = raw.paymentStatus ?? raw.PaymentStatus;

  return {
    id: String(raw.id ?? raw.Id ?? ""),
    listingId: String(raw.listingId ?? raw.ListingId ?? ""),
    tenantId: String(raw.tenantId ?? raw.TenantId ?? ""),
    startDate: String(raw.startDate ?? raw.StartDate ?? "").slice(0, 10),
    endDate: String(raw.endDate ?? raw.EndDate ?? "").slice(0, 10),
    totalPrice: Number(raw.totalPrice ?? raw.TotalPrice ?? 0),
    currency: String(raw.currency ?? raw.Currency ?? ""),
    status,
    paymentStatus:
      typeof explicitPaymentStatus === "string" && explicitPaymentStatus !== ""
        ? (explicitPaymentStatus as BookingDTO["paymentStatus"])
        : derivePaymentStatus(status),
  };
}

export const bookingService = {
  create: async (
    token: string,
    body: CreateBookingBody
  ): Promise<BookingCreatedResponse> => {
    const res = await fetch(`${API_URL}/api/v1/bookings`, {
      method: "POST",
      headers: authHeaders(token),
      body: JSON.stringify(body),
    });
    if (!res.ok) throw await readError(res);
    return res.json();
  },

  listForCurrentUser: async (token: string): Promise<BookingDTO[]> => {
    const url =
      BACKEND_TYPE === "team2"
        ? `${API_URL}/api/v1/bookings/me`
        : `${API_URL}/api/v1/bookings/${ME_BOOKING_ID_PLACEHOLDER}/me`;
    const res = await fetch(url, {
      method: "GET",
      headers: authHeaders(token),
    });
    if (!res.ok) throw await readError(res);
    const data = (await res.json()) as Record<string, unknown>[];
    return data.map(normalizeBooking);
  },

  getById: async (token: string, bookingId: string): Promise<BookingDTO> => {
    const res = await fetch(`${API_URL}/api/v1/bookings/${bookingId}`, {
      method: "GET",
      headers: authHeaders(token),
    });
    if (!res.ok) throw await readError(res);
    const data = (await res.json()) as Record<string, unknown>;
    return normalizeBooking(data);
  },

  accept: async (
    token: string,
    bookingId: string
  ): Promise<AcceptBookingResponse> => {
    const res = await fetch(`${API_URL}/api/v1/bookings/${bookingId}/accept`, {
      method: "POST",
      headers: authHeaders(token),
    });
    if (!res.ok) throw await readError(res);
    // Team2 zwraca 200 OK z pustym ciałem — obsługujemy oba przypadki
    const text = await res.text();
    if (!text) {
      return {
        bookingId,
        status: "PendingPayment",
        acceptedAt: new Date().toISOString(),
        paymentRequiredUntil: "",
      };
    }
    return JSON.parse(text) as AcceptBookingResponse;
  },

  reject: async (
    token: string,
    bookingId: string,
    body: ReasonBody
  ): Promise<RejectBookingResponse> => {
    const res = await fetch(`${API_URL}/api/v1/bookings/${bookingId}/reject`, {
      method: "POST",
      headers: authHeaders(token),
      body: JSON.stringify(body),
    });
    if (!res.ok) throw await readError(res);
    return res.json();
  },

  cancel: async (
    token: string,
    bookingId: string,
    body: ReasonBody
  ): Promise<CancelBookingResponse> => {
    const res = await fetch(`${API_URL}/api/v1/bookings/${bookingId}/cancel`, {
      method: "POST",
      headers: authHeaders(token),
      body: JSON.stringify(body),
    });
    if (!res.ok) throw await readError(res);
    return res.json();
  },

  pay: async (
    token: string,
    bookingId: string,
    body: PayBookingBody
  ): Promise<PaymentInitiatedResponse> => {
    // Team2 nie oczekuje ciała — wysyłamy je tylko dla team1
    const res = await fetch(`${API_URL}/api/v1/bookings/${bookingId}/pay`, {
      method: "POST",
      headers: authHeaders(token),
      body: BACKEND_TYPE === "team2" ? undefined : JSON.stringify(body),
    });
    if (!res.ok) throw await readError(res);
    const data = (await res.json()) as Record<string, unknown>;
    return {
      paymentId: String(data.paymentId ?? data.PaymentId ?? ""),
      bookingId: String(data.bookingId ?? data.BookingId ?? ""),
      status: String(data.status ?? data.Status ?? ""),
      // Team2 zwraca checkoutUrl zamiast redirectUrl
      redirectUrl: String(
        data.redirectUrl ?? data.RedirectUrl ??
        data.checkoutUrl ?? data.CheckoutUrl ?? ""
      ),
      amount: Number(data.amount ?? data.Amount ?? 0),
      currency: String(data.currency ?? data.Currency ?? ""),
    };
  },
};
