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

The database consists of 7 core tables:

| Table | Description |
|-------|-------------|
| `users` | Stores all user authentication and profile information |
| `EventOrganisers` | Organiser-specific information linked to users |
| `events` | Core event information including date, location, and type |
| `categories` | Age and distance categories for each event |
| `enrolments` | Links participants to events and categories |
| `results` | Race results for enrolled participants |
| `EventRoutes` | Route information for events (optional) |

### Entity Relationship Diagram

![ERD](docs/erd.png)

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

---
