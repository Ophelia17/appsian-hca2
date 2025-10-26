# Home Assignment 2 - Project & Task Management System

A full-stack application for managing projects and tasks with JWT authentication.

## 🚀 Live Deployment

The application is deployed and accessible at: **[https://appsian-hca2.vercel.app/](https://appsian-hca2.vercel.app/)**

> **Note:** The backend is running on a free tier of Render, which goes to sleep after periods of inactivity. If the application seems slow or unresponsive, please click this link to wake up the backend: [https://smart-scheduler-api-rbno.onrender.com](https://smart-scheduler-api-rbno.onrender.com)

## Tech Stack

### Server
- .NET 8 Web API
- Entity Framework Core with SQLite
- JWT Bearer Authentication
- Refresh Token Flow

### Client
- React 18 with TypeScript
- Vite
- React Router
- Axios
- React Bootstrap

## Setup Instructions

### Prerequisites
- .NET 8 SDK
- Node.js 18+ and npm

### Server Setup

1. Navigate to the server directory:
   ```bash
   cd server
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

The server will start on `http://localhost:5000` or `https://localhost:5001`

### Client Setup

1. Navigate to the client directory:
   ```bash
   cd client
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Run the development server:
   ```bash
   npm run dev
   ```

The client will start on `http://localhost:5173`

## API Endpoints

### Authentication

#### Register
```bash
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

#### Login
```bash
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}

Response:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "random256bitbase64token"
}
```

#### Refresh Token
```bash
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "current_refresh_token"
}

Response:
{
  "accessToken": "new_access_token",
  "refreshToken": "new_refresh_token"
}
```

#### Logout
```bash
POST /api/auth/logout
Content-Type: application/json

{
  "refreshToken": "refresh_token_to_revoke"
}
```

### Projects

#### Get All Projects
```bash
GET /api/projects
Authorization: Bearer {accessToken}
```

#### Get Project by ID
```bash
GET /api/projects/{id}
Authorization: Bearer {accessToken}

Response includes embedded tasks array
```

#### Create Project
```bash
POST /api/projects
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "title": "My Project",
  "description": "Project description (optional)"
}
```

#### Delete Project
```bash
DELETE /api/projects/{id}
Authorization: Bearer {accessToken}
```

### Tasks

#### Create Task
```bash
POST /api/projects/{projectId}/tasks
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "title": "Task title",
  "dueDate": "2024-12-31T23:59:59" (optional)
}
```

#### Update Task
```bash
PUT /api/tasks/{taskId}
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "title": "Updated title",
  "dueDate": "2024-12-31T23:59:59" (optional),
  "isCompleted": false
}
```

#### Delete Task
```bash
DELETE /api/tasks/{taskId}
Authorization: Bearer {accessToken}
```

## Features

- User authentication with JWT access tokens (60-minute expiry)
- Refresh token rotation for secure session management
- Per-user project and task isolation
- Full CRUD operations for projects and tasks
- Automatic token refresh on API calls
- Protected routes with authentication guards
- Responsive UI with React Bootstrap

## Validation Rules

- Email: Valid email format, unique constraint
- Password: Minimum 8 characters
- Project Title: 3-100 characters, required
- Project Description: Maximum 500 characters, optional
- Task Title: Required
- Task Due Date: Optional datetime

## Security

- Passwords are hashed using SHA-256
- JWT tokens signed with HMAC-SHA256
- Refresh tokens stored as SHA-256 hashes in database
- SQL injection protection via Entity Framework parameterization
- CORS configured for client origin only

## Database

The application uses SQLite with Entity Framework Core. The database file (`app.db`) is created automatically on first run.

## Testing

### Server Tests
```bash
cd server
dotnet test
```

### Client Tests
```bash
cd client
npm run test
```