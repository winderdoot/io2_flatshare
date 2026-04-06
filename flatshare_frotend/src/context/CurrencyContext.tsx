import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { PLN_PER_USD } from "../constants/exchange";

export type DisplayCurrency = "PLN" | "USD";

const STORAGE_KEY = "flatshare-currency";

type CurrencyContextValue = {
  currency: DisplayCurrency;
  setCurrency: (c: DisplayCurrency) => void;
  formatRentPln: (amountPln: number) => string;
};

const CurrencyContext = createContext<CurrencyContextValue | null>(null);

function readStoredCurrency(): DisplayCurrency {
  if (typeof window === "undefined") return "PLN";
  return localStorage.getItem(STORAGE_KEY) === "USD" ? "USD" : "PLN";
}

export function CurrencyProvider({ children }: { children: ReactNode }) {
  const [currency, setCurrencyState] = useState<DisplayCurrency>(readStoredCurrency);

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, currency);
  }, [currency]);

  const setCurrency = useCallback((c: DisplayCurrency) => {
    setCurrencyState(c);
  }, []);

  const formatRentPln = useCallback(
    (amountPln: number) => {
      if (currency === "PLN") {
        return new Intl.NumberFormat(undefined, {
          style: "currency",
          currency: "PLN",
          maximumFractionDigits: 0,
        }).format(amountPln);
      }
      const usd = amountPln / PLN_PER_USD;
      return new Intl.NumberFormat(undefined, {
        style: "currency",
        currency: "USD",
        maximumFractionDigits: 0,
      }).format(usd);
    },
    [currency]
  );

  const value = useMemo(
    () => ({ currency, setCurrency, formatRentPln }),
    [currency, setCurrency, formatRentPln]
  );

  return (
    <CurrencyContext.Provider value={value}>{children}</CurrencyContext.Provider>
  );
}

export function useCurrency() {
  const ctx = useContext(CurrencyContext);
  if (!ctx) {
    throw new Error("useCurrency must be used within CurrencyProvider");
  }
  return ctx;
}
