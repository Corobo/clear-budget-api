# Clear Budget API

Clear Budget API is a backend service designed to manage personal finance operations, including transaction tracking, category management, and financial reporting. Built with a microservices architecture using .NET technologies, it offers a scalable and maintainable solution for budgeting applications.

## 📚 Table of Contents

- [Technologies Used](#technologies-used)
- [Microservices Architecture](#microservices-architecture)
- [Installation and Setup](#installation-and-setup)
- [Running the Services](#running-the-services)
- [Running Tests](#running-tests)
- [Contributing](#contributing)
- [License](#license)
- [Author](#author)

## ⚙️ Technologies Used

- **.NET 8** — Core framework for building the services.
- **ASP.NET Core** — Web API development.
- **Docker & Docker Compose** — Containerization and orchestration.
- **RabbitMQ** — Event-based messaging between services.
- **Entity Framework Core** — ORM for PostgreSQL and SQLite.
- **Testcontainers** — Integration testing with disposable containers.
- **Serilog** — Centralized logging for all services.

## 🧱 Microservices Architecture

Clear Budget API consists of the following microservices:

- `APIGateway`: Routes incoming HTTP requests to other services.
- `CategoriesService`: Manages global and user-specific categories.
- `TransactionsService`: Handles user transactions and links them to categories.
- `ReportingService`: Provides aggregated, read-only financial reports.
- `Shared.Messaging`: Shared project for RabbitMQ event infrastructure.

Each service follows a clean architecture with:

```
- Controllers/
- Models/
  - DTO/
- Repositories/
  - Data/
  - Impl/
- Services/
  - Impl/
- Profiles/             # For AutoMapper
- Data/                 # DbContext and seed logic
- Configuration/        # Service-specific options and startup config
```

✅ JWT Authentication and role-based authorization  
✅ CORS configuration per environment  
✅ Integration tests with real PostgreSQL and RabbitMQ  
✅ Docker Compose for full environment setup

## 🛠️ Installation and Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker](https://www.docker.com/)
- [RabbitMQ (via Docker)](https://www.rabbitmq.com/)
- PostgreSQL (optional, handled via Docker in development)

### 1. Clone the repository

```bash
git clone https://github.com/Corobo/clear-budget-api.git
cd clear-budget-api
```

### 2. Environment configuration

Configure your environment variables via `appsettings.json` or `.env` files if needed for:

- JWT Authority and Audience
- PostgreSQL connection strings
- RabbitMQ settings

### 3. Restore dependencies

```bash
dotnet restore
```

## 🚀 Running the Services

You can run each service individually:

```bash
cd CategoriesService
dotnet run
```

## 🧪 Running Tests

Each microservice includes:

- **Unit Tests** using xUnit and Moq
- **Integration Tests** using Testcontainers and WebApplicationFactory

Example for running tests:

```bash
cd CategoriesService.Tests
dotnet test
```

## 🤝 Contributing

Contributions are welcome! To contribute:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin feature/your-feature-name`
5. Open a Pull Request

Please write clear commit messages and include appropriate tests.

## 🪪 License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

## 👤 Author

Made with ❤️ by **Adrián Jiménez Villarreal**  
[GitHub Profile](https://github.com/Corobo)
