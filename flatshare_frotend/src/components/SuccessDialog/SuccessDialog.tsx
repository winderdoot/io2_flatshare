import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";
import { SuccessDialogProps } from "./SuccessDialogProps";
import "./SuccessDialog.css";

const SuccessDialog = ({
        title,
        message
    }: SuccessDialogProps
) => {
    const { t } = useTranslation();

    return (
        <div className="form-container" style={{width: "fit-content"}}>
            <h2 style={{fontSize: "1.7rem"}}>{title}</h2>

            <span className="ok-icon">&#10004;</span>

            {message}
        </div>
    )
}

export default SuccessDialog
