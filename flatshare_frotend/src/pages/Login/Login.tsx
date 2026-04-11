
import { useTranslation } from "react-i18next";
import { useState } from "react";
import { CustomTextInput } from "../../components/CustomTextInput/CustomTextInput";
import "./Login.css";
import { Link, useNavigate, useLocation, Navigate } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { authService } from "../../auth/AuthService";


export const Login = () => {      
  const { t } = useTranslation();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { login, user } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  if (user) {
    const from = location.state?.from?.pathname || "/";
    return <Navigate to={from} replace />;
  }

  const handleSubmit = async () => {
    if (!email || !password) {
      setError("Fill all fields");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const {token, loggedInUser} = await authService.login(email, password);

      if (!token || !loggedInUser) {
        setError("Invalid email or password");
        return;
      }

      login(token, loggedInUser);

      const from = location.state?.from?.pathname;

      navigate(from || "/", { replace: true });

    } catch (err) {
      setError("Invalid email or password");
    } finally {
      setLoading(false);
    }
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

          <CustomTextInput
            label={t("login.passwordLabel")}
            placeholder={t("login.passwordPlaceholder")}
            value={password}
            onChange={setPassword}
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