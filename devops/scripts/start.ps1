# Quick startup (without rebuilding)
docker compose -f devops/docker/docker-compose.yml --profile monitoring up -d
Write-Host "Starting EHR Platform..."
Start-Sleep -Seconds 3
docker compose -f devops/docker/docker-compose.yml --profile monitoring ps
