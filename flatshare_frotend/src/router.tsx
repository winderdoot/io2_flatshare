import * as React from "react";
import { createBrowserRouter } from "react-router-dom";
import MainLayout from "./layouts/MainLayout";

void React;

import { Home } from "./pages/Home";
import { Login } from "./pages/Login/Login";
import { Registry } from "./pages/Registry/Registry";
import AuthLayout from "./layouts/AuthLayout";
import FlatOffers from "./pages/FlatOffers/FlatOffers";
import { ListingDetail } from "./pages/ListingDetail/ListingDetail";
import { PrivateRoute } from "./auth/PrivateRoute";
import { RestartPassword } from "./pages/RestartPassword/RestartPassword";
import { TenantPreferences } from "./pages/TenantPreferences/TenantPreferences";
import { LandlordListings } from "./pages/LandlordListings/LandlordListings";
import { ListingEditor } from "./pages/LandlordListings/ListingEditor";
import { ListingAvailability } from "./pages/LandlordListings/ListingAvailability";
import { AdminPanel } from "./pages/AdminPanel/AdminPanel";
import { AdminRoute } from "./auth/AdminRoute";
import { MyBookings } from "./pages/Bookings/MyBookings";
import { BookingDetail } from "./pages/Bookings/BookingDetail";
import { LandlordBookingRequests } from "./pages/Bookings/LandlordBookingRequests";

export const router = createBrowserRouter([
  {
    element: <MainLayout />,
    children: [
      {
        path: "/",
        element: <Home />,
      },
      {
        path: "/offer",
        element: (
          <PrivateRoute>
            <FlatOffers />
          </PrivateRoute>
        ),
      },
      {
        path: "/offer/:listingId",
        element: (
          <PrivateRoute>
            <ListingDetail />
          </PrivateRoute>
        ),
      },
      {
        path: "/preferences",
        element: (
          <PrivateRoute>
            <TenantPreferences />
          </PrivateRoute>
        ),
      },
      {
        path: "/my-listings",
        element: (
          <PrivateRoute>
            <LandlordListings />
          </PrivateRoute>
        ),
      },
      {
        path: "/my-listings/new",
        element: (
          <PrivateRoute>
            <ListingEditor key="listing-new" />
          </PrivateRoute>
        ),
      },
      {
        path: "/my-listings/:listingId/edit",
        element: (
          <PrivateRoute>
            <ListingEditor />
          </PrivateRoute>
        ),
      },
      {
        path: "/my-listings/:listingId/availability",
        element: (
          <PrivateRoute>
            <ListingAvailability />
          </PrivateRoute>
        ),
      },
      {
        path: "/my-bookings",
        element: (
          <PrivateRoute>
            <MyBookings />
          </PrivateRoute>
        ),
      },
      {
        path: "/my-bookings/:bookingId",
        element: (
          <PrivateRoute>
            <BookingDetail />
          </PrivateRoute>
        ),
      },
      {
        path: "/booking-requests",
        element: (
          <PrivateRoute>
            <LandlordBookingRequests />
          </PrivateRoute>
        ),
      },
      {
        path: "/booking-requests/:bookingId",
        element: (
          <PrivateRoute>
            <BookingDetail />
          </PrivateRoute>
        ),
      },
      {
        path: "/admin",
        element: (
          <AdminRoute>
            <AdminPanel />
          </AdminRoute>
        ),
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
      {
        path: "/forgot-password",
        element: <RestartPassword />,
      },
    ],
  },
]);
