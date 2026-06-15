import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import styles from "./SearchFilters.module.css";
import { cityDisplayName, districtDisplayName, locationConfig, type City } from "./locationConfig";
import FormattedNumberInput from "../FormattedNumberInput/FormattedNumberInput";
import { Filters, Profile, useFiltersStore } from "./FiltersStore";
import { profileConfig } from "./profileConfig";
import { useSearchPreferences } from "./useSearchPreferences";
import { useCurrency } from "../../context/CurrencyContext";

export default function SearchFilters() {
  const { t, i18n } = useTranslation();
  const { currency } = useCurrency();
  const priceSuffix = currency === "USD" ? "$" : "zł";

  const { filters, setFilter, applyFilters } = useFiltersStore();
  const preferencesReady = useSearchPreferences();

  useEffect(() => {
    if (!preferencesReady) return;

    const timer = window.setTimeout(() => {
      applyFilters();
    }, 400);

    return () => window.clearTimeout(timer);
  }, [filters, preferencesReady, applyFilters]);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target;

    if (name === "profile") {
      setFilter("profile", value as Profile);
      return;
    }

    setFilter(
      name as keyof Filters,
      type === "checkbox"
        ? (e.target as HTMLInputElement).checked
        : value
    );
  };

  const handleCityChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const city = e.target.value as City | "";

    setFilter("city", city);
    setFilter("district", "");
  };

  return (
    <div className={styles.container}>
      <div className={styles.row}>
        <div className={styles.col}>
          <label htmlFor="filter-city">{t("filters.city")}</label>
          <select
            id="filter-city"
            name="city"
            value={filters.city}
            onChange={handleCityChange}
            className={styles.select}
          >
            <option value="">{t("filters.city")}</option>
            {Object.keys(locationConfig).map((city) => (
              <option key={city} value={city}>
                {cityDisplayName(city as City, i18n.language)}
              </option>
            ))}
          </select>
        </div>

        <div className={styles.col}>
          <label htmlFor="filter-district">{t("filters.district")}</label>
          <select
            id="filter-district"
            name="district"
            value={filters.district}
            onChange={handleChange}
            disabled={!filters.city}
            className={styles.select}
          >
            <option value="">{t("filters.district")}</option>
            {filters.city &&
              locationConfig[filters.city].map((d) => (
                <option key={d} value={d}>
                  {districtDisplayName(filters.city as City, d, i18n.language)}
                </option>
              ))}
          </select>
        </div>

        <div className={styles.col}>
          <label htmlFor="filter-profile">{t("filters.profile")}</label>
          <select
            id="filter-profile"
            name="profile"
            value={filters.profile}
            onChange={handleChange}
            className={styles.select}
          >
            <option value="">{t("filters.profile")}</option>
            {Object.entries(profileConfig).map(([value, label]) => (
              <option key={value} value={value}>
                {t(label)}
              </option>
            ))}
          </select>
        </div>
      </div>

      <div className={styles.row}>
        <div className={styles.col}>
          <label>{t("filters.price")}</label>
          <div className={styles.row}>     
            <FormattedNumberInput
                value={filters.minPrice}
                onChange={(val) => setFilter("minPrice", val)}
                placeholder={t("filters.from")}
                suffix={priceSuffix}
              />
            <FormattedNumberInput
              value={filters.maxPrice}
              onChange={(val) => setFilter("maxPrice", val)}
              placeholder={t("filters.to")}
              suffix={priceSuffix}
            />
          </div>
        </div>

        <div className={styles.col}>
          <label>{t("filters.area")}</label>
          <div className={styles.row}>
            <FormattedNumberInput
              value={filters.minArea}
              onChange={(val) => setFilter("minArea", val)}
              placeholder={t("filters.from")}
              suffix="m²"
            />
            <FormattedNumberInput
              value={filters.maxArea}
              onChange={(val) => setFilter("maxArea", val)}
              placeholder={t("filters.to")}
              suffix="m²"
            />
          </div>
        </div>
      </div>

      <div className={styles.row}>
        <label className={styles.checkboxGroup}>
          <input
            type="checkbox"
            name="petsAllowed"
            checked={filters.petsAllowed}
            onChange={handleChange}
          />
          {t("filters.petsAllowed")}
        </label>

        <label className={styles.checkboxGroup}>
          <input
            type="checkbox"
            name="nonSmokingOnly"
            checked={filters.nonSmokingOnly}
            onChange={handleChange}
          />
          {t("filters.nonSmoking")}
        </label>

        <label className={styles.checkboxGroup}>
          <input
            type="checkbox"
            name="closeToShops"
            checked={filters.closeToShops}
            onChange={handleChange}
          />
          {t("filters.closeToShops")}
        </label>

        <div className={styles.col}>
          <label>{t("filters.startDate")}</label>
          <input
            type="date"
            name="startDate"
            value={filters.startDate}
            onChange={handleChange}
            className={styles.input}
          />
        </div>
      </div>

      <button className={styles.button} onClick={applyFilters}>
        {t("filters.search")}
      </button>
    </div>
  );
}