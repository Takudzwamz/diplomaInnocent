# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["StorefrontRazor/StorefrontRazor.csproj", "StorefrontRazor/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Core/Core.csproj", "Core/"]
RUN dotnet restore "StorefrontRazor/StorefrontRazor.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/StorefrontRazor"
RUN dotnet build "StorefrontRazor.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "StorefrontRazor.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- Final Stage ---
# Use the smaller ASP.NET runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
COPY --from=publish /app/publish .

# --- START: UPDATED INSTRUCTIONS ---
# 1. Set an environment variable to tell ASP.NET Core to listen on port 80
ENV ASPNETCORE_URLS=http://+:80

# 2. Expose port 80 in the Docker image metadata
EXPOSE 80
# --- END: UPDATED INSTRUCTIONS ---

ENTRYPOINT ["dotnet", "StorefrontRazor.dll"]