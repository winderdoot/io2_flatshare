import { useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import "./Payment.css";

/**
 * Strona powrotu po anulowaniu płatności Stripe (team2).
 * Team2 hardkoduje CancelUrl jako /payments/cancel
 */
export function PaymentCancel() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const handledRef = useRef(false);

  useEffect(() => {
    if (handledRef.current) return;
    handledRef.current = true;

    const timer = setTimeout(() => {
      navigate("/my-bookings", { replace: true });
    }, 3500);

    return () => clearTimeout(timer);
  }, [navigate]);

  return (
    <div className="payment-result">
      <div className="payment-result__icon payment-result__icon--cancel">✕</div>
      <h1 className="payment-result__title">{t("payment.cancelTitle")}</h1>
      <p className="payment-result__desc">{t("payment.cancelDesc")}</p>
      <p className="payment-result__redirect">{t("payment.redirecting")}</p>
    </div>
  );
}
