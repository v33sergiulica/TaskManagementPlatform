# Task Management Platform

Acesta este un proiect individual dezvoltat pentru a demonstra competențe avansate de dezvoltare web folosind ecosistemul **.NET**. Aplicația este o platformă completă pentru gestionarea proiectelor și a task-urilor, facilitând colaborarea și organizarea eficientă.

## 🚀 Despre Proiect

Proiectul a fost realizat pentru a pune în practică concepte moderne de Software Engineering și Web Development. Scopul principal a fost crearea unei aplicații robuste, scalabile și sigure, care integrează funcționalități complexe precum autentificarea utilizatorilor, gestionarea bazelor de date relaționale și integrarea cu servicii de inteligență artificială.

## 💻 Tehnologii și Concepte Utilizate

În dezvoltarea acestui proiect am utilizat următorul stack tehnologic și concepte de programare:

### Backend & Arhitectură
*   **ASP.NET Core MVC**: Arhitectura Model-View-Controller pentru separarea clară a logicii de business de interfața cu utilizatorul.
*   **C#**: Limbajul principal de programare, utilizând facilități moderne (LINQ, Async/Await).
*   **Entity Framework Core**: ORM (Object-Relational Mapping) pentru interacțiunea cu baza de date (abordare **Code-First**).
*   **Dependency Injection (DI)**: Pentru gestionarea dependențelor și creșterea testabilității codului.
*   **Identity Framework**: Pentru un sistem complet de autentificare și autorizare (Roles: Member, Organizer, Admin).

### Baze de Date & Infrastructură
*   **Microsoft SQL Server**: Stocarea persistentă a datelor.
*   **Docker & Docker Compose**: Containerizarea aplicației și a bazei de date pentru a asigura un mediu de dezvoltare consistent și ușor de replicat.

### Integrări Externe
*   **OpenAI API**: Integrare pentru generarea automată a sumarizărilor sau sugestiilor pentru task-uri, aducând o componentă de AI proiectului.

### Frontend
*   **Razor Views**: Generarea dinamică a paginilor HTML.
*   **Bootstrap 5**: Pentru un design responsive și modern.
*   **JavaScript & jQuery**: Pentru validări client-side și interactivitate.

## ✨ Funcționalități Cheie

1.  **Managementul Proiectelor**: Creare, editare, ștergere proiecte; vizualizare progres.
2.  **Sistem de Task-uri**: Alocare task-uri, statusuri, deadline-uri, priorități.
3.  **Dashboard Interactiv**: Statistici în timp real despre proiecte și task-uri.
4.  **Securitate**: Autentificare sigură, permisiuni bazate pe roluri (Admin vs User vs Organizer).
5.  **Comentarii & Colaborare**: Posibilitatea de a adăuga comentarii la task-uri.
6.  **AI Assistant**: Funcționalități smart powered by OpenAI.

## 🛠️ Cum se rulează proiectul

### Pre-requisites
*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Rulare cu Docker (Recomandat)

1.  Clonează repository-ul:
    ```bash
    git clone [https://github.com/UTILIZATORUL_TAU/TaskManagementPlatform.git](https://github.com/UTILIZATORUL_TAU/TaskManagementPlatform.git)
    cd TaskManagementPlatform
    ```

2.  Configurează fișierul `appsettings.json` adăugând cheia ta OpenAI (dacă este necesar) sau folosind `appsettings.json.example` ca model.

3.  Pornește aplicația:
    ```bash
    docker-compose up --build
    ```

Aplicația va fi accesibilă la `http://localhost:8080`.

### Rulare Locală (fără Docker)

1.  Asigură-te că ai o instanță de SQL Server locală.
2.  Actualizează `ConnectionStrings` în `appsettings.json`.
3.  Aplică migrările:
    ```bash
    dotnet ef database update
    ```
4.  Rulează aplicația:
    ```bash
    dotnet run
    ```

---
*Acest proiect este o demonstrație a abilităților tehnice și a capacității de a livra un produs software complet, de la arhitectură la implementare.*
