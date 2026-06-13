import { PLN_PER_USD, type DisplayCurrency } from "../constants/exchange";

export function convertToDisplayAmount(
  amount: number,
  sourceCurrency: string,
  displayCurrency: DisplayCurrency
): number {
  const src = sourceCurrency.trim().toUpperCase();
  if (displayCurrency === "PLN") {
    if (src === "PLN") return amount;
    if (src === "USD") return amount * PLN_PER_USD;
    return amount;
  }
  if (src === "USD") return amount;
  if (src === "PLN") return amount / PLN_PER_USD;
  return amount / PLN_PER_USD;
}

export function formatMoneyAmount(
  amount: number,
  displayCurrency: DisplayCurrency,
  locale?: string
): string {
  return new Intl.NumberFormat(locale, {
    style: "currency",
    currency: displayCurrency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);
}

export function formatListingPrice(
  amount: number,
  sourceCurrency: string,
  displayCurrency: DisplayCurrency,
  locale?: string
): string {
  const converted = convertToDisplayAmount(amount, sourceCurrency, displayCurrency);
  return formatMoneyAmount(converted, displayCurrency, locale);
}

export function formatPlainAmount(amount: number, locale?: string): string {
  return new Intl.NumberFormat(locale, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);
}
