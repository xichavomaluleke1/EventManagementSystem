# 🏃 RaceDay - Event Management System

## 📋 Overview

RaceDay is a full-stack web-based event management system designed specifically for the South African road running, walking, and cycling community. The platform allows Event Organisers to create and manage events, categories, and participant results, while Participants can browse upcoming events, enter events, track their personal performance history, and prepare for race day using live weather and route information.

---

## 👥 User Roles

The system supports two distinct user roles:

### 🏢 Organiser
- Create, edit, and delete events
- Manage event categories (age and distance-based)
- View all event enrolments
- Capture and publish participant results
- Upload event banner images
- Dashboard showing event statistics and summaries

### 🏃 Participant
- Create an account and manage profile
- Browse upcoming events with filtering options
- Enter events by selecting a category
- View personal enrolment history
- Track personal race results and performance
- Upload profile picture
- View event details with route information

---

## 📁 Project Structure
RaceDay/
├── docs/
│ ├── erd.png # Entity Relationship Diagram
│ ├── api-endpoint-plan.pdf # Complete API endpoint specifications
│ └── RaceDay_Schema.sql # SQL database creation script
├── .github/
│ └── workflows/
│ └── ci.yml # GitHub Actions CI/CD workflow
└── README.md # This file


---

## 🗄️ Database Schema

The database consists of 6 core tables:

| Table | Description |
|-------|-------------|
| `users` | Stores all user authentication and profile information |
| `EventOrganisers` | Organiser-specific information linked to users |
| `events` | Core event information including date, location, and type |
| `categories` | Age and distance categories for each event |
| `enrolments` | Links participants to events and categories |
| `results` | Race results for enrolled participants |

### Entity Relationship Diagram

![ERD](https://github.com/xichavomaluleke1/EventManagementSystem/blob/main/docs/ERD.png)

---

## 🔗 API Endpoints

The API exposes **46 endpoints** organized by functionality:

| Category | Endpoints | Description |
|----------|-----------|-------------|
| Authentication | 4 | Register, Login, Logout, Current User |
| User Profile | 5 | View, Update, Profile Picture, Change Password |
| Events | 9 | CRUD operations, Banner Upload, Publish |
| Categories | 5 | CRUD operations for event categories |
| Enrolments | 8 | Create, Cancel, View, Statistics |
| Results | 7 | Capture, Update, Delete, Leaderboard |
| Organiser Dashboard | 4 | Dashboard summary, Event management |
| Participant Dashboard | 3 | Dashboard, Enrolments, Results |
| System | 1 | Health check |

### Full API Documentation

[View Complete API Endpoint Plan](docs/api-endpoint-plan.pdf)

---

## 🛠️ Setup Instructions

### Prerequisites

- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (2019 or later)
- [SQL Server Management Studio (SSMS)](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- [Git](https://git-scm.com/downloads)
- [.NET SDK 8.0](https://dotnet.microsoft.com/download) (for Part 2)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for Part 3)

Set Up the Database
Open SQL Server Management Studio (SSMS)
Connect to your SQL Server instance
Open the script file:

text
docs/Raceday_Database.sql
Execute the script (press F5 or click Execute)

Verify the database was created:
sql
USE raceday_database;
SELECT * FROM users;
SELECT * FROM events;
SELECT * FROM enrolments;
Expected Output:

Database raceday_database created
6 tables created
Seed data inserted:
4 Users (2 Organisers, 2 Participants)
4 Events
17 Categories
6 Enrolments
2 Results

 Verify Database Setup
Run these verification queries in SSMS:

sql
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
Expected Results:

TableName	Count
users	4
EventOrganisers	2
events	4
categories	17
enrolments	6
results	2

🔄 CI/CD Pipeline
GitHub Actions Workflow
The repository includes a GitHub Actions workflow that automatically:
Validates repository structure
Checks for /docs folder
Verifies required files exist
Validates file contents
SQL script contains CREATE TABLE statements
SQL script contains INSERT INTO statements (seed data)
ERD is a valid JPEG image
API Endpoint Plan is a valid PDF file
Counts files
Ensures minimum of 3 files in /docs folder

Green Build Screenshot:                                                                                
<img width="669" height="376" alt="image" src="https://github.com/user-attachments/assets/7df560bf-8085-4b7e-b4bb-159d791cd107" /> 

📹 Video Presentation
Watch the full walkthrough of the RaceDay system: 

📝 Part 1 Submission Checklist
☑ GitHub repository created
☑ /docs folder created
☑ ERD diagram saved as ERD.jpeg
☑ API endpoint plan saved as API ENDPOINT PLAN.pdf
☑ SQL script saved as Raceday_Database.sql
☑ README.md with setup instructions
☑ 20+ meaningful commits made
☑ CI/CD workflow configured
☑ CI/CD green build screenshot included
☑ YouTube video link included
☑ GitHub repository link submitted on ARC

📚 Technologies Used
Technology	Purpose
SQL Server	Database management
Entity Framework Core	ORM for database access
ASP.NET Core Web API	Backend API (Part 2)
ASP.NET Core MVC	Frontend web application (Part 3)
Azure Blob Storage	File/Image storage (Part 3)
Docker	Containerization (Part 3)
GitHub Actions	CI/CD automation
Swagger/OpenAPI	API documentation
Bootstrap	UI styling (Part 3)

👨‍💻 Author
Name: Xichavo Maluleke
GitHub: https://github.com/xichavomaluleke1/EventManagementSystem.git
