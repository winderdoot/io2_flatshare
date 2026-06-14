const REFRESH_BUFFER_MS = 5 * 60 * 1000;

export const AUTH_SESSION_ID_KEY = "sessionId";
export const AUTH_TOKEN_EXPIRES_AT_KEY = "tokenExpiresAt";

type JwtPayload = {
  session_id?: string;
  exp?: number;
};

function decodeJwtPayload(token: string): JwtPayload | null {
  try {
    const payload = token.split(".")[1];
    if (!payload) return null;
    const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
    return JSON.parse(atob(normalized)) as JwtPayload;
  } catch {
    return null;
  }
}

export function getSessionIdFromToken(token: string): string | null {
  const payload = decodeJwtPayload(token);
  return payload?.session_id ?? null;
}

export function getTokenExpiresAt(token: string): number | null {
  const payload = decodeJwtPayload(token);
  if (typeof payload?.exp !== "number") return null;
  return payload.exp * 1000;
}

export function computeTokenExpiresAt(expiresInSec: number): number {
  return Date.now() + expiresInSec * 1000;
}

export function msUntilRefresh(expiresAt: number, now = Date.now()): number {
  return expiresAt - now - REFRESH_BUFFER_MS;
}

export function parseSessionResponse(data: Record<string, unknown>) {
  return {
    token: String(data.token ?? data.Token ?? ""),
    sessionId: String(data.sessionId ?? data.SessionId ?? ""),
    expiresIn: Number(data.expiresIn ?? data.ExpiresIn ?? 0),
  };
}
