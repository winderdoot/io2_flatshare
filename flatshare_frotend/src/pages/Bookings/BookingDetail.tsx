import { useEffect, useRef, useState, type FormEvent } from "react";
import { Link, useParams, useSearchParams } from "react-router-dom";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../auth/AuthContext";
import { landlordListingsService } from "../LandlordListings/LandlordListingsService";
import { bookingService } from "./bookingService";
import { messageForBookingFailure } from "./bookingErrorUtils";
import { paymentService } from "./paymentService";
import type { GatewayPaymentStatus } from "../../models/payment";
import {
  canLandlordAcceptReject,
  canTenantCancel,
  formatBookingDate,
  formatDateTime,
  statusClassName,
} from "./bookingUtils";
import { compareIso, toIsoDate } from "../../components/AvailabilityCalendar/availabilityCalendarUtils";
import "./Bookings.css";

function tenantBookingDetailUrl(bookingId: string): string {
  return `${window.location.origin}/my-bookings/${bookingId}`;
}

function buildPaymentUrls(bookingId: string) {
  const base = tenantBookingDetailUrl(bookingId);
  return {
    returnUrl: `${base}?payment=return`,
    cancelUrl: `${base}?payment=cancelled`,
  };
}

function messageForPaymentReturn(
  status: GatewayPaymentStatus,
  t: (key: string) => string
): { kind: "success" | "error" | "info"; message: string } {
  switch (status) {
    case "Succeeded":
      return { kind: "success", message: t("booking.paymentSuccess") };
    case "Failed":
      return { kind: "error", message: t("booking.paymentFailed") };
    case "Cancelled":
      return { kind: "error", message: t("booking.paymentCancelled") };
    default:
      return { kind: "info", message: t("booking.paymentProcessing") };
  }
}

