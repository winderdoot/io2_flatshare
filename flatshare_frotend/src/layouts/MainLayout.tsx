import { Outlet } from "react-router-dom";
import Navbar from "../components/Navbar/Navbar";

const MainLayout = () => {
  return (
    <>
        <Navbar />
        <main style={{paddingTop: "var(--navbar-height)"}}>
          <Outlet />
        </main>
    </>
  );
};

export default MainLayout;