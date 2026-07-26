# Stop all containers (keep volumes)
docker compose -f devops/docker/docker-compose.yml --profile monitoring down

Write-Host "All containers stopped."
Write-Host "Use 'down -v' to also remove volumes: docker compose -f devops/docker/docker-compose.yml --profile monitoring down -v"
