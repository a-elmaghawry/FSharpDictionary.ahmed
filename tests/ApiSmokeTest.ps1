Write-Host "Starting Dictionary API Test..."

$baseUrl = "http://localhost:5000"

$body = @{
    term = "automation"
    definition = "automatic test word"
    tags = @("test","auto")
} | ConvertTo-Json

Invoke-RestMethod -Uri "$baseUrl/word" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"

Write-Host "Add word: OK"

$words = Invoke-RestMethod -Uri "$baseUrl/words" -Method Get

if ($words.Count -gt 0) {
    Write-Host "Get words: OK"
} else {
    Write-Error "Test Failed: No words found"
}

Write-Host "Automation Test Finished Successfully"
