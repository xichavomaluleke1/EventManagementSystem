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
