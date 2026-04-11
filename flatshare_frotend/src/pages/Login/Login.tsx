
import { useTranslation } from "react-i18next";
import { JSX, useState } from "react";
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
      const token = await authService.login(email, password);

      login(token);

      const from = location.state?.from?.pathname;
      console.log(from);
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
            label="Email"
            placeholder="Enter your email"
            value={email}
            onChange={setEmail}
          />

          <CustomTextInput
            label="Password"
            placeholder="Enter your password"
            value={password}
            onChange={setPassword}
          />
        </div>

        <div className="buttons-container">
          <button onClick={handleSubmit} disabled={loading}>
            {loading ? "Logging in..." : "Login"}
          </button>

          {error && <p className="error-message">{error}</p>}

          <label>
            Don't have an account?{" "}
            <Link to="/create-account">Create account</Link>
          </label>
        </div>
      </div>
    </div>
  );
};