import { useCallback, useEffect, useMemo, useRef } from "react";
import { useTranslation } from "react-i18next";
import type { Unavailability } from "../../models/listing";
import {
  buildMonthGrid,
  enumerateMonths,
  monthKey,
  parseIsoDate,
  toIsoDate,
} from "./availabilityCalendarUtils";
import "./AvailabilityCalendar.css";

type Props = {
  availableSince: string;
  availableUntil: string;
  unavailabilities?: Unavailability[];
  /** Mniejszy wariant w panelu bocznym szczegółów oferty */
  compact?: boolean;
};

const WEEKDAY_KEYS = ["mon", "tue", "wed", "thu", "fri", "sat", "sun"] as const;

export const AvailabilityCalendar = ({
  availableSince,
  availableUntil,
  unavailabilities = [],
  compact = false,
}: Props) => {
  const { t, i18n } = useTranslation();
  const scrollRef = useRef<HTMLDivElement>(null);
  const todayIso = useMemo(() => toIsoDate(new Date()), []);

  const months = useMemo(
    () => enumerateMonths(availableSince, availableUntil),
    [availableSince, availableUntil]
  );

  const monthFormatter = useMemo(
    () =>
      new Intl.DateTimeFormat(i18n.language, {
        month: "long",
        year: "numeric",
      }),
    [i18n.language]
  );

  const scrollByMonth = useCallback((direction: -1 | 1) => {
    const el = scrollRef.current;
    if (!el) return;
    const card = el.querySelector<HTMLElement>(".avail-cal-month");
    const gap = 12;
    const step = (card?.offsetWidth ?? 280) + gap;
    el.scrollBy({ left: direction * step, behavior: "smooth" });
  }, []);

  const todayMonthKey = useMemo(() => {
    const d = new Date();
    return `${d.getFullYear()}-${d.getMonth()}`;
  }, []);

  useEffect(() => {
    const el = scrollRef.current;
    if (!el) return;
    const target = el.querySelector<HTMLElement>(
      `[data-month-key="${todayMonthKey}"]`
    );
    if (target) {
      target.scrollIntoView({ behavior: "instant", inline: "start", block: "nearest" });
    }
  }, [todayMonthKey, months.length]);

  return (
    <section
      className={[
        "avail-cal",
        compact ? "avail-cal--compact" : "",
      ]
        .filter(Boolean)
        .join(" ")}
      aria-label={t("landlordListings.calendarTitle")}
    >
      <div className="avail-cal-header">
        <h3 className="avail-cal-title">{t("landlordListings.calendarTitle")}</h3>
        <div className="avail-cal-nav">
          <button
            type="button"
            className="avail-cal-nav-btn"
            onClick={() => scrollByMonth(-1)}
            aria-label={t("landlordListings.calendarPrev")}
          >
            ‹
          </button>
          <button
            type="button"
            className="avail-cal-nav-btn"
            onClick={() => scrollByMonth(1)}
            aria-label={t("landlordListings.calendarNext")}
          >
            ›
          </button>
        </div>
      </div>

      <div className="avail-cal-scroll" ref={scrollRef}>
        {months.map((ym) => {
          const cells = buildMonthGrid(
            ym.year,
            ym.month,
            availableSince,
            availableUntil,
            unavailabilities,
            todayIso
          );
          const label = monthFormatter.format(
            parseIsoDate(`${ym.year}-${String(ym.month + 1).padStart(2, "0")}-01`)
          );

          return (
            <div
              key={monthKey(ym)}
              className="avail-cal-month"
              data-month-key={monthKey(ym)}
              aria-label={label}
            >
              <p className="avail-cal-month-label">{label}</p>
              <div className="avail-cal-weekdays" aria-hidden>
                {WEEKDAY_KEYS.map((key) => (
                  <span key={key} className="avail-cal-weekday">
                    {t(`landlordListings.calendarWeekday.${key}`)}
                  </span>
                ))}
              </div>
              <div className="avail-cal-grid" role="grid">
                {cells.map((cell, idx) =>
                  cell ? (
                    <div
                      key={cell.iso}
                      role="gridcell"
                      className={[
                        "avail-cal-day",
                        !cell.inRange ? "avail-cal-day--out" : "",
                        cell.unavailable ? "avail-cal-day--blocked" : "",
                        cell.inRange && !cell.unavailable
                          ? "avail-cal-day--free"
                          : "",
                        cell.isToday ? "avail-cal-day--today" : "",
                      ]
                        .filter(Boolean)
                        .join(" ")}
                      title={cell.message}
                      aria-label={
                        cell.unavailable && cell.message
                          ? `${cell.day}, ${cell.message}`
                          : String(cell.day)
                      }
                    >
                      <span>{cell.day}</span>
                    </div>
                  ) : (
                    <div
                      key={`empty-${monthKey(ym)}-${idx}`}
                      className="avail-cal-day avail-cal-day--empty"
                      aria-hidden
                    />
                  )
                )}
              </div>
            </div>
          );
        })}
      </div>

      <div className="avail-cal-legend">
        <span className="avail-cal-legend-item">
          <span className="avail-cal-swatch avail-cal-swatch--free" />
          {t("landlordListings.calendarLegendAvailable")}
        </span>
        <span className="avail-cal-legend-item">
          <span className="avail-cal-swatch avail-cal-swatch--blocked" />
          {t("landlordListings.calendarLegendUnavailable")}
        </span>
        <span className="avail-cal-legend-item">
          <span className="avail-cal-swatch avail-cal-swatch--out" />
          {t("landlordListings.calendarLegendOutOfRange")}
        </span>
      </div>
    </section>
  );
};
