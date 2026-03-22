import { useState } from "react";
import { CustomTextInput } from "../../components/CustomTextInput/CustomTextInput";
import "./Login.css";

export const Login = () => {
  const [name, setName] = useState("");
  const [nameError, setError] = useState(false);

  const [password, setPassword] = useState("");
  const [passwordError, _] = useState(false);

  const handleSubmit = () => {
    if (name.length < 3) {
      setError(true);
    } else {
      setError(false);
      console.log(name);
    }
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
              error={nameError}
              errorMessage="Minimum 3 characters"
              />

            <CustomTextInput
              label="Password"
              placeholder="Enter your password"
              value={password}
              onChange={setPassword}
              error={passwordError}
              errorMessage="Minimum 3 characters"
              />
          </div>

          <button onClick={handleSubmit}>Submit</button>
        </div>
      </div>
    </>
  );
};