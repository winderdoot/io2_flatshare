import { useState } from "react";
import { CustomTextInput } from "../../components/CustomTextInput/CustomTextInput";
import "./Login.css";
import { Link } from "react-router-dom";

export const Login = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const handleSubmit = () => {
    // TODO call API
    console.log(email, password);    
  };

  return (
    <>
      <div className="background">
        <img src="src/assets/rent_house.png"/>
        <div className="login-form-container">
          <div className="fields-container">
            <CustomTextInput
              label="Name"
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
            <button onClick={handleSubmit}>Submit</button>
            <label>Don't have an account? <Link to="/create-account">Create account</Link></label>
          </div>
        </div>
      </div>
    </>
  );
};