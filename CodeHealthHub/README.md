## Running CodeHealthHub with Docker

### Prerequisites
- Docker and Docker Compose installed

### Quick Start (SQLite - Default)
1. Clone the repository
2. Run `docker-compose up -d`
3. Access at http://localhost:5000

### Using SQL Server
1. Set environment variables in .env file:
    - e.g. DATABASE_CONNECTION_STRING="Server=localhost;Database=codehealthhub;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
    - DATABASE_PROVIDER="sqlserver"
2. Run `docker-compose up -d`

## Adding SonarQube Instances to CodeHealthHub

### Prerequisites
- SonarQube instance running

### If running in local container
1. Get IP address of instance. If container is named sonarqube:
    - Run `docker inspect sonarqube | Select-String IPAddress` on powershell
    - Run `docker inspect sonarqube | grep IPAddress` on linux
2. Get authentication token for the instance
3. Add connection details
![alt text](Add-Instance-Screenshot.png)
    - Adding will send a ping to the SonarQube, if there is no pong response a error message will pop-up

### If running in another server on the network
1. Get authentication token for the instance
2. Add connection details
![alt text](Add-Instance-Screenshot.png)
    - Adding will send a ping to the SonarQube, if there is no pong response a error message will pop-up

## Populating Database for Dashboard
1. Click 'Projects' in navigation sidebar
2. Click 'Refresh Project Data' button above table
3. Click 'Issues' in navigation sidebar
4. Click 'Refresh Issues Data' button above table
5. Click 'Dashboard' in navigation sidebar