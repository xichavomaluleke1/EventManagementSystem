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

--Inserting values to the tables in the database

--Insert Users (2 Organisers, 2 Participants)
INSERT INTO users (email, passwordHash, role, firstName, lastName, dateOfBirth, gender, phoneNumber)
VALUES 
    -- Organisers
    ('john.organiser@raceday.com', 'hashed_password_1', 'Organiser', 'John', 'Smith', '1985-03-15', 'Male', '+27 82 123 4567'),
    ('sarah.events@raceday.com', 'hashed_password_2', 'Organiser', 'Sarah', 'Johnson', '1990-07-20', 'Female', '+27 83 789 0123'),
    -- Participants
    ('mike.runner@email.com', 'hashed_password_3', 'Participant', 'Michael', 'Brown', '1990-05-15', 'Male', '+27 72 345 6789'),
    ('lisa.cyclist@email.com', 'hashed_password_4', 'Participant', 'Lisa', 'Davis', '1985-11-20', 'Female', '+27 73 456 7890');

--Insert Event Organisers
INSERT INTO EventOrganisers (UserID, OrganizationName, OrganizationType, Website, ContactNumber, Bio)
VALUES 
    (1, 'Elite Sports Events', 'Professional', 'https://elitesports.co.za', '+27 82 123 4567', 'Professional event organiser with 10+ years experience'),
    (2, 'Community Run SA', 'Community', 'https://communityrunsa.org', '+27 83 789 0123', 'Organising community running events across South Africa');

--Insert Events
INSERT INTO events (OrganiserID, EventName, Description, EventDate, Location, Distance, EventType, MaxParticipants, EntryFee, Status, IsPublished)
VALUES 
    -- Cape Town Cycle Tour
    (1, 'Cape Town Cycle Tour 2026', 'The largest timed cycle race in the world. A 109km route around the Cape Peninsula.', 
     '2026-03-08 06:00:00', 'Grand Parade, Cape Town', 109.00, 'Cycle', 35000, 450.00, 'Upcoming', 1),
    
    -- Comrades Marathon
    (2, 'Comrades Marathon 2026', 'The Ultimate Human Race - a 90km ultra-marathon between Pietermaritzburg and Durban.',
     '2026-06-14 05:30:00', 'Pietermaritzburg City Hall', 90.00, 'Run', 25000, 850.00, 'Upcoming', 1),
    
    -- Soweto Marathon
    (1, 'Soweto Marathon 2026', 'One of South Africa''s most iconic marathons through the streets of Soweto.',
     '2026-11-01 06:00:00', 'FNB Stadium, Soweto', 42.20, 'Run', 20000, 350.00, 'Upcoming', 1);

--Insert Categories
-- Cape Town Cycle Tour Categories
INSERT INTO categories (EventID, CategoryName, Description, MinAge, MaxAge, EntryFee)
VALUES 
    (1, 'Elite Men', 'Elite competitive category for men aged 18-39', 18, 39, 500.00),
    (1, 'Elite Women', 'Elite competitive category for women aged 18-39', 18, 39, 500.00),
    (1, 'Masters Men', 'Competitive category for men aged 40-60', 40, 60, 450.00),
    (1, 'Masters Women', 'Competitive category for women aged 40-60', 40, 60, 450.00),
    (1, 'Recreational', 'Recreational category for all ages', 18, 70, 350.00);

-- Comrades Marathon Categories
INSERT INTO categories (EventID, CategoryName, Description, MinAge, MaxAge, EntryFee)
VALUES 
    (2, 'Open Men', 'Open category for men aged 20-39', 20, 39, 900.00),
    (2, 'Open Women', 'Open category for women aged 20-39', 20, 39, 900.00),
    (2, 'Veteran Men', 'Veteran category for men aged 40-49', 40, 49, 850.00),
    (2, 'Veteran Women', 'Veteran category for women aged 40-49', 40, 49, 850.00),
    (2, 'Masters Men', 'Masters category for men aged 50-60', 50, 60, 800.00),
    (2, 'Masters Women', 'Masters category for women aged 50-60', 50, 60, 800.00);

