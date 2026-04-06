import { useState } from "react";
import { useTranslation } from "react-i18next";
import { CustomTextInput } from "../../components/CustomTextInput/CustomTextInput";
import "./Login.css";
import { Link } from "react-router-dom";

export const Login = () => {
  const { t } = useTranslation();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const handleSubmit = () => {
    // TODO call API
    console.log(email, password);
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