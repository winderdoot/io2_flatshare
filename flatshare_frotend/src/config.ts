export const API_URL: string =
  (import.meta.env.VITE_API_URL as string | undefined) ?? "http://localhost:7047";

/**
 * "team1" → własny backend (io2_flatshare, domyślny)
 * "team2" → backend drugiego zespołu (projekt_2)
 * Ustaw przez zmienną VITE_BACKEND_TYPE w pliku .env
 */
export const BACKEND_TYPE: "team1" | "team2" =
  (import.meta.env.VITE_BACKEND_TYPE as string | undefined) === "team2"
    ? "team2"
    : "team1";