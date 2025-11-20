# Interview-Project

## 🧩 Project Overview

The project simulates a power consumption management platform for an energy company.
Its purpose is to provide an environment where users can:

- View available plans and tax groups
- Calculate and receive plan recommendations based on their energy consumption and tax rules
- Manage their own user profiles
- Receive automated insights and analytics about energy usage

The system is also equipped with administrator capabilities, allowing authorized staff to:

- Create, update, and delete pricing plans and tax groups
- View statistics about user behavior (e.g., most used plans, user count per tax group)
- Access data visualization dashboards for analytics

The application is divided into two main components:

- **Backend API** — built with ASP.NET Core, handling authentication, business logic, and database access
- **Frontend** — a React application providing a user-friendly interface for both regular users and admins

## 🎯 Key Features

- Dynamic plan recommendation based on user energy consumption
- Management of plans, tax groups, and users with proper validation rules
- Authentication and role-based authorization using JWT tokens
- Dockerized setup for isolated and reproducible environments
- Integration and unit tests for critical backend components

## ⚙️ Backend (API)

The backend is a .NET 8 Web API project that exposes RESTful endpoints for all core operations.
It follows a layered architecture separating Controllers, Services, and Repositories.

### 🧱 Architecture Overview
```
Controllers → Services → Repositories → Database (PostgreSQL)
```

- **Controllers**: Handle HTTP requests, validation, and responses
- **Services**: Contain business logic (plan recommendation, validation, analytics)
- **Repositories**: Manage database persistence using Entity Framework Core
- **Models**: Define the database schema and domain entities
- **DTOs (Data Transfer Objects)**: Define structured data for requests/responses

### 🧪 Testing

The backend is extensively tested using xUnit and Moq, with both:

- Unit tests for individual services, repositories, and controllers
- Integration tests simulating real API interactions with an in-memory database

All critical features such as plan calculations, CRUD operations, and authentication logic have passing test suites. 
To run the tests from the Project root folder run the following commands:

```bash
cd Backend/api.Test
dotnet test
```

### 🧰 Technologies Used

| Category | Technology |
|----------|------------|
| Framework | ASP.NET Core 8.0 |
| ORM | Entity Framework Core (PostgreSQL provider) |
| Authentication | JWT Bearer Tokens |
| Testing | xUnit, Moq, EFCore InMemory |
| Containerization | Docker & Docker Compose |
| Database | PostgreSQL |
| API Docs | Swagger / OpenAPI |

This builds and runs the backend (api), frontend, and PostgreSQL database in a shared network.

After startup:

- Swagger is available at: http://localhost:5000/swagger
- API base URL: http://localhost:5000

### 🔑 Authentication

The application uses JWT tokens for authentication and role-based authorization.

**Roles:**

- `user`: Standard user with limited access
- `admin`: Can create/update/delete plans, tax groups, and users

You can configure the JWT settings in appsettings.json:
```json
"Jwt": {
  "Key": "this_is_a_super_secret_jwt_key_123456",
  "Issuer": "elcompany",
  "Audience": "elcompany_users",
  "ExpiresInMinutes": 60
}
```

## 🖥️ Frontend

The frontend is a modern React-based single-page application (SPA) that provides a clean, responsive interface for both users and administrators. It communicates directly with the backend API for authentication, data management, and analytics.

### 📁 Location

`/Frontend`

### 🧰 Tech Stack

- ⚛️ **React 18** — modern functional components with hooks
- ⚡ **Vite** — fast and lightweight development build system
- 🎨 **Ant Design (AntD)** — professional UI components and layout system
- 🔗 **Axios** — simplified HTTP client for API communication
- 🧭 **React Router v6** — client-side routing and role-based navigation (admin/user separation)

### 🚀 Key Features

#### 👤 Authentication & Roles

- User registration and JWT-based login
- Role-based access control with protected routes for admin and user roles

#### 🧮 Core Functionality

- Dynamic plan selection and tax group management
- CRUD interfaces for plans and tax groups (admin only)
- Plan recommendation system based on consumption and tax group
- User plan selection stored and displayed with visual highlights

