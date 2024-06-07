OptimazedCvStorage API

This API allows you to manage CVs (Curriculum Vitae) by providing endpoints for creating, reading, updating, and deleting CV data.

Technologies Used

    ASP.NET Core 3.1
    Entity Framework Core
    MySQL
    dotenv.net (for loading environment variables)

Getting Started

Prerequisites

    .NET Core SDK installed on your machine.
    MySQL Server installed and running.

Installation

    Clone the repository:

    git clone https://github.com/your-username/OptimazedCvStorage.git

    Navigate to the project directory:

    cd OptimazedCvStorage

    Create a .env file in the root directory and add your MySQL connection string:

    OptimazedCvStorage=YourConnectionStringHere

    Restore dependencies and build the project:

    dotnet restore

    Run the database migrations to create the necessary tables:

    dotnet ef database update

    Run the application:

    dotnet run

    The API should now be running and accessible at http://localhost:5000.

Endpoints

    GET /api/users: Retrieve all CVs stored in the database.
    POST /api/users: Create a new CV.
    PUT /api/users/{id}: Update an existing CV.
    DELETE /api/users/{id}: Delete a CV by its ID.
    DELETE /api/users/deleteAll: Delete all CVs and reset identity columns.
