
--CREATING DATABASE
CREATE DATABASE RaceDayDB;

--Using for re-using the database
USE RaceDayDB;

--EVENT ORGANISER
CREATE TABLE EventOrganiser
(
    OrganizerID INT IDENTITY(1,1) NOT NULL,
    OrganizerName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Phone VARCHAR(20),

    CONSTRAINT PK_EventOrganiser
        PRIMARY KEY (OrganizerID),

    CONSTRAINT UQ_EventOrganiser_Email
        UNIQUE (Email)
);

--PARTICIPANT
CREATE TABLE participant2
(
    ParticipantID INT IDENTITY(1,1) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Phone VARCHAR(20),
    DateOfBirth DATE,

    CONSTRAINT PK_Participant
        PRIMARY KEY (ParticipantID),

    CONSTRAINT UQ_Participant_Email
        UNIQUE (Email)
);


 --CATEGORY
CREATE TABLE Category2
(
    CategoryID INT IDENTITY(1,1) NOT NULL,
    CategoryName VARCHAR(50) NOT NULL,
    Description VARCHAR(255),

    CONSTRAINT PK_Category
        PRIMARY KEY (CategoryID),

    CONSTRAINT UQ_Category_Name
        UNIQUE (CategoryName)
);

--ROUTE
CREATE TABLE Route2
(
    RouteID INT IDENTITY(1,1) NOT NULL,
    RouteName VARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    StartPoint VARCHAR(150) NOT NULL,
    FinishPoint VARCHAR(150) NOT NULL,
    Description VARCHAR(255),

    CONSTRAINT PK_Route
        PRIMARY KEY (RouteID),

    CONSTRAINT UQ_Route_Name
        UNIQUE (RouteName),

    CONSTRAINT CK_Route_Distance
        CHECK (DistanceKm > 0)
);

--EVENT
CREATE TABLE [Event2]
(
    EventID INT IDENTITY(1,1) NOT NULL,

    OrganizerID INT NOT NULL,
    CategoryID INT NOT NULL,
    RouteID INT NOT NULL,

    EventName VARCHAR(150) NOT NULL,
    EventDate DATE NOT NULL,
    Location VARCHAR(150) NOT NULL,

    EntryFee DECIMAL(10,2) NOT NULL
        CONSTRAINT DF_Event_EntryFee DEFAULT (0.00),

    Status VARCHAR(30) NOT NULL
        CONSTRAINT DF_Event_Status DEFAULT ('Open'),

    CONSTRAINT PK_Event
        PRIMARY KEY (EventID),

    CONSTRAINT FK_Event_Organizer
        FOREIGN KEY (OrganizerID)
        REFERENCES EventOrganiser(OrganizerID),

    CONSTRAINT FK_Event_Category2
        FOREIGN KEY (CategoryID)
        REFERENCES Category2(CategoryID),

    CONSTRAINT FK_Event_Route2
        FOREIGN KEY (RouteID)
        REFERENCES Route2(RouteID),

    CONSTRAINT CK_Event_EntryFee
        CHECK (EntryFee >= 0)
);

--REGISTRATION
CREATE TABLE Registration2
(
    RegistrationID INT IDENTITY(1,1) NOT NULL,

    EventID INT NOT NULL,
    ParticipantID INT NOT NULL,

    RegistrationDate DATE NOT NULL
        CONSTRAINT DF_Registration_Date DEFAULT (GETDATE()),

    RaceNumber INT,

    PaymentStatus VARCHAR(30) NOT NULL
        CONSTRAINT DF_Registration_Payment DEFAULT ('Pending'),

    FinishTime TIME,
    Position INT,

    AveragePace DECIMAL(5,2),

    ResultStatus VARCHAR(30)
        CONSTRAINT DF_Registration_ResultStatus DEFAULT ('Registered'),

    CONSTRAINT PK_Registration
        PRIMARY KEY (RegistrationID),

    CONSTRAINT FK_Registration2_Event
        FOREIGN KEY (EventID)
        REFERENCES [Event2](EventID),

    CONSTRAINT FK_Registration_Participant2
        FOREIGN KEY (ParticipantID)
        REFERENCES Participant2(ParticipantID),


    -- Prevents the same participant registering twice for the same event.
    CONSTRAINT UQ_Registration_Event_Participant
        UNIQUE (EventID, ParticipantID),

    CONSTRAINT CK_Registration_Position
        CHECK (Position IS NULL OR Position > 0),


    CONSTRAINT CK_Registration_AveragePace
        CHECK (AveragePace IS NULL OR AveragePace > 0)
);




