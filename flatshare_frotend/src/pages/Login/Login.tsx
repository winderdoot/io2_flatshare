import { useState } from "react";
import { CustomTextInput } from "../../components/CustomTextInput/CustomTextInput";
import "./Login.css";

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

          <button onClick={handleSubmit}>Submit</button>
        </div>
      </div>
    </>
  );
};