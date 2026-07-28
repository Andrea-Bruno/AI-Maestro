# Maestro-AI

## AI Roasting Machine — Professional Coffee Roasting Platform

Maestro-AI is a comprehensive, enterprise-grade coffee roasting management platform built on a modern client-server architecture. It combines advanced AI capabilities, real-time hardware control, and sophisticated data analysis to deliver a professional roasting experience.

### Architecture

Maestro-AI implements a **separation of concerns architecture**:
- **Backend**: .NET 10 RESTful API server providing core business logic, hardware control, and data processing
- **Frontend**: Modern HTML5/CSS3/JavaScript client (`Maestro-AI-Client/`) for intuitive user interaction
- **Hardware Layer**: Support for 86+ roasting machine profiles with configurable drivers and real-time telemetry

This modern architecture ensures:
✅ Scalability and multi-user support  
✅ Real-time data processing and hardware control  
✅ Seamless integration with existing systems via REST APIs  
✅ Separation of UI concerns from business logic  
✅ Future-proof extensibility

### Core Features

#### 🤖 AI & Intelligence
- **AI Profile Generation** — Automatic roasting profile creation based on bean characteristics and desired outcomes
- **Predictive Analysis** — Machine learning-based optimization suggestions
- **Smart Crack Detection** — AI-powered first and second crack identification
- **Energy Analysis** — Consumption optimization and sustainability metrics

#### 🔥 Roasting Management
- **Profile Design & Management** — Full lifecycle: create, modify, compare, and archive roasting profiles
- **Real-Time Monitoring** — Live temperature tracking (BT/ET), phase visualization, and telemetry
- **Phase Detection** — Automatic detection of roasting phases with customizable thresholds
- **Multi-Machine Support** — Control 86+ different roasting machine models
- **Batch Processing** — Asynchronous batch roasting and processing

#### 📊 Analysis & Reporting
- **Profile Comparison** — Side-by-side analysis of multiple roasting profiles
- **Cupping Evaluation** — Structured flavor profile assessment
- **Advanced Reporting** — Custom report generation and analytics
- **Energy Reports** — Detailed energy consumption analysis
- **Data Transformation** — Flexible data pipelines for custom processing

#### 🎛️ Hardware Control
- **Sensor Integration** — Multi-source data input (serial, Bluetooth, simulators)
- **PID Controller** — Precision temperature control algorithms
- **Scale Management** — Integration with weight measurement devices
- **Real-Time Simulation** — Hardware simulator for testing and training
- **Diagnostics** — System health monitoring and error detection

#### 📁 Import/Export & Integration
- **Profile Sharing** — Multiple format support (JSON, CSV)
- **Batch Import** — Drag-and-drop profile importing with duplicate detection
- **Profile Signing** — Security and verification of roasting profiles
- **API Integration** — Full REST API for third-party integration

#### 🛡️ Security & Management
- **User Identity Management** — Authentication and authorization
- **PIN Protection** — Configurable access control for sensitive operations
- **Role-Based Access** — Easy, Monitoring, and Full modes for different user levels
- **System Diagnostics** — Comprehensive logging and troubleshooting

### Quick Start

```bash
cd Maestro-AI
dotnet run --launch-profile http
# Server running on http://localhost:5252
```

Open `Maestro-AI-Client/index.html` in your browser to access the client interface.

### Documentation

Complete documentation is available in `docs/` (multi-language support: en, it, de, fr, es, ru) and accessible via API:

```bash
curl -X POST http://localhost:5252/api/GetDoc -H "Content-Type: application/json" \
  -d '{"topic":"quickstart","lang":"en"}'
```

### Build

```bash
dotnet build
```

## Hardware Configuration

Maestro-AI supports **86+ roasting machine profiles**. Configure your hardware in `appsettings.json` under the `Hardware` section with:
- Machine model and version
- Driver configuration
- Sensor mappings
- Control parameters

## User Interface Modes

