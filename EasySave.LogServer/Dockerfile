# Use the .NET SDK image to build the project (Updated to 10.0)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project file and download dependencies.
COPY ["EasySave.LogServer.csproj", "./"]
RUN dotnet restore "EasySave.LogServer.csproj"

# Copy all source files and build the release.
COPY . .
RUN dotnet publish "EasySave.LogServer.csproj" -c Release -o /app/publish

# Use the ASP.NET runtime image for the final container (Updated to 10.0)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Define a volume to save logs on the host machine.
VOLUME /app/CentralizedLogs

# Open port 8080 for network traffic.
EXPOSE 8080

# Command to run the server.
ENTRYPOINT ["dotnet", "EasySave.LogServer.dll"]