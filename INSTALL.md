# Installation

Maestro-AI is a self-contained .NET application: the release archives include the runtime and every component, so **no .NET installation is needed** on the target machine. This page covers installing the ready-made release on a Linux machine (the recommended setup for a roastery control unit) and building from source on any platform.

## Prerequisites

| Requirement | Details |
|---|---|
| **OS** | 64-bit Linux (Debian/Ubuntu-based recommended; the archive is self-contained) |
| **Architecture** | ARM64 (e.g. Orange Pi 5 / Raspberry Pi class) or x64 — pick the matching archive |
| **Storage** | At least 200 MB free |
| **Memory** | 1 GB or more recommended |
| **Network** | Internet access for the one-line install (the app itself runs fully on your LAN) |
| **Access** | A terminal with `sudo` |

## One-line install (Linux)

Copy and paste this into the terminal of the machine that will run the control unit:

```bash
curl -fsSL https://raw.githubusercontent.com/Andrea-Bruno/AI-Maestro/main/install.sh | bash
```

What the installer does automatically:

1. Detects the platform (ARM64 or x64 Linux) and installs `curl`, `tar` and `libicu` if missing.
2. Downloads the **latest** release archive (`maestro-linux-arm64.tar.gz` or `maestro-linux-x64.tar.gz`) from GitHub.
3. Unpacks it into `/opt/maestro-ai` and fixes file ownership.
4. Configures the app for **LAN appliance mode** (plain HTTP on port `5252`, no certificates needed).
5. Creates and starts a **systemd service** (`maestro-ai`) that keeps the control unit always on and starts it at boot.

### Options

```bash
# Install a specific version instead of latest
curl -fsSL https://raw.githubusercontent.com/Andrea-Bruno/AI-Maestro/main/install.sh | \
  MAESTRO_VERSION=v1.26.08.30 bash

# Custom folder / port, without the systemd service
curl -fsSL https://raw.githubusercontent.com/Andrea-Bruno/AI-Maestro/main/install.sh | \
  MAESTRO_HOME=/home/roaster/maestro MAESTRO_PORT=8080 MAESTRO_NO_SERVICE=1 bash
```

## Verify the install

```bash
curl -s http://localhost:5252/api    # the generated API client (JSON/C#) -> 200
curl -s -o /dev/null -w '%{http_code}\n' http://localhost:5252/   # the web UI -> 200
```

Open `http://<machine-ip>:5252` in a browser: the Maestro-AI UI loads. The app starts in **simulated mode** (no hardware attached) — see the Quick Start guide for the roast simulation walkthrough.

## Connecting your roasting machine

Edit `/opt/maestro-ai/appsettings.json`:

```json
"Hardware": {
  "Enabled": true,
  "MachineType": "YourMachineType",
  ...
}
```

Then restart the service:

```bash
sudo systemctl restart maestro-ai
```

The Hardware guide (`docs/en/09-hardware.md`) lists the 88 supported machine profiles across 8 protocols (S7/Modbus PLC, MQTT, serial, BLE, GPIO, WebSocket, ...).

## Building from source (any platform)

The project builds with the .NET 10 SDK. The sibling repositories (UISupportBlazor, EncryptedMessaging, SecureStorage, FullDuplexStreamSupport) are resolved automatically: from the source tree they are used as projects, on a clean machine the published NuGet packages are restored instead.

```bash
git clone https://github.com/Andrea-Bruno/AI-Maestro.git
cd AI-Maestro/Maestro-AI
dotnet run --launch-profile http      # development, http://localhost:5252
```

For a self-contained single-file build (same as the release archives):

```bash
dotnet publish Maestro-AI.csproj -c Release -r linux-arm64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish
```

## Updating

Re-run the one-line installer: it downloads the latest release and restarts the service. Your configuration (`appsettings.json`), roast profiles and batch history are never touched.

## Uninstalling

```bash
sudo systemctl stop maestro-ai
sudo systemctl disable maestro-ai
sudo rm -f /etc/systemd/system/maestro-ai.service
sudo systemctl daemon-reload
sudo rm -rf /opt/maestro-ai
```

## Troubleshooting

| Problem | Fix |
|---|---|
| Service does not start | `systemctl status maestro-ai` and `journalctl -u maestro-ai -n 30`; common causes: missing `libicu`, malformed `appsettings.json` |
| Port 5252 already in use | Change the port with `MAESTRO_PORT` on install, or edit `ExecStart` in `/etc/systemd/system/maestro-ai.service` |
| The web UI redirects to HTTPS | The installer sets `"ForceHttps": false`; if you edited `appsettings.json` manually, make sure it is still `false` on a certificate-free LAN |
| Hardware does not connect | The machine is not attached or `MachineType` is wrong — check the Hardware guide and the driver log (`Log.IsEnabled` in `Program.cs` for verbose logs) |