The interface offers **three operational modes**, configurable in Settings → GUI Mode (PIN-protected, default `0000`):

| Mode | Description | Use Case | Permissions |
|------|-------------|----------|-------------|
| **👁 Monitoring** | Read-only view | Production floors, demo screens, quality control | View-only: Dashboard, Roast visualization, Diagnostics |
| **👍 Easy** | Simplified interface | Daily operations, standard users | Core roasting functions with safety restrictions |
| **⚡ Full** | Complete access | Advanced users, developers, administrators | All features including advanced analysis, system config |

### 🔐 PIN Protection

- Default PIN: `0000` (change via code or localStorage)
- Protects: Settings (language, temperature units, server URL, machine identity)
- GUI mode selection is always accessible without PIN

### 📥 Profile Import

- Automatic duplicate detection
- Content-based comparison (BT/ET arrays + name)
- Non-invasive workflow (duplicates shown as `⚠️ Duplicate — not saved`)

## REST API Endpoints

Maestro-AI provides a comprehensive REST API with 22+ endpoint modules:

| Category | Endpoints |
|----------|-----------|
| **Roasting** | `/api/Roast`, `/api/RoastProperties` |
| **Profiles** | `/api/Profile`, `/api/Designer` |
| **Analysis** | `/api/Analysis`, `/api/Cupping`, `/api/Comparator`, `/api/Reports` |
| **Hardware** | `/api/Hardware`, `/api/Sensor`, `/api/Scale` |
| **AI & Intelligence** | `/api/Ai`, `/api/Calculator`, `/api/Transform` |
| **Control** | `/api/PID`, `/api/Simulator`, `/api/Diagnostics` |
| **Data Management** | `/api/ImportExport`, `/api/Batch`, `/api/Events` |
| **Security & Settings** | `/api/Identity`, `/api/Settings`, `/api/Misc` |
| **Utilities** | `/api/Docs`, `/api/Master`, `/api/Diagnostics` |

All endpoints follow REST conventions and support JSON serialization for seamless integration.

## System Requirements

- **.NET**: 10.0 or later
- **Operating System**: Windows, Linux, macOS
- **Memory**: Minimum 512 MB (recommended 2 GB+)
- **Hardware**: Compatible roasting machine with supported drivers
- **Browser**: Modern browsers supporting HTML5/ES6 (Chrome, Firefox, Safari, Edge)

## Project Structure

```
Maestro-AI/
├── Api/                      # 22+ REST API endpoint modules
├── Services/                 # Business logic layer
│   ├── AiProfileGenerator    # AI-powered profile creation
│   ├── PidController         # Temperature control algorithms
│   ├── PhaseDetector         # Roasting phase identification
│   ├── CrackDetector         # AI-based crack detection
│   ├── EnergyAnalyzer        # Energy consumption analysis
│   └── ...
├── Models/                   # Domain models (Roast, Profile, etc.)
├── Hardware/                 # Hardware drivers and management
├── Components/               # UI components
├── docs/                     # Multi-language documentation
└── wwwroot/                  # Static assets

Maestro-AI-Client/           # Modern frontend (HTML5/CSS3/JS)
├── index.html
├── js/                       # Client application logic
├── css/                      # Styling
├── icons/                    # UI assets
└── lang/                     # Multi-language support
```

## Multi-Language Support

Maestro-AI includes built-in support for:
- 🇬🇧 English (en)
- 🇮🇹 Italian (it)
- 🇩🇪 German (de)
- 🇫🇷 French (fr)
- 🇪🇸 Spanish (es)
- 🇷🇺 Russian (ru)

Language configuration is managed via API and stored in browser localStorage.

## Getting Help

- **API Documentation**: Available via `/api/GetDoc` endpoint
- **System Diagnostics**: Use Diagnostics tab for system health and logs
- **Settings**: Configure via GUI or `appsettings.json`

## License & Contributing

For contribution guidelines and license information, see the root directory.
