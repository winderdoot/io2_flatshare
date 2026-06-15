import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import type { DisplayCurrency } from "../constants/exchange";
import { formatListingPrice, formatMoneyAmount, convertToDisplayAmount } from "../utils/formatMoney";

export type { DisplayCurrency };

const STORAGE_KEY = "flatshare-currency";

type CurrencyContextValue = {
  currency: DisplayCurrency;
  setCurrency: (c: DisplayCurrency) => void;
  formatRentPln: (amountPln: number) => string;
  formatListingPrice: (amount: number, sourceCurrency: string, locale?: string) => string;
  formatPlainInDisplayCurrency: (amountPln: number, locale?: string) => string;
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
    (amountPln: number) => formatListingPrice(amountPln, "PLN", currency),
    [currency]
  );

  const formatListingPriceForDisplay = useCallback(
    (amount: number, sourceCurrency: string, locale?: string) =>
      formatListingPrice(amount, sourceCurrency, currency, locale),
    [currency]
  );

  const formatPlainInDisplayCurrency = useCallback(
    (amountPln: number, locale?: string) => {
      const converted = convertToDisplayAmount(amountPln, "PLN", currency);
      return formatMoneyAmount(converted, currency, locale);
    },
    [currency]
  );

  const value = useMemo(
    () => ({
      currency,
      setCurrency,
      formatRentPln,
      formatListingPrice: formatListingPriceForDisplay,
      formatPlainInDisplayCurrency,
    }),
    [currency, setCurrency, formatRentPln, formatListingPriceForDisplay, formatPlainInDisplayCurrency]
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
