## Running CodeHealthHub with Docker

### Prerequisites
- Docker and Docker Compose installed

### Quick Start (SQLite - Default)
1. Clone the repository
2. Run `docker-compose up -d`
3. Access at http://localhost:5000

### Using SQL Server
1. Set environment variables:
    - e.g. DATABASE_CONNECTION_STRING="Server=localhost;Database=codehealthhub;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
    - DATABASE_PROVIDER="sqlserver"
2. Run `docker-compose up -d`