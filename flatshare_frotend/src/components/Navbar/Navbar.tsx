import { Link } from "react-router-dom";
import styles from "./Navbar.module.css";

const Navbar = () => {
  return (
    <nav className={styles.navbar}>
      <div className={styles.inner}>
        <div className={styles.logo}>FlatShare</div>

        <div className={styles.links}>
          <Link to="/">Home</Link>
          <Link to="/color-palette">Colors</Link>
          <Link to="/login">Login</Link>
          <Link to="/create-account">Create account</Link>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;