import { CustomTextInputProps } from "./CustomTextInputProps";
import styles from "./CustomTextInput.module.css";

export const CustomTextInput = ({
  label,
  child,
  placeholder,
  value,
  onChange,
  error,
  errorMessage,
}: CustomTextInputProps) => {
  return (
    <div className={styles.wrapper}>
      <div className={styles.label}>
        {label && <label className={styles.label}>{label}</label>}
        {child}
      </div>

      <input
        className={`${styles.input} ${error ? styles.inputError : ""}`}
        placeholder={placeholder}
        value={value}
        onChange={(e) => onChange(e.target.value)}
      />

      {error && errorMessage && (
        <span className={styles.errorText}>{errorMessage}</span>
      )}
    </div>
  );
};