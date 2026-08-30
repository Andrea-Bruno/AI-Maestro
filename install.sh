#!/usr/bin/env bash
# Maestro-AI — one-line installer for Linux (Orange Pi / small server).
#
#   curl -fsSL https://raw.githubusercontent.com/Andrea-Bruno/AI-Maestro/main/install.sh | bash
#
# Downloads the latest Maestro-AI release archive for the detected platform from GitHub,
# unpacks it into /opt/maestro-ai and installs it as a systemd service that auto-starts at
# boot (plain HTTP on port 5252 — no certificates needed on a LAN appliance). The archive
# is self-contained (~100 MB): no .NET runtime needed. Missing dependencies are installed
# automatically.
#
# Environment overrides:
#   MAESTRO_VERSION            release tag to install, e.g. v1.26.08.30 (default: latest)
#   MAESTRO_HOME               install directory (default: /opt/maestro-ai)
#   MAESTRO_NO_SERVICE=1       unpack only, no systemd service
set -euo pipefail

REPO="Andrea-Bruno/AI-Maestro"
VERSION="${MAESTRO_VERSION:-latest}"
DEST="${MAESTRO_HOME:-/opt/maestro-ai}"
NO_SERVICE="${MAESTRO_NO_SERVICE:-0}"
PORT="${MAESTRO_PORT:-5252}"

# --- platform check (Linux, aarch64 or x64) ---
[ "$(uname -s)" = "Linux" ] || { echo "This installer targets Linux." >&2; exit 1; }
arch="$(uname -m | tr '[:upper:]' '[:lower:]')"
case "$arch" in
  x86_64|amd64) arch="x64" ;;
  aarch64|arm64) arch="arm64" ;;
  *) echo "Unsupported architecture '$arch' (arm64 and x64 only)." >&2; exit 1 ;;
esac

# --- dependencies (curl, tar, libicu) ---
missing=""
command -v curl >/dev/null 2>&1 || missing="$missing curl"
command -v tar >/dev/null 2>&1 || missing="$missing tar"
if [ -n "$missing" ]; then
  echo "Installing missing dependencies:$missing"
  if command -v apt-get >/dev/null 2>&1; then
    sudo apt-get update -qq && sudo apt-get install -y -qq curl tar
  else
    echo "Please install curl and tar, then re-run." >&2
    exit 1
  fi
fi
if command -v dpkg >/dev/null 2>&1; then
  if ! /sbin/ldconfig -p 2>/dev/null | grep -q 'libicuuc\.so'; then
    echo "Installing libicu (.NET globalization)..."
    sudo apt-get update -qq && sudo apt-get install -y -qq libicu-dev || \
      echo "WARN: libicu install failed (the app may need it on this distro)." >&2
  fi
  # System.Device.Gpio requires the native libgpiod to drive the 40-pin GPIO header.
  # Without it the GPIO driver falls back to simulation with a clear error.
  if ! /sbin/ldconfig -p 2>/dev/null | grep -q 'libgpiod\.so'; then
    echo "Installing libgpiod (40-pin GPIO support)..."
    sudo apt-get update -qq && sudo apt-get install -y -qq libgpiod2 || \
      echo "WARN: libgpiod install failed (GPIO driver will fall back to simulation)." >&2
  fi
fi

# --- download the latest (or pinned) release ---
if [ "$VERSION" = "latest" ]; then
  BASE="https://github.com/$REPO/releases/latest/download"
else
  BASE="https://github.com/$REPO/releases/download/$VERSION"
fi
ASSET="maestro-linux-$arch.tar.gz"
echo "Downloading $BASE/$ASSET ..."
sudo mkdir -p "$DEST"
tmp="$(mktemp -d)"
trap 'rm -rf "$tmp"' EXIT
curl -fL --retry 3 -o "$tmp/$ASSET" "$BASE/$ASSET"

# --- unpack directly into the destination (the tmp dir may be a small tmpfs) ---
echo "Unpacking into $DEST ..."
sudo tar -xzf "$tmp/$ASSET" -C "$DEST"
rm -f "$tmp/$ASSET"

# The archive files carry the CI build uid; force a sane owner so the app can write
# its runtime state without permission errors.
sudo chown -R root:root "$DEST"
sudo chmod +x "$DEST/Maestro-AI"

# --- LAN appliance: serve plain HTTP (no certificates) ---
SETTINGS="$DEST/appsettings.json"
if [ -f "$SETTINGS" ]; then
  sudo sed -i 's/"ForceHttps"[[:space:]]*:[[:space:]]*true/"ForceHttps": false/' "$SETTINGS"
  echo "ForceHttps set to false (LAN appliance mode)."
fi

# --- systemd service (auto-start at boot) ---
if [ "$NO_SERVICE" = "0" ] && [ -d /run/systemd/system ]; then
  echo "Installing systemd service (auto-start at boot) on port $PORT..."
  sudo tee /etc/systemd/system/maestro-ai.service >/dev/null <<EOF
[Unit]
Description=Maestro-AI Coffee Roasting Platform
After=network-online.target
Wants=network-online.target

[Service]
Type=simple
WorkingDirectory=$DEST
ExecStart=$DEST/Maestro-AI --urls http://0.0.0.0:$PORT
Restart=always
RestartSec=3

[Install]
WantedBy=multi-user.target
EOF
  sudo systemctl daemon-reload
  sudo systemctl enable maestro-ai
  sudo systemctl restart maestro-ai
  sleep 3
  if systemctl is-active --quiet maestro-ai; then
    echo "Maestro-AI service is active."
  else
    echo "WARNING: the service did not start - check: systemctl status maestro-ai" >&2
    journalctl -u maestro-ai -n 10 --no-pager >&2 || true
  fi
fi

echo
echo "Maestro-AI installed to $DEST."
echo "Web UI / API: http://<this-host>:$PORT"
if [ "$NO_SERVICE" = "1" ]; then
  echo "Start it with: $DEST/Maestro-AI --urls http://0.0.0.0:$PORT"
fi
