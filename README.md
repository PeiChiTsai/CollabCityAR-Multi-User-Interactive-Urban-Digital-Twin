<div align="center">

# CollabCity AR

**Multi-User Interactive Urban Digital Twin for Collaborative Geospatial Interaction**

[![Unity](https://img.shields.io/badge/Unity-6000.0.46f1-000000?logo=unity&logoColor=white)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-Android-3DDC84?logo=android&logoColor=white)](https://www.android.com/)
[![ARCore](https://img.shields.io/badge/AR-Google%20ARCore-4285F4?logo=google&logoColor=white)](https://developers.google.com/ar)
[![Cesium](https://img.shields.io/badge/Geospatial-Cesium-6C6CFF)](https://cesium.com/)
[![Ubiq](https://img.shields.io/badge/Networking-Ubiq-111827)](https://ubiq.online/)

**[Overview](#overview) · [Architecture](#system-architecture) · [Quick Start](#quick-start) · [Usage](#usage) · [Tech Stack](#technology-stack)**

</div>

---

## Overview

**CollabCity AR** is a Unity-based multi-user Augmented Reality (AR) prototype for interacting with a geospatial **Urban Digital Twin (UDT)**.

The project creates a shared AR environment where multiple handheld devices can:

- Align to the same physical spatial reference
- Visualise georeferenced 3D city data
- Join the same multi-user session
- Place and manipulate shared spatial markers
- Synchronise object states in real time
- Record geospatial interaction events for analysis

The prototype was developed as part of Pei-Chi Tsai's MRes research at the **Centre for Advanced Spatial Analysis (CASA), UCL**.

The current prototype uses **Queen Elizabeth Olympic Park, London** as its geospatial context.

---

## What it does

```text
Physical Environment
        │
        ▼
┌──────────────────────────┐
│   ARCore Cloud Anchor    │
│   Shared spatial frame   │
└────────────┬─────────────┘
             │
             ▼
┌──────────────────────────┐
│    Cesium for Unity      │
│  Geospatial 3D context   │
└────────────┬─────────────┘
             │
             ▼
┌──────────────────────────┐
│     Ubiq Networking      │
│  Multi-user interaction  │
└────────────┬─────────────┘
             │
             ▼
┌──────────────────────────┐
│   Interactive Markers    │
│ Place · Move · Rotate    │
│ Scale · Delete           │
└────────────┬─────────────┘
             │
             ▼
┌──────────────────────────┐
│    Interaction Logs      │
│  Spatial + temporal data │
└──────────────────────────┘
```

The system separates **spatial alignment**, **geospatial visualisation**, **multi-user synchronisation**, and **interaction logging** into a four-layer workflow.

---

# Quick Start

### 1. Clone the repository

```bash
git clone https://github.com/PeiChiTsai/CollabCityAR-Multi-User-Interactive-Urban-Digital-Twin.git
cd CollabCityAR-Multi-User-Interactive-Urban-Digital-Twin
```

### 2. Open the project

Open the repository in:

```text
Unity 6000.0.46f1
```

### 3. Install dependencies

The project requires:

- **AR Foundation**
- **Google ARCore**
- **ARCore Extensions**
- **Cesium for Unity**
- **Ubiq**

### 4. Configure external services

The prototype relies on external services for spatial alignment and geospatial streaming.

You will need to configure:

- **Google ARCore API / Cloud Anchors**
- **Cesium ion**

Keep API credentials and access tokens local. Do **not** commit them to the repository.

### 5. Build for Android

Build the project for an **ARCore-compatible Android device**.

The original prototype was tested on:

- Google Pixel 6a
- Google Pixel 7
- Android 12+

A stable internet connection is required for the multi-user networking workflow.

---

# Features

## Spatial Alignment

### Google ARCore Cloud Anchors

The application uses **ARCore Cloud Anchors** to establish a shared spatial reference between devices.

Two workflows are supported:

```text
Host a New Anchor
        │
        ├── Detect surface
        ├── Place anchor
        ├── Capture environment features
        └── Upload / share Anchor ID
```

and:

```text
Resolve Existing Anchor
        │
        ├── Enter Anchor ID
        ├── Scan environment
        ├── Match visual features
        └── Resolve shared spatial position
```

This shared anchor provides the spatial reference used by the multi-user AR environment.

---

## Geospatial Visualisation

### Cesium for Unity

The urban environment is rendered using **Cesium for Unity** and **Cesium ion**.

The prototype uses:

- Google Photorealistic 3D Tiles
- Cesium Georeference
- WGS84 geographic coordinates
- Georeferenced 3D terrain and building geometry

The current scene is configured around **Queen Elizabeth Olympic Park, London**.

---

## Multi-User Networking

### Ubiq

**Ubiq for Unity** provides real-time synchronisation between participants.

The networking layer handles shared:

- User identity
- Object creation
- Object position
- Object rotation
- Object state

Objects are synchronised using coordinates relative to the shared AR anchor, allowing each device to reconstruct the corresponding world-space position.

---

## Interactive Spatial Markers

The prototype provides a set of placeable 3D markers for spatial interaction.

Users can:

```text
Place
  ↓
Move
  ↓
Rotate
  ↓
Scale
  ↓
Delete
```

Markers are used to externalise spatial ideas and support shared interaction within the AR scene.

Visual indicators communicate interaction state and object ownership across devices. User-created objects are colour-coded by user identity, while selected and actively manipulated objects are visually distinguished.

---

## Interaction Logging

The application records spatial interaction events for later analysis.

Logged information can include:

| Field | Description |
|---|---|
| Timestamp | Time of interaction |
| Object ID | Marker / object identifier |
| User ID | User associated with the object |
| Longitude | Geographic longitude |
| Latitude | Geographic latitude |
| Elevation | Object height |
| Interaction state | Current interaction / object state |

The logging workflow links user interactions to their geographic locations within the digital twin.

---

# System Architecture

The prototype is organised into four main layers.

### Layer 1 — AR Spatial Alignment

**Google ARCore + Cloud Anchors**

Establishes a common physical reference frame across devices.

### Layer 2 — Spatial Data Visualisation

**Cesium for Unity + Cesium ion**

Provides the geospatial 3D environment and real-world coordinate reference.

### Layer 3 — Multi-User Networking

**Ubiq**

Synchronises users and shared object interactions in real time.

### Layer 4 — Data Logging

**Unity logging workflow**

Records interaction events together with their geospatial coordinates and timestamps.

```text
┌──────────────────────────────────────────┐
│  Layer 1                                 │
│  ARCore Cloud Anchors                    │
│  Spatial Alignment                       │
└─────────────────────┬────────────────────┘
                      │
                      ▼
┌──────────────────────────────────────────┐
│  Layer 2                                 │
│  Cesium for Unity                        │
│  Geospatial Visualisation                │
└─────────────────────┬────────────────────┘
                      │
                      ▼
┌──────────────────────────────────────────┐
│  Layer 3                                 │
│  Ubiq                                    │
│  Multi-User Synchronisation              │
└─────────────────────┬────────────────────┘
                      │
                      ▼
┌──────────────────────────────────────────┐
│  Layer 4                                 │
│  Interaction Logging                     │
│  Spatial + Temporal Records              │
└──────────────────────────────────────────┘
```

---

# Coordinate Synchronisation

A key part of the implementation is the conversion between device-local coordinates and the shared AR anchor coordinate system.

For an object with world-space position `p_world`:

```text
p_local = Rᵀ (p_world − t)
```

where:

- `p_local` = anchor-relative position
- `p_world` = world-space position
- `t` = anchor translation
- `R` = anchor rotation
- `Rᵀ` = inverse anchor rotation

The system transmits anchor-relative transforms through Ubiq. Receiving devices reconstruct the world-space transform using their locally resolved Cloud Anchor.

This separates **physical spatial alignment** from **networked interaction synchronisation**.

---

# Usage

## Host a New Shared Space

1. Launch the application.
2. Select **Host a New Anchor**.
3. Detect a suitable physical surface.
4. Place the anchor.
5. Scan the surrounding environment.
6. Wait until sufficient visual features are detected.
7. Upload the Cloud Anchor.
8. Share the generated Anchor ID.

## Join an Existing Shared Space

1. Launch the application.
2. Select **Resolve an Existing Cloud Anchor**.
3. Enter the shared Anchor ID.
4. Scan the same physical environment.
5. Allow ARCore to resolve the anchor.
6. Join the shared multi-user session.

## Interact with the Digital Twin

Once synchronised, users can interact with the shared environment and:

- Place markers
- Move markers
- Rotate markers
- Scale markers
- Delete markers
- Observe other users' interactions

---

# Technology Stack

| Category | Technology |
|---|---|
| Engine | **Unity 6000.0.46f1** |
| AR Framework | **AR Foundation** |
| Mobile AR | **Google ARCore** |
| Spatial Alignment | **ARCore Cloud Anchors** |
| Geospatial Engine | **Cesium for Unity** |
| 3D Geospatial Data | **Google Photorealistic 3D Tiles** |
| Geospatial Platform | **Cesium ion** |
| Networking | **Ubiq for Unity** |
| Platform | **Android** |
| Development Language | **C#** |
| Tested Devices | **Google Pixel 6a / Pixel 7** |

---

# Geospatial Data

The prototype combines a 3D city environment with georeferenced Point-of-Interest data.

### Base Environment

```text
Cesium ion
    ↓
Google Photorealistic 3D Tiles
    ↓
Cesium for Unity
    ↓
Georeferenced AR environment
```

### POI Layers

The prototype includes contextual spatial datasets represented as interactive POI layers, including:

- Wildlife observations
- Bicycle / shared-bike locations

The experimental implementation uses static mock datasets based on real-world open-data concepts so that the same spatial information can be used consistently within the prototype.

---

# Project Structure

A Unity project typically contains:

```text
CollabCityAR-Multi-User-Interactive-Urban-Digital-Twin/
│
├── Assets/
│   ├── Scenes/
│   ├── Scripts/
│   ├── Prefabs/
│   ├── Materials/
│   ├── Resources/
│   └── ...
│
├── Packages/
│
├── ProjectSettings/
│
├── Logs/
│
└── README.md
```

The exact organisation may vary between project versions.

---

# Requirements

## Hardware

The device should support:

- ARCore
- Camera-based spatial tracking
- Gyroscope
- Depth / environmental sensing where supported
- Stable internet connectivity

## Development Environment

The project was developed and tested with:

```text
Unity 6000.0.46f1
Android 12+
ARCore-compatible hardware
```

---

# Notes

### API Credentials

This project depends on external services such as Google ARCore and Cesium ion.

Before running the application, configure the required credentials locally.

**Never commit API keys, access tokens, or private service credentials to GitHub.**

### Network Environment

Multi-user interaction depends on network connectivity. For the original prototype, devices were connected to the same Wi-Fi environment during testing.

### Device Performance

The application combines AR tracking, 3D geospatial rendering, and real-time networking. Performance may therefore vary between Android devices.

---

# Research Context

This Unity prototype was developed for:

**Collaborative Geospatial Decision-Making Using XR-Integrated Urban Digital Twins**

> *System usability evaluation and behavioural analysis in multi-user AR environments*

**Pei-Chi Tsai**  
MRes Urban Spatial Science  
Centre for Advanced Spatial Analysis (CASA)  
University College London (UCL)

---

# Related Publication

**Comparing XR-Integrated Urban Digital Twins and 2D Platforms for Collaborative Spatial Decision-Making**

Submitted to *Smart Cities*.

---

# Citation

```bibtex
@thesis{tsai2025collaborative,
  author      = {Tsai, Pei-Chi},
  title       = {Collaborative Geospatial Decision-Making Using XR-Integrated Urban Digital Twins:
                 System usability evaluation and behavioural analysis in multi-user AR environments},
  school      = {University College London},
  institution = {Centre for Advanced Spatial Analysis},
  year        = {2025},
  type        = {MRes Dissertation}
}
```

---

# Acknowledgements

Developed at the **Centre for Advanced Spatial Analysis (CASA), UCL**.

Supervisors:

- Dr. Valerio Signorelli
- Prof. Andy Hudson-Smith