#### 📊 Analytics Dashboard

- Displays plan popularity and tax group distribution (admin view)
- Uses clean and responsive charts for easy data insights

#### 💅 UI & UX

- Fully responsive layout using Ant Design grid system
- Consistent design language across all pages (landing, login, dashboards)
- Visual feedback with modals, confirmations, and error handling

### 🧱 Folder Structure (Simplified)
```
frontend/
├── src/
│   ├── components/        # Shared UI components (Header, dashboards, etc.)
│   ├── pages/             # Route-based pages (Login, Home, Admin, etc.)
│   ├── routes/            # Role-protected route components
│   ├── services/          # API service modules (Auth, User, Plan, TaxGroup)
│   ├── App.jsx            # Main application routes and layout
│   └── main.jsx           # React entry point
└── vite.config.js         # Vite configuration
```

### 🔐 Role-Based Routing Overview

| Route | Role | Description |
|-------|------|-------------|
| `/` | Public | Landing page |
| `/login` | Public | Login page |
| `/register` | Public | User registration |
| `/home` | User | User dashboard & plan selection |
| `/admin` | Admin | Admin dashboard with CRUD and analytics |
| `/register-success` | Public | Registration success confirmation |

Protected routes are managed using custom route guards:

- **AdminRoute** – restricts access to admin-only pages
- **UserRoute** – restricts access to authenticated user pages

## 🐳 Running the Application with Docker

The entire stack (frontend + backend + PostgreSQL database) can be built and started with a single command.

### 🧱 Requirements

- **Docker Desktop** installed and running
- Optional: `git` if you're cloning from GitHub

### 🚀 Build & Run

From the project root (where docker-compose.yml is located):
```bash
docker-compose up --build
```

This command will:

- Build and run the backend (.NET 8 API)
- Build and serve the frontend (React + Vite + Nginx)
- Start a PostgreSQL database and initialize it using backup.sql

Once started:

| Service | URL | Description |
|---------|-----|-------------|
| Frontend | http://localhost:3000 | React SPA served by Nginx |
| Backend (API) | http://localhost:5000/swagger | .NET API and Swagger UI |
| Database | localhost:5432 | PostgreSQL container (user `postgres`, password `1234`) |

### 💾 Data Persistence

The database runs on a named Docker volume (`db_data`) so that data persists across restarts.

The `backup.sql` file is imported only on the first run when the volume is empty.

To reset the database and re-import the backup:
```bash
docker-compose down -v
docker-compose up --build
```

### 🧹 Stopping the Stack
```bash
docker-compose down
```

This stops and removes the containers but keeps the persisted database volume.

## ⚙️ Local Development (without Docker)

You can run the backend and frontend locally without Docker. This is useful for development or debugging.

### 1. **Backend (API)**

Navigate to the backend folder and restore/build/run the API:
```bash
cd api
dotnet restore
dotnet build
dotnet ef database update
dotnet run
```

- The backend will start on the configured port (e.g., http://localhost:5289).
- Swagger is available at http://localhost:5289/swagger.
- `dotnet ef database update` ensures the local database is created/updated via EF Core migrations.

### 2. **Frontend**

Navigate to the frontend folder, install dependencies, update API URLs, and run:
```bash
cd Frontend/el-company
npm install
```

In your service files (`AuthService.js`, `UserService.js`, etc.), set the backend URL to your local API port:
```js
const API_BASE = 'http://localhost:5289';
```

Start the Vite development server:
```bash
npm run dev
```

Frontend will run on http://localhost:5173 and communicate with the local backend.

### ⚠️ When running locally, remember:

- EF Core migrations must be applied for the database
- Frontend service URLs must point to your local backend port
- Backend and frontend are run separately

## 🧩 Troubleshooting

- Make sure no other service is using ports `3000`, `5000`, or `5432`.
- If you update code and need to rebuild images, use:
```bash
  docker-compose up --build
```
- Check logs:
```bash
  docker-compose logs -f
```
