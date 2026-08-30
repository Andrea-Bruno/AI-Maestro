# Frequently asked questions

## What is Maestro-AI?

A professional software platform for coffee roasting: a control unit (server) that talks to your roasting machine, runs the roasting logic and the AI, and a universal web client that works on any screen — control room PC, tablet, phone. It learns from past roasts to reproduce your best profile batch after batch, optimises energy consumption, and can certify every batch with a digital, blockchain-backed certificate.

## Installation and hardware

**Do I need to install .NET or any runtime?**
No. The release archives are self-contained — the runtime and every component are included (~50 MB compressed). The installer only needs `curl`, `tar` and `libicu`, and installs them automatically when missing.

**Which machines does it support?**
88 machine profiles across 8 protocols: Siemens S7 and Modbus PLC, MQTT brokers, serial (Modbus RTU), BLE, GPIO (Raspberry Pi / Orange Pi class), WebSocket and simulated. See the Hardware guide for the full list.

**Do I need a roasting machine to try it?**
No. The app runs in **simulated mode** by default: you can run a complete roast, watch the BT/ET curves, first crack, drop and the saved profile — all without hardware. Configure `Hardware.Enabled: true` when you connect your machine.

**Can I install it on Windows?**
The one-line installer targets Linux. On Windows you build from source with the .NET 10 SDK (`dotnet run --launch-profile http`) — see the Installation guide.

**Where does it get installed on Linux?**
`/opt/maestro-ai`, running as a `maestro-ai` systemd service that starts at boot. Change with `MAESTRO_HOME`.

## Using it

**How do I access the interface?**
Open `http://<machine-ip>:5252` in any browser on your network. There is nothing to install on the client — no apps, no plugins.

**Which port does it use?**
5252 by default (change with `MAESTRO_PORT`). The API lives on the same port (`/api`).

**Do I need an internet connection?**
No. Everything runs on your LAN. An internet connection is only needed to download updates.

**How do I connect my machine?**
Edit `Hardware` in `appsettings.json` (`Enabled: true`, your `MachineType` and connection parameters), then restart the service. The Hardware guide documents every supported protocol and machine.

## Data and privacy

**Where are my roast profiles and batch history stored?**
On the machine running Maestro-AI, in its own data store. They never leave your facility.

**Do my data leave the building?**
No — unless you explicitly use a cloud feature, everything is local. The blockchain certificates are signed locally; you decide what to share with customers.

## Updates and support

**How do I update?**
Re-run the one-line installer — it downloads the latest release and restarts the service. Your configuration, profiles and history are never touched.

**What if the service does not start?**
`journalctl -u maestro-ai -n 30` shows the reason; the Installation guide's troubleshooting table covers the common cases (missing libicu, malformed config, port in use).
