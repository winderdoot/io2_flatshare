import { Link, NavLink, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { LocaleSwitcher } from "../LocaleSwitcher/LocaleSwitcher";
import { ThemeToggle } from "../ThemeToggle/ThemeToggle";
import styles from "./Navbar.module.css";
import { useAuth } from "../../auth/AuthContext";

const Navbar = () => {
  const { t } = useTranslation();
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    // Najpierw wejdź na publiczną stronę. /offer, /preferences i /my-listings
    // są za PrivateRoute — po logout() bez zmiany URL PrivateRoute robi
    // przekierowanie na /login z state.from = aktualna ścieżka (np. /preferences).
    navigate("/", { replace: true });
    logout();
  };

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `${styles.link} ${isActive ? styles.linkActive : ""}`;

  return (
    <nav className={styles.navbar}>
      <div className={styles.inner}>
        <Link to="/" className={styles.brand} aria-label="FlatShare">
          <span className={styles.brandMark} aria-hidden="true">FS</span>
          <span className={styles.brandText}>FlatShare</span>
        </Link>

        <div className={styles.links}>
          <NavLink to="/" end className={linkClass}>{t("nav.home")}</NavLink>
          <NavLink to="/offer" className={linkClass}>{t("nav.offer")}</NavLink>
          {user?.role === "TENANT" && (
            <>
              <NavLink to="/preferences" className={linkClass}>{t("nav.preferences")}</NavLink>
              <NavLink to="/my-bookings" className={linkClass}>{t("nav.myBookings")}</NavLink>
            </>
          )}
          {user?.role === "LANDLORD" && (
            <>
              <NavLink to="/my-listings" className={linkClass}>{t("nav.myListings")}</NavLink>
              <NavLink to="/booking-requests" className={linkClass}>{t("nav.bookingRequests")}</NavLink>
            </>
          )}
          {user?.role === "ADMIN" && (
            <NavLink to="/admin" className={linkClass}>{t("nav.admin")}</NavLink>
          )}
        </div>

        <div className={styles.right}>
          <ThemeToggle />
          <LocaleSwitcher />
          {user ? (
            <button
              type="button"
              onClick={handleLogout}
              className={styles.authBtn}
            >
              <span className={styles.authBtnUser}>{user.firstName}</span>
              <span className={styles.authBtnLabel}>{t("nav.log_out")}</span>
            </button>
          ) : (
            <Link to="/login" className={`${styles.authBtn} ${styles.authBtnWarm}`}>
              {t("nav.account")}
            </Link>
          )}
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
