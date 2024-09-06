# Use the official ASP.NET 8.0 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the official .NET SDK 8.0 image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project files
COPY ["ProjectMonitor/ProjectMonitor.csproj", "ProjectMonitor/"]

# Restore dependencies
RUN dotnet restore "ProjectMonitor/ProjectMonitor.csproj"

# Copy the rest of the application files
COPY . .
COPY ./ProjectMonitor/static ./app/static

# Build the application
WORKDIR "/src/ProjectMonitor"
RUN dotnet build "ProjectMonitor.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "ProjectMonitor.csproj" -c Release -o /app/publish

# Use the base image to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjectMonitor.dll"]
