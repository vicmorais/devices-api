
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY nuget.config .


COPY src/Devices.API/Devices.API.csproj src/Devices.API/
COPY src/Devices.Application/Devices.Application.csproj src/Devices.Application/
COPY src/Devices.Domain/Devices.Domain.csproj src/Devices.Domain/
COPY src/Devices.Infrastructure/Devices.Infrastructure.csproj src/Devices.Infrastructure/

RUN dotnet restore src/Devices.API/Devices.API.csproj


COPY src/ src/
RUN dotnet publish src/Devices.API/Devices.API.csproj -c Release -o /app/publish



FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app


COPY --from=build /app/publish .


EXPOSE 8080


ENTRYPOINT ["dotnet", "Devices.API.dll"]