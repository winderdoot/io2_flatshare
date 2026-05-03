import { useEffect, useState } from "react";
import "./FormattedNumberInput.css";

interface Props {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  suffix?: string;
}

const formatNumber = (val: string) => {
  if (!val) return "";
  return val.replace(/\B(?=(\d{3})+(?!\d))/g, " ");
};

const cleanNumber = (val: string) => val.replace(/\s/g, "");

export default function FormattedNumberInput({
  value,
  onChange,
  placeholder,
  suffix
}: Props) {
  const [displayValue, setDisplayValue] = useState("");

  useEffect(() => {
    setDisplayValue(formatNumber(value));
  }, [value]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const raw = cleanNumber(e.target.value);

    if (!/^\d*$/.test(raw)) return;

    onChange(raw);
  };

  return (
    <div className="formatted-input-wrapper">
      <input
        type="text"
        value={displayValue}
        onChange={handleChange}
        placeholder={placeholder}
        className="formatted-input"
      />
      {suffix && (
        <span className="formatted-input-suffix">{suffix}</span>
      )}
    </div>
  );
}