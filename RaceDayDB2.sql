

USE RaceDayDB;
GO

SELECT UserAccountID, FullName, Email, Role
FROM UserAccounts;
GO

USE RaceDayDB;
GO

UPDATE dbo.UserAccounts
SET Role = 'Organiser'
WHERE Email = '';
GO

SELECT Email, Role
FROM dbo.UserAccounts;
GO

USE RaceDayDB;
GO

UPDATE dbo.UserAccounts
SET Role = 'Organiser'
WHERE Email = 'Tumi26@gmail.com';
GO

SELECT UserAccountID, FullName, Email, Role
FROM dbo.UserAccounts
WHERE Email = 'Tumi26@gmail.com';
GO