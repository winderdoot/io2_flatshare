import { useCallback, useEffect, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../auth/AuthContext";
import type { TenantPreferences } from "../../models/tenantPreferences";
import {
  isPreferencesRequestError,
  tenantPreferencesService,
} from "./TenantPreferencesService";
import "./TenantPreferences.css";

const parseDistrictsInput = (raw: string): string[] =>
  raw
    .split(/[,;\n]/)
    .map((s) => s.trim())
    .filter(Boolean);

const boolSelectValue = (v: boolean | null | undefined): string => {
  if (v === true) return "true";
  if (v === false) return "false";
  return "";
};

const parseBoolSelect = (v: string): boolean | null => {
  if (v === "true") return true;
  if (v === "false") return false;
  return null;
};

function messageForPrefsFailure(
  e: unknown,
  t: (key: string, opt?: Record<string, string>) => string,
  kind: "load" | "save"
): string {
  if (isPreferencesRequestError(e)) {
    if (e.status === 401) return t("tenantPreferences.error401");
    if (e.status === 403) return t("tenantPreferences.error403");
    return kind === "load"
      ? t("tenantPreferences.loadErrorDetail", { message: e.message })
      : t("tenantPreferences.saveErrorDetail", { message: e.message });
  }
  return t("tenantPreferences.networkError");
}

function formatPriceDisplay(
  maxPrice: number | null | undefined,
  currency: string | null | undefined,
  locale: string
): string {
  if (maxPrice == null || Number.isNaN(maxPrice)) return "";
  const lng = locale === "en" ? "en-US" : "pl-PL";
  const num = maxPrice.toLocaleString(lng, {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  });
  return currency ? `${num} ${currency}` : num;
}

export const TenantPreferences = () => {
  const { t, i18n } = useTranslation();
  const { user, token } = useAuth();

  const [loaded, setLoaded] = useState<TenantPreferences | null>(null);
  const [maxPriceInput, setMaxPriceInput] = useState("");
  const [currency, setCurrency] = useState("");
  const [smoking, setSmoking] = useState("");
  const [pets, setPets] = useState("");
  const [districtsInput, setDistrictsInput] = useState("");

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [savedOk, setSavedOk] = useState(false);
  const [isEditing, setIsEditing] = useState(false);

  const applyDtoToForm = useCallback((dto: TenantPreferences) => {
    setMaxPriceInput(
      dto.maxPrice != null && !Number.isNaN(dto.maxPrice)
        ? String(dto.maxPrice)
        : ""
    );
    setCurrency(dto.currency ?? "");
    setSmoking(boolSelectValue(dto.smokingAllowed));
    setPets(boolSelectValue(dto.petsAllowed));
    setDistrictsInput((dto.preferredDistricts ?? []).join(", "));
  }, []);

  useEffect(() => {
    if (user?.role !== "TENANT" || !token) {
      setLoading(false);
      return;
    }

    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const dto = await tenantPreferencesService.get(token);
        if (cancelled) return;
        setLoaded(dto);
        applyDtoToForm(dto);
      } catch (e: unknown) {
        if (!cancelled) {
          setError(messageForPrefsFailure(e, t, "load"));
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [user?.role, token, applyDtoToForm, t]);

  const openEdit = () => {
    if (loaded) applyDtoToForm(loaded);
    setError(null);
    setSavedOk(false);
    setIsEditing(true);
  };

  const closeEdit = () => {
    if (loaded) applyDtoToForm(loaded);
    setError(null);
    setSavedOk(false);
    setIsEditing(false);
  };

  if (user?.role !== "TENANT") {
    return (
      <div className="tenant-prefs">
        <div className="tenant-prefs-inner">
          <h1 className="tenant-prefs-title">{t("tenantPreferences.title")}</h1>
          <p className="tenant-prefs-notice">{t("tenantPreferences.tenantOnly")}</p>
        </div>
      </div>
    );
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!token) return;

    const maxPriceTrim = maxPriceInput.trim();
    let maxPrice: number | null = null;
    if (maxPriceTrim !== "") {
      const n = Number(maxPriceTrim.replace(",", "."));
      if (Number.isNaN(n)) {
        setError(t("tenantPreferences.invalidPrice"));
        return;
      }
      maxPrice = n;
    }

    const payload: TenantPreferences = {
      maxPrice,
      currency: currency.trim() === "" ? null : currency.trim(),
      smokingAllowed: parseBoolSelect(smoking),
      petsAllowed: parseBoolSelect(pets),
      preferredDistricts: parseDistrictsInput(districtsInput),
    };

    setSaving(true);
    setError(null);
    setSavedOk(false);
    try {
      const updated = await tenantPreferencesService.put(token, payload);
      setLoaded(updated);
      applyDtoToForm(updated);
      setSavedOk(true);
      setIsEditing(false);
    } catch (e: unknown) {
      setError(messageForPrefsFailure(e, t, "save"));
    } finally {
      setSaving(false);
    }
  };

  const boolLabel = (v: boolean | null | undefined) => {
    if (v === true) return t("tenantPreferences.yes");
    if (v === false) return t("tenantPreferences.no");
    return t("tenantPreferences.notSet");
  };

  return (
    <div className="tenant-prefs">
      <div className="tenant-prefs-inner">
        <header className="tenant-prefs-head">
          <h1 className="tenant-prefs-title">{t("tenantPreferences.title")}</h1>
          <p className="tenant-prefs-subtitle">{t("tenantPreferences.subtitle")}</p>
        </header>

        {loading && (
          <div className="tenant-prefs-hero tenant-prefs-hero--skeleton" aria-busy="true">
            <div className="tenant-prefs-skel-row" />
            <div className="tenant-prefs-skel-grid">
              <div className="tenant-prefs-skel-block" />
              <div className="tenant-prefs-skel-block" />
              <div className="tenant-prefs-skel-block" />
              <div className="tenant-prefs-skel-block" />
            </div>
            <div className="tenant-prefs-skel-row tenant-prefs-skel-row--short" />
            <p className="tenant-prefs-loading-text">{t("tenantPreferences.loading")}</p>
          </div>
        )}

        {!loading && error && !loaded && (
          <div className="tenant-prefs-hero tenant-prefs-hero--error">
            <p className="tenant-prefs-error-msg">{error}</p>
          </div>
        )}

        {!loading && loaded && !isEditing && (
          <section className="tenant-prefs-hero" aria-labelledby="prefs-view-heading">
            <div className="tenant-prefs-hero-top">
              <h2 id="prefs-view-heading" className="tenant-prefs-hero-heading">
                {t("tenantPreferences.viewHeading")}
              </h2>
              {savedOk && (
                <p className="tenant-prefs-toast" role="status">
                  {t("tenantPreferences.saved")}
                </p>
              )}
            </div>

            <div className="tenant-prefs-metrics">
              <div className="tenant-prefs-metric">
                <span className="tenant-prefs-metric-label">
                  {t("tenantPreferences.maxPrice")}
                </span>
                <span className="tenant-prefs-metric-value tenant-prefs-metric-value--accent">
                  {loaded.maxPrice != null
                    ? formatPriceDisplay(
                        loaded.maxPrice,
                        loaded.currency,
                        i18n.language
                      )
                    : t("tenantPreferences.notSet")}
                </span>
              </div>
              <div className="tenant-prefs-metric">
                <span className="tenant-prefs-metric-label">
                  {t("tenantPreferences.smoking")}
                </span>
                <span className="tenant-prefs-metric-value">{boolLabel(loaded.smokingAllowed)}</span>
              </div>
              <div className="tenant-prefs-metric">
                <span className="tenant-prefs-metric-label">
                  {t("tenantPreferences.pets")}
                </span>
                <span className="tenant-prefs-metric-value">{boolLabel(loaded.petsAllowed)}</span>
              </div>
            </div>

            <div className="tenant-prefs-districts-section">
              <span className="tenant-prefs-metric-label">
                {t("tenantPreferences.districts")}
              </span>
              {(loaded.preferredDistricts?.length ?? 0) === 0 ? (
                <p className="tenant-prefs-districts-empty">{t("tenantPreferences.noneDistricts")}</p>
              ) : (
                <ul className="tenant-prefs-chip-list">
                  {loaded.preferredDistricts!.map((d) => (
                    <li key={d} className="tenant-prefs-chip">
                      {d}
                    </li>
                  ))}
                </ul>
              )}
            </div>

            <div className="tenant-prefs-hero-actions">
              <button type="button" className="tenant-prefs-btn tenant-prefs-btn-primary" onClick={openEdit}>
                {t("tenantPreferences.editCta")}
              </button>
            </div>
          </section>
        )}

        {!loading && isEditing && (
          <section className="tenant-prefs-edit" aria-labelledby="prefs-edit-heading">
            <div className="tenant-prefs-edit-card">
              <h2 id="prefs-edit-heading" className="tenant-prefs-edit-title">
                {t("tenantPreferences.editTitle")}
              </h2>
              <p className="tenant-prefs-edit-lead">{t("tenantPreferences.editLead")}</p>

              <form className="tenant-prefs-form" onSubmit={handleSubmit}>
                <div className="tenant-prefs-form-grid">
                  <div className="tenant-prefs-field">
                    <label htmlFor="maxPrice">{t("tenantPreferences.maxPrice")}</label>
                    <input
                      id="maxPrice"
                      type="number"
                      min={0}
                      step="0.01"
                      value={maxPriceInput}
                      onChange={(e) => setMaxPriceInput(e.target.value)}
                      placeholder={t("tenantPreferences.maxPricePlaceholder")}
                    />
                  </div>

                  <div className="tenant-prefs-field">
                    <label htmlFor="currency">{t("tenantPreferences.currency")}</label>
                    <select
                      id="currency"
                      value={currency}
                      onChange={(e) => setCurrency(e.target.value)}
                    >
                      <option value="">{t("tenantPreferences.currencyUnset")}</option>
                      <option value="PLN">PLN</option>
                      <option value="USD">USD</option>
                      <option value="EUR">EUR</option>
                      {currency &&
                        !["PLN", "USD", "EUR", ""].includes(currency) && (
                          <option value={currency}>{currency}</option>
                        )}
                    </select>
                  </div>

                  <div className="tenant-prefs-field">
                    <label htmlFor="smoking">{t("tenantPreferences.smoking")}</label>
                    <select
                      id="smoking"
                      value={smoking}
                      onChange={(e) => setSmoking(e.target.value)}
                    >
                      <option value="">{t("tenantPreferences.boolUnset")}</option>
                      <option value="true">{t("tenantPreferences.yes")}</option>
                      <option value="false">{t("tenantPreferences.no")}</option>
                    </select>
                  </div>

                  <div className="tenant-prefs-field">
                    <label htmlFor="pets">{t("tenantPreferences.pets")}</label>
                    <select
                      id="pets"
                      value={pets}
                      onChange={(e) => setPets(e.target.value)}
                    >
                      <option value="">{t("tenantPreferences.boolUnset")}</option>
                      <option value="true">{t("tenantPreferences.yes")}</option>
                      <option value="false">{t("tenantPreferences.no")}</option>
                    </select>
                  </div>
                </div>

                <div className="tenant-prefs-field tenant-prefs-field--full">
                  <label htmlFor="districts">{t("tenantPreferences.districts")}</label>
                  <textarea
                    id="districts"
                    value={districtsInput}
                    onChange={(e) => setDistrictsInput(e.target.value)}
                    placeholder={t("tenantPreferences.districtsPlaceholder")}
                    rows={4}
                  />
                  <p className="tenant-prefs-hint">{t("tenantPreferences.districtsHint")}</p>
                </div>

                {error && <p className="tenant-prefs-error-msg">{error}</p>}

                <div className="tenant-prefs-form-actions">
                  <button
                    type="button"
                    className="tenant-prefs-btn tenant-prefs-btn-secondary"
                    onClick={closeEdit}
                    disabled={saving}
                  >
                    {t("tenantPreferences.cancel")}
                  </button>
                  <button
                    type="submit"
                    className="tenant-prefs-btn tenant-prefs-btn-primary"
                    disabled={saving}
                  >
                    {saving ? t("tenantPreferences.saving") : t("tenantPreferences.save")}
                  </button>
                </div>
              </form>
            </div>
          </section>
        )}
      </div>
    </div>
  );
};
