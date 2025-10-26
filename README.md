# Home Assignment 2 - Project & Task Management System

A full-stack application for managing projects and tasks with JWT authentication.

## Live Deployment

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

### Smart Scheduler

#### Get Recommended Task Order
```bash
POST /api/scheduler/order
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "tasks": [
    {
      "title": "Design Database Schema",
      "estimatedHours": 3.0,
      "dueDate": "2024-12-15T23:59:59",
      "dependencies": []
    },
    {
      "title": "Implement API",
      "estimatedHours": 8.0,
      "dueDate": "2024-12-20T23:59:59",
      "dependencies": ["Design Database Schema"]
    },
    {
      "title": "Write Tests",
      "estimatedHours": 4.0,
      "dueDate": "2024-12-18T23:59:59",
      "dependencies": ["Implement API"]
    }
  ],
  "strategy": 0
}

Response:
{
  "recommendedOrder": [
    "Design Database Schema",
    "Implement API",
    "Write Tests"
  ],
  "strategyUsed": "DepsDueSjf"
}
```

**Strategy Options:**
- `0` - `DepsDueSjf`: Dependencies first, then earliest due date, then shortest job first (default)
- `1` - `DepsSjfDue`: Dependencies first, then shortest job first, then earliest due date
- `2` - `DepsDueLjf`: Dependencies first, then earliest due date, then longest job first

**Validation Rules:**
- Task titles must be unique
- Dependencies must reference existing task titles
- No self-dependencies allowed
- No circular dependencies allowed
- Estimated hours must be positive
- Due dates are optional

**Error Responses:**
- `400 Bad Request`: Invalid input (duplicate titles, unknown dependencies, self-dependencies)
- `422 Unprocessable Entity`: Circular dependencies detected (deadlock scenario)

### Feedback

#### Submit User Feedback
```bash
POST /api/feedback
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "rating": 5,
  "comment": "Great app!" (optional)
}

Response:
{
  "id": "guid",
  "rating": 5,
  "comment": "Great app!",
  "createdAt": "2024-12-01T10:30:00Z"
}
```

#### Get User Feedback
```bash
GET /api/feedback
Authorization: Bearer {accessToken}

Response:
[
  {
    "id": "guid",
    "rating": 5,
    "comment": "Great app!",
    "createdAt": "2024-12-01T10:30:00Z"
  }
]
```

## Features

### Core Features
- User authentication with JWT access tokens (60-minute expiry)
- Refresh token rotation for secure session management
- Per-user project and task isolation
- Full CRUD operations for projects and tasks
- Automatic token refresh on API calls
- Protected routes with authentication guards
- Responsive UI with React Bootstrap

### Smart Scheduler
- **Dependency-Aware Task Ordering**: Automatically orders tasks based on their dependencies using topological sort (Kahn's algorithm)
- **Multiple Scheduling Strategies**: Choose from three different tie-breaking strategies:
  - Dependencies → Due Date → Shortest Job First (default)
  - Dependencies → Shortest Job First → Due Date
  - Dependencies → Due Date → Longest Job First
- **Cycle Detection**: Identifies circular dependencies and provides clear error messages
- **Validation**: Comprehensive input validation for task titles, dependencies, and constraints
- **Interactive UI**: User-friendly interface for adding tasks, setting dependencies, and viewing recommended order

### Task Management
- **Task Filtering**: View tasks by status (All, Active, Completed)
- **Task Editing**: Update task titles, due dates, and completion status
- **Task Creation Timestamps**: Automatic tracking of when tasks and projects are created
- **Task Completion Tracking**: Mark tasks as complete and filter by completion status

### User Feedback System
- **Star Rating**: 5-star rating system with visual feedback
- **Optional Comments**: Add detailed feedback with up to 1000 characters
- **Feedback History**: View all previously submitted feedback
- **Toast Notifications**: Real-time feedback on submission success/failure

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