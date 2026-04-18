#!/bin/bash

# Quick start script for Video Processor SaaS
# Supports both Docker and Podman

set -e

COMPOSE_CMD="docker-compose"

# Check if podman-compose is available
if command -v podman-compose &> /dev/null; then
    COMPOSE_CMD="podman-compose"
    echo "Using podman-compose"
elif command -v docker-compose &> /dev/null; then
    COMPOSE_CMD="docker-compose"
    echo "Using docker-compose"
else
    echo "Error: Neither docker-compose nor podman-compose found"
    exit 1
fi

echo ""
echo "🚀 Starting Video Processor SaaS..."
echo ""

$COMPOSE_CMD up --build

echo ""
echo "✅ Services started!"
echo ""
echo "Frontend: http://localhost:3000"
echo "Backend:  http://localhost:5000"
echo "API Docs: http://localhost:5000/swagger"
echo ""
echo "To stop: Press Ctrl+C"
