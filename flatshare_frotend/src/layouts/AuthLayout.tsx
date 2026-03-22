import { Outlet } from "react-router-dom";
import Navbar from "../components/Navbar/Navbar";

const AuthLayout = () => {
  return (
    <>
        <Navbar/>
        <Outlet />
    </>
  );
};

export default AuthLayout;