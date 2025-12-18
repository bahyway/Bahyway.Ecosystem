# BahyWay Website Platform

![BahyWay Logo](./docs/bahyway-website/images/logo/01_The_Original.png)

A modern, full-stack web application combining a corporate website, community platform, and news aggregation service.

## Overview

**BahyWay** is a multi-faceted platform that serves as:
- **Corporate Website:** Professional presence for the BahyWay company
- **Community Platform:** Member profiles, projects showcase, and discussions
- **News Agency:** Aggregated news from multiple sources and subscriber feeds
- **Knowledge Hub:** Projects documentation with feedback and collaboration features

## Technology Stack

### Backend
- **FastAPI** - Modern, fast Python web framework
- **PostgreSQL** - Robust relational database with Apache AGE extension
- **Apache AGE** - Graph database for knowledge graphs
- **Redis** - Caching and Celery task queue
- **SQLAlchemy** - ORM for database operations
- **Pydantic** - Data validation
- **JWT** - Secure authentication

### Frontend
- **Next.js 14** - Production-grade React framework
- **React 18** - Component-based UI library
- **TypeScript** - Type-safe JavaScript
- **Tailwind CSS** - Utility-first CSS framework
- **Axios** - HTTP client

### DevOps & Infrastructure
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **Kubernetes** - Container orchestration
- **Minikube** - Local Kubernetes development
- **Ansible** - Infrastructure automation
- **Vagrant** - Development environment provisioning
- **Nginx** - Reverse proxy and load balancer
- **Debian 12** - Target operating system

## Architecture

### Microservices Architecture

Following Domain-Driven Design (DDD) principles:

1. **Users Service** (Port 8000) - Authentication, profiles, and user management
2. **Projects Service** (Port 8001) - Project CRUD, showcasing, and management
3. **News Service** (Port 8002) - Feed aggregation, content processing, and delivery
4. **Frontend** (Port 3000) - Next.js web application

### Directory Structure

```
BahyWay/
├── backend-services/
│   ├── users-service/          # User management microservice
│   │   ├── app/
│   │   │   ├── routers/
│   │   │   ├── main.py
│   │   │   ├── models.py
│   │   │   ├── schemas.py
│   │   │   ├── security.py
│   │   │   └── database.py
│   │   ├── requirements.txt
│   │   └── Dockerfile
│   ├── projects-service/       # Projects microservice (placeholder)
│   └── news-service/           # News aggregation microservice (placeholder)
│
├── frontend-apps/
│   └── main-website/           # Next.js frontend application
│       ├── src/
│       │   ├── app/
│       │   ├── components/
│       │   ├── lib/
│       │   └── types/
│       ├── package.json
│       ├── tsconfig.json
│       └── Dockerfile
│
├── devops/
│   ├── ansible/
│   │   ├── provision.yml       # VM provisioning playbook
│   │   └── deploy.yml          # Kubernetes deployment playbook
│   ├── kubernetes-manifests/
│   │   ├── namespace.yaml
│   │   ├── configmap.yaml
│   │   ├── secret.yaml
│   │   ├── postgres-deployment.yaml
│   │   ├── redis-deployment.yaml
│   │   ├── users-service/
│   │   └── ingress.yaml
│   ├── vagrant/
│   │   └── Vagrantfile         # Debian 12 development VM
│   └── nginx/
│       ├── nginx.conf
│       └── conf.d/
│
├── scripts/
│   └── init-db.sql             # Database initialization
│
├── docs/
│   └── bahyway-website/        # Comprehensive documentation
│
├── docker-compose.website.yml   # Docker Compose configuration
├── .env.website                 # Environment variables
└── WEBSITE-README.md            # This file
```

## Quick Start

### Option 1: Docker Compose (Recommended for Development)

1. **Clone the repository**
   ```bash
   git clone https://github.com/bahyway/BahyWay.git
   cd BahyWay
   ```

2. **Copy environment file**
   ```bash
   cp .env.website .env
   # Edit .env and change default passwords
   ```

3. **Build and start services**
   ```bash
   docker-compose -f docker-compose.website.yml up --build
   ```

4. **Access the application**
   - Frontend: http://localhost:3000
   - Users API: http://localhost:8000/docs
   - Projects API: http://localhost:8001/docs
   - News API: http://localhost:8002/docs

### Option 2: Vagrant + Minikube (Production-like Environment)

1. **Start the Vagrant VM**
   ```bash
   cd devops/vagrant
   vagrant up
   vagrant ssh
   ```

2. **Inside the VM, start Minikube**
   ```bash
   minikube start --cpus=2 --memory=2048
   minikube addons enable ingress
   ```

3. **Build Docker images**
   ```bash
   cd /home/vagrant/BahyWay

   # Build Users Service
   cd backend-services/users-service
   docker build -t bahyway/users-service:latest .
   minikube image load bahyway/users-service:latest
   ```

