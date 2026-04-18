FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["VideoProcessor.csproj", "./"]
RUN dotnet restore "VideoProcessor.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "VideoProcessor.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "VideoProcessor.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install ffmpeg
RUN apt-get update && apt-get install -y \
    ffmpeg \
    && rm -rf /var/lib/apt/lists/*

# Create data directory with proper permissions
RUN mkdir -p /data && chown -R app:app /data && chmod -R 755 /data

# Copy entrypoint script
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Copy published files with correct ownership
COPY --from=build --chown=app:app /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["/app/entrypoint.sh"]
CMD ["dotnet", "VideoProcessor.dll"]
