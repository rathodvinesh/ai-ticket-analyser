FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AI-ticket-analyzer.csproj", "./"]
RUN dotnet restore "AI-ticket-analyzer.csproj"

COPY . .
RUN dotnet build "AI-ticket-analyzer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AI-ticket-analyzer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AI-ticket-analyzer.dll"]
