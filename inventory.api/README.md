# dotnet-webapi

This is a .NET Core Web API project that provides weather forecast data. 

## Project Structure

- **dotnet-webapi.sln**: Solution file that organizes the project and its dependencies.
- **src/dotnet-webapi**: Contains the main application code.
  - **Controllers**: Contains API controllers.
    - **WeatherForecastController.cs**: Handles HTTP requests related to weather forecasts.
  - **Models**: Contains data models.
    - **WeatherForecast.cs**: Represents the weather data model.
  - **Properties**: Contains application properties.
    - **launchSettings.json**: Settings for launching the application.
  - **appsettings.json**: Configuration settings for the application.
  - **appsettings.Development.json**: Development-specific configuration settings.
  - **Program.cs**: Entry point of the application.
  - **Startup.cs**: Configures services and the application's request pipeline.
  - **dotnet-webapi.csproj**: Project file defining dependencies and settings.
- **tests/dotnet-webapi.Tests**: Contains unit tests for the application.
  - **dotnet-webapi.Tests.csproj**: Project file for the test project.
  - **WeatherForecastTests.cs**: Unit tests for the WeatherForecast model and controller.
- **.gitignore**: Specifies files and directories to ignore in version control.
- **Dockerfile**: Instructions for building a Docker image for the application.

## Setup Instructions

1. Clone the repository:
   ```
   git clone <repository-url>
   cd dotnet-webapi
   ```

2. Restore dependencies:
   ```
   dotnet restore
   ```

3. Run the application:
   ```
   dotnet run --project src/dotnet-webapi/dotnet-webapi.csproj
   ```

4. Access the API at `http://localhost:5000/weatherforecast`.

## Usage

The API provides a single endpoint to retrieve weather forecast data. You can send a GET request to `/weatherforecast` to receive a list of weather forecasts. 

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any improvements or bug fixes.