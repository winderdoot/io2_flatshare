import { useState } from "react";
import { CustomTextInput } from "../../components/CustomTextInput/CustomTextInput";
import "./Login.css";

export const Login = () => {
  const [name, setName] = useState("");

  const [password, setPassword] = useState("");

  const handleSubmit = () => {
    console.log(name, password);
  };

  return (
    <>
      <div className="background">
        <div className="login-form-container">
          <div className="fields-container">
            <CustomTextInput
              label="Name"
              placeholder="Enter your name"
              value={name}
              onChange={setName}
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