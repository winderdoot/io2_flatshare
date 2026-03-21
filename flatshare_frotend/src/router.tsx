import { createBrowserRouter } from "react-router-dom";
import MainLayout from "./layouts/MainLayout";
import { Home } from "./pages/Home";
import { ColorPalette } from "./pages/ColorPalette";

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
    ],
  },
]);