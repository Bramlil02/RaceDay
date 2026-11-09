# RaceDay

# Explanation

RaceDay is an event management system for running, walking and cycling events. The system allows event organisers to create and manage events, while participants can view events and register for them. So this assignment makes use of database to store information about organisers, participants, categories, routes, events and registrations.

Added Entity Relationship Diagram

## Main Entities

The database contains six main entities:

1. EventOrganiser
   Stores information about people or organisations that manage events.
2. Participant
   Stores information about people who participate in events.
3. Category
   Stores the different types of events, such as running, walking and cycling.
4. Route
   Stores information about the route and distance of an event.
5. Event
   Stores information about each RaceDay event.
6. Registration
   Connects participants to events and stores their registration and result information.

Part 3 adds one more table, **EventCategory**, which stores the age and distance categories of each event (see Part 3 below).

## Main Features

The RaceDay system allows users to:

- Register and manage participant information.
- Create and manage events.
- Create event categories.
- Create and manage race routes.
- Register participants for events.
- View event registrations.
- Record and view race results.

Added SQL file

## Database

The database was created using Microsoft SQL Server. Primary keys and foreign keys are used to connect the tables. Constraints such as NOT NULL, UNIQUE, DEFAULT and CHECK are also used to help keep the data accurate.

Added API Endpoint Plan

## API

The API will allow the application to communicate with the database. It will provide endpoints for authentication, profiles, events, categories, routes, registrations and results.

# YouTube Video

Part 1: https://youtu.be/nfmrflvSIIs

Part 3: _add your Part 3 video link here_

---

# Part 3 – MVC Web Application, Azure Blob Storage and Docker

## Overview

Part 3 adds the web front end for RaceDay. An ASP.NET Core MVC application consumes the Part 2 REST API for **all** data operations. The MVC project contains no database access, no Azure access and no replicated business logic. Azure Blob Storage was added to the API so that event banners and profile pictures are stored in the cloud, and the MVC application is containerised with Docker.

## Solution Structure

| Folder / file | Purpose |
|---|---|
| `Part 2/` | ASP.NET Core Web API (SQL Server, EF Core, JWT, Swagger, Azure Blob Storage) |
| `RaceDayMVC/` | ASP.NET Core MVC front end |
| `Database/Part3_Schema_Changes.sql` | Script that adds the Part 3 columns and the EventCategory table |
| `docker-compose.yml` | Starts the MVC application in Docker with one command |
| `RaceDayMVC/Dockerfile` | Dockerfile for the MVC application |
| `.github/workflows/ci.yml` | GitHub Actions workflow that builds the API, the MVC project and the Docker image |

## Features

### Authentication and role-based navigation
- Users register and log in through the MVC application, which calls the Part 2 API.
- The JWT token, name, email and role are kept in session, so the user stays signed in across pages.
- Participants and Organisers each get their own navigation menu and pages.
- Unauthenticated users are redirected to the login page, and users cannot open pages that belong to the other role.
- Users can log out, which ends the session and returns them to the home page.
- If the API token expires, the user is sent back to the login page with a message.

### Organiser interface
- **Dashboard:** total events created, total enrolments per event and upcoming event dates.
- **Events management:** create, edit and delete events with name, description, date, location, distance, event type and entry fee.
- **Banner upload:** an event banner image can be uploaded when creating or editing an event.
- **Category management:** add or remove age and distance categories for each event.
- **Enrolments:** see every participant enrolled in an event with their selected category and enrolment status, and confirm or cancel enrolments.
- **Results entry:** capture and publish finish times and finishing positions after the event has taken place. Positions are ranked automatically from the finish times when left empty, and the average pace is calculated.

### Participant interface
- **Home:** all upcoming events with filters (search, event type, distance and date range).
- **Event details:** full event information, banner image and the available categories.
- **Enrol:** a logged-in participant enrols in an event and selects a category. The enrolment is recorded immediately.
- **My Enrolments:** a personal view of all enrolled events and their status.
- **My Results:** race history for completed events with event name, date, category, finish time and position (for example "47th out of 312 finishers").
- **Profile:** view and update personal information and upload a profile picture.

### Azure Blob Storage
- The API stores uploaded images in Azure Blob Storage. The MVC application never talks to Azure directly; all file handling goes through the API.
- Uploads are limited to JPG, PNG, WEBP and GIF images of 5 MB or less, and the file contents are checked, not just the file extension.
- Event banners are shown on the event cards and the event detail page, and profile pictures are shown on the profile page and in the navigation bar.