-- Soweto Marathon Categories
INSERT INTO categories (EventID, CategoryName, Description, MinAge, MaxAge, EntryFee)
VALUES 
    (3, 'Senior Men', 'Senior category for men aged 18-34', 18, 34, 400.00),
    (3, 'Senior Women', 'Senior category for women aged 18-34', 18, 34, 400.00),
    (3, 'Masters Men', 'Masters category for men aged 35-49', 35, 49, 350.00),
    (3, 'Masters Women', 'Masters category for women aged 35-49', 35, 49, 350.00),
    (3, 'Grandmasters Men', 'Grandmasters category for men aged 50-70', 50, 70, 300.00),
    (3, 'Grandmasters Women', 'Grandmasters category for women aged 50-70', 50, 70, 300.00);

--Insert Enrolments
INSERT INTO enrolments (ParticipantID, EventID, CategoryID, Status, PaymentStatus, paymentReference)
VALUES 
    -- Michael Brown enrols in Cape Town Cycle Tour - Recreational
    (3, 1, 5, 'Confirmed', 'Paid', 'PAY-001'),
    
    -- Michael Brown enrols in Soweto Marathon - Senior Men
    (3, 3, 11, 'Confirmed', 'Paid', 'PAY-002'),
    
    -- Lisa Davis enrols in Comrades Marathon - Open Women
    (4, 2, 8, 'Pending', 'Pending', NULL),
    
    -- Lisa Davis enrols in Cape Town Cycle Tour - Elite Women
    (4, 1, 2, 'Confirmed', 'Paid', 'PAY-003');

--Add a Past Event with Results
INSERT INTO events (OrganiserID, EventName, Description, EventDate, Location, Distance, EventType, MaxParticipants, EntryFee, Status, IsPublished)
VALUES 
    (2, 'Two Oceans Marathon 2025', 'The Two Oceans Marathon is a 56km ultra-marathon in Cape Town.',
     '2025-04-19 06:00:00', 'Newlands, Cape Town', 56.00, 'Run', 15000, 750.00, 'Completed', 1);

-- Add categories for Two Oceans
INSERT INTO categories (EventID, CategoryName, Description, MinAge, MaxAge, EntryFee)
VALUES 
    (4, 'Ultra Men', 'Ultra category for men', 20, 39, 800.00),
    (4, 'Ultra Women', 'Ultra category for women', 20, 39, 800.00);

-- Enrol participants for Two Oceans
INSERT INTO enrolments (ParticipantID, EventID, CategoryID, Status, PaymentStatus, paymentReference)
VALUES 
    (3, 4, 17, 'Completed', 'Paid', 'PAY-004'),
    (4, 4, 18, 'Completed', 'Paid', 'PAY-005');

-- Add Results for Two Oceans
INSERT INTO results (EnrolmentID, FinishTime, FinishPosition, TotalParticipants, CategoryPosition, TotalCategoryParticipants, Status, RecordedBy, Notes)
VALUES 
    (5, '04:45:30', 156, 1500, 23, 150, 'Confirmed', 2, 'Finished strong'),
    (6, '05:12:45', 289, 1500, 45, 150, 'Confirmed', 2, 'Good performance');

-- Check all tables have data
SELECT 'users' AS TableName, COUNT(*) AS Count FROM users
UNION ALL
SELECT 'EventOrganisers', COUNT(*) FROM EventOrganisers
UNION ALL
SELECT 'events', COUNT(*) FROM events
UNION ALL
SELECT 'categories', COUNT(*) FROM categories
UNION ALL
SELECT 'enrolments', COUNT(*) FROM enrolments
UNION ALL
SELECT 'results', COUNT(*) FROM results;

-- Show role summary
SELECT role, COUNT(*) AS UserCount FROM users GROUP BY role;

-- Show enrolment summary by event
SELECT 
    e.EventName,
    COUNT(en.EnrolmentID) AS TotalEnrolments,
    COUNT(CASE WHEN en.Status = 'Confirmed' THEN 1 END) AS Confirmed,
    COUNT(r.ResultID) AS ResultsEntered
FROM events e
LEFT JOIN enrolments en ON e.EventID = en.EventID
LEFT JOIN results r ON en.EnrolmentID = r.EnrolmentID
GROUP BY e.EventID, e.EventName
ORDER BY e.EventName;
