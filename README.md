# Part 2
. Project Description

The RaceDay Event Management System is a web API developed for the PROG6212 Programming assignment. The system is designed to manage race events, participants, event categories, race routes, enrolments and race results.

Part 2 focuses on developing the API that communicates with the SQL Server database and provides endpoints for managing the information in the system.

2. Technologies Used

C#

ASP.NET Core Web API

Microsoft SQL Server

SQL Server Management Studio (SSMS)

Entity Framework Core

Visual Studio

Main Features

The API is designed to provide the following features:

User Authentication: Register an account and log in.

User Profiles: View and update profile information.

Event Management: View, create, update and delete race events.

Category Management: Manage race categories.

Route Management: Manage race routes and their details.

Enrolment Management: Enrol participants in events, view enrolments and cancel enrolments.

Results Management: View, record and update race results.

Role-Based Access: Restrict certain operations to authorised users.

The project uses Microsoft SQL Server with a database named RaceDayDB.

The main database tables are:

EventOrganiser

Participant

Category

Route

Event

Registration

A UserAccount table is also used if authentication is implemented using the additional account model.

Primary keys and foreign keys are used to connect related records and maintain data integrity.

Installation and Setup

Open the project in Visual Studio.

Open the SQL script in SQL Server Management Studio.

Execute the script to create the RaceDayDB database and its tables.

Open appsettings.json in the API project.

Update the SQL Server connection string to match your computer.

Install the required NuGet packages if they are not already installed.

Build the solution in Visual Studio.

Run the API project.

Project Structure

The project is organised into the following main folders:

Controllers: Contains API endpoints for authentication, profiles, events, categories, routes, enrolments and results.

Models: Contains the C# classes representing database entities.

Data: Contains the database context and Entity Framework Core configuration.

DTOs: Contains request models used to receive API data.

appsettings.json: Contains application configuration, including the database connection string.

 Conclusion

Part 2 develops the API component of the RaceDay Event Management System. The API connects the application to the database and provides endpoints for managing events, participants, enrolments and results. Testing the endpoints helps verify that the system behaves as expected.


Swagger for API testing

JWT authentication for secure access

Youtube Link:
https://youtu.be/_p-vxFmHZEY
