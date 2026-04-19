#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT_DIR"

CONTAINER_CMD=""
if command -v podman >/dev/null 2>&1; then
  if podman compose version >/dev/null 2>&1; then
    CONTAINER_CMD="podman compose"
  elif command -v podman-compose >/dev/null 2>&1; then
    CONTAINER_CMD="podman-compose"
  fi
fi

if [[ -z "$CONTAINER_CMD" ]]; then
  if command -v docker >/dev/null 2>&1; then
    if docker compose version >/dev/null 2>&1; then
      CONTAINER_CMD="docker compose"
    elif command -v docker-compose >/dev/null 2>&1; then
      CONTAINER_CMD="docker-compose"
    fi
  fi
fi

if [[ -z "$CONTAINER_CMD" ]]; then
  echo "ERROR: No container compose command found. Install Podman Compose or Docker Compose."
  exit 1
fi

COMPOSE_BINARY="${CONTAINER_CMD%% *}"

log() {
  local prefix="$1"
  shift
  printf "[%s] %s\n" "$prefix" "$*"
}

check_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "ERROR: Required command '$1' not found."
    exit 1
  fi
}

check_command "$COMPOSE_BINARY"
check_command dotnet
check_command npm

wait_for_port() {
  local port="$1"
  local label="$2"
  local timeout_seconds=60
  local interval=2
  local elapsed=0

  while [[ $elapsed -lt $timeout_seconds ]]; do
    if nc -z localhost "$port" >/dev/null 2>&1; then
      log "ready" "$label is available on port $port"
      return 0
    fi
    sleep "$interval"
    elapsed=$((elapsed + interval))
  done

  echo "ERROR: $label did not become available on port $port after $timeout_seconds seconds."
  return 1
}

if ! command -v nc >/dev/null 2>&1; then
  echo "ERROR: 'nc' (netcat) is required to check port readiness. Install it with 'brew install netcat'."
  exit 1
fi

# CLI paths
AUTH_SERVICE_DIR="src/AuthenticationService"
AUCTION_SERVICE_DIR="src/AuctionService"
SEARCH_SERVICE_DIR="src/SearchService"
FRONTEND_DIR="frontend"
LOG_DIR="run-all-logs"
PID_FILE="run-all.pids"

mkdir -p "$LOG_DIR"
rm -f "$PID_FILE"

trap 'echo "Shutting down..."; if [[ -f "$PID_FILE" ]]; then xargs kill 2>/dev/null < "$PID_FILE" || true; fi; exit 0' EXIT INT TERM

log "info" "Starting container services with '$CONTAINER_CMD up -d'..."
$CONTAINER_CMD up -d

log "info" "Waiting for database services..."
wait_for_port 5432 "PostgreSQL"
wait_for_port 6379 "Redis"
wait_for_port 27017 "MongoDB"

start_service() {
  local name="$1"
  local workdir="$2"
  shift 2
  local cmd=("$@")
  local logpath="$LOG_DIR/${name// /-}.log"

  log "start" "$name -> $logpath"
  (
    cd "$workdir"
    "${cmd[@]}"
  ) >"$logpath" 2>&1 &

  echo "$!" >> "$PID_FILE"
  sleep 1
}

log "info" "Building and starting Authentication Service..."
start_service "Authentication Service" "$AUTH_SERVICE_DIR" dotnet run

log "info" "Building and starting Auction Service..."
start_service "Auction Service" "$AUCTION_SERVICE_DIR" dotnet run

log "info" "Building and starting Search Service..."
start_service "Search Service" "$SEARCH_SERVICE_DIR" dotnet run

log "info" "Preparing frontend..."
cd "$FRONTEND_DIR"
if [[ ! -d node_modules ]]; then
  log "install" "Installing npm dependencies..."
  npm install
fi
cd "$ROOT_DIR"

log "info" "Starting Frontend..."
start_service "Frontend" "$FRONTEND_DIR" npm run dev

log "done" "All services have been launched."
cat <<EOF
Services are starting in the background. Check output logs in '$LOG_DIR':
  - Authentication Service: $LOG_DIR/Authentication-Service.log
  - Auction Service:        $LOG_DIR/Auction-Service.log
  - Search Service:         $LOG_DIR/Search-Service.log
  - Frontend:               $LOG_DIR/Frontend.log

To stop everything, press Ctrl+C in this terminal or run:
  pkill -F "$PID_FILE" || true

NOTE: If you need to inspect a service log live, use:
  tail -f "$LOG_DIR/Authentication-Service.log"
EOF

wait
