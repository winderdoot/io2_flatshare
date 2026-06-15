import type { Unavailability } from "../../models/listing";

export type YearMonth = { year: number; month: number };

export function parseIsoDate(iso: string): Date {
  const [y, m, d] = iso.split("-").map(Number);
  return new Date(y, m - 1, d);
}

export function toIsoDate(d: Date): string {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${y}-${m}-${day}`;
}

export function compareIso(a: string, b: string): number {
  return a.localeCompare(b);
}

export function enumerateMonths(
  availableSince: string,
  availableUntil: string
): YearMonth[] {
  const start = parseIsoDate(availableSince);
  const end = parseIsoDate(availableUntil);
  const months: YearMonth[] = [];
  let y = start.getFullYear();
  let m = start.getMonth();
  const endY = end.getFullYear();
  const endM = end.getMonth();

  while (y < endY || (y === endY && m <= endM)) {
    months.push({ year: y, month: m });
    m += 1;
    if (m > 11) {
      m = 0;
      y += 1;
    }
  }
  return months;
}

export function daysInMonth(year: number, month: number): number {
  return new Date(year, month + 1, 0).getDate();
}

/** Poniedziałek = 0 … niedziela = 6 */
export function mondayBasedWeekday(date: Date): number {
  return (date.getDay() + 6) % 7;
}

export type DayCell = {
  iso: string;
  day: number;
  inRange: boolean;
  unavailable: boolean;
  message?: string;
  isToday: boolean;
};

export function buildMonthGrid(
  year: number,
  month: number,
  availableSince: string,
  availableUntil: string,
  unavailabilities: Unavailability[],
  todayIso: string
): (DayCell | null)[] {
  const first = new Date(year, month, 1);
  const leading = mondayBasedWeekday(first);
  const totalDays = daysInMonth(year, month);
  const cells: (DayCell | null)[] = Array.from({ length: leading }, () => null);

  for (let d = 1; d <= totalDays; d++) {
    const date = new Date(year, month, d);
    const iso = toIsoDate(date);
    const inRange =
      compareIso(iso, availableSince) >= 0 &&
      compareIso(iso, availableUntil) <= 0;
    let unavailable = false;
    let message: string | undefined;
    if (inRange) {
      for (const u of unavailabilities) {
        if (
          compareIso(iso, u.since) >= 0 &&
          compareIso(iso, u.until) <= 0
        ) {
          unavailable = true;
          message = u.message || undefined;
          break;
        }
      }
    }
    cells.push({
      iso,
      day: d,
      inRange,
      unavailable,
      message,
      isToday: iso === todayIso,
    });
  }

  while (cells.length % 7 !== 0) {
    cells.push(null);
  }
  return cells;
}

export function monthKey({ year, month }: YearMonth): string {
  return `${year}-${month}`;
}
