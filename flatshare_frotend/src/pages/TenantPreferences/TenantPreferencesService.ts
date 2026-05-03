import { API_URL } from "../../config";
import type { TenantPreferences } from "../../models/tenantPreferences";

const authHeaders = (token: string) => ({
  "Content-Type": "application/json",
  Accept: "application/json",
  Authorization: `Bearer ${token}`,
});

export type PreferencesRequestError = {
  status: number;
  message: string;
};

export function isPreferencesRequestError(
  e: unknown
): e is PreferencesRequestError {
  return (
    typeof e === "object" &&
    e !== null &&
    "status" in e &&
    "message" in e &&
    typeof (e as PreferencesRequestError).status === "number" &&
    typeof (e as PreferencesRequestError).message === "string"
  );
}

async function readErrorMessage(res: Response): Promise<string> {
  try {
    const data = (await res.json()) as { error?: string; title?: string };
    if (typeof data.error === "string") return data.error;
    if (typeof data.title === "string") return data.title;
  } catch {
    /* ignore */
  }
  return res.statusText || `HTTP ${res.status}`;
}

export const tenantPreferencesService = {
  get: async (token: string): Promise<TenantPreferences> => {
    const res = await fetch(`${API_URL}/api/v1/users/me/preferences`, {
      method: "GET",
      headers: authHeaders(token),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies PreferencesRequestError;
    }
    return res.json();
  },

  put: async (
    token: string,
    body: TenantPreferences
  ): Promise<TenantPreferences> => {
    const res = await fetch(`${API_URL}/api/v1/users/me/preferences`, {
      method: "PUT",
      headers: authHeaders(token),
      body: JSON.stringify({
        maxPrice: body.maxPrice,
        currency: body.currency,
        smokingAllowed: body.smokingAllowed,
        petsAllowed: body.petsAllowed,
        preferredDistricts: body.preferredDistricts ?? [],
      }),
    });
    if (!res.ok) {
      const message = await readErrorMessage(res);
      throw { status: res.status, message } satisfies PreferencesRequestError;
    }
    return res.json();
  },
};
