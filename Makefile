.PHONY: help build up down logs clean backend-build frontend-build

help:
	@echo "Video Processor SaaS - Available Commands"
	@echo ""
	@echo "  make up              - Start containers with docker-compose"
	@echo "  make down            - Stop containers"
	@echo "  make build           - Build images"
	@echo "  make logs            - Show container logs"
	@echo "  make clean           - Remove containers and volumes"
	@echo "  make backend-build   - Build backend only"
	@echo "  make frontend-build  - Build frontend only"

up:
	docker-compose up --build

down:
	docker-compose down

build:
	docker-compose build

logs:
	docker-compose logs -f

clean:
	docker-compose down -v
	rm -rf backend/bin backend/obj
	rm -rf frontend/dist frontend/node_modules

backend-build:
	cd backend && dotnet restore && dotnet build --configuration Release

frontend-build:
	cd frontend && npm install && npm run build

dev-backend:
	cd backend && dotnet run

dev-frontend:
	cd frontend && npm run dev
