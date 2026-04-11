
import { useTranslation } from "react-i18next";
import { useState } from "react";
import { CustomTextInput } from "../../components/CustomTextInput/CustomTextInput";
import "./RestartPassword.css";
import { Link } from "react-router-dom";


export const RestartPassword = () => {      
  const { t } = useTranslation();
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmitCodeSent = async () => {
    alert(t("resetPassword.emailSent"));

    // TODO send email and code verification
    setLoading(true);
    setError(null);
    setLoading(false);
  };

  return (
    <div className="background">
      <img src="src/assets/rent_house.png" alt="background" />

      <div className="login-form-container">
        <div className="fields-container">
          <CustomTextInput
            label={t("resetPassword.emailLabel")}
            placeholder={t("resetPassword.emailPlaceholder")}
            value={email}
            onChange={setEmail}
          />


        </div>

        <div className="buttons-container">
          <button onClick={handleSubmitCodeSent} disabled={loading}>
            {loading ? t("resetPassword.submitLoading") : t("resetPassword.submit")}
          </button>

          {error && <p className="error-message">{error}</p>}

          <label>
            {t("resetPassword.hasAccount")}{" "}
            <Link to="/login">{t("resetPassword.loginLink")}</Link>
          </label>
        </div>
      </div>
    </div>
  );
};