--INSERT DATA

-- 2 ORGANISERS
INSERT INTO EventOrganiser
(OrganizerName, Email, Phone)

VALUES
('Cape Events Management', 'info@capeevents.co.za', '0215551234'),

('Durban Race Organisers', 'info@durbanraces.co.za', '0315552345');


--PARTICIPANTS
INSERT INTO participant2
(FirstName, LastName, Email, Phone, DateOfBirth)

VALUES
('Thandi', 'Mokoena', 'thandi.mokoena@email.com', '0721234567', '1998-06-15'),

('Daniel', 'Naidoo', 'daniel.naidoo@email.com', '0762345678', '1996-11-30');


-- CATEGORIES
INSERT INTO Category2
(CategoryName, Description)

VALUES
('Road Running', 'Running events held on roads'),
('Walking', 'Walking and fun walk events'),

('Cycling', 'Road cycling events');


-- ROUTES

INSERT INTO Route2
(RouteName,DistanceKm,
 StartPoint,FinishPoint,
 Description)

VALUES
('Cape Town City Route', 10.00, 'Cape Town Stadium', 'Green Point', '10 km city road route'),

('Durban Beach Route', 15.00, 'Moses Mabhida Stadium', 'Durban Beachfront', '15 km coastal route'),

('Johannesburg Cycling Route', 21.10, 'Sandton', 'FNB Stadium', '21.1 km cycling route');

-- 3 EVENTS
INSERT INTO [Event2]
(OrganizerID, CategoryID,
 RouteID, EventName,
 EventDate, Location,
 EntryFee, Status )

VALUES

( 1, 1, 1, 'Cape Town Summer Run', '2026-10-10', 'Cape Town', 150.00, 'Open'),

(2, 2, 2, 'Durban Beach Walk', '2026-10-17', 'Durban', 100.00, 'Open'),

(1, 3, 3, 'Johannesburg Cycling Challenge', '2026-11-01', 'Johannesburg', 250.00, 'Open');



-- SAMPLE ENROLMENTS / REGISTRATIONS
INSERT INTO Registration2
(EventID,ParticipantID,
 RaceNumber,PaymentStatus
)
VALUES
(1, 1, 101, 'Paid'),

(1, 2, 102, 'Paid'),

(2, 1, 201, 'Pending'),

(3, 2, 301, 'Paid');

-- SAMPLE RESULTS
UPDATE Registration2
SET
    FinishTime = '00:52:30',
    Position = 15,
    AveragePace = 5.25,
    ResultStatus = 'Finished'
WHERE RegistrationID = 1;


UPDATE Registration2
SET
    FinishTime = '00:58:45',
    Position = 24,
    AveragePace = 5.88,
    ResultStatus = 'Finished'
WHERE RegistrationID = 2;



--VERIFY THE DATA
SELECT * FROM EventOrganiser;

SELECT * FROM Participant2;

SELECT * FROM Category2;

SELECT * FROM Route2;

SELECT * FROM [Event2];

SELECT * FROM Registration2;

--TEST THE EVENT RELATIONSHIPS

SELECT
    E.EventID,
    E.EventName,
    EO.OrganizerName,
    C.CategoryName,
    R.RouteName,
    E.EventDate,
    E.Location,
    E.EntryFee,
    E.Status
FROM [Event2] AS E INNER JOIN EventOrganiser AS EO
ON E.OrganizerID = EO.OrganizerID INNER JOIN Category2 AS C
ON E.CategoryID = C.CategoryID INNER JOIN Route2 AS R
ON E.RouteID = R.RouteID;



-- TEST PARTICIPANT ENROLMENTS AND RESULTS
SELECT
    P.ParticipantID,
    P.FirstName,
    P.LastName,
    E.EventName,
    R.RaceNumber,
    R.PaymentStatus,
    R.FinishTime,
    R.Position,
    R.AveragePace,
    R.ResultStatus
FROM Registration2 AS R INNER JOIN Participant2 AS P
ON R.ParticipantID = P.ParticipantID INNER JOIN [Event2] AS E
ON R.EventID = E.EventID;
