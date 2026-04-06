import { useTranslation } from "react-i18next";
import { useCurrency } from "../context/CurrencyContext";
import styles from "./Home.module.css";

const SAMPLE_RENT_PLN = [
  { key: "listing1" as const, pln: 1200 },
  { key: "listing2" as const, pln: 2800 },
  { key: "listing3" as const, pln: 950 },
];

export const Home = () => {
  const { t } = useTranslation();
  const { formatRentPln } = useCurrency();

  return (
    <div className={styles.page}>
      <h1>{t("home.title")}</h1>
      <p className={styles.subtitle}>{t("home.subtitle")}</p>
      <div className={styles.grid}>
        {SAMPLE_RENT_PLN.map(({ key, pln }) => (
          <article key={key} className={styles.card}>
            <h2>{t(`home.${key}`)}</h2>
            <p className={styles.price}>
              {formatRentPln(pln)} {t("home.perMonth")}
            </p>
          </article>
        ))}
      </div>
    </div>
  );
};