### UI design
- A sporting RaceDay theme (navy and orange) with consistent colours and typography across all pages.
- Bootstrap 5 for layout and responsiveness.
- Enrolment statuses are colour coded: green for Confirmed, amber for Pending and red for Cancelled.
- Invalid input and API problems are shown as friendly messages. Raw errors are never displayed to the user.

## API Changes for Part 3

| Change | Endpoint / detail |
|---|---|
| Image uploads | `POST api/Uploads/event-banner/{eventId}` and `POST api/Uploads/profile-picture` |
| Event categories | `GET`, `POST` and `DELETE api/EventCategories` |
| Organiser's own events | `GET api/Events/mine` |
| Event enrolment list | `GET api/Enrolments/event/{eventId}` |
| Enrolment status | `PUT api/Enrolments/{id}/status` |
| Enrolment with category | `POST api/Enrolments` now accepts an `EventCategoryID` and creates the participant record automatically on first enrolment |
| Results | `PUT api/Results/{registrationId}` now checks that the organiser owns the event |
| Event ownership | The organiser is taken from the login token, and only the owner (or an Admin) can edit or delete an event, manage its categories or record its results |
| Organiser registration | An optional organiser access code on registration creates an Organiser account |

## Database Changes for Part 3

Run `Database/Part3_Schema_Changes.sql` once against `RaceDayDB`. It is safe to run more than once. It adds:

- `Event.Description` and `Event.BannerUrl`
- `UserAccounts.ProfilePictureUrl`
- the `EventCategory` table (age and distance categories of an event)
- `Registration.EventCategoryID` (the category the participant chose)

## How to Run

### 1. Configure Azure Blob Storage (API only)

Set the connection string in `Part 2/appsettings.json` (section `AzureBlob`), ideally with user secrets:

```
cd "Part 2"
dotnet user-secrets init
dotnet user-secrets set "AzureBlob:ConnectionString" "<your storage account connection string>"
```

The default value `UseDevelopmentStorage=true` works with the Azurite emulator. The `raceday-images` container is created automatically with public read access, so the storage account must allow public blob access for the images to display.

### 2. Start the API

The API must listen on all interfaces so the Docker container can reach it:

```
dotnet run --project "Part 2" --launch-profile https --urls "https://0.0.0.0:7088"
```

### 3. Start the MVC application in Docker (single command)

From the root folder of the repository:

```
docker compose up --build
```

Then open http://localhost:8080

The same thing without Docker Compose:

```
docker build -t raceday-mvc ./RaceDayMVC && docker run --rm -p 8080:8080 -e ApiSettings__BaseUrl=https://host.docker.internal:7088/ -e ApiSettings__AllowInvalidCertificate=true --add-host=host.docker.internal:host-gateway raceday-mvc
```

`AllowInvalidCertificate=true` is for local development only, because the API uses a developer certificate.

### Running the MVC application locally (without Docker)

```
dotnet run --project RaceDayMVC --launch-profile https
```

The MVC application uses `ApiSettings:BaseUrl` (default `https://localhost:7088/`) to find the API. The containerised application behaves the same as the one running locally.

## Accounts

- **Participant:** register on the Register page.
- **Organiser:** enter the organiser access code on the Register page. The code is the `Registration:OrganiserCode` setting in the API's `appsettings.json` (default `RACEDAY-ORG`; change it before submitting). An existing account can also be promoted with the SQL at the bottom of the schema script.

## Docker

The `Dockerfile` uses a multi-stage build: the .NET 8 SDK image restores and publishes the application, and the smaller ASP.NET runtime image runs it as a non-root user on port 8080. Packages are restored in a separate layer to make rebuilds faster, and a `.dockerignore` keeps build output out of the image.

## GitHub and CI/CD

The `Part 3` work is pushed to the repository in a clearly named branch. The GitHub Actions workflow (`.github/workflows/ci.yml`) now:

1. builds and tests the Part 2 API,
2. builds the MVC project, and
3. builds the MVC Docker image.

## Technologies Used

- ASP.NET Core 8 (Web API and MVC), C#
- Entity Framework Core and Microsoft SQL Server
- JWT authentication and session-based authentication in the MVC application
- Azure Blob Storage
- Bootstrap 5
- Docker and Docker Compose
- GitHub Actions
