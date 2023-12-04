FROM mcr.microsoft.com/dotnet/sdk:8.0.100 AS build-env

WORKDIR /src


# Copy application (API)
COPY ./*.csproj .

# Restore nuget packages
RUN dotnet restore *.csproj

# Copy all the source code and build
COPY . .

# Build and publish the application. Used the "--no-restore" and "--no-build" to benefit the layer caches
RUN dotnet build --configuration Release *.csproj
RUN dotnet publish *.csproj --configuration Release --no-build --no-restore -o /app



FROM mcr.microsoft.com/dotnet/aspnet:8.0.0 AS final-env

WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT Production
ENV ASPNETCORE_URLS http://*:80

COPY --from=build-env /app .

ENTRYPOINT ["dotnet", "Demo.Api.dll"]