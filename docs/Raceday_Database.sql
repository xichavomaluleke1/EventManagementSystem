--create a database 
create database raceday_database;

--use database
USE [raceday_database];

--create table users
CREATE TABLE users(
    userID INT PRIMARY KEY IDENTITY(1,1),
    email VARCHAR(255) NOT NULL UNIQUE,
    passwordHash VARCHAR(255) NOT NULL,
    role VARCHAR(20) NOT NULL CHECK (role IN ('Organiser', 'Participant')),
    firstName VARCHAR(100) NOT NULL,
    lastName VARCHAR(100) NOT NULL,
    dateOfBirth DATE NOT NULL,
    gender VARCHAR(10) NOT NULL,
    phoneNumber VARCHAR(20) NULL,
    createdAt DATETIME DEFAULT GETDATE(),
    updateAt DATETIME DEFAULT GETDATE(),
    isActive BIT DEFAULT(1),
    lastLoginDate DATETIME NULL
);

--create table events
CREATE TABLE events(
    EventID INT PRIMARY KEY IDENTITY(1,1),
    OrganiserID INT NOT NULL,
    EventName VARCHAR(255) NOT NULL,
    Description VARCHAR(1000) NULL,
    EventDate DATETIME NOT NULL,
    Location VARCHAR(500) NOT NULL,
    Distance DECIMAL(10,2) NOT NULL,
    EventType VARCHAR(20) NOT NULL CHECK (EventType IN ('Run', 'Walk', 'Cycle')),
    MaxParticipants INT NULL,
    EntryFee DECIMAL(10,2) DEFAULT(0),
    BannerImage VARCHAR(500) NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Upcoming' CHECK (Status IN ('Upcoming', 'Ongoing', 'Completed', 'Cancelled')),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    IsPublished BIT DEFAULT(1),
    
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserID) REFERENCES users(userID)
);