import { useState } from "react";
import { useTranslation } from "react-i18next";
import { CustomTextInput } from "../../components/CustomTextInput/CustomTextInput";
import "./Registry.css";
import { Link } from "react-router-dom";
import { registryService } from "./RegistryService";

export const Registry = () => {
  const { t } = useTranslation();
  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    confirmPassword: "",
    role: "LANDLORD",
  });
  const [errors, setErrors] = useState({
    firstNameError: "",
    lastNameError: "",
    emailError: "",
    passwordError: "",
    confirmPasswordError: "",
  });
  const [registered, setRegistered] = useState(false);

  const setField = (field: string, value: string) => {
    setForm((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const handleSubmit = async () => {
    if (form.password !== form.confirmPassword) {
      setErrors((prev) => ({
        ...prev,
        confirmPasswordError: "Passwords do not match",
      }));
      return;
    }

    const data = await registryService.register(form.firstName, form.lastName, form.email, form.password, form.role);

    console.log(data);

    if (data.user) {
      setRegistered(true);
    }
    else {
      for (let i = 0; i < data.fieldErrors.length; i++) {
        if (data.fieldErrors[i].field === "FirstName") {
          setErrors((prev) => ({
            ...prev,
            firstNameError: data.fieldErrors[i].message,
          }));
        }
        if (data.fieldErrors[i].field === "LastName") {
          setErrors((prev) => ({
            ...prev,
            lastNameError: data.fieldErrors[i].message,
          }));
        }
        if (data.fieldErrors[i].field === "Email") {
          setErrors((prev) => ({
            ...prev,
            emailError: data.fieldErrors[i].message,
          }));
        }
        if (data.fieldErrors[i].field === "Password") {
          setErrors((prev) => ({
            ...prev,
            passwordError: data.fieldErrors[i].message,
          }));
        }
      }
    }
  };

  return (
    <>
      <div className="background">
        <img src="src/assets/rent_house.png" alt="" />
        {!registered
          &&
        <div className="registry-form-container">          
          <div className="fields-container">
            <div className="connected-fields">
              <CustomTextInput
                label={t("registry.nameLabel")}
                placeholder={t("registry.namePlaceholder")}
                value={form.firstName}
                error={!!errors.firstNameError}
                errorMessage={errors.firstNameError}
                onChange={
                  (v) => { 
                      setField("firstName", v)
                      setErrors((prev) => ({
                        ...prev,
                        firstNameError: "",
                      }))                
                    }
                }
              />

              <CustomTextInput
                label={t("registry.lastNameLabel")}
                placeholder={t("registry.lastNamePlaceholder")}
                value={form.lastName}
                error={!!errors.lastNameError}
                errorMessage={errors.lastNameError}
                onChange={
                  (v) => { 
                      setField("lastName", v)
                      setErrors((prev) => ({
                        ...prev,
                        lastNameError: "",
                      }))                
                    }
                }
              />
            </div>

            <CustomTextInput
              label={t("registry.emailLabel")}
              placeholder={t("registry.emailPlaceholder")}
              value={form.email}
              error={!!errors.emailError}
              errorMessage={errors.emailError}
              onChange={
                (v) => { 
                    setField("email", v)
                    setErrors((prev) => ({
                      ...prev,
                      emailError: "",
                    }))                
                  }
                }
            />

            <div className="connected-fields">
              <CustomTextInput
                label={t("registry.passwordLabel")}
                placeholder={t("registry.passwordPlaceholder")}
                value={form.password}
                error={!!errors.passwordError}
                errorMessage={errors.passwordError}
                onChange={
                  (v) => { 
                      setField("password", v)
                      setErrors((prev) => ({
                        ...prev,
                        passwordError: "",
                      }))                
                    }
                }
              />

              <CustomTextInput
                label={t("registry.confirmPasswordLabel")}
                placeholder={t("registry.confirmPasswordPlaceholder")}
                value={form.confirmPassword}
                error={!!errors.confirmPasswordError}
                errorMessage={errors.confirmPasswordError}
                onChange={
                  (v) => { 
                      setField("confirmPassword", v)
                      setErrors((prev) => ({
                        ...prev,
                        confirmPasswordError: "",
                      }))                
                    }
                }
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
        </div>}

        {registered &&
          <div className="registry-form-container" style={{width: "fit-content"}}>
            <h2 style={{fontSize: "1.7rem"}}>{t("registry.registered")}</h2>

            <span className="ok-icon">&#10004;</span>

            <label>
              {t("registry.registeredInfo")}{" "}
              <Link to="/login">{t("registry.loginLink")}</Link>
            </label>
          </div>}
      </div>
    </>
  );
};