export type CustomTextInputProps = {
  label?: string;
  child?: React.ReactNode;
  placeholder?: string;
  value: string;
  onChange: (value: string) => void;
  error?: boolean;
  errorMessage?: string;
};