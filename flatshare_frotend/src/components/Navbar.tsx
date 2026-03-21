import { Link } from "react-router-dom";

const Navbar = () => {
  return (
    <nav style={{ display: "flex", gap: "16px", padding: "16px" }}>
      <Link to="/">Home</Link>
      <Link to="/color-palette">Colors</Link>
    </nav>
  );
};

export default Navbar;