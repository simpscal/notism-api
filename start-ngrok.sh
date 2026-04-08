#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [ -z "${NGROK_AUTHTOKEN:-}" ]; then
  echo "Error: NGROK_AUTHTOKEN is not set."
  echo "Export it before running: export NGROK_AUTHTOKEN=<your-token>"
  exit 1
fi

ngrok start --config "$SCRIPT_DIR/ngrok.yml" api
