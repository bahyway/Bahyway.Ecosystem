from fastapi import FastAPI

app = FastAPI(title="BahyWay Projects Service", version="1.0.0")

@app.get("/")
def root():
    return {"message": "BahyWay Projects Service", "status": "coming soon"}

@app.get("/health")
def health():
    return {"status": "healthy", "service": "projects-service"}
