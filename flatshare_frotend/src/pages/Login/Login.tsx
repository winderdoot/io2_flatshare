
import { useTranslation } from "react-i18next";
import { useState } from "react";
import { CustomTextInput } from "../../components/CustomTextInput/CustomTextInput";
import "./Login.css";
import { Link, useNavigate, useLocation, Navigate } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { authService } from "../../auth/AuthService";
import { getPostLoginPath } from "../../auth/postLoginRedirect";


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
    const from = location.state?.from?.pathname;
    return (
      <Navigate
        to={getPostLoginPath(from, user.role)}
        replace
        state={{}}
      />
    );
  }

  const handleSubmit = async () => {
    if (!email || !password) {
      setError("Fill all fields");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const { token, sessionId, expiresIn, loggedInUser } =
        await authService.login(email, password);

      if (!token || !loggedInUser || !sessionId || !expiresIn) {
        setError("Invalid email or password");
        return;
      }

      login(token, loggedInUser, { sessionId, expiresIn });

      const from = location.state?.from?.pathname;
      navigate(getPostLoginPath(from, loggedInUser.role), {
        replace: true,
        state: {},
      });

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
            child={<Link to="/forgot-password"><label style={{fontSize: ".8rem", cursor: "pointer"}}>{t("login.forgotPassword")}</label></Link>}
            placeholder={t("login.passwordPlaceholder")}
            value={password}
            onChange={setPassword}
          />
        </div>

        <div className="buttons-container">
          <button onClick={handleSubmit} disabled={loading} data-testid="login-button">
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