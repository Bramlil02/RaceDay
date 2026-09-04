# RaceDay

#Explaination

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



## Main Features
The RaceDay system allows users to:

- Register and manage participant information.
- Create and manage events.
- Create event categories.
- Create and manage race routes.
- Register participants for events.
- View event registrations.
- Record and view race results.

Added SQl file
## Database
The database was created using Microsoft SQL Server. Primary keys and foreign keys are used to connect the tables. Constraints such as NOT NULL, UNIQUE, DEFAULT and CHECK are also used to help keep the data accurate.
Added API Endpoint Plan

## API
The API will allow the application to communicate with the database. It will provide endpoints for authentication, profiles, events, categories, routes, registrations and results.

