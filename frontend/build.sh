#!/bin/bash

# Docker build script for frontend
# This script builds the React application

set -e

cd frontend

echo "Installing dependencies..."
npm ci

echo "Building application..."
npm run build

echo "✅ Frontend build complete!"
