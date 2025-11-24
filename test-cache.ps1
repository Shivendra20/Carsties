# Redis Cache Testing Script
# This script demonstrates the caching functionality

$baseUrl = "http://localhost:5000/api"

Write-Host "=== Redis Cache Testing ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check cache stats
Write-Host "1. Checking cache stats..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/cache/stats" -Method Get
    Write-Host "Cache Status: Operational" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "Error: Cannot connect to API. Make sure the application is running." -ForegroundColor Red
    Write-Host "Run: dotnet run (in src/AuctionService directory)" -ForegroundColor Yellow
    exit
}

# Test 2: Clear all auction cache
Write-Host "2. Clearing all auction cache..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/cache/clear/auctions" -Method Delete
    Write-Host $response.message -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "Warning: Could not clear cache" -ForegroundColor Yellow
    Write-Host ""
}

# Test 3: First request (should be cache MISS - from database)
Write-Host "3. First request to /api/Auctions (Cache MISS - from database)..." -ForegroundColor Yellow
$startTime = Get-Date
try {
    $auctions = Invoke-RestMethod -Uri "$baseUrl/Auctions" -Method Get
    $duration = (Get-Date) - $startTime
    Write-Host "Response time: $($duration.TotalMilliseconds) ms" -ForegroundColor Magenta
    Write-Host "Auctions count: $($auctions.Count)" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "Error fetching auctions" -ForegroundColor Red
    Write-Host ""
}

# Test 4: Check if cache exists
Write-Host "4. Checking if 'auctions:all' is now cached..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/cache/exists/auctions:all" -Method Get
    if ($response.exists) {
        Write-Host "Cache exists: YES ✓" -ForegroundColor Green
    } else {
        Write-Host "Cache exists: NO ✗" -ForegroundColor Red
    }
    Write-Host ""
} catch {
    Write-Host "Could not check cache" -ForegroundColor Yellow
    Write-Host ""
}

# Test 5: Second request (should be cache HIT - from Redis)
Write-Host "5. Second request to /api/Auctions (Cache HIT - from Redis)..." -ForegroundColor Yellow
$startTime = Get-Date
try {
    $auctions = Invoke-RestMethod -Uri "$baseUrl/Auctions" -Method Get
    $duration = (Get-Date) - $startTime
    Write-Host "Response time: $($duration.TotalMilliseconds) ms" -ForegroundColor Magenta
    Write-Host "Auctions count: $($auctions.Count)" -ForegroundColor Green
    Write-Host ""
    Write-Host "Notice: Second request should be MUCH faster! 🚀" -ForegroundColor Cyan
    Write-Host ""
} catch {
    Write-Host "Error fetching auctions" -ForegroundColor Red
    Write-Host ""
}

# Test 6: Test individual auction caching
if ($auctions -and $auctions.Count -gt 0) {
    $firstAuctionId = $auctions[0].id
    Write-Host "6. Testing individual auction cache (ID: $firstAuctionId)..." -ForegroundColor Yellow
    
    # First request
    $startTime = Get-Date
    try {
        $auction = Invoke-RestMethod -Uri "$baseUrl/Auctions/$firstAuctionId" -Method Get
        $duration = (Get-Date) - $startTime
        Write-Host "First request time: $($duration.TotalMilliseconds) ms (Cache MISS)" -ForegroundColor Magenta
    } catch {
        Write-Host "Error fetching auction" -ForegroundColor Red
    }
    
    # Second request
    $startTime = Get-Date
    try {
        $auction = Invoke-RestMethod -Uri "$baseUrl/Auctions/$firstAuctionId" -Method Get
        $duration = (Get-Date) - $startTime
        Write-Host "Second request time: $($duration.TotalMilliseconds) ms (Cache HIT)" -ForegroundColor Magenta
        Write-Host ""
    } catch {
        Write-Host "Error fetching auction" -ForegroundColor Red
        Write-Host ""
    }
}

Write-Host "=== Testing Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "- Cached requests should be significantly faster" -ForegroundColor White
Write-Host "- Cache expires after 30 minutes (configurable)" -ForegroundColor White
Write-Host "- Cache is automatically invalidated on updates/deletes" -ForegroundColor White
Write-Host ""
Write-Host "Useful endpoints:" -ForegroundColor Yellow
Write-Host "- GET  /api/cache/stats - View cache information" -ForegroundColor White
Write-Host "- GET  /api/cache/exists/{key} - Check if key exists" -ForegroundColor White
Write-Host "- DELETE /api/cache/{key} - Remove specific cache entry" -ForegroundColor White
Write-Host "- DELETE /api/cache/clear/auctions - Clear all auction cache" -ForegroundColor White
