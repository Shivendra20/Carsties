# Script to run Docker, backend, and frontend simultaneously
# This script starts Docker services first, then builds and runs the .NET backend, then starts the frontend

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Carsties - Full Stack Launcher" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Define paths
$BackendPath = "src\AuctionService"
$FrontendPath = "frontend"

# Function to check if a port is in use
function Test-Port {
    param([int]$Port)
    try {
        $connection = Test-NetConnection -ComputerName localhost -Port $Port -WarningAction SilentlyContinue -InformationLevel Quiet
        return $connection
    }
    catch {
        return $false
    }
}

# Function to wait for a service to be ready
function Wait-ForService {
    param(
        [string]$ServiceName,
        [int]$Port,
        [int]$MaxAttempts = 30
    )
    
    Write-Host "Waiting for $ServiceName to be ready on port $Port..." -ForegroundColor Cyan
    $attempts = 0
    
    while ($attempts -lt $MaxAttempts) {
        if (Test-Port -Port $Port) {
            Write-Host "$ServiceName is ready!" -ForegroundColor Green
            return $true
        }
        Start-Sleep -Seconds 2
        $attempts++
        Write-Host "  Attempt $attempts/$MaxAttempts..." -ForegroundColor Gray
    }
    
    Write-Host "$ServiceName failed to start within expected time!" -ForegroundColor Red
    return $false
}

# Step 1: Start Docker Compose
Write-Host "[1/5] Starting Docker Services..." -ForegroundColor Yellow

# Check if Docker is running
try {
    docker info | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Docker is not running! Please start Docker Desktop first." -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "Docker is not running! Please start Docker Desktop first." -ForegroundColor Red
    exit 1
}

# Start Docker Compose services
Write-Host "Starting PostgreSQL and Redis containers..." -ForegroundColor Cyan
docker-compose up -d

if ($LASTEXITCODE -ne 0) { 
    Write-Host "Failed to start Docker services!" -ForegroundColor Red
    exit 1
}

# Step 2: Wait for Docker services to be ready
Write-Host "[2/5] Waiting for Docker Services..." -ForegroundColor Yellow

if (-not (Wait-ForService -ServiceName "PostgreSQL" -Port 5432)) {
    Write-Host "PostgreSQL did not start properly. Check Docker logs." -ForegroundColor Red
    exit 1
}

if (-not (Wait-ForService -ServiceName "Redis" -Port 6379)) {
    Write-Host "Redis did not start properly. Check Docker logs." -ForegroundColor Red
    exit 1
}

Write-Host "All Docker services are ready!" -ForegroundColor Green
Start-Sleep -Seconds 2

# Step 3: Build and Run Backend
Write-Host "[3/5] Building .NET Backend..." -ForegroundColor Yellow
Push-Location $BackendPath

# Build the backend
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Backend build failed!" -ForegroundColor Red
    Pop-Location
    exit 1
}

Write-Host "[4/5] Starting .NET Backend..." -ForegroundColor Yellow

# Start backend in a new window
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD'; Write-Host 'Starting Backend Server...' -ForegroundColor Green; dotnet run"

Pop-Location

# Wait a bit for backend to start
Write-Host "Waiting for backend to initialize..." -ForegroundColor Cyan
Start-Sleep -Seconds 5

# Step 4: Build and Run Frontend
Write-Host "[5/5] Starting Frontend..." -ForegroundColor Yellow
Push-Location $FrontendPath

# Install dependencies if needed
if (-Not (Test-Path "node_modules")) {
    Write-Host "Installing npm packages..." -ForegroundColor Cyan
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "npm install failed!" -ForegroundColor Red
        Pop-Location
        exit 1
    }
}

# Start frontend in a new window
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD'; Write-Host 'Starting Frontend Server...' -ForegroundColor Green; npm run dev"

Pop-Location

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  All services are starting!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Docker: PostgreSQL (5432) + Redis (6379)" -ForegroundColor Cyan
Write-Host "Backend: Check the .NET window" -ForegroundColor Cyan
Write-Host "Frontend: Check the npm window" -ForegroundColor Cyan
Write-Host ""
Write-Host "To stop all services:" -ForegroundColor Yellow
Write-Host "  1. Close backend and frontend windows (or Ctrl+C)" -ForegroundColor Gray
Write-Host "  2. Run: docker-compose down" -ForegroundColor Gray
Write-Host ""
Write-Host "Press any key to exit this launcher..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
