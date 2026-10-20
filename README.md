<div align="center">

# 🏢 DormMS – Dormitory Management & Services System

<p align="center">
  <b>A modern web application for automating administration, room allocation, and student services in dormitories.</b>
</p>

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)](https://react.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

</div>

---

## 📋 Table of Contents

- [About The Project](#-about-the-project)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Features](#-features)
- [Getting Started](#-getting-started)
  - [Prerequisites](#prerequisites)
  - [1. Clone the Repository](#1-clone-the-repository)
  - [2. Backend & Database Setup](#2-backend--database-setup)
  - [3. Frontend Setup](#3-frontend-setup)
- [Project Structure](#-project-structure)
- [License](#-license)

---

## 📖 About The Project

**DormMS** (Dormitory Management System) is a comprehensive software solution designed to streamline and automate dormitory management. It simplifies administrative workflows, tracks student accommodation and services, provides multi-role authentication, and leverages vector-based search for efficient data retrieval.

---

## ⚙️ Tech Stack

### **Backend**
* **C# / .NET 8 Web API** – RESTful web service architecture
* **Entity Framework Core** – Object-Relational Mapping (ORM)
* **PostgreSQL** – Relational database with `pgvector` extension support

### **Frontend**
* **React** – Component-based UI framework (powered by Vite)
* **CSS / Tailwind CSS** – Responsive and modern styling

---

## 🏗 Architecture

The project is structured as a 3-tier system divided into distinct modules:

1. **Frontend (`DormMS.Client`)**: React SPA handling UI rendering and client-side logic.
2. **Backend (`DormMS.Server`)**: ASP.NET Core REST API handling business logic and API endpoints.
3. **Database (`DormMS.Database`)**: Entity Framework Core data context, models, and migrations.

---

## ✨ Features

* 🎓 **Student & Accommodation Management** – Efficient tracking of student profiles, room allocations, and capacity.
* 🛠 **Maintenance & Services** – Issue reporting, room service requests, and maintenance tracking.
* 🔒 **Authentication & Authorization** – Secure role-based access control (Admin, Student, Staff).
* 🔍 **Smart Vector Search** – Fast and context-aware query processing using PostgreSQL vector embeddings.

---

## 🚀 Getting Started

### Prerequisites

Make sure you have the following installed on your machine:
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Node.js](https://nodejs.org/) (v18 or higher)
* [PostgreSQL](https://www.postgresql.org/download/)

---

### 1. Clone the Repository

```bash
git clone [https://github.com/arsenic02/DormMS.git](https://github.com/arsenic02/DormMS.git)
cd DormMS
