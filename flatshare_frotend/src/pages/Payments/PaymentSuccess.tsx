import { useEffect, useRef } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { paymentService } from "../Bookings/paymentService";
import { useAuth } from "../../auth/AuthContext";
import "./Payment.css";

/**
 * Strona powrotu po udanej płatności Stripe (team2).
 * Team2 hardkoduje SuccessUrl jako /payments/success?session_id=...
 * Po wyświetleniu przekierowuje na listę rezerwacji.
 */
export function PaymentSuccess() {
  const { t } = useTranslation();
  const { token } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const handledRef = useRef(false);

  useEffect(() => {
    if (handledRef.current) return;
    handledRef.current = true;

    const sessionId = searchParams.get("session_id");

    if (token && sessionId) {
      paymentService
        .getById(token, sessionId)
        .catch(() => undefined);
    }

    const timer = setTimeout(() => {
      navigate("/my-bookings", { replace: true });
    }, 3500);

    return () => clearTimeout(timer);
  }, [token, searchParams, navigate]);

  return (
    <div className="payment-result">
      <div className="payment-result__icon payment-result__icon--success">✓</div>
      <h1 className="payment-result__title">{t("payment.successTitle")}</h1>
      <p className="payment-result__desc">{t("payment.successDesc")}</p>
      <p className="payment-result__redirect">{t("payment.redirecting")}</p>
    </div>
  );
}
