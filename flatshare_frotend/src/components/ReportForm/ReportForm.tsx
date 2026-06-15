import { useEffect, useRef, useState, type FormEvent } from "react";
import { useTranslation } from "react-i18next";
import type { CreateReportBody, ReportType } from "../../models/report";
import { isReportRequestError, reportService } from "../../pages/Reports/reportService";
import { BACKEND_TYPE } from "../../config";
import "./ReportForm.css";

type ReportFormProps = {
  open: boolean;
  onClose: () => void;
  token: string;
  type: ReportType;
  targetId: string;
  title: string;
  onSuccess?: () => void;
};

type Step = "form" | "success";

export function ReportForm({
  open,
  onClose,
  token,
  type,
  targetId,
  title,
  onSuccess,
}: ReportFormProps) {
  const { t } = useTranslation();
  const formOverlayRef = useRef<HTMLDivElement>(null);
  const successOverlayRef = useRef<HTMLDivElement>(null);
  const [step, setStep] = useState<Step>("form");
  const [reason, setReason] = useState("");
  const [details, setDetails] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!open) return;
    setStep("form");
    setReason("");
    setDetails("");
    setError(null);
    setSubmitting(false);
  }, [open, type, targetId]);

  const handleClose = () => {
    if (submitting) return;
    onClose();
  };

  useEffect(() => {
    if (!open) return;
    const handleKey = (e: KeyboardEvent) => {
      if (e.key === "Escape" && !submitting) onClose();
    };
    window.addEventListener("keydown", handleKey);
    return () => window.removeEventListener("keydown", handleKey);
  }, [open, submitting, onClose]);

  if (!open) return null;

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!reason.trim()) {
      setError(t("report.validationReason"));
      return;
    }
    if (!details.trim()) {
      setError(t("report.validationDetails"));
      return;
    }

    setSubmitting(true);
    setError(null);
    try {
      const body: CreateReportBody = {
        type,
        targetId,
        reason: reason.trim(),
        details: details.trim(),
      };
      await reportService.create(token, body);
      onSuccess?.();
      setStep("success");
    } catch (err: unknown) {
      const msg = isReportRequestError(err)
        ? t("report.submitError", { message: err.message })
        : t("report.networkError");
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  if (step === "success") {
    return (
      <div
        className="report-overlay report-overlay--success"
        ref={successOverlayRef}
        onClick={(e) => {
          if (e.target === successOverlayRef.current) handleClose();
        }}
        role="dialog"
        aria-modal="true"
        aria-labelledby="report-success-title"
      >
        <div className="report-modal report-modal--success">
          <div className="report-success-icon" aria-hidden="true">
            ✓
          </div>
          <h2 id="report-success-title" className="report-modal-title">
            {t("report.successTitle")}
          </h2>
          <p className="report-success-message">{t("report.submitSuccess")}</p>
          <button
            type="button"
            className="report-btn report-btn--success"
            onClick={handleClose}
          >
            {t("report.successOk")}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div
      className="report-overlay"
      ref={formOverlayRef}
      onClick={(e) => {
        if (e.target === formOverlayRef.current && !submitting) handleClose();
      }}
      role="dialog"
      aria-modal="true"
      aria-labelledby="report-form-title"
    >
      <div className="report-modal">
        <button
          type="button"
          className="report-modal-close"
          onClick={handleClose}
          disabled={submitting}
          aria-label={t("report.closeLabel")}
        >
          ×
        </button>

        <h2 id="report-form-title" className="report-modal-title">
          {title}
        </h2>
        <p className="report-modal-lead">{t("report.formLead")}</p>

        <form className="report-form" onSubmit={handleSubmit}>
          <label className="report-field">
            <span>{t("report.reasonLabel")}</span>
            <input
              type="text"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder={t("report.reasonPlaceholder")}
              disabled={submitting}
              maxLength={BACKEND_TYPE === "team2" ? 100 : 200}
            />
          </label>

          <label className="report-field">
            <span>{t("report.detailsLabel")}</span>
            <textarea
              value={details}
              onChange={(e) => setDetails(e.target.value)}
              placeholder={t("report.detailsPlaceholder")}
              disabled={submitting}
              rows={5}
              maxLength={BACKEND_TYPE === "team2" ? 1000 : 2000}
            />
          </label>

          {error && (
            <p className="report-error" role="alert">
              {error}
            </p>
          )}

          <div className="report-actions">
            <button
              type="button"
              className="report-btn report-btn--ghost"
              onClick={handleClose}
              disabled={submitting}
            >
              {t("report.cancel")}
            </button>
            <button
              type="submit"
              className="report-btn report-btn--primary"
              disabled={submitting}
            >
              {submitting ? t("report.submitting") : t("report.submit")}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
