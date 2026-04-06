import { useState } from "react";
import { useTranslation } from "react-i18next";
import { CustomTextInput } from "../../components/CustomTextInput/CustomTextInput";
import "./Registry.css";
import { Link } from "react-router-dom";

export const Registry = () => {
  const { t } = useTranslation();
  const [name, setName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  const handleSubmit = () => {
    console.log(name, password);
  };

  return (
    <>
      <div className="background">
        <img src="src/assets/rent_house.png" alt="" />
        <div className="registry-form-container">
          <div className="fields-container">
            <div className="connected-fields">
              <CustomTextInput
                label={t("registry.nameLabel")}
                placeholder={t("registry.namePlaceholder")}
                value={name}
                onChange={setName}
              />

              <CustomTextInput
                label={t("registry.lastNameLabel")}
                placeholder={t("registry.lastNamePlaceholder")}
                value={lastName}
                onChange={setLastName}
              />
            </div>

            <CustomTextInput
              label={t("registry.emailLabel")}
              placeholder={t("registry.emailPlaceholder")}
              value={email}
              onChange={setEmail}
            />

            <div className="connected-fields">
              <CustomTextInput
                label={t("registry.passwordLabel")}
                placeholder={t("registry.passwordPlaceholder")}
                value={password}
                onChange={setPassword}
              />

              <CustomTextInput
                label={t("registry.confirmPasswordLabel")}
                placeholder={t("registry.confirmPasswordPlaceholder")}
                value={confirmPassword}
                onChange={setConfirmPassword}
              />
            </div>
          </div>

          <div className="buttons-container">
            <button type="button" onClick={handleSubmit}>
              {t("registry.submit")}
            </button>
            <label>
              {t("registry.hasAccount")}{" "}
              <Link to="/login">{t("registry.loginLink")}</Link>
            </label>
          </div>
        </div>
      </div>
    </>
  );
};