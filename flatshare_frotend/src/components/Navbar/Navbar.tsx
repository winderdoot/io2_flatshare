import { Link } from "react-router-dom";
import styles from "./Navbar.module.css";
import { useAuth } from "../../auth/AuthContext";

const Navbar = () => {
  const { user, logout } = useAuth();

  const handleLogout = () => {
    logout();
  };

  return (
    <nav className={styles.navbar}>
      <div className={styles.inner}>
        <div className={styles.logo}>FlatShare</div>

        <div className={styles.links}>
          <Link to="/">Home</Link>
          <Link to="/color-palette">Colors</Link>
          <Link to="/offer">Offer</Link>
          {!user && <Link to="/login">Login</Link> }
          {user && <Link to="/" onClick={handleLogout}>Log out {user}</Link>}
        </div>
      </div>
    </nav>
  );
};

export default Navbar;