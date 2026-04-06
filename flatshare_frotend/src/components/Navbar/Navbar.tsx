import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { LocaleSwitcher } from "../LocaleSwitcher/LocaleSwitcher";
import styles from "./Navbar.module.css";

const Navbar = () => {
  const { t } = useTranslation();

  return (
    <nav className={styles.navbar}>
      <div className={styles.inner}>
        <div className={styles.logo}>FlatShare</div>

        <div className={styles.right}>
          <LocaleSwitcher />
          <div className={styles.links}>
            <Link to="/">{t("nav.home")}</Link>
            <Link to="/color-palette">{t("nav.colors")}</Link>
            <Link to="/login">{t("nav.account")}</Link>
          </div>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;