# 🚑 RescueGrid 1122

**An intelligent emergency dispatch system that triages calls, finds the fastest rescue unit, and reserves a hospital bed in real time.**

![Status](https://img.shields.io/badge/status-in%20development-orange)
![C#](https://img.shields.io/badge/C%23-.NET-512BD4)
![Python](https://img.shields.io/badge/Python-ML-3776AB)
![React](https://img.shields.io/badge/React-Dashboard-61DAFB)
![SQLite](https://img.shields.io/badge/SQLite-Database-003B57)

---

## 📌 Overview

RescueGrid 1122 is a local simulation of an emergency response control room. When an emergency call arrives, the system ranks it by urgency, picks the fastest available ambulance or fire unit using shortest-path routing, locks a bed at the nearest suitable hospital, and sends the dispatch order. Everything is shown live on a web dashboard.

This project was developed for the **Software Engineering** course at **University of Engineering and Technology**.

## ✨ Key Features

- **Smart triage queue:** a custom Max-Heap with *aging*, so calls that wait longer gain priority automatically.
- **Fastest-unit dispatch:** Dijkstra's algorithm on a weighted road graph picks the closest available unit of the right type.
- **Hospital bed reservation:** atomic bed locking with a TCP handshake (`RESERVE_BED` → `ACK_LOCKED` / `NACK_FULL`) and automatic fallback to the next hospital.
- **Live fleet tracking:** units send UDP telemetry every 2 seconds and a watchdog detects lost signals.
- **Automatic recovery:** if a unit goes silent for more than 8 seconds, its ticket is re-queued and dispatched again.
- **Choke point simulation:** admins can block roads or raise congestion, and the next dispatch uses the new weights.
- **ML analytics:** K-Means recommends standby points and a regression model predicts arrival times (ETA).
- **Real-time dashboard:** live map, triage queue, bed counts and analytics, updated through SignalR.

## 🏗️ System Architecture

The system follows a layered architecture:

| Layer | Responsibility |
| --- | --- |
| **Data Source** | Unit simulators, call-taker client, hospital ER client |
| **Network** | UDP Telemetry Listener, TCP Message Server |
| **Processing** | C# core server: hash map registry, Max-Heap triage queue, dispatch engine, Dijkstra router, bed reservation service, watchdog |
| **Storage** | SQLite database |
| **Analytics** | Python scripts for K-Means and ETA regression |
| **API** | ASP.NET Core REST API and SignalR hub |
| **Presentation** | React dashboard |

Core data structures and algorithms (graph, Dijkstra, MinHeap, MaxHeapAging, HashMap) are **built from scratch** in a custom DSA module.

## 🛠️ Tech Stack

- **Backend:** C#, .NET, ASP.NET Core, SignalR
- **Frontend:** React
- **Database:** SQLite
- **Machine Learning:** Python (K-Means, regression)
- **Communication:** UDP (telemetry), TCP (dispatch and bed reservation), REST and SignalR (dashboard)
- **Design patterns:** State, Observer, Command

## 👥 Actors

| Actor | What they can do |
| --- | --- |
| **Call Taker** | Log emergency calls, view the dashboard |
| **Hospital ER Staff** | Confirm or refuse bed reservations, update bed availability |
| **Admin** | Simulate choke points, set packet-loss level, view the dashboard |
| **Analyst** | View the dashboard, standby recommendations and ETA predictions |
| **Unit Simulator** | Send UDP telemetry, receive dispatch orders |

## 🗄️ Database

The database is in third normal form with 12 entities, including `Road_Nodes`, `Road_Graph_Edges`, `Rescue_Units`, `Hospitals`, `Hospital_Beds`, `Bed_Reservations`, `Emergency_Incidents`, `Dispatch_Logs`, `Standby_Recommendations` and `ETA_Predictions`.

## 📂 Project Structure

```
[Update this to match your actual folders, for example:]
RescueGrid/
├── Server/        # C# dispatch server and DSA module
├── Dashboard/     # React frontend
├── Analytics/     # Python ML scripts
├── Simulators/    # Unit, call-taker and hospital clients
├── Docs/          # SRS and diagrams
└── README.md
```

## 🚀 Getting Started

### Prerequisites

- .NET 10.0 SDK version
- Node.js and npm
- Python 3.x
- Git

### Installation

```bash
# 1. Clone the repository
git clone (https://github.com/IconicEmblem/RescueGrid_1122)
cd RescueGrid_1122

# 2. Run the server
[command to start the C# server]

# 3. Run the dashboard
[command to start the React app]

# 4. Run the simulators
[command to start the simulators]
```

> During development, all components can run on a single laptop. For the full demo, the simulators run on separate laptops over the same WiFi.

## 📖 Documentation

The SRS and system design diagrams (DFD, use case, activity, state, sequence, class, ERD and deployment) are in the [`Docs`](./Docs) folder.

## 🧑‍💻 Team

| Name | Role |
| Muhammad Subhan | Backend developer and Team manager |
| Arham Rafique | Backend developer and code reviewer |
| Shahram Husain | Frontend developer and code tester |
| Bismillah Batool | Frontend developer and designer |
| Zunaira Hanif | Database Design and Software designer |

**Supervisor:** Muhammad Ali

## 📄 License

This project was created for academic purposes at University of Engineering and Technology.
