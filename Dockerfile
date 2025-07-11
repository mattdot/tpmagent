# Use a simpler approach with a prebuilt .NET image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory
WORKDIR /app

# Copy source files
COPY src/ ./src/

# Build the application
WORKDIR /app/src
RUN dotnet restore --no-cache
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

# Create runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0

# Install TPM tools
RUN apt-get update && apt-get install -y \
    tpm2-tools \
    && rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# Copy entrypoint script
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Set the entrypoint
ENTRYPOINT ["/entrypoint.sh"]