4. **Deploy with Ansible**
   ```bash
   cd /home/vagrant/BahyWay
   ansible-playbook devops/ansible/deploy.yml
   ```

5. **Access via Minikube**
   ```bash
   minikube service -n bahyway users-service
   ```

## API Documentation

### Users Service

Once running, access the interactive API documentation:
- Swagger UI: http://localhost:8000/docs
- ReDoc: http://localhost:8000/redoc

#### Key Endpoints

**Authentication:**
- `POST /auth/register` - Register new user
- `POST /auth/login` - Login and get JWT token
- `GET /auth/me` - Get current user info

**Users:**
- `GET /users` - List all users
- `GET /users/{id}` - Get user by ID
- `PUT /users/me` - Update current user
- `DELETE /users/me` - Delete current user

### Testing the API

```bash
# Register a new user
curl -X POST "http://localhost:8000/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "username": "johndoe",
    "password": "securepassword123",
    "full_name": "John Doe"
  }'

# Login
curl -X POST "http://localhost:8000/auth/login" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=johndoe&password=securepassword123"

# Get current user (requires token)
curl -X GET "http://localhost:8000/auth/me" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Development Workflow

### Backend Development

1. **Install dependencies**
   ```bash
   cd backend-services/users-service
   python3 -m venv venv
   source venv/bin/activate
   pip install -r requirements.txt
   ```

2. **Run locally**
   ```bash
   uvicorn app.main:app --reload
   ```

### Frontend Development

1. **Install dependencies**
   ```bash
   cd frontend-apps/main-website
   npm install
   ```

2. **Run development server**
   ```bash
   npm run dev
   ```

3. **Build for production**
   ```bash
   npm run build
   npm start
   ```

## Environment Variables

Key environment variables (see `.env.website`):

```bash
# Database
POSTGRES_USER=bahyway
POSTGRES_PASSWORD=change_me_in_production
POSTGRES_DB=bahyway_db
DATABASE_URL=postgresql://...

# JWT Authentication
SECRET_KEY=your-secret-key-min-32-chars
ALGORITHM=HS256
ACCESS_TOKEN_EXPIRE_MINUTES=30

# Redis
REDIS_URL=redis://redis:6379/0

# Service URLs
USERS_SERVICE_URL=http://users-service:8000
PROJECTS_SERVICE_URL=http://projects-service:8001
NEWS_SERVICE_URL=http://news-service:8002
```

## Database

### PostgreSQL with Apache AGE

The platform uses PostgreSQL with the Apache AGE extension for:
- Relational data (users, projects, articles)
- Graph data (relationships, knowledge graphs)

### Initialization

Database is automatically initialized with:
- Apache AGE extension
- Knowledge graph: `bahyway_graph`
- Proper schemas and permissions

## Next Steps

1. **Complete Projects Service**: Implement project management functionality
2. **Complete News Service**: Implement RSS aggregation and Celery tasks
3. **Add WebSocket Support**: Real-time discussions and notifications
4. **Implement Apache AGE Queries**: Knowledge graph visualizations
5. **Add Authentication UI**: Login/register forms in frontend
6. **Create Profile Pages**: User and project detail pages
7. **Add Tests**: Unit and integration tests for all services
8. **Set up CI/CD**: GitHub Actions for automated testing and deployment

## Documentation

For detailed documentation, see:
- [FastAPI News Agency Guide](./docs/bahyway-website/01_fastapi-news-agency-guide.md)
- [BahyWay Comprehensive Guide](./docs/bahyway-website/02_bahyway-comprehensive-guide.md)
- [Brand Assets](./docs/bahyway-website/images/logo/README.md)

## Troubleshooting

### Docker Compose Issues

```bash
# Stop all containers
docker-compose -f docker-compose.website.yml down

# Remove volumes and rebuild
docker-compose -f docker-compose.website.yml down -v
docker-compose -f docker-compose.website.yml up --build
```

### Kubernetes Issues

```bash
# Check pod status
kubectl get pods -n bahyway

# Check logs
kubectl logs -n bahyway deployment/users-service-deployment

# Restart deployment
kubectl rollout restart deployment/users-service-deployment -n bahyway
```

### Database Connection Issues

```bash
# Check if PostgreSQL is running
docker-compose -f docker-compose.website.yml ps postgres

# Access PostgreSQL
docker-compose -f docker-compose.website.yml exec postgres psql -U bahyway -d bahyway_db
```

## Contributing

This is a learning-focused project. Follow the established patterns and best practices.

## License

[Specify your license]

## Contact

**Website:** [www.bahyway.com](http://www.bahyway.com)

---

**Built the "BAHY WAY"** - Professional, scalable, and built for learning.

*Last Updated: November 27, 2025*
