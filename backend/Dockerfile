# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TaskFlow.sln .
COPY src/TaskFlow.Domain/TaskFlow.Domain.csproj            src/TaskFlow.Domain/
COPY src/TaskFlow.Application/TaskFlow.Application.csproj  src/TaskFlow.Application/
COPY src/TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj src/TaskFlow.Infrastructure/
COPY src/TaskFlow.API/TaskFlow.API.csproj                  src/TaskFlow.API/

RUN dotnet restore

COPY . .
RUN dotnet publish src/TaskFlow.API/TaskFlow.API.csproj -c Release -o /app/publish --no-restore

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskFlow.API.dll"]
