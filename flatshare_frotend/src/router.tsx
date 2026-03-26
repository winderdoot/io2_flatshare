import { createBrowserRouter } from "react-router-dom";
import MainLayout from "./layouts/MainLayout";
import { Home } from "./pages/Home";
import { ColorPalette } from "./pages/ColorPalette";
import { Login } from "./pages/Login/Login";
import { Registry } from "./pages/Registry/Registry";
import AuthLayout from "./layouts/AuthLayout";
import FlatOffers from "./pages/FlatOffers/FlatOffers";

export const router = createBrowserRouter([
  {
    
    element: <MainLayout />,
    children: [
      {
        path: "/",
        element: <Home />,
      },
      {
        path: "/color-palette",
        element: <ColorPalette />,
      },
      {
        path: "/offer",
        element: <FlatOffers />,
      },
    ],
  },
  {    
    element: <AuthLayout />,
    children: [
      {
        path: "/login",
        element: <Login />,
      },
      {
        path: "/create-account",
        element: <Registry />,
      },
    ],
  },
]);