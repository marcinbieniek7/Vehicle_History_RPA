**ENGLISH VERSION BELOW**

# Vehicle History GOV – Dispatcher & Performer (UiPath)

## Informacja dla Rekruterów i Tech Leadów

To repozytorium zawiera projekt *proof-of-concept* w UiPath, którego celem jest zaprezentowanie architektury automatyzacji klasy Enterprise. Zamiast prostego, liniowego skryptu, projekt demonstruje w pełni skalowalny model Dispatcher-Performer oparty na dostosowanym szablonie REFramework.

Służy on jako praktyczny dowód mojego nacisku na dobre praktyki RPA: ścisłą modularyzację, logowanie operacyjne
Projekt oparty jest na konfiguracji, z wysoką odpornością na błędy.

## Scenariusz Biznesowy

Automatyzacja przeprowadza audyt danych pojazdów z wykorzystaniem publicznego serwisu rządowego (gov.pl). Dla każdego pojazdu robot wprowadza identyfikatory na portalu internetowym, pobiera wygenerowany raport PDF, wyodrębnia wartości istotne biznesowo (np. data następnego badania technicznego, ważność polisy OC, status dokumentu) i loguje wynik operacji.

## Architektura Rozwiązania

### 1. Dispatcher

* Odczytuje lokalne ustawienia środowiskowe z pliku Data\Config.json.
* Wczytuje dane wejściowe pojazdów z pliku CSV.
* Przesyła prawidłowe rekordy do kolejek Orchestratora za pomocą ustrukturyzowanych danych SpecificContent (Numer rejestracyjny, VIN, Data pierwszej rejestracji), używając numeru rejestracyjnego jako unikalnego Queue Item Reference.

### 2. Performer (REFramework)

* Sterowany przez kolejki Orchestratora oraz konfigurację wczytaną z pliku Config.xlsx.
* UI Automation: Steruje przeglądarką Chrome w celu interakcji z portalem gov.pl i bezpiecznego pobierania raportów PDF.
* Przetwarzanie Danych: Przekazuje pobrany plik PDF do dedykowanego modułu odpowiedzialnego za ekstrakcję.
* Zarządzanie Statusem: Aktualizuje status transakcji w kolejce (Success, Business Exception, System Exception) oraz wykonuje operacje czyszczące i zrzuty ekranu w przypadku błędu.

## Kluczowe Aspekty Techniczne i Zaprezentowane Umiejętności

* Zaawansowana Ekstrakcja z PDF (Regex): Wykorzystanie wyrażeń regularnych .NET z opcją RegexOptions.RightToLeft w celu niezawodnego parsowania i wyciągania najnowszych dat przeglądów ze skomplikowanych, wielostronicowych osi czasu w formacie PDF.
* Obsługa Błędów: Jasny podział (separation of concerns) między wyjątkami biznesowymi (Business Rule Exceptions, np. "Zatrzymany dowód rejestracyjny", "Brak danych w PDF") a systemowymi (System Exceptions, np. brak odpowiedzi portalu), co gwarantuje, że mechanizm Auto-Retry Orchestratora jest uruchamiany wyłącznie dla problemów infrastrukturalnych.
* Logowanie: Precyzyjne użycie poziomów logowania oraz aktywności Add Log Fields, zapewniające pełną identyfikowalność (traceability) każdej transakcji bez zaśmiecania bazy danych Orchestratora.

# Vehicle History GOV – Dispatcher & Performer (UiPath)

## Note to Recruiters & Tech Leads

This repository is a proof-of-concept UiPath project designed to showcase enterprise-grade automation architecture. Rather than a simple linear script, it demonstrates a fully scalable Dispatcher-Performer model built on a customized REFramework.

It serves as a practical demonstration of my focus on RPA engineering best practices: strict modularization, operational logging, configuration-driven design, and robust recoverability.

## Business Scenario

The automation audits vehicle-related data using the public Polish government service (gov.pl). For each vehicle, the robot inputs identifiers into the web portal, downloads the generated PDF report, extracts business-relevant values (e.g., next inspection date, insurance expiry, document status), and logs the outcome.

## Solution Architecture

### 1. Dispatcher

* Reads local environment settings from Data\Config.json.
* Loads input vehicle records from a CSV file.
* Pushes valid records to Orchestrator Queues with structured SpecificContent (Registration Number, VIN, First Registration Date), using the registration number as the unique Queue Item Reference.

### 2. Performer (REFramework)

* Driven by Orchestrator queues and configurations loaded from Config.xlsx.
* UI Automation: Controls Chrome to interact with the gov.pl portal and safely download PDF reports.
* Data Processing: Passes the downloaded PDF to a dedicated extraction workflow.
* Status Management: Updates Queue Item status (Success, Business Exception, System Exception) and performs cleanup/screenshot capture upon failure.

## Key Technical Highlights & Skills Showcased

* Advanced PDF Extraction (Regex): Utilized .NET Regular Expressions with RegexOptions.RightToLeft to reliably parse and extract the latest inspection dates from complex, multi-page PDF timelines.
* Exception Handling: Clear separation of concerns between Business Rule Exceptions (e.g., "Registration document withheld", "Missing data in PDF") and System Exceptions (e.g., portal timeouts), ensuring Orchestrator's Auto-Retry mechanism is only triggered for infrastructure issues.
* Operational Logging: Granular use of log levels and Add Log Fields to ensure complete traceability of every transaction without cluttering the Orchestrator database.
