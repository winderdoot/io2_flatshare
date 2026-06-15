import { useState, type FormEvent } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";
import type { ListingDTO } from "../../models/listing";
import { compareIso } from "../../components/AvailabilityCalendar/availabilityCalendarUtils";
import { bookingService } from "../Bookings/bookingService";
import { formatPlainAmount } from "../../utils/formatMoney";

type Props = {
  listingId: string;
  listing: ListingDTO;
  token: string;
};

export const BookingForm = ({ listingId, listing, token }: Props) => {
  const { t, i18n } = useTranslation();
  const queryClient = useQueryClient();
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [formError, setFormError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const createMutation = useMutation({
    mutationFn: () =>
      bookingService.create(token, {
        listingId,
        startDate,
        endDate,
      }),
    onSuccess: async (resp) => {
      setFormError(null);
      setSuccess(
        t("booking.form.created", {
          price: formatPlainAmount(resp.totalPrice, i18n.language),
          currency: resp.currency,
        })
      );
      setStartDate("");
      setEndDate("");
      await queryClient.invalidateQueries({ queryKey: ["bookings"] });
    },
    onError: (e: unknown) => {
      setSuccess(null);
      setFormError(messageForBookingFailure(e, t));
    },
  });

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    setFormError(null);
    setSuccess(null);

    if (!startDate || !endDate) {
      setFormError(t("booking.form.validationDates"));
      return;
    }
    if (endDate <= startDate) {
      setFormError(t("booking.form.validationRange"));
      return;
    }
    if (
      compareIso(startDate, listing.availableSince) < 0 ||
      compareIso(endDate, listing.availableUntil) > 0
    ) {
      setFormError(t("booking.form.validationAvailability"));
      return;
    }

    for (const u of listing.unavailabilities ?? []) {
      if (!(compareIso(endDate, u.since) <= 0 || compareIso(startDate, u.until) >= 0)) {
        setFormError(t("booking.form.validationUnavailability"));
        return;
      }
    }

    createMutation.mutate();
  };

  return (
    <div className="listing-detail-booking">
      <h2 className="listing-detail-booking-title">{t("booking.form.title")}</h2>
      <p className="listing-detail-booking-lead">{t("booking.form.lead")}</p>

      {success && (
        <p className="listing-detail-booking-success" role="status">
          {success}{" "}
          <Link to="/my-bookings">{t("booking.form.viewMyBookings")}</Link>
        </p>
      )}
      {formError && (
        <p className="listing-detail-booking-error" role="alert">
          {formError}
        </p>
      )}

      <form onSubmit={handleSubmit} aria-label={t("booking.form.ariaLabel")}>
        <div className="listing-detail-booking-fields">
          <label className="listing-detail-booking-field">
            <span>{t("booking.form.startDate")}</span>
            <input
              type="date"
              value={startDate}
              min={listing.availableSince}
              max={listing.availableUntil}
              disabled={createMutation.isPending}
              onChange={(e) => setStartDate(e.target.value)}
              required
            />
          </label>
          <label className="listing-detail-booking-field">
            <span>{t("booking.form.endDate")}</span>
            <input
              type="date"
              value={endDate}
              min={listing.availableSince}
              max={listing.availableUntil}
              disabled={createMutation.isPending}
              onChange={(e) => setEndDate(e.target.value)}
              required
            />
          </label>
        </div>
        <button
          type="submit"
          className="listing-detail-booking-submit"
          disabled={createMutation.isPending}
        >
          {createMutation.isPending
            ? t("booking.form.submitting")
            : t("booking.form.submit")}
        </button>
      </form>
    </div>
  );
};
