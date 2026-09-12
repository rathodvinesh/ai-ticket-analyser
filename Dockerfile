# Stage 1: Build Next.js Frontend
FROM node:20-alpine AS frontend-build
WORKDIR /frontend
COPY frontend/package*.json ./
RUN npm install
COPY frontend/ ./
RUN npm run build

# Stage 2: Build ASP.NET Core API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AI-ticket-analyzer.csproj", "./"]
COPY ["nuget.config", "./"]
RUN dotnet restore "AI-ticket-analyzer.csproj"

COPY . .
RUN dotnet build "AI-ticket-analyzer.csproj" -c Release -o /app/build

# Stage 3: Publish API & Copy Frontend Assets to wwwroot
FROM build AS publish
RUN dotnet publish "AI-ticket-analyzer.csproj" -c Release -o /app/publish /p:UseAppHost=false
COPY --from=frontend-build /frontend/out /app/publish/wwwroot

# Stage 4: Runtime Environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AI-ticket-analyzer.dll"]
