import { useState } from "react";
import { useTranslation } from "react-i18next";
import { CustomTextInput } from "../../components/CustomTextInput/CustomTextInput";
import "./Registry.css";
import { Link } from "react-router-dom";
import { registryService } from "./RegistryService";
import SuccessDialog from "../../components/SuccessDialog/SuccessDialog";

export const Registry = () => {
  const { t } = useTranslation();
  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    confirmPassword: "",
    role: "TENANT",
  });
  const [errors, setErrors] = useState({
    firstNameError: "",
    lastNameError: "",
    emailError: "",
    passwordError: "",
    confirmPasswordError: "",
  });
  const [registered, setRegistered] = useState(false);  
  const [loading, setLoading] = useState(false);

  const setField = (field: string, value: string) => {
    setForm((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const clearError = (field: string) => {
  setErrors((prev) => ({
    ...prev,
    [`${field}Error`]: "",
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

    try {
      setLoading(true);

      const data = await registryService.register(
        form.firstName,
        form.lastName,
        form.email,
        form.password,
        form.role
      );

      if (data.user) {
        setRegistered(true);
      } else {
        const newErrors = {
          firstNameError: "",
          lastNameError: "",
          emailError: "",
          passwordError: "",
          confirmPasswordError: "",
        };

        if (data.fieldErrors && Array.isArray(data.fieldErrors)) {
          data.fieldErrors.forEach((err: any) => {
            if (err.field === "FirstName") newErrors.firstNameError = err.message;
            if (err.field === "LastName") newErrors.lastNameError = err.message;
            if (err.field === "Email") newErrors.emailError = err.message;
            if (err.field === "Password") newErrors.passwordError = err.message;
          });
        }

        setErrors(newErrors);
      }
    } catch (e) {
      console.error(e);

      setErrors((prev) => ({
        ...prev,
        emailError: "Server error",
      }));
    } finally {
      setLoading(false);
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
                      setField("firstName", v);
                      clearError("firstName");
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
                      setField("lastName", v);
                      clearError("lastName");              
                    }
                }
              />
            </div>

            <div className="connected-fields">
              <CustomTextInput
                label={t("registry.emailLabel")}
                placeholder={t("registry.emailPlaceholder")}
                value={form.email}
                error={!!errors.emailError}
                errorMessage={errors.emailError}
                onChange={
                  (v) => { 
                      setField("email", v);
                      clearError("email"); 
                    }
                  }
              />

              <div className="role-toggle">
                <button
                  type="button"
                  className={form.role === "TENANT" ? "active" : ""}
                  onClick={() => setField("role", "TENANT")}
                >
                  Tenant
                </button>

                <button
                  type="button"
                  className={form.role === "LANDLORD" ? "active" : ""}
                  onClick={() => setField("role", "LANDLORD")}
                >
                  Landlord
                </button>
              </div>
            </div>

            <div className="connected-fields">
              <CustomTextInput
                label={t("registry.passwordLabel")}
                placeholder={t("registry.passwordPlaceholder")}
                value={form.password}
                error={!!errors.passwordError}
                errorMessage={errors.passwordError}
                onChange={
                  (v) => { 
                      setField("password", v);
                      clearError("password");
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
                      setField("confirmPassword", v);
                      clearError("confirmPassword");          
                    }
                }
              />
            </div>
          </div>

          <div className="buttons-container">
            <button type="button" onClick={handleSubmit} disabled={loading} data-testid="register-button">
              {t("registry.submit")}
            </button>
            <label>
              {t("registry.hasAccount")}{" "}
              <Link to="/login">{t("registry.loginLink")}</Link>
            </label>
          </div>
        </div>}

        {registered &&
          <SuccessDialog title={t("registry.registered")} message={
            <label>
              {t("registry.registeredInfo")}{" "}
              <Link to="/login">{t("registry.loginLink")}</Link>
            </label>} />
          }
      </div>
    </>
  );
};