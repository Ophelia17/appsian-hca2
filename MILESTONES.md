# Milestone Plan - Home Assignment 2

## Implementation Roadmap

1. **Repo scaffold & tooling** - analyzers, nullable, TS strict, ESLint/Prettier
2. **Server scaffold** - .NET 8, EF Core, SQLite, DbContext, migrations
3. **Entities/DTOs** - User, Project, TaskItem, RefreshToken (+ Create/Update/View DTOs)
4. **Auth** - register/login (hashing), JWT issue (60m, sub=user.Id)
5. **Refresh tokens** - DB-backed, rotation on refresh, revoke on logout
6. **Project CRUD** - ownership checks and GET /projects/{id} embedding tasks
7. **Task CRUD** - create under project, PUT update, delete with ownership checks
8. **Server tests** - unit (token & ownership), light integration (auth→CRUD flow)
9. **Client scaffold** - Vite, React, Router, Axios, React-Bootstrap; base URL /api
10. **Auth pages** - Register/Login, token storage, Axios interceptor, route guard
11. **Dashboard** - list/create/delete projects
12. **Project Details** - fetch project+tasks; create/update/delete tasks + RTL tests
