#!/bin/bash

# Stop all containers and clean up volumes

set -e

COMPOSE_CMD="docker-compose"

# Check if podman-compose is available
if command -v podman-compose &> /dev/null; then
    COMPOSE_CMD="podman-compose"
fi

echo "Stopping and removing containers and volumes..."
$COMPOSE_CMD down -v

echo "Cleaning up build artifacts..."
rm -rf backend/bin backend/obj
rm -rf frontend/dist frontend/node_modules

echo "✅ Cleanup complete!"