export const BookingDetail = () => {
  const { bookingId } = useParams<{ bookingId: string }>();
  const [searchParams, setSearchParams] = useSearchParams();
  const { t, i18n } = useTranslation();
  const { user, token } = useAuth();
  const queryClient = useQueryClient();
  const returnHandledRef = useRef<string | null>(null);

  const [cancelReason, setCancelReason] = useState("");
  const [rejectReason, setRejectReason] = useState("");
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [acceptInfo, setAcceptInfo] = useState<string | null>(null);
  const [verifyingPayment, setVerifyingPayment] = useState(false);

  const isLandlord = user?.role === "LANDLORD";
  const isTenant = user?.role === "TENANT";
  const backTo = isLandlord ? "/booking-requests" : "/my-bookings";

  const { data: booking, isLoading, isError, error } = useQuery({
    queryKey: ["booking", bookingId],
    queryFn: () => bookingService.getById(token!, bookingId!),
    enabled: !!token && !!bookingId,
    staleTime: 0,
  });

  const { data: listing } = useQuery({
    queryKey: ["listing", booking?.listingId],
    queryFn: () => landlordListingsService.getById(booking!.listingId),
    enabled: !!booking?.listingId,
  });

  const refresh = async () => {
    await queryClient.invalidateQueries({ queryKey: ["booking", bookingId] });
    await queryClient.invalidateQueries({ queryKey: ["bookings"] });
  };

  const acceptMutation = useMutation({
    mutationFn: () => bookingService.accept(token!, bookingId!),
    onSuccess: async (resp) => {
      setActionError(null);
      setAcceptInfo(
        t("booking.acceptSuccess", {
          deadline: formatDateTime(resp.paymentRequiredUntil, i18n.language),
        })
      );
      await refresh();
    },
    onError: (e: unknown) => {
      setSuccess(null);
      setActionError(messageForBookingFailure(e, t));
    },
  });

  const rejectMutation = useMutation({
    mutationFn: () =>
      bookingService.reject(token!, bookingId!, { reason: rejectReason.trim() }),
    onSuccess: async () => {
      setShowRejectModal(false);
      setRejectReason("");
      setActionError(null);
      setSuccess(t("booking.rejectSuccess"));
      await refresh();
    },
    onError: (e: unknown) => {
      setActionError(messageForBookingFailure(e, t));
    },
  });

  const cancelMutation = useMutation({
    mutationFn: () =>
      bookingService.cancel(token!, bookingId!, { reason: cancelReason.trim() }),
    onSuccess: async () => {
      setActionError(null);
      setSuccess(t("booking.cancelSuccess"));
      setCancelReason("");
      await refresh();
    },
    onError: (e: unknown) => {
      setActionError(messageForBookingFailure(e, t));
    },
  });

  const payMutation = useMutation({
    mutationFn: () => {
      const urls = buildPaymentUrls(bookingId!);
      return bookingService.pay(token!, bookingId!, {
        paymentMethod: "card",
        returnUrl: urls.returnUrl,
        cancelUrl: urls.cancelUrl,
      });
    },
    onSuccess: (resp) => {
      if (!resp.redirectUrl) {
        setActionError(t("booking.paymentNoRedirect"));
        return;
      }
      window.location.assign(resp.redirectUrl);
    },
    onError: (e: unknown) => {
      setSuccess(null);
      setActionError(messageForBookingFailure(e, t));
    },
  });

  const paymentReturn = searchParams.get("payment");

  useEffect(() => {
    if (!paymentReturn || !token || !bookingId || !isTenant) return;

    const key = `${bookingId}:${paymentReturn}`;
    if (returnHandledRef.current === key) return;
    returnHandledRef.current = key;

    setSearchParams({}, { replace: true });

    if (paymentReturn === "cancelled") {
      setSuccess(null);
      setActionError(t("booking.paymentCancelled"));
      return;
    }

    if (paymentReturn !== "return") return;

    const verify = async () => {
      setVerifyingPayment(true);
      setActionError(null);
      setSuccess(null);
      try {
        const payment = await paymentService.getByBookingId(token, bookingId);
        await refresh();
        const { kind, message } = messageForPaymentReturn(payment.status, t);
        if (kind === "success") {
          setSuccess(message);
        } else if (kind === "error") {
          setActionError(message);
        } else {
          setSuccess(message);
        }
      } catch (e: unknown) {
        setActionError(messageForBookingFailure(e, t));
      } finally {
        setVerifyingPayment(false);
      }
    };

    void verify();
  }, [
    paymentReturn,
    token,
    bookingId,
    isTenant,
    setSearchParams,
    t,
    queryClient,
  ]);

  const handleCancel = (e: FormEvent) => {
    e.preventDefault();
    setActionError(null);
    setSuccess(null);
    if (!cancelReason.trim()) {
      setActionError(t("booking.cancelReasonRequired"));
      return;
    }
    cancelMutation.mutate();
  };

  const handleReject = (e: FormEvent) => {
    e.preventDefault();
    setActionError(null);
    if (!rejectReason.trim()) {
      setActionError(t("booking.rejectReasonRequired"));
      return;
    }
    rejectMutation.mutate();
  };

  const busy =
    acceptMutation.isPending ||
    rejectMutation.isPending ||
    cancelMutation.isPending ||
    payMutation.isPending ||
    verifyingPayment;

  const showPayButton =
    isTenant && booking?.status === "PendingPayment";

  const todayIso = toIsoDate(new Date());
  const stayStarted =
    booking && compareIso(todayIso, booking.startDate) >= 0;
  const showCancelForm =
    isTenant &&
    booking &&
    canTenantCancel(booking.status) &&
    !stayStarted;

  if (!bookingId) {
    return (
      <div className="bookings-page">
        <div className="bookings-inner">
          <p className="bookings-error">{t("booking.missingId")}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bookings-page">
      <div className="bookings-inner">
        <Link to={backTo} className="bookings-back">
          {t("booking.backToList")}
        </Link>

        <header className="bookings-head">
          <h1 className="bookings-title">{t("booking.detailTitle")}</h1>
        </header>

        {(isLoading || verifyingPayment) && (
          <p>{verifyingPayment ? t("booking.verifyingPayment") : t("booking.loading")}</p>
        )}

        {isError && (
          <p className="bookings-error" role="alert">
            {messageForBookingFailure(error, t)}
          </p>
        )}

        {success && (
          <p className="bookings-success" role="status">
            {success}
          </p>
        )}
        {acceptInfo && (
          <p className="bookings-success" role="status">
            {acceptInfo}
          </p>
        )}
        {actionError && (
          <p className="bookings-error" role="alert">
            {actionError}
          </p>
        )}

        {booking && (
          <div className="bookings-detail-card">
            <div className="bookings-detail-grid">
              <div className="bookings-detail-field">
                <label>{t("booking.fieldListing")}</label>
                <p>
                  {listing ? (
                    <Link
                      to={`/offer/${booking.listingId}`}
                      className="bookings-link"
                    >
                      {listing.title}
                    </Link>
                  ) : (
                    booking.listingId
                  )}
                </p>
              </div>
              <div className="bookings-detail-field">
                <label>{t("booking.colStatus")}</label>
                <p>
                  <span className={statusClassName(booking.status)}>
                    {t(`booking.status.${booking.status}`)}
                  </span>
                </p>
              </div>
              <div className="bookings-detail-field">
                <label>{t("booking.fieldDates")}</label>
                <p>
                  {formatBookingDate(booking.startDate, i18n.language)} –{" "}
                  {formatBookingDate(booking.endDate, i18n.language)}
                </p>
              </div>
              <div className="bookings-detail-field">
                <label>{t("booking.colPrice")}</label>
                <p>
                  {booking.totalPrice} {booking.currency}
                </p>
              </div>
              <div className="bookings-detail-field">
                <label>{t("booking.colPayment")}</label>
                <p>{t(`booking.paymentStatus.${booking.paymentStatus}`)}</p>
              </div>
              {isLandlord && (
                <div className="bookings-detail-field">
                  <label>{t("booking.fieldTenantId")}</label>
                  <p>{booking.tenantId}</p>
                </div>
              )}
            </div>

            {showPayButton && (
              <div className="bookings-actions bookings-actions--pay">
                <p className="bookings-pay-lead">{t("booking.payLead")}</p>
                <button
                  type="button"
                  className="bookings-btn bookings-btn--primary bookings-btn--pay"
                  disabled={busy}
                  onClick={() => {
                    setActionError(null);
                    setSuccess(null);
                    payMutation.mutate();
                  }}
                >
                  {payMutation.isPending ? (
                    <>
                      <span className="spinner" aria-hidden="true" />
                      {t("booking.paying")}
                    </>
                  ) : (
                    t("booking.payNow")
                  )}
                </button>
              </div>
            )}

            {isLandlord && canLandlordAcceptReject(booking.status) && (
              <div className="bookings-actions">
                <button
                  type="button"
                  className="bookings-btn bookings-btn--primary"
                  disabled={busy}
                  onClick={() => {
                    setActionError(null);
                    setSuccess(null);
                    acceptMutation.mutate();
                  }}
                >
                  {acceptMutation.isPending
                    ? t("booking.accepting")
                    : t("booking.accept")}
                </button>
                <button
                  type="button"
                  className="bookings-btn bookings-btn--danger"
                  disabled={busy}
                  onClick={() => {
                    setActionError(null);
                    setShowRejectModal(true);
                  }}
                >
                  {t("booking.reject")}
                </button>
              </div>
            )}

            {showCancelForm && (
              <form className="bookings-form" onSubmit={handleCancel}>
                <h3>{t("booking.cancelSection")}</h3>
                <label>
                  {t("booking.cancelReasonLabel")}
                  <textarea
                    value={cancelReason}
                    onChange={(e) => setCancelReason(e.target.value)}
                    disabled={busy}
                    placeholder={t("booking.cancelReasonPlaceholder")}
                    required
                  />
                </label>
                <button
                  type="submit"
                  className="bookings-btn bookings-btn--danger"
                  disabled={busy}
                >
                  {cancelMutation.isPending
                    ? t("booking.cancelling")
                    : t("booking.cancel")}
                </button>
              </form>
            )}
          </div>
        )}

        {showRejectModal && (
          <div
            className="bookings-modal-overlay"
            role="dialog"
            aria-modal="true"
            aria-labelledby="reject-modal-title"
          >
            <div className="bookings-modal">
              <h3 id="reject-modal-title">{t("booking.rejectModalTitle")}</h3>
              <p>{t("booking.rejectModalLead")}</p>
              <form onSubmit={handleReject}>
                <label className="bookings-form">
                  {t("booking.rejectReasonLabel")}
                  <textarea
                    value={rejectReason}
                    onChange={(e) => setRejectReason(e.target.value)}
                    disabled={rejectMutation.isPending}
                    placeholder={t("booking.rejectReasonPlaceholder")}
                    required
                  />
                </label>
                <div className="bookings-modal-actions">
                  <button
                    type="button"
                    className="bookings-btn bookings-btn--secondary"
                    disabled={rejectMutation.isPending}
                    onClick={() => {
                      setShowRejectModal(false);
                      setRejectReason("");
                      setActionError(null);
                    }}
                  >
                    {t("booking.rejectModalCancel")}
                  </button>
                  <button
                    type="submit"
                    className="bookings-btn bookings-btn--danger"
                    disabled={rejectMutation.isPending}
                  >
                    {rejectMutation.isPending
                      ? t("booking.rejecting")
                      : t("booking.rejectConfirm")}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
