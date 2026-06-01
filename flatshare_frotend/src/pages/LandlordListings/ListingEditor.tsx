import { useCallback, useEffect, useState, type FormEvent } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../auth/AuthContext";
import type { ListingDTO, ListingTenantProfile } from "../../models/listing";
import {
  isListingRequestError,
  landlordListingsService,
  type CreateListingBody,
} from "./LandlordListingsService";
import "./ListingEditor.css";

function toDateOnly(d: Date): string {
  return d.toISOString().slice(0, 10);
}

function addYearsDateOnly(iso: string, years: number): string {
  const [y, m, day] = iso.split("-").map(Number);
  const d = new Date(Date.UTC(y, m - 1, day));
  d.setUTCFullYear(d.getUTCFullYear() + years);
  return toDateOnly(d);
}

function messageForListingFailure(
  e: unknown,
  t: (key: string, opt?: Record<string, string>) => string,
  kind: "load" | "save"
): string {
  if (isListingRequestError(e)) {
    if (e.status === 401) return t("landlordListings.error401");
    if (e.status === 403) return t("landlordListings.error403");
    return kind === "load"
      ? t("landlordListings.loadErrorDetail", { message: e.message })
      : t("landlordListings.saveErrorDetail", { message: e.message });
  }
  return t("landlordListings.networkError");
}

function dtoToFormState(dto: ListingDTO) {
  return {
    title: dto.title,
    description: dto.description,
    price: String(dto.price),
    currency: dto.currency,
    availableSince: dto.availableSince,
    availableUntil: dto.availableUntil,
    ownerContact: dto.ownerContact,
    area: String(dto.area),
    city: dto.location.city,
    district: dto.location.district,
    street: dto.location.street,
    aptNumber: dto.location.aptNumber,
    petsAllowed: dto.attributes.petsAllowed,
    nonSmokingOnly: dto.attributes.nonSmokingOnly,
    closeToShops: dto.attributes.closeToShops,
    profile: dto.attributes.profile,
  };
}

const defaultCreateState = () => {
  const since = toDateOnly(new Date());
  return {
    title: "",
    description: "",
    price: "",
    currency: "PLN",
    availableSince: since,
    availableUntil: addYearsDateOnly(since, 1),
    ownerContact: "",
    area: "",
    city: "",
    district: "",
    street: "",
    aptNumber: "",
    petsAllowed: true,
    nonSmokingOnly: false,
    closeToShops: true,
    profile: "Student" as ListingTenantProfile,
  };
};

type FormState = ReturnType<typeof defaultCreateState>;

