import { useState } from "react";
import { CustomTextInput } from "../../components/CustomTextInput/CustomTextInput";
import "./Registry.css";

export const Registry = () => {
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
            <div className="registry-form-container">
            <div className="fields-container">
                <div className="connected-fields">
                    <CustomTextInput
                    label="Name"
                    placeholder="Enter your name"
                    value={name}
                    onChange={setName}
                    />

                    <CustomTextInput
                    label="Last Name"
                    placeholder="Enter your last name"
                    value={lastName}
                    onChange={setLastName}
                    />
                </div>                

                <CustomTextInput
                label="Email"
                placeholder="Enter your email"
                value={email}
                onChange={setEmail}
                />

                <div className="connected-fields">                
                    <CustomTextInput
                    label="Password"
                    placeholder="Enter your password"
                    value={password}
                    onChange={setPassword}
                    />

                    <CustomTextInput
                    label="Confirm Password"
                    placeholder="Confirm your password"
                    value={confirmPassword}
                    onChange={setConfirmPassword}
                    />
                </div>
            </div>

            <button onClick={handleSubmit}>Create account</button>
            </div>
        </div>
        </>
    );
};