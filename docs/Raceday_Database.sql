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

--create table categories
CREATE TABLE categories(
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    EventID INT NOT NULL,
    CategoryName VARCHAR(100) NOT NULL,
    Description VARCHAR(500) NULL,
    MinAge INT NULL,
    MaxAge INT NULL,
    MinDistance DECIMAL(10,2) NULL,
    MaxDistance DECIMAL(10,2) NULL,
    EntryFee DECIMAL(10,2) DEFAULT(0),
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT(1),
    
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventID) REFERENCES events(EventID) ON DELETE CASCADE
);

--create table enrolments
CREATE TABLE enrolments(
    EnrolmentID INT PRIMARY KEY IDENTITY(1,1),
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME DEFAULT GETDATE(),
    Status VARCHAR(20) NOT NULL DEFAULT('Pending') CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled', 'Withdrawn', 'Completed')),
    PaymentStatus VARCHAR(20) NOT NULL DEFAULT('Pending') CHECK (PaymentStatus IN ('Pending', 'Paid', 'Refunded', 'Cancelled')),
    paymentReference VARCHAR(100) NULL,
    Notes VARCHAR(500) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantID) REFERENCES users(userID),
    CONSTRAINT FK_Enrolments_Events FOREIGN KEY (EventID) REFERENCES events(EventID),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryID) REFERENCES categories(CategoryID)
);

--create table results
CREATE TABLE results(
    ResultID INT PRIMARY KEY IDENTITY(1,1),
    EnrolmentID INT NOT NULL,
    FinishTime TIME NULL,
    FinishPosition INT NULL,
    TotalParticipants INT NULL,
    CategoryPosition INT NULL,
    TotalCategoryParticipants INT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT('DNS') CHECK (Status IN ('Pending', 'Confirmed', 'DNS', 'DNF', 'Disqualified')),
    RecordedAt DATETIME DEFAULT GETDATE(),
    RecordedBy INT NOT NULL,
    Notes VARCHAR(500) NULL,
    
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentID) REFERENCES enrolments(EnrolmentID) ON DELETE CASCADE,
    CONSTRAINT FK_Results_Users FOREIGN KEY (RecordedBy) REFERENCES users(userID)
);

--create table EventOrganisers
CREATE TABLE EventOrganisers(
    EventOrganiserID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL UNIQUE,
    OrganizationName VARCHAR(255) NULL,
    OrganizationType VARCHAR(50) NULL,
    Website VARCHAR(255) NULL,
    ContactNumber VARCHAR(20) NULL,
    Bio VARCHAR(1000) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_EventOrganisers_Users FOREIGN KEY (UserID) REFERENCES users(userID) ON DELETE CASCADE
);