export const ListingEditor = () => {
  const { t } = useTranslation();
  const { listingId } = useParams<{ listingId: string }>();
  const isEdit = Boolean(listingId);
  const navigate = useNavigate();
  const { user, token } = useAuth();

  const [form, setForm] = useState<FormState>(() => defaultCreateState());
  const [loadedStatus, setLoadedStatus] = useState<string | null>(null);
  const [loading, setLoading] = useState(isEdit);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const canEdit =
    !isEdit ||
    loadedStatus === "Draft" ||
    loadedStatus === "Hidden";

  const applyDto = useCallback((dto: ListingDTO) => {
    setForm(dtoToFormState(dto));
    setLoadedStatus(dto.status);
  }, []);

  useEffect(() => {
    if (!isEdit || !listingId) return;

    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const dto = await landlordListingsService.getById(listingId);
        if (cancelled) return;
        applyDto(dto);
      } catch (e: unknown) {
        if (!cancelled) {
          setError(messageForListingFailure(e, t, "load"));
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [isEdit, listingId, applyDto, t]);

  const setText =
    (field: keyof FormState) => (value: string | boolean) => {
      setForm((prev) => ({ ...prev, [field]: value }));
    };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!token || user?.role !== "LANDLORD") return;
    if (isEdit && !canEdit) return;

    setSaving(true);
    setError(null);
    setSuccess(null);

    const price = Number(form.price.replace(",", "."));
    const area = Number(form.area.replace(",", "."));

    if (!form.title.trim() || !form.description.trim() || !form.ownerContact.trim()) {
      setError(t("landlordListings.validationRequired"));
      setSaving(false);
      return;
    }
    if (!Number.isFinite(price) || price <= 0) {
      setError(t("landlordListings.validationPrice"));
      setSaving(false);
      return;
    }
    if (!Number.isFinite(area) || area <= 0) {
      setError(t("landlordListings.validationArea"));
      setSaving(false);
      return;
    }
    if (!form.currency.trim()) {
      setError(t("landlordListings.validationCurrency"));
      setSaving(false);
      return;
    }

    const body: CreateListingBody = {
      title: form.title.trim(),
      description: form.description.trim(),
      price,
      currency: form.currency.trim(),
      availableSince: form.availableSince,
      availableUntil: form.availableUntil,
      ownerContact: form.ownerContact.trim(),
      area,
      location: {
        city: form.city.trim(),
        district: form.district.trim(),
        street: form.street.trim(),
        aptNumber: form.aptNumber.trim(),
      },
      attributes: {
        petsAllowed: form.petsAllowed,
        nonSmokingOnly: form.nonSmokingOnly,
        closeToShops: form.closeToShops,
        profile: form.profile,
      },
    };

    try {
      if (isEdit && listingId) {
        await landlordListingsService.update(token, listingId, body);
        navigate("/my-listings", {
          replace: true,
          state: { toast: { message: t("landlordListings.updated"), kind: "success" } },
        });
      } else {
        await landlordListingsService.create(token, body);
        navigate("/my-listings", {
          replace: true,
          state: { toast: { message: t("landlordListings.created"), kind: "success" } },
        });
      }
    } catch (e: unknown) {
      setError(messageForListingFailure(e, t, "save"));
    } finally {
      setSaving(false);
    }
  };

  if (user?.role !== "LANDLORD") {
    return (
      <div className="listing-editor">
        <div className="listing-editor-inner">
          <p className="listing-editor-banner">{t("landlordListings.landlordOnly")}</p>
          <Link className="listing-editor-back" to="/my-listings">
            {t("landlordListings.backToList")}
          </Link>
        </div>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="listing-editor">
        <div className="listing-editor-inner">
          <p className="listing-editor-lead">{t("landlordListings.loading")}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="listing-editor">
      <div className="listing-editor-inner">
        <header className="listing-editor-page-head">
          <Link className="listing-editor-back" to="/my-listings">
            ← {t("landlordListings.backToList")}
          </Link>
          <h1 className="listing-editor-title">
            {isEdit ? t("landlordListings.editTitle") : t("landlordListings.createTitle")}
          </h1>
          <p className="listing-editor-lead">
            {isEdit ? t("landlordListings.editLead") : t("landlordListings.createLead")}
          </p>
        </header>

        {isEdit && loadedStatus && (
          <p className="listing-editor-banner">
            {t("landlordListings.currentStatus", {
              status: t(`landlordListings.status.${loadedStatus}`),
            })}
          </p>
        )}

        {!canEdit && (
          <p className="listing-editor-banner">{t("landlordListings.editDisabled")}</p>
        )}

        {error && <div className="listing-editor-error">{error}</div>}
        {success && <div className="listing-editor-success">{success}</div>}

        <form className="listing-editor-form" onSubmit={handleSubmit}>
          <section className="listing-editor-panel" aria-labelledby="listing-panel-basic">
            <h2 id="listing-panel-basic" className="listing-editor-panel-title">
              {t("landlordListings.sectionBasic")}
            </h2>
          <div className="listing-editor-field">
            <label className="listing-editor-label" htmlFor="listing-title">
              {t("landlordListings.titleLabel")}
            </label>
            <input
              id="listing-title"
              className="listing-editor-input"
              value={form.title}
              onChange={(e) => setText("title")(e.target.value)}
              disabled={!canEdit}
              required
            />
          </div>
          <div className="listing-editor-field">
            <label className="listing-editor-label" htmlFor="listing-desc">
              {t("landlordListings.descriptionLabel")}
            </label>
            <textarea
              id="listing-desc"
              className="listing-editor-textarea"
              value={form.description}
              onChange={(e) => setText("description")(e.target.value)}
              disabled={!canEdit}
              required
            />
          </div>

          <div className="listing-editor-row">
            <div className="listing-editor-field">
              <label className="listing-editor-label" htmlFor="listing-price">
                {t("landlordListings.priceLabel")}
              </label>
              <input
                id="listing-price"
                className="listing-editor-input"
                inputMode="decimal"
                value={form.price}
                onChange={(e) => setText("price")(e.target.value)}
                disabled={!canEdit}
                required
              />
            </div>
            <div className="listing-editor-field">
              <label className="listing-editor-label" htmlFor="listing-currency">
                {t("landlordListings.currencyLabel")}
              </label>
              <select
                id="listing-currency"
                className="listing-editor-select"
                value={form.currency}
                onChange={(e) => setText("currency")(e.target.value)}
                disabled={!canEdit}
                required
              >
                <option value="PLN">PLN</option>
                <option value="EUR">EUR</option>
                <option value="USD">USD</option>
              </select>
            </div>
          </div>

          <div className="listing-editor-row">
            <div className="listing-editor-field">
              <label className="listing-editor-label" htmlFor="listing-since">
                {t("landlordListings.availableSince")}
              </label>
              <input
                id="listing-since"
                type="date"
                className="listing-editor-input"
                value={form.availableSince}
                onChange={(e) => setText("availableSince")(e.target.value)}
                disabled={!canEdit}
                required
              />
            </div>
            <div className="listing-editor-field">
              <label className="listing-editor-label" htmlFor="listing-until">
                {t("landlordListings.availableUntil")}
              </label>
              <input
                id="listing-until"
                type="date"
                className="listing-editor-input"
                value={form.availableUntil}
                onChange={(e) => setText("availableUntil")(e.target.value)}
                disabled={!canEdit}
                required
              />
            </div>
          </div>

          <div className="listing-editor-field">
            <label className="listing-editor-label" htmlFor="listing-contact">
              {t("landlordListings.contactLabel")}
            </label>
            <input
              id="listing-contact"
              className="listing-editor-input"
              value={form.ownerContact}
              onChange={(e) => setText("ownerContact")(e.target.value)}
              disabled={!canEdit}
              required
            />
          </div>

          <div className="listing-editor-field">
            <label className="listing-editor-label" htmlFor="listing-area">
              {t("landlordListings.areaLabel")}
            </label>
            <input
              id="listing-area"
              className="listing-editor-input"
              inputMode="decimal"
              value={form.area}
              onChange={(e) => setText("area")(e.target.value)}
              disabled={!canEdit}
              required
            />
          </div>
          </section>

          <section className="listing-editor-panel" aria-labelledby="listing-panel-address">
            <h2 id="listing-panel-address" className="listing-editor-panel-title">
              {t("landlordListings.sectionAddress")}
            </h2>
          <div className="listing-editor-row">
            <div className="listing-editor-field">
              <label className="listing-editor-label" htmlFor="listing-city">
                {t("landlordListings.city")}
              </label>
              <input
                id="listing-city"
                className="listing-editor-input"
                value={form.city}
                onChange={(e) => setText("city")(e.target.value)}
                disabled={!canEdit}
                required
              />
            </div>
            <div className="listing-editor-field">
              <label className="listing-editor-label" htmlFor="listing-district">
                {t("landlordListings.district")}
              </label>
              <input
                id="listing-district"
                className="listing-editor-input"
                value={form.district}
                onChange={(e) => setText("district")(e.target.value)}
                disabled={!canEdit}
                required
              />
            </div>
          </div>
          <div className="listing-editor-row">
            <div className="listing-editor-field">
              <label className="listing-editor-label" htmlFor="listing-street">
                {t("landlordListings.street")}
              </label>
              <input
                id="listing-street"
                className="listing-editor-input"
                value={form.street}
                onChange={(e) => setText("street")(e.target.value)}
                disabled={!canEdit}
                required
              />
            </div>
            <div className="listing-editor-field">
              <label className="listing-editor-label" htmlFor="listing-apt">
                {t("landlordListings.aptNumber")}
              </label>
              <input
                id="listing-apt"
                className="listing-editor-input"
                value={form.aptNumber}
                onChange={(e) => setText("aptNumber")(e.target.value)}
                disabled={!canEdit}
                required
              />
            </div>
          </div>
          </section>

          <section className="listing-editor-panel" aria-labelledby="listing-panel-attrs">
            <h2 id="listing-panel-attrs" className="listing-editor-panel-title">
              {t("landlordListings.sectionAttributes")}
            </h2>
            <div className="listing-editor-field">
              <label className="listing-editor-label" htmlFor="listing-profile">
                {t("landlordListings.profileLabel")}
              </label>
              <select
                id="listing-profile"
                className="listing-editor-select"
                value={form.profile}
                onChange={(e) =>
                  setText("profile")(e.target.value as ListingTenantProfile)
                }
                disabled={!canEdit}
              >
                <option value="Student">{t("landlordListings.profileStudent")}</option>
                <option value="Tourist">{t("landlordListings.profileTourist")}</option>
              </select>
            </div>
            <div className="listing-editor-check-grid">
              <label className="listing-editor-check">
                <input
                  type="checkbox"
                  checked={form.petsAllowed}
                  onChange={(e) => setText("petsAllowed")(e.target.checked)}
                  disabled={!canEdit}
                />
                <span>{t("landlordListings.petsAllowed")}</span>
              </label>
              <label className="listing-editor-check">
                <input
                  type="checkbox"
                  checked={form.nonSmokingOnly}
                  onChange={(e) => setText("nonSmokingOnly")(e.target.checked)}
                  disabled={!canEdit}
                />
                <span>{t("landlordListings.nonSmokingOnly")}</span>
              </label>
              <label className="listing-editor-check">
                <input
                  type="checkbox"
                  checked={form.closeToShops}
                  onChange={(e) => setText("closeToShops")(e.target.checked)}
                  disabled={!canEdit}
                />
                <span>{t("landlordListings.closeToShops")}</span>
              </label>
            </div>
          </section>

          <div className="listing-editor-actions">
            <button
              data-testid="submit-button"
              type="submit"
              className="listing-editor-submit"
              disabled={!canEdit || saving || !token}
            >
              {saving
                ? t("landlordListings.saving")
                : isEdit
                  ? t("landlordListings.save")
                  : t("landlordListings.createSubmit")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
