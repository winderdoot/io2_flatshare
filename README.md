# Podstawowe informacje o naszym projekcie

**Stos technologiczny**:
* **Backend**: .NET 9 (Web API), Entity Framework Core
* **Baza danych**: PostgreSQL (w kontenerach Docker na start)
* **Cloud storage**: Azure Blob (np. do przechowania zdjęć pokoi)
* **Frontend**: React

**Organizacja pracy**:
- [YouTrack](https://io2flatshare.youtrack.cloud/projects/FLA)

# Plan Sprintów

## Sprint 1: Fundamenty, Infrastruktura i Tożsamość (Identity)
**Cel:** Skonfigurowanie środowiska, bazy danych oraz uwierzytelniania.

* Przygotowanie docker compose'a dla bazy PostgreSQL.
* Inicjalizacja projektów frontend i backend oraz konfiguracja połączenia z bazą danych.
* Wdrożenie ustandaryzowanych odpowiedzi błędów API (np. `timestamp`, `status`, `error`, `fieldErrors`).
* Implementacja modelu użytkownika (klasa `User` oraz kompozycja ról `TenantRole`, `LandlordRole`).
* Realizacja rejestracji i bezpiecznego logowania (generowanie i walidacja tokenów JWT).
* Dwuetapowy mechanizm resetu hasła (żądanie i potwierdzenie).

## Sprint 2: Core Użytkownika i Start Ogłoszeń (Listings CRUD)
**Cel:** Budowa profili użytkowników i umożliwienie właścicielom tworzenia ogłoszeń.

* Zapis i aktualizacja preferencji Lokatora powiązanych z rolą `TenantRole` (np. zwierzęta, palenie, budżet).
* Implementacja bazowego modelu domenowego dla ogłoszeń (`Listing` powiązany z `Apartment` i `Room`).
* Stworzenie endpointu do wystawiania ogłoszeń ze statusem początkowym `DRAFT` lub `UNDER_REVIEW`.
* Integracja zewnętrznego magazynu plików (Azure blob) dla zdjęć (zapisywanie samych URL-i w bazie).

## Sprint 3: Zaawansowane Ogłoszenia i Wyszukiwanie
**Cel:** Domknięcie cyklu życia ogłoszenia i rozpoczęcie wyszukiwarki/dopasowań.

* Pełna obsługa cyklu życia ogłoszenia: przejścia między stanami `ACTIVE`, `HIDDEN`, `ARCHIVED`.
* Wdrożenie encji `Unavailability` do zarządzania okresami niedostępności pokoju (np. na czas remontu).
* Podstawowy endpoint pobierający dopasowane da lokatora mieszkania z użyciem stronicowania (`page`, `size`).
* Implementacja algorytmu wyliczającego `matchScore` na podstawie preferencji.

## Sprint 4: Rezerwacja i płatność
**Cel:** Rozwinięcie wyszukiwania i rozpoczęcie interakcji wynajmu.
* Stworzenie obiektu `Booking` i endpointu inicjującego rezerwację (status `PENDING_APPROVAL`).
* Zarządzanie rezerwacją przez właściciela: endpointy do akceptacji (`/accept`) i odrzucania (`/reject`).
* Anulowanie rezerwacji przez lokatora (`/cancel`) i obsługa konfliktów terminów (błąd `409 Conflict`).
* Stworzenie obiektu `Payment` i integracja z zewnętrznym systemem płatności (stany `Initiated`, `Redirected`, `Succeeded`/`Failed`).
* Automatyczna zmiana statusu rezerwacji na `CONFIRMED` po pomyślnej płatności.
* Wdrożenie bramki płatności przy akceptacji rezerwacji przez właściciela
* Automatyczne anulowanie opłaconych rezerwacji i inicjacja zwrotów w przypadku blokady konta właściciela przez admina.

## Sprint 5: Moderacja i integracja z zewnętrznym API
**Cel:** Domknięcie obsługi rezerwacji oraz budowa modułu moderacji dla administratorów.
* Budowa obiektu `ViolationReport` dla zgłoszeń naruszeń (stany: `Open`, `UnderReview`, `ActionTaken`).
* Realizacja blokady konta przez administratora (zmiana statusu na `BLOCKED` i ukrycie ogłoszeń).
* Próba integracji z backendem przygotowanym przez naszych kolegów

## Sprint 6: Powiadomienia i Finalizacja
**Cel:** Integracja bramek płatniczych, powiadomień oraz ostateczne szlify.
* Wdrożenie serwisu powiadomień (interfejs `INotificationPort`) do wysyłania maili/pushy po ważnych zdarzeniach biznesowych.
* Testy końcowe i opcjonalne wdrożenie aplikacji.
