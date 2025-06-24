# Auth-Service
# Overview
- auth-app is a full-stack authentication system with a role-based dashboard. The frontend, built with Next.js, provides a user-friendly interface for login, registration, and user management. The backend, powered by ASP.NET Core, implements secure authentication using Microsoft Identity, supporting JWT-based login, registration with email confirmation, and role management. The application uses the Unit of Work and Repository patterns for clean data access and maintainability.

# Features
- User registration with email confirmation
- JWT-based login and refresh tokens
- Role-based access control (User, Admin)
- User management (view and update roles)
- Responsive UI with Ant Design

# Tech Stack
# Frontend
- Next.js: React framework for server-side rendering and routing
- Ant Design: UI component library for forms, menus, and layouts
- Axios: HTTP client for API requests
- TypeScript: Typed JavaScript for better code quality

# Backend
- ASP.NET Core: Web framework for building APIs
- Microsoft Identity: Authentication and authorization framework
- Entity Framework Core: ORM for database operations
- SQL Server: Relational database for user data
- MailKit: Library for sending email confirmations
- JWT: JSON Web Tokens for secure authentication

# Prerequisites
- Node.js (v18 or higher)
- .NET SDK (v8 or higher)
- SQL Server

