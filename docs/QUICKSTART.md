# BahyWay Website - Quick Start Guide

Get the BahyWay platform running in 5 minutes!

## Prerequisites

- Docker and Docker Compose installed
- Git installed
- 4GB+ RAM available

## Step 1: Clone and Configure

```bash
# Clone the repository
git clone https://github.com/bahyway/BahyWay.git
cd BahyWay

# Copy environment file
cp .env.website .env

# IMPORTANT: Edit .env and change passwords!
nano .env  # or use your favorite editor
```

## Step 2: Start the Platform

```bash
# Build and start all services
docker-compose -f docker-compose.website.yml up --build
```

This will start:
- PostgreSQL with Apache AGE
- Redis
- Users Service (FastAPI)
- Projects Service (FastAPI)
- News Service (FastAPI)
- Frontend (Next.js)
- Nginx reverse proxy

## Step 3: Access the Platform

Wait for all services to be healthy (check the logs), then access:

- **Frontend:** http://localhost:3000
- **Users API Docs:** http://localhost:8000/docs
- **Projects API Docs:** http://localhost:8001/docs
- **News API Docs:** http://localhost:8002/docs

## Step 4: Test the API

### Register a User

```bash
curl -X POST "http://localhost:8000/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "username": "testuser",
    "password": "testpass123",
    "full_name": "Test User"
  }'
```

### Login

```bash
curl -X POST "http://localhost:8000/auth/login" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=testuser&password=testpass123"
```

Copy the `access_token` from the response.

### Get Your Profile

```bash
curl -X GET "http://localhost:8000/auth/me" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

## Stopping the Platform

```bash
# Stop all services
docker-compose -f docker-compose.website.yml down

# Stop and remove volumes (database data)
docker-compose -f docker-compose.website.yml down -v
```

## Next Steps

1. Explore the API documentation at http://localhost:8000/docs
2. Check out the frontend at http://localhost:3000
3. Read the full documentation in `WEBSITE-README.md`
4. Start developing your features!

## Troubleshooting

**Services won't start?**
- Check if ports 3000, 8000, 8001, 8002, 5432, 6379 are available
- Check Docker logs: `docker-compose -f docker-compose.website.yml logs`

**Database connection error?**
- Wait 30 seconds for PostgreSQL to fully initialize
- Check logs: `docker-compose -f docker-compose.website.yml logs postgres`

**Frontend can't connect to API?**
- Check that all backend services are running: `docker-compose -f docker-compose.website.yml ps`

## Need Help?

See the full documentation: [WEBSITE-README.md](./WEBSITE-README.md)
