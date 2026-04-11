
import { useTranslation } from "react-i18next";
import { JSX, useState } from "react";
import { CustomTextInput } from "../../components/CustomTextInput/CustomTextInput";
import "./Login.css";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";


  export const Login = ({ children }: { children: JSX.Element | null }) => {
  const { t } = useTranslation();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = () => {
    // fake API
    const fakeToken = "abc123";

    login(fakeToken);
    if (children) return children;
    navigate("/");   
  };

  return (
    <>
      <div className="background">
        <img src="src/assets/rent_house.png" alt="" />
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
            <button type="button" onClick={handleSubmit}>
              {t("login.submit")}
            </button>
            <label>
              {t("login.noAccount")}{" "}
              <Link to="/create-account">{t("login.createLink")}</Link>
            </label>
          </div>
        </div>
      </div>
    </>
  );
};