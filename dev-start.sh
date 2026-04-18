#!/bin/bash

# Development startup script
# Starts both backend and frontend in development mode

set -e

COMPOSE_CMD="docker-compose"

if command -v podman-compose &> /dev/null; then
    COMPOSE_CMD="podman-compose"
fi

echo "🚀 Starting development environment..."

$COMPOSE_CMD -f docker-compose.dev.yml up

echo "Development environment started!"
echo "Frontend: http://localhost:3000"
echo "Backend:  http://localhost:5000"
