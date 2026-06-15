import { useState, type FormEvent } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import type { ListingDTO, Unavailability } from "../../models/listing";
import {
  isListingRequestError,
  landlordListingsService,
} from "../LandlordListings/LandlordListingsService";
import "../LandlordListings/ListingAvailability.css";

type Props = {
  listingId: string;
  listing: ListingDTO;
  token: string;
  formatDate: (iso: string) => string;
};

function messageForUnavailabilityFailure(
  e: unknown,
  t: (key: string, opt?: Record<string, string>) => string
): string {
  if (isListingRequestError(e)) {
    if (e.status === 401) return t("landlordListings.error401");
    if (e.status === 403) return t("landlordListings.error403");
    if (e.status === 409) return t("landlordListings.unavailConflict");
    return t("landlordListings.unavailErrorDetail", { message: e.message });
  }
  return t("landlordListings.networkError");
}

const emptyForm = () => ({
  since: "",
  until: "",
  message: "",
});

export const UnavailabilityManager = ({
  listingId,
  listing,
  token,
  formatDate,
}: Props) => {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [form, setForm] = useState(emptyForm);
  const [formError, setFormError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [removingKey, setRemovingKey] = useState<string | null>(null);

  const refreshListing = () =>
    queryClient.refetchQueries({ queryKey: ["listing", listingId] });

  const addMutation = useMutation({
    mutationFn: (body: Unavailability) =>
      landlordListingsService.addUnavailability(token, listingId, body),
    onSuccess: async () => {
      await refreshListing();
      setForm(emptyForm());
      setFormError(null);
      setActionError(null);
      setSuccess(t("landlordListings.unavailAdded"));
    },
    onError: (e: unknown) => {
      setSuccess(null);
      setFormError(messageForUnavailabilityFailure(e, t));
    },
  });

  const removeMutation = useMutation({
    mutationFn: (body: Pick<Unavailability, "since" | "until">) =>
      landlordListingsService.removeUnavailability(token, listingId, body),
    onSuccess: async () => {
      await refreshListing();
      setRemovingKey(null);
      setActionError(null);
      setSuccess(t("landlordListings.unavailRemoved"));
    },
    onError: (e: unknown) => {
      setRemovingKey(null);
      setSuccess(null);
      setActionError(messageForUnavailabilityFailure(e, t));
    },
  });

  const periods = listing.unavailabilities ?? [];
  const busy = addMutation.isPending || removeMutation.isPending;

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    setFormError(null);
    setSuccess(null);
    setActionError(null);

    if (!form.since || !form.until) {
      setFormError(t("landlordListings.unavailValidationDates"));
      return;
    }
    if (form.until <= form.since) {
      setFormError(t("landlordListings.unavailValidationRange"));
      return;
    }

    addMutation.mutate({
      since: form.since,
      until: form.until,
      message: form.message.trim(),
    });
  };

  const handleRemove = (u: Unavailability) => {
    const key = `${u.since}|${u.until}`;
    setRemovingKey(key);
    setActionError(null);
    setSuccess(null);
    removeMutation.mutate({ since: u.since, until: u.until });
  };

  return (
    <div className="listing-availability-manage">
      <h2 className="listing-availability-manage-title">
        {t("landlordListings.unavailManageSection")}
      </h2>
      <p className="listing-availability-manage-lead">
        {t("landlordListings.unavailLead")}
      </p>

      {success && (
        <p className="listing-availability-manage-success" role="status">
          {success}
        </p>
      )}
      {actionError && (
        <p className="listing-availability-manage-error" role="alert">
          {actionError}
        </p>
      )}

      {periods.length > 0 ? (
        <ul className="listing-availability-manage-list">
          {periods.map((u) => {
            const key = `${u.since}|${u.until}`;
            const isRemoving = removingKey === key;
            return (
              <li key={key} className="listing-availability-manage-item">
                <div className="listing-availability-manage-item-text">
                  <span className="listing-availability-manage-dates">
                    {formatDate(u.since)} – {formatDate(u.until)}
                  </span>
                  {u.message ? (
                    <span className="listing-availability-manage-msg">
                      {u.message}
                    </span>
                  ) : null}
                </div>
                <button
                  type="button"
                  className="listing-availability-manage-remove"
                  disabled={busy}
                  onClick={() => handleRemove(u)}
                  aria-busy={isRemoving}
                >
                  {isRemoving
                    ? t("landlordListings.unavailRemoving")
                    : t("landlordListings.unavailRemove")}
                </button>
              </li>
            );
          })}
        </ul>
      ) : (
        <p className="listing-availability-manage-empty">
          {t("landlordListings.unavailEmpty")}
        </p>
      )}

      <form
        className="listing-availability-manage-form"
        onSubmit={handleSubmit}
        aria-label={t("landlordListings.unavailFormLabel")}
      >
        <h3 className="listing-availability-manage-form-title">
          {t("landlordListings.unavailAddTitle")}
        </h3>
        {formError && (
          <p className="listing-availability-manage-error" role="alert">
            {formError}
          </p>
        )}
        <div className="listing-availability-manage-fields">
          <label className="listing-availability-manage-field">
            <span>{t("landlordListings.unavailSince")}</span>
            <input
              type="date"
              value={form.since}
              min={listing.availableSince}
              max={listing.availableUntil}
              disabled={busy}
              onChange={(e) =>
                setForm((prev) => ({ ...prev, since: e.target.value }))
              }
              required
            />
          </label>
          <label className="listing-availability-manage-field">
            <span>{t("landlordListings.unavailUntil")}</span>
            <input
              type="date"
              value={form.until}
              min={listing.availableSince}
              max={listing.availableUntil}
              disabled={busy}
              onChange={(e) =>
                setForm((prev) => ({ ...prev, until: e.target.value }))
              }
              required
            />
          </label>
        </div>
        <label className="listing-availability-manage-field listing-availability-manage-field--full">
          <span>{t("landlordListings.unavailMessage")}</span>
          <input
            type="text"
            value={form.message}
            maxLength={200}
            disabled={busy}
            placeholder={t("landlordListings.unavailMessagePlaceholder")}
            onChange={(e) =>
              setForm((prev) => ({ ...prev, message: e.target.value }))
            }
          />
        </label>
        <button
          type="submit"
          className="listing-availability-manage-submit"
          disabled={busy}
        >
          {addMutation.isPending
            ? t("landlordListings.unavailAdding")
            : t("landlordListings.unavailAdd")}
        </button>
      </form>
    </div>
  );
};
