import { useTranslation } from "react-i18next";
import { useCurrency } from "../../context/CurrencyContext";
import styles from "./LocaleSwitcher.module.css";

export function LocaleSwitcher() {
  const { t, i18n } = useTranslation();
  const { currency, setCurrency } = useCurrency();
  const lang = i18n.resolvedLanguage ?? i18n.language;

  return (
    <div className={styles.wrap}>
      <div className={styles.group} role="group" aria-label={t("locale.language")}>
        <span className={styles.srOnly}>{t("locale.language")}</span>
        <button
          type="button"
          className={lang.startsWith("pl") ? styles.active : ""}
          onClick={() => void i18n.changeLanguage("pl")}
        >
          {t("locale.pl")}
        </button>
        <button
          type="button"
          className={lang.startsWith("en") ? styles.active : ""}
          onClick={() => void i18n.changeLanguage("en")}
        >
          {t("locale.en")}
        </button>
      </div>
      <div className={styles.group} role="group" aria-label={t("locale.currency")}>
        <span className={styles.srOnly}>{t("locale.currency")}</span>
        <button
          type="button"
          className={currency === "PLN" ? styles.active : ""}
          onClick={() => setCurrency("PLN")}
        >
          zł
        </button>
        <button
          type="button"
          className={currency === "USD" ? styles.active : ""}
          onClick={() => setCurrency("USD")}
        >
          $
        </button>
      </div>
    </div>
  );
}
