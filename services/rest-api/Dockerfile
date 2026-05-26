FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN rm -rf /root/.nuget/packages && dotnet restore services/rest-api/src/RestApi/RestApi.csproj --disable-parallel
RUN dotnet publish services/rest-api/src/RestApi/RestApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "RestApi.dll"]
