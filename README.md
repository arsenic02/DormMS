<div align="center">

# 🏢 DormMS – Dormitory Management & Services System

<p align="center">
  <b>A modern web application for managing application contests, room allocations, student ranking, and daily dormitory services.</b>
</p>

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)](https://react.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-18-4169E1?logo=postgresql)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

</div>

---

## 📋 Table of Contents

- [About The Project](#-about-the-project)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Core Modules & Features](#-core-modules--features)
  - [1. Admissions & Room Allocation](#1-admissions--room-allocation)
  - [2. Daily Dormitory Services](#2-daily-dormitory-services)
- [Getting Started](#-getting-started)
- [Prerequisites](#prerequisites)
- [License](#-license)

---

## 📖 About The Project

**DormMS** (Dormitory Management System) is a comprehensive software solution built to digitize and automate the entire lifecycle of student accommodation. It handles the complete admission process—from application contests and automated student ranking to room assignment—as well as daily resident services such as laundry machine booking, room swaps, and check-outs.

---

## ⚙️ Tech Stack

### **Backend**
* **C# / .NET 10 Web API** – RESTful web service architecture
* **Entity Framework Core** – Object-Relational Mapping (ORM)
* **PostgreSQL 18** – Relational database with `pgvector` extension support

### **Frontend**
* **React 19** – Component-based UI framework (powered by Vite)
* **CSS / Tailwind CSS** – Responsive and modern styling

---

## 🏗 Architecture

The project is structured as a 3-tier application divided into distinct modules:

1. **Frontend (`DormMS.Client`)**: React SPA handling user interfaces, application forms, and service booking dashboards.
2. **Backend (`DormMS.Server`)**: ASP.NET Core REST API providing endpoints for rankings, room management, and student service operations.
3. **Database (`DormMS.Database`)**: Entity Framework Core data context, schema migrations, and vector storage.

---

## ✨ Core Modules & Features

### 1. Admissions & Room Allocation
* 📢 **Contest & Application Management** – Creation and administration of dormitory placement contests with online student applications.
* 📊 **Student Ranking System** – Automated calculation of points and ranking lists based on academic success and student criteria.
* 🔑 **Room Allocation** – Transparent room assignment and capacity tracking based on placement results.

### 2. Daily Dormitory Services
* 🧺 **Laundry Room Booking** – Scheduling system for reserving washing machines and dryers.
* 🔄 **Room Swaps** – Managed workflow for students to request and execute room exchanges.
* 🚪 **Check-out / Departure** – Streamlined process for student departures and room clearance.
* 🔍 **Vector-Powered Search** – Fast contextual querying across system data using PostgreSQL vector embeddings.

---

## 🚀 Getting Started

### Prerequisites

Make sure you have the following installed on your machine:
* [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
* [Node.js](https://nodejs.org/) (v18 or higher)
* [PostgreSQL 18](https://www.postgresql.org/download/)

---

## 📜 License

Distributed under the MIT License. See [LICENSE](LICENSE) for more information.
