# AuthApi

AuthApi is a secure authentication API built with .NET Core Web API and PostgreSQL. It provides essential authentication functionalities such as user registration, login, role management, and more.

## Getting Started

To get started with AuthApi, follow these steps:

### 1. Clone the Repository

Clone the AuthApi repository to your local machine:

```bash
git clone https://github.com/Sadek00/AuthApi.git



###  3. Update Connection String
In the appsettings.json file, update the connection string to point to your PostgreSQL database:

json
Copy code
"ConnectionStrings": {
  "DefaultConnection": "Your_PostgreSQL_Connection_String_Here"
}
### 4. Apply Migrations
Run the following commands to apply Entity Framework Core migrations and create the necessary database schema:

bash
Copy code
cd AuthApi
dotnet ef database update
### 5. Build and Run the Application
Build and run the AuthApi application using the following commands:

bash
Copy code
dotnet build
dotnet run
The API will start running on https://localhost:5001 (or http://localhost:5000).

### Usage
Once the application is running, you can use tools like Postman or curl to interact with the API endpoints for user registration, login, role management, etc.

Example Requests
Register User
http
Copy code
POST https://localhost:5001/api/account/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "P@ssw0rd",
  "confirmPassword": "P@ssw0rd"
}
Login
http
Copy code
POST https://localhost:5001/api/account/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "P@ssw0rd"
}
### API Documentation
For detailed API documentation, you can use Swagger UI. Once the application is running, navigate to https://localhost:5001/swagger to explore and interact with the API endpoints.

### Contributing
Contributions are welcome! If you find any issues or have suggestions for improvements, please open an issue or submit a pull request.
