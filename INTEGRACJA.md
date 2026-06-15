# Wyniki integracji frontendu io2_flatshare z backendem drugiego zespołu

## Co udało się zintegrować ✅

| Funkcja | Status |
|---|---|
| Rejestracja konta (Tenant / Landlord) | ✅ |
| Logowanie i wylogowywanie | ✅ |
| Odświeżanie sesji JWT | ✅ |
| Przeglądanie ogłoszeń (lista, wyszukiwanie, filtry) | ✅ |
| Szczegóły ogłoszenia | ✅ |
| Tworzenie ogłoszenia (przez landlorda) | ✅ |
| Edycja ogłoszenia | ✅ |
| Zmiana statusu ogłoszenia (submit, publish, hide, archive) | ✅ |
| Dodawanie okresu niedostępności ogłoszenia | ✅ |
| Tworzenie rezerwacji (tenant) | ✅ |
| Akceptowanie rezerwacji (landlord) | ✅ |
| Odrzucanie rezerwacji (landlord) | ✅ |
| Anulowanie rezerwacji (tenant) | ✅ |
| Przeglądanie rezerwacji | ✅ |
| Płatność Stripe (po konfiguracji klucza) | ✅ |
| Strony powrotu po płatności (`/payments/success`, `/payments/cancel`) | ✅ |
| Panel admina — lista ogłoszeń | ✅ |
| Panel admina — zatwierdzanie ogłoszeń (adapter) | ✅ |
| Panel admina — ukrywanie przez moderację (adapter) | ✅ |
| Panel admina — żądanie poprawek | ✅ |
| Panel admina — lista raportów | ✅ |
| Panel admina — zmiana statusu raportu | ✅ |
| Panel admina — banowanie użytkownika | ✅ |
| Zgłaszanie naruszeń | ✅ |
| Przelicznik walut PLN/USD | ✅ |

## Co nie działa z backendem drugiego zespołu ❌

| Funkcja | Powód | Możliwe rozwiązanie |
|---|---|---|
| **Reset hasła** | Backend drugiego zespołu nie implementuje tych endpointów | Wymaga implementacji po stronie drugiego zespołu |
| **Usuwanie okresu niedostępności** | Brak endpointu `DELETE /listings/{id}/unavailability` | Wymaga implementacji po stronie drugiego zespołu |
| **Zdjęcia ogłoszeń** | Wymaga uruchomienia Azurite przez Docker | `docker compose up -d azurite` |
| **Automatyczna aktualizacja statusu po płatności** | Brak skonfigurowanego webhooka Stripe | Konfiguracja Stripe CLI + `Stripe__WebhookSecret` |

---

## Napotkane problemy

Integracja wymagała sporej ilości pracy adaptacyjnej, ponieważ oba backendy — mimo tej samej domeny — znacząco różnią się w detalach implementacyjnych. Główne kategorie problemów:

**Niezgodności w konwencjach nazewnictwa** — backend drugiego zespołu konsekwentnie używa `SCREAMING_SNAKE_CASE` dla wartości enumów (np. `AWAITING_REVIEW`, `PENDING_APPROVAL`), podczas gdy nasz backend używa `PascalCase` (`UnderReview`, `PendingApproval`). Podobne różnice dotyczyły nazw pól: daty dostępności ogłoszenia to `availableFrom`/`availableSince` zamiast `availableSince`/`availableUntil`, cena rezerwacji to `totalCost` zamiast `totalPrice`, daty rezerwacji to `since`/`until` zamiast `startDate`/`endDate`. Błędy walidacji z rejestracji zwracane były z polami w lowercase (`"email"`) zamiast PascalCase (`"Email"`).

**Różne trasy i struktury odpowiedzi API** — backend drugiego zespołu nie ma CORS (obejście przez proxy Vite), wymaga autoryzacji nawet na publicznych endpointach GET, zwraca listę raportów jako czystą tablicę zamiast obiektu paginowanego, endpoint akceptacji rezerwacji zwraca puste ciało zamiast JSON, a po płatności Stripe przekierowuje na hardkodowane trasy (`/payments/success`, `/payments/cancel`), których nie było w routerze.

**Brakujące lub różne endpointy admina** — backend drugiego zespołu nie posiada endpointów `approve` ani `reinstate`. Jako adapter użyto `/publish` (ustawia ogłoszenie na `ACTIVE`). Endpoint `/hide` jest dostępny tylko dla roli `LANDLORD` (nie `ADMIN`) — zmapowano na `/archive`. Aktualizacja statusu raportu to jeden endpoint `PATCH .../status` z body zamiast osobnych `/open` i `/dismiss`.

---

## Uruchomienie z backendem drugiego zespołu

Wymagane jest odpowiednie ustawienie zmiennych w pliku `.env` frontendu:

```
VITE_API_URL=
VITE_BACKEND_TYPE=team2
VITE_PROXY_TARGET=http://localhost:5282
```

Pusty `VITE_API_URL` powoduje, że żądania trafiają jako ścieżki relatywne (`/api/v1/...`) i są przechwytywane przez proxy Vite, które przekazuje je do backendu drugiego zespołu — omijając problem braku CORS.
