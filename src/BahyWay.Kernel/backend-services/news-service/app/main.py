from fastapi import FastAPI

app = FastAPI(title="BahyWay News Service", version="1.0.0")

@app.get("/")
def root():
    return {"message": "BahyWay News Agency Service", "status": "coming soon"}

@app.get("/health")
def health():
    return {"status": "healthy", "service": "news-service"}
