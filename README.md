# Sales Web System

## Introduction

This project is a full-stack application with a backend service developed in C# using .NET 7 and a frontend built with Angular 18. The backend uses EntityFramework Core 7 for object-relational mapping and SQL Server as the database. The system is hosted on AWS and features scheduled tasks, caching, and other performance optimizations.

## Technologies Used

- **Backend**: C#, .NET 7
- **Frontend**: Angular 18
- **Database**: SQL Server, EntityFramework Core 7
- **Cloud**: AWS (Amazon Web Services)
- **Caching**: Output Caching in .NET
- **Task Scheduling**: Automated periodic tasks (e.g., background jobs)
- **Authentication**: JWT-based authentication

## Features

- **Sales management**: Manage products, customers, and transactions.
- **Predictive analytics**: Use ML.NET for sales forecasting based on historical data.
- **Automated tasks**: Scheduled tasks for data backup, report generation, and cache invalidation.
- **Caching**: Improves response times using Output Caching in .NET.
- **Responsive UI**: A modern web interface built with Angular 18 and Material Design.
- **AWS deployment**: The application is deployed and hosted in AWS, taking advantage of its scalability and security.

## Getting Started

### Prerequisites

- **Backend**:
  - .NET 7 SDK
  - EntityFramework Core 7
  - SQL Server
- **Frontend**:
  - Node.js (v16+)
  - Angular CLI (v18)
- **AWS**:
  - AWS CLI configured with appropriate permissions
  - AWS account for hosting and deployment

### Installation

1. Clone the repository:
   ```sh
   git clone https://github.com/Gerson-Solano23/SalesSystemWeb.git
