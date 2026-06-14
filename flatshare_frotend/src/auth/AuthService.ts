import { API_URL } from "../config";
import { User } from "../models/user";
import { parseSessionResponse } from "./tokenUtils";

const jsonHeaders = (token?: string) => {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    Accept: "application/json",
  };
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }
  return headers;
};

export type LoginResult = {
  token: string | null;
  sessionId: string | null;
  expiresIn: number | null;
  loggedInUser: User | null;
};

export type RefreshResult = {
  token: string;
  sessionId: string;
  expiresIn: number;
};

export const authService = {
  login: async (email: string, password: string): Promise<LoginResult> => {
    try {
      const createdSession = await fetch(`${API_URL}/api/v1/sessions`, {
        method: "POST",
        body: JSON.stringify({ email, password }),
        headers: jsonHeaders(),
      });

      if (!createdSession.ok) {
        return {
          token: null,
          sessionId: null,
          expiresIn: null,
          loggedInUser: null,
        };
      }

      const createdSessionData = (await createdSession.json()) as Record<
        string,
        unknown
      >;
      const session = parseSessionResponse(createdSessionData);
      if (!session.token || !session.sessionId) {
        return {
          token: null,
          sessionId: null,
          expiresIn: null,
          loggedInUser: null,
        };
      }

      const userId = await fetch(
        `${API_URL}/api/v1/sessions/${session.sessionId}`,
        {
          method: "GET",
          headers: jsonHeaders(session.token),
        }
      );

      if (!userId.ok) {
        return {
          token: null,
          sessionId: null,
          expiresIn: null,
          loggedInUser: null,
        };
      }

      const userIdData = (await userId.json()) as {
        userId?: string;
        UserId?: string;
      };
      const resolvedUserId = String(
        userIdData.userId ?? userIdData.UserId ?? ""
      );
      if (!resolvedUserId) {
        return {
          token: null,
          sessionId: null,
          expiresIn: null,
          loggedInUser: null,
        };
      }

      const user = await fetch(`${API_URL}/api/v1/users/${resolvedUserId}`, {
        method: "GET",
        headers: jsonHeaders(session.token),
      });

      if (!user.ok) {
        return {
          token: null,
          sessionId: null,
          expiresIn: null,
          loggedInUser: null,
        };
      }

      const userData = await user.json();

      const loggedInUser: User = {
        id: userData.id,
        firstName: userData.firstName,
        lastName: userData.lastName,
        email: userData.email,
        role: userData.role,
      };

      return {
        token: session.token,
        sessionId: session.sessionId,
        expiresIn: session.expiresIn,
        loggedInUser,
      };
    } catch {
      return {
        token: null,
        sessionId: null,
        expiresIn: null,
        loggedInUser: null,
      };
    }
  },

  refreshSession: async (
    sessionId: string,
    token: string
  ): Promise<RefreshResult | null> => {
    try {
      const res = await fetch(`${API_URL}/api/v1/sessions/${sessionId}`, {
        method: "PATCH",
        headers: jsonHeaders(token),
      });

      if (!res.ok) {
        return null;
      }

      const data = (await res.json()) as Record<string, unknown>;
      const session = parseSessionResponse(data);
      if (!session.token || !session.sessionId || session.expiresIn <= 0) {
        return null;
      }

      return session;
    } catch {
      return null;
    }
  },
};
