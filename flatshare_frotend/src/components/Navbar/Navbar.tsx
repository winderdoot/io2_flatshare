import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { LocaleSwitcher } from "../LocaleSwitcher/LocaleSwitcher";
import styles from "./Navbar.module.css";
import { useAuth } from "../../auth/AuthContext";

const Navbar = () => {
  const { t } = useTranslation();
  const { user, logout } = useAuth();

  const handleLogout = () => {
    logout();
  };

  return (
    <nav className={styles.navbar}>
      <div className={styles.inner}>
        <div className={styles.logo}>FlatShare</div>

        <div className={styles.links}>          
          <Link to="/">{t("nav.home")}</Link>
          <Link to="/color-palette">{t("nav.colors")}</Link>
          <Link to="/offer">{t("nav.offer")}</Link>
          {user?.role === "TENANT" && (
            <Link to="/preferences">{t("nav.preferences")}</Link>
          )}
          {user?.role === "LANDLORD" && (
            <Link to="/my-listings">{t("nav.myListings")}</Link>
          )}
          {user?.role === "ADMIN" && (
            <Link to="/admin">{t("nav.admin")}</Link>
          )}
          { user && <Link to="/" onClick={handleLogout}>{t("nav.log_out")} {user.firstName}</Link>}
          { !user && <Link to="/login">{t("nav.account")}</Link>}
        </div>

        <div className={styles.locale}>
          <LocaleSwitcher />          
        </div>
      </div>
    </nav>
  );
};

export default Navbar;