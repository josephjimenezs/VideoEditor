#!/bin/bash
set -e

echo "Building Video Processor SaaS..."

# Build backend
echo "Building backend..."
cd backend
dotnet restore
dotnet build --configuration Release
dotnet publish --configuration Release --output bin/publish
cd ..

# Build frontend
echo "Building frontend..."
cd frontend
npm install
npm run build
cd ..

echo "Build complete!"
echo ""
echo "To run with Docker:"
echo "  podman-compose up --build"
echo ""
echo "To run locally:"
echo "  Backend:  cd backend && dotnet run"
echo "  Frontend: cd frontend && npm run dev"
