# Basic Dockerfile for FootballTrackerApi
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Copy published output from build (assume build done externally)
COPY ./bin/Debug/net9.0/ ./

# Accept SQL connection string as environment variable
ENV ConnectionStrings__DefaultConnection=""

# Expose default ASP.NET port
EXPOSE 80

# Start the API, passing environment variable
ENTRYPOINT ["dotnet", "BE.dll"]
