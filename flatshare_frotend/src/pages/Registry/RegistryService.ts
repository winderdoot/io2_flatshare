import { API_URL } from "../../config";

export type RegistryFieldError = { field: string; message: string };

export type RegistryResponse =
  | { user: { id: string; email: string; role: string } }
  | { fieldErrors: RegistryFieldError[]; error?: string; message?: string };

export const registryService = {
  register: async (
    firstName: string,
    lastName: string,
    email: string,
    password: string,
    role: string
  ): Promise<RegistryResponse> => {
    const res = await fetch(`${API_URL}/api/v1/users`, {
      method: "POST",
      body: JSON.stringify({ firstName, lastName, email, password, role }),
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
      },
    });

    const data = await res.json();

    if (!res.ok) {
      // Normalizujemy nazwy pól do PascalCase żeby pasowały do sprawdzeń w Registry.tsx
      // (team1 zwraca "Email", team2 zwraca "email")
      if (Array.isArray(data.fieldErrors)) {
        return {
          ...data,
          fieldErrors: (data.fieldErrors as RegistryFieldError[]).map((e) => ({
            ...e,
            field: e.field.charAt(0).toUpperCase() + e.field.slice(1),
          })),
        };
      }
      // Brak fieldErrors — przenosimy wiadomość błędu jako fieldError dla emaila
      const message =
        typeof data.message === "string" && data.message
          ? data.message
          : typeof data.error === "string" && data.error
            ? data.error
            : `HTTP ${res.status}`;
      return { fieldErrors: [{ field: "Email", message }] };
    }

    return data as RegistryResponse;
  },
};