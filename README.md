# AI Ticket Analyzer & Classification System

An intelligent, real-time support ticket classification and summarization system powered by **ASP.NET Core 8 Web API**, **Groq Llama-3.3 70B AI**, and a high-performance **Next.js 14 (React, Tailwind CSS, Shadcn UI)** dashboard.

---

## ✨ Features

- **⚡ Real-Time Neural Classification**: Classifies incoming tickets into Categories (*Bug*, *Billing*, *Login*, *Feature Request*, *Other*) and Priority Levels (*High*, *Medium*, *Low*) in under 500ms using Groq AI.
- **🎨 Glassmorphic Dashboard**: Dark mode UI featuring glowing telemetry stats, active response gauges, copy-to-clipboard utilities, and prompt template chips.
- **🗄️ SQL Server Persistence & Live Stream**: Automatically saves and reads tickets from SQL Server Database.
- **🚀 1-Click Render Deployment**: Docker containerization and `render.yaml` Blueprint setup included.

---

## 🛠️ Tech Stack

- **Backend**: ASP.NET Core 8 Web API, Entity Framework Core 8, SQL Server.
- **AI Model**: Groq Cloud Platform (`llama-3.3-70b-versatile`).
- **Frontend**: Next.js 14 (App Router), TypeScript, Tailwind CSS, Lucide React, Framer Motion.
- **Hosting**: Docker, Render.

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
