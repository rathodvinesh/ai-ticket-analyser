# AI-Assisted Support Ticket Triage
.NET 8 · EF Core · Next.js · MS SQL Server · Groq LLM

## Overview

This project demonstrates a production-oriented backend system that uses an LLM (Groq) to enrich support tickets while keeping SQL Server as the system of record.

The system is intentionally not a chat application.
Instead, it performs deterministic AI enrichment on stored data:

- Summarizes support tickets
- Assigns a category
- Assigns a priority

The focus is on scalable backend architecture, database-driven queries, and safe AI integration.

---

## Problem Statement

Support teams receive large volumes of unstructured tickets:

- Poorly written titles
- Long descriptions
- Missing category
- Missing priority

This slows down ticket routing and response times.

Many AI-based solutions:
- Load entire datasets into memory
- Perform AI calls inline with requests
- Treat AI output as authoritative

This project avoids those issues by design.

---

## Solution Approach

The system is designed as an AI enrichment pipeline, not a conversational system.

1. Raw tickets are stored in SQL Server
2. AI is triggered explicitly to enrich existing records
3. AI output is validated and constrained
4. All filtering, grouping, and aggregation happens in the database

AI improves data quality, but never replaces database authority.

---\

## 🛠️ Tech Stack

- **Backend**: ASP.NET Core 8 Web API, Entity Framework Core 8, SQL Server.
- **AI Model**: Groq Cloud Platform (`llama-3.3-70b-versatile`).
- **Frontend**: Next.js 14 (App Router), TypeScript, Tailwind CSS, Lucide React, Framer Motion.
- **Hosting**: Docker, Render.

---

## Architecture

Controller  
    ↓  
Query / AI Services  
    ↓  
EF Core  
    ↓  
SQL Server (Source of Truth)

### Core Principles

- Thin controllers
- Explicit services
- Database-first querying
- AI as a dependency, not a decision-maker

---

## Tech Stack

- .NET 8 Web API
- EF Core
- MS SQL Server
- Groq LLM (OpenAI-compatible API)
- HttpClientFactory
- Dependency Injection

---

## Database Design

### SupportTicket Table

| Column | Description |
|------|------------|
| TicketId | Primary key |
| RawTitle | Original user title |
| RawDescription | Original user description |
| CreatedAt | UTC timestamp |
| AI_Summary | AI-generated summary |
| AI_Category | AI-generated category |
| AI_Priority | AI-generated priority |
| AI_ProcessedAt | AI processing timestamp |

### Database Optimizations

- Filtered index for unprocessed tickets
- Covering index for category-based queries
- Check constraints to validate AI output
- Snapshot isolation to prevent read/write blocking

---

## AI Integration (Groq)

Groq is used as an external LLM inference provider via an OpenAI-compatible API.

### AI Responsibilities

- Generate a one-sentence summary
- Assign a category from a fixed list
- Assign a priority from a fixed list

### AI Safety Measures

- Deterministic prompts
- Temperature set to 0
- Strict JSON output format
- Validation in application code
- Enforcement via SQL check constraints

AI output is treated as untrusted input.

---

## Mock AI Support

The AI integration is abstracted behind an interface.

- Real Groq implementation for production
- Deterministic mock implementation for development
- Toggle via configuration

This allows:
- Zero-cost local development
- Faster testing
- Stable and predictable behavior

---

## API Endpoints

### Overview

The API exposes a small, focused set of endpoints designed around
**explicit state changes**, **database-driven queries**, and **controlled AI enrichment**.

---

### Ticket Management

| Method | Endpoint | Description |
|------|---------|-------------|
| POST | `/api/tickets` | Create a new support ticket with raw user input |
| POST | `/api/tickets/{id}/process-ai` | Enrich an existing ticket using AI (summary, category, priority) |

---

### Ticket Queries & Reporting

| Method | Endpoint | Description |
|------|---------|-------------|
| GET | `/api/tickets/high-priority-count` | Returns count of tickets marked as **High** priority |
| GET | `/api/tickets/category/{category}` | Returns tickets filtered by category |

---

## Query Design Philosophy

All aggregation and filtering is executed at the database level.

Why this matters:
- Prevents loading full tables into memory
- Enables index usage
- Reduces network overhead
- Improves scalability

Move computation as close to the data as possible.

EF Core is used as a controlled ORM, not an abstraction leak.

---

## What This Project Intentionally Avoids

- Chat-based UI
- In-memory filtering
- AI-driven business rules
- Over-engineering
- Hidden ORM behavior

---

## Scalability Considerations

- Idempotent AI processing
- Index-driven queries
- Mockable external dependencies
- Clean separation of concerns

The architecture supports:
- Background AI processing
- Queue-based workflows
- Pagination
- Reporting dashboards

---

## How to Run

1. Configure SQL Server connection string
2. Add Groq API key (or enable mock AI)
3. Run EF Core migrations
4. Start the API

---

## Future Improvements

- Background worker for AI processing
- Pagination for ticket queries
- Category-level analytics
- Distributed caching
- Authentication and authorization

---

## 🚀 Quick Start

### 1. Run ASP.NET Core Backend
```bash
dotnet run
```
*(Runs on `http://localhost:5069`)*

### 2. Run Next.js Frontend
```bash
cd frontend
npm install
npm run dev
```
*(Runs on `http://localhost:3000`)*


## Final Notes

This project is intentionally backend-focused.
It demonstrates real-world AI integration, database optimization, and clean architecture.

---
