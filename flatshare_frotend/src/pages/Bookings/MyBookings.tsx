import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../../auth/AuthContext";
import { bookingService } from "./bookingService";
import { messageForBookingFailure } from "./bookingErrorUtils";
import { useCurrency } from "../../context/CurrencyContext";
import {
  formatBookingDate,
  statusClassName,
  bookingStatusLabel,
  paymentStatusLabel,
} from "./bookingUtils";
import "./Bookings.css";

export const MyBookings = () => {
  const { t, i18n } = useTranslation();
  const { formatListingPrice } = useCurrency();
  const { user, token } = useAuth();

  const { data, isLoading, isError, error } = useQuery({
    queryKey: ["bookings", "me", user?.id],
    queryFn: () => bookingService.listForCurrentUser(token!),
    enabled: !!token && !!user && user.role === "TENANT",
    staleTime: 0,
  });

  if (!user || user.role !== "TENANT") {
    return (
      <div className="bookings-page">
        <div className="bookings-inner">
          <p className="bookings-banner">{t("booking.tenantOnly")}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bookings-page">
      <div className="bookings-inner">
        <header className="bookings-head">
          <h1 className="bookings-title">{t("booking.myBookingsTitle")}</h1>
          <p className="bookings-lead">{t("booking.myBookingsLead")}</p>
        </header>

        {isLoading && <p>{t("booking.loading")}</p>}

        {isError && (
          <p className="bookings-error" role="alert">
            {messageForBookingFailure(error, t)}
          </p>
        )}

        {!isLoading && !isError && data?.length === 0 && (
          <div className="bookings-table-wrap">
            <p className="bookings-empty">{t("booking.empty")}</p>
          </div>
        )}

        {!isLoading && !isError && data && data.length > 0 && (
          <div className="bookings-table-wrap">
            <table className="bookings-table">
              <thead>
                <tr>
                  <th>{t("booking.colDates")}</th>
                  <th>{t("booking.colPrice")}</th>
                  <th>{t("booking.colStatus")}</th>
                  <th>{t("booking.colPayment")}</th>
                  <th>{t("booking.colActions")}</th>
                </tr>
              </thead>
              <tbody>
                {data.map((b) => (
                  <tr key={b.id}>
                    <td>
                      {formatBookingDate(b.startDate, i18n.language)} –{" "}
                      {formatBookingDate(b.endDate, i18n.language)}
                    </td>
                    <td>
                      {formatListingPrice(b.totalPrice, b.currency, i18n.language)}
                    </td>
                    <td>
                      <span className={statusClassName(b.status)}>
                        {bookingStatusLabel(b.status, t)}
                      </span>
                    </td>
                    <td>{paymentStatusLabel(b.paymentStatus, t)}</td>
                    <td>
                      <Link
                        to={`/my-bookings/${b.id}`}
                        className="bookings-link"
                      >
                        {t("booking.viewDetails")}
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};
