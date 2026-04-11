import "./i18n";
import { CurrencyProvider } from "./context/CurrencyContext";
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.jsx'
import { AuthProvider } from './auth/AuthContext.js'

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <CurrencyProvider>
      <AuthProvider>
        <App />
      </AuthProvider>
    </CurrencyProvider>
  </StrictMode>
);
