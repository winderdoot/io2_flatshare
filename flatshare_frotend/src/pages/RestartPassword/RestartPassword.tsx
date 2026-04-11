
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

  const handleSubmit = async () => {
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
            label={t("login.emailLabel")}
            placeholder={t("login.emailPlaceholder")}
            value={email}
            onChange={setEmail}
          />


        </div>

        <div className="buttons-container">
          <button onClick={handleSubmit} disabled={loading}>
            {loading ? t("login.submitLoading") : t("login.submit")}
          </button>

          {error && <p className="error-message">{error}</p>}

          <label>
            {t("login.noAccount")}{" "}
            <Link to="/create-account">{t("login.createLink")}</Link>
          </label>
        </div>
      </div>
    </div>
  );
};