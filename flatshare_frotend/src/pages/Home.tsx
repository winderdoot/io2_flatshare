import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import styles from "./Home.module.css";

const FEATURES = [
  { key: "Verified", titleKey: "home.featureVerifiedTitle", descKey: "home.featureVerifiedDesc" },
  { key: "Matching", titleKey: "home.featureMatchingTitle", descKey: "home.featureMatchingDesc" },
  { key: "Bilingual", titleKey: "home.featureBilingualTitle", descKey: "home.featureBilingualDesc" },
];

export const Home = () => {
  const { t } = useTranslation();

  return (
    <div className={styles.page}>
      <section className={styles.hero}>
        <div className={styles.heroGlowA} aria-hidden="true" />
        <div className={styles.heroGlowB} aria-hidden="true" />

        <div className={styles.heroInner}>
          <div className={styles.brandRow}>
            <span className={styles.brandMark} aria-hidden="true">FS</span>
            <span className={styles.brandWordmark}>FlatShare</span>
          </div>

          <span className={styles.heroBadge}>{t("home.heroBadge")}</span>

          <h1 className={styles.heroTitle}>{t("home.heroTitle")}</h1>
          <p className={styles.heroSubtitle}>{t("home.heroSubtitle")}</p>

          <div className={styles.heroActions}>
            <Link to="/offer" className={`${styles.btn} ${styles.btnPrimary}`}>
              {t("home.heroPrimary")}
            </Link>
            <Link to="/login" className={`${styles.btn} ${styles.btnGhost}`}>
              {t("home.heroSecondary")}
            </Link>
          </div>
        </div>
      </section>

      <section className={styles.features}>
        {FEATURES.map((f) => (
          <article key={f.key} className={styles.featureCard}>
            <span className={styles.featureDot} aria-hidden="true" />
            <h3 className={styles.featureTitle}>{t(f.titleKey)}</h3>
            <p className={styles.featureDesc}>{t(f.descKey)}</p>
          </article>
        ))}
      </section>
    </div>
  );
};
