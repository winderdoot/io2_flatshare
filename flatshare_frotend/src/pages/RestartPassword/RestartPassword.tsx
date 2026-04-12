
import { useTranslation } from "react-i18next";
import { useState } from "react";
import { CustomTextInput } from "../../components/CustomTextInput/CustomTextInput";
import "./RestartPassword.css";
import { Link } from "react-router-dom";
import { API_URL } from "../../config";
import SuccessDialog from "../../components/SuccessDialog/SuccessDialog";

export const RestartPassword = () => {      
  const { t } = useTranslation();
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [sent, setSent] = useState(false);

  const [resetToken, setResetToken] = useState<string>("");
  const [newPassword, setNewPassword] = useState<string>("");
  const [confirmPassword, setConfirmPassword] = useState<string>("");

  const [registered, setRegistered] = useState(false);

  const handleSubmitCodeSent = async () => {   
    setSent(false);
    setError(null);  

    try {
      setLoading(true);

      const  res = await fetch(`${API_URL}/api/v1/auth/password-reset/request`,{method: "POST", body: JSON.stringify({email}), headers: {"Content-Type": "application/json", "Accept": "application/json",}});   
    
      if (!res.ok) {
        setError("Server error");
        return;
      }

      setSent(true);
      alert(t("resetPassword.emailSent"));
    } catch (e) {
      setError("Network error");
    } finally {
      setLoading(false);
    }
  };

  const handleSubmitNewPassword = async () => {
    setError(null);
    
    if (newPassword !== confirmPassword) {
      setError("Passwords do not match");
      return;
    }
    
    if (newPassword.length < 8) {
      setError("Password must be at least 8 characters long");
      return;
    }
    
    setLoading(true);
    try {      
      const res = await fetch(`${API_URL}/api/v1/auth/password-reset/confirm`,{method: "POST", body: JSON.stringify({resetToken, email, newPassword}), headers: {"Content-Type": "application/json", "Accept": "application/json",}});
    
      if (!res.ok) {
        setError("Invalid token, must be requested again");
        return;
      }
      
      setRegistered(true);
    }
    catch (e) {
      setError("Network error");
    }
    finally {
      setLoading(false);
    }

  };

  return (
    <div className="background">
      <img src="src/assets/rent_house.png" alt="background" />

      {!registered && <div className="login-form-container">
        <div className="fields-container">
          <CustomTextInput
            label={t("resetPassword.emailLabel")}
            placeholder={t("resetPassword.emailPlaceholder")}
            value={email}
            onChange={setEmail}
          />

          {sent && <div className="password-reset-container">
              <CustomTextInput
                label={t("resetPassword.token")}
                placeholder={t("resetPassword.tokenPlaceholder")}
                value={resetToken}
                onChange={setResetToken}
              />
              <CustomTextInput
                label={t("resetPassword.passwordLabel")}
                placeholder={t("resetPassword.passwordPlaceholder")}
                value={newPassword}
                onChange={setNewPassword}
              />
              <CustomTextInput
                label={t("resetPassword.confirmPasswordLabel")}
                placeholder={t("resetPassword.confirmPasswordPlaceholder")}
                value={confirmPassword}
                onChange={setConfirmPassword}
              />
            </div>
          }
        </div>

        {!sent && <div className="buttons-container">
          
          <button onClick={handleSubmitCodeSent} disabled={loading}>
            {loading ? t("resetPassword.submitLoading") : t("resetPassword.submit")}
          </button>         

          {error && <p className="error-message">{error}</p>}

          <label>
            {t("resetPassword.hasAccount")}{" "}
            <Link to="/login">{t("resetPassword.loginLink")}</Link>
          </label>
        </div>}    

        {sent && <div className="buttons-container">
          {sent &&  <button onClick={handleSubmitNewPassword} disabled={loading}>
              {loading ? t("resetPassword.submitPasswordLoading") : t("resetPassword.submitPassword")}
            </button>}

          {error && <p className="error-message">{error}</p>} 

          <label>
            {t("resetPassword.problem")}{". "}
          <span onClick={handleSubmitCodeSent} className="link">
            {t("resetPassword.resent")}
          </span>
          </label>
        </div>}

      </div>}

      {registered &&
          <SuccessDialog title={t("resetPassword.passwordChanged")} message={
            <label>
              {t("resetPassword.passwordChangedInfo")}{" "}
              <Link to="/login">{t("resetPassword.loginLink")}</Link>
            </label>} />
          }
    </div>
  );
};