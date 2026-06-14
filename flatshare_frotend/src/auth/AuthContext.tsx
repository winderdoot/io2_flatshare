import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";
import { User } from "../models/user";
import { authService } from "./AuthService";
import {
  AUTH_SESSION_ID_KEY,
  AUTH_TOKEN_EXPIRES_AT_KEY,
  computeTokenExpiresAt,
  getSessionIdFromToken,
  getTokenExpiresAt,
  msUntilRefresh,
} from "./tokenUtils";

type SessionMeta = {
  sessionId: string;
  expiresIn: number;
};

type AuthContextType = {
  user: User | null;
  token: string | null;
  loading: boolean;
  login: (token: string, user: User, session?: SessionMeta) => void;
  logout: () => void;
};

const AuthContext = createContext<AuthContextType | null>(null);

function persistSession(
  token: string,
  user: User,
  sessionId: string,
  tokenExpiresAt: number
) {
  localStorage.setItem("token", token);
  localStorage.setItem("user", JSON.stringify(user));
  localStorage.setItem(AUTH_SESSION_ID_KEY, sessionId);
  localStorage.setItem(AUTH_TOKEN_EXPIRES_AT_KEY, String(tokenExpiresAt));
}

function clearSessionStorage() {
  localStorage.removeItem("token");
  localStorage.removeItem("user");
  localStorage.removeItem(AUTH_SESSION_ID_KEY);
  localStorage.removeItem(AUTH_TOKEN_EXPIRES_AT_KEY);
}

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [tokenExpiresAt, setTokenExpiresAt] = useState<number | null>(null);
  const [loading, setLoading] = useState(true);

  const refreshInFlight = useRef(false);
  const sessionIdRef = useRef<string | null>(null);
  const tokenRef = useRef<string | null>(null);

  useEffect(() => {
    sessionIdRef.current = sessionId;
  }, [sessionId]);

  useEffect(() => {
    tokenRef.current = token;
  }, [token]);

  const logout = useCallback(() => {
    clearSessionStorage();
    setToken(null);
    setUser(null);
    setSessionId(null);
    setTokenExpiresAt(null);
  }, []);

  const applyRefreshedSession = useCallback(
    (nextToken: string, nextSessionId: string, expiresIn: number) => {
      const nextExpiresAt = computeTokenExpiresAt(expiresIn);
      setToken(nextToken);
      setSessionId(nextSessionId);
      setTokenExpiresAt(nextExpiresAt);

      const storedUser = localStorage.getItem("user");
      if (storedUser) {
        localStorage.setItem("token", nextToken);
        localStorage.setItem(AUTH_SESSION_ID_KEY, nextSessionId);
        localStorage.setItem(AUTH_TOKEN_EXPIRES_AT_KEY, String(nextExpiresAt));
      }
    },
    []
  );

  const refreshToken = useCallback(async () => {
    const currentSessionId = sessionIdRef.current;
    const currentToken = tokenRef.current;
    if (!currentSessionId || !currentToken || refreshInFlight.current) {
      return;
    }

    refreshInFlight.current = true;
    try {
      const result = await authService.refreshSession(
        currentSessionId,
        currentToken
      );
      if (!result) {
        logout();
        return;
      }
      applyRefreshedSession(result.token, result.sessionId, result.expiresIn);
    } finally {
      refreshInFlight.current = false;
    }
  }, [applyRefreshedSession, logout]);

  useEffect(() => {
    const storedToken = localStorage.getItem("token");
    const storedUser = localStorage.getItem("user");

    if (storedToken && storedUser) {
      const parsedUser = JSON.parse(storedUser) as User;
      const resolvedSessionId =
        localStorage.getItem(AUTH_SESSION_ID_KEY) ??
        getSessionIdFromToken(storedToken);
      const storedExpiresAt = localStorage.getItem(AUTH_TOKEN_EXPIRES_AT_KEY);
      const resolvedExpiresAt = storedExpiresAt
        ? Number(storedExpiresAt)
        : getTokenExpiresAt(storedToken);

      setToken(storedToken);
      setUser(parsedUser);
      setSessionId(resolvedSessionId);
      setTokenExpiresAt(resolvedExpiresAt);

      if (resolvedSessionId && !storedExpiresAt && resolvedExpiresAt) {
        localStorage.setItem(
          AUTH_TOKEN_EXPIRES_AT_KEY,
          String(resolvedExpiresAt)
        );
      }
      if (resolvedSessionId && !localStorage.getItem(AUTH_SESSION_ID_KEY)) {
        localStorage.setItem(AUTH_SESSION_ID_KEY, resolvedSessionId);
      }
    }

    setLoading(false);
  }, []);

  useEffect(() => {
    if (!token || !sessionId || !tokenExpiresAt) {
      return;
    }

    const delay = msUntilRefresh(tokenExpiresAt);
    if (delay <= 0) {
      void refreshToken();
      return;
    }

    const timer = window.setTimeout(() => {
      void refreshToken();
    }, delay);

    return () => window.clearTimeout(timer);
  }, [token, sessionId, tokenExpiresAt, refreshToken]);

  const login = useCallback(
    (nextToken: string, nextUser: User, session?: SessionMeta) => {
      const resolvedSessionId =
        session?.sessionId ?? getSessionIdFromToken(nextToken);
      const resolvedExpiresAt = session?.expiresIn
        ? computeTokenExpiresAt(session.expiresIn)
        : getTokenExpiresAt(nextToken);

      if (!resolvedSessionId || !resolvedExpiresAt) {
        return;
      }

      persistSession(
        nextToken,
        nextUser,
        resolvedSessionId,
        resolvedExpiresAt
      );
      setToken(nextToken);
      setUser(nextUser);
      setSessionId(resolvedSessionId);
      setTokenExpiresAt(resolvedExpiresAt);
    },
    []
  );

  return (
    <AuthContext.Provider value={{ user, token, loading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used inside AuthProvider");
  return context;
};
