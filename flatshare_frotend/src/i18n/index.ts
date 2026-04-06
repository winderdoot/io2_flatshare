import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import en from "./locales/en.json";
import pl from "./locales/pl.json";

const LANG_KEY = "flatshare-lang";

const saved =
  typeof window !== "undefined"
    ? (localStorage.getItem(LANG_KEY) as "pl" | "en" | null)
    : null;
const initialLng = saved === "en" || saved === "pl" ? saved : "pl";

void i18n.use(initReactI18next).init({
  resources: {
    en: { translation: en },
    pl: { translation: pl },
  },
  lng: initialLng,
  fallbackLng: "pl",
  interpolation: { escapeValue: false },
});

i18n.on("languageChanged", (lng) => {
  if (lng === "pl" || lng === "en") {
    localStorage.setItem(LANG_KEY, lng);
    if (typeof document !== "undefined") {
      document.documentElement.lang = lng;
    }
  }
});

if (typeof document !== "undefined") {
  document.documentElement.lang = initialLng;
}

export default i18n;
