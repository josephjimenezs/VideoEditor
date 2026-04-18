#!/bin/bash

# Docker build script for backend
# This script builds the .NET application

set -e

cd backend

echo "Restoring dependencies..."
dotnet restore

echo "Building application..."
dotnet build --configuration Release

echo "Publishing application..."
dotnet publish --configuration Release --output bin/publish --no-build

echo "✅ Backend build complete!"
