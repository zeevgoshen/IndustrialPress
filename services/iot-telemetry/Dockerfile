FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN rm -rf /root/.nuget/packages && dotnet restore services/iot-telemetry/src/IotTelemetry/IotTelemetry.csproj --disable-parallel
RUN dotnet publish services/iot-telemetry/src/IotTelemetry/IotTelemetry.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IotTelemetry.dll"]
