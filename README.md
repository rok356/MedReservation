# MedReservation

Demonstration of an ASP.NET Web API application built using Entity Framework and Microsoft SQL Server for database management. This repository showcases the implementation of a robust web API with features like appointment management, patient-doctor interactions, and more. Explore the code to see how ASP.NET Web API and Entity Framework are utilized to create a seamless experience for users, with a focus on efficient data handling and secure communication. Dive into the MSSQL database schema to understand the structure supporting the application's functionalities.

## Configuration

### Configuring appsettings.json:

1. Open the `appsettings.json` file located in the project's root directory.
2. Find the line `"DefaultConnection": "Server=localhost;Database=MedReservation;Trusted_connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"`.
3. Update the connection string with your own server and database name, and any other settings as needed.

## Database Setup

### Updating the Database:

1. Open the Package Manager Console in Visual Studio.
2. Execute the command `update-database`.
3. This script from the "Migration" folder will create the necessary tables in the configured database.

## Adding Initial Data to the Database
For the initial setup, I've included a script to populate some sample data into the tables. This script will automatically run the first time the application is launched, ensuring the database is populated with necessary data for testing and development purposes.
