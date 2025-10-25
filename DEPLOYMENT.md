# Deployment Guide

## Server (Render)

1. **Create a new Web Service on Render**
   - Connect your GitHub repository
   - Choose "Docker" as the environment
   - Set the following environment variables:
     - `JWT_SECRET`: A secure random string (minimum 256 bits)
     - `JWT_ISSUER`: "Server"
     - `JWT_AUDIENCE`: "Client"
     - `ConnectionStrings__Default`: "Data Source=app.db"

2. **Configure Render settings:**
   - Build Command: `cd server && docker build -t app .`
   - Start Command: `cd server && docker run -p 8080:8080 app`
   - Port: 8080

3. **Health Check:**
   - The server exposes `/healthz` endpoint for health checks
   - Render will automatically use this for health monitoring

## Client (Vercel)

1. **Deploy to Vercel**
   - Connect your GitHub repository
   - Set the root directory to `client`
   - Add environment variable:
     - `VITE_API_BASE_URL`: Your Render server URL (e.g., `https://your-app.onrender.com`)

2. **Build Configuration:**
   - Build Command: `npm run build`
   - Output Directory: `dist`
   - Framework: Vite

## Environment Variables

### Server (Render)
```
JWT_SECRET=your-super-secret-jwt-key-minimum-256-bits-long
JWT_ISSUER=Server
JWT_AUDIENCE=Client
ConnectionStrings__Default=Data Source=app.db
```

### Client (Vercel)
```
VITE_API_BASE_URL=https://your-render-app.onrender.com
```

## CORS Configuration

The server is configured to allow requests from:
- `http://localhost:5173` (local development)
- `https://*.vercel.app` (Vercel deployments)

## Database

The application uses SQLite with Entity Framework Core. The database is automatically created on first run using `EnsureCreated()`.

## Health Check

The server exposes a health check endpoint at `/healthz` that returns "OK" for monitoring purposes.
