# CollabCity AR - Multi-User Interactive Urban Digital Twin

A Unity-based multi-user Augmented Reality (AR) prototype for collaborative interaction with a geospatial Urban Digital Twin (UDT).

## Overview

**CollabCity AR** is a multi-user AR prototype developed as part of Pei-Chi Tsai's MRes research at the Centre for Advanced Spatial Analysis (CASA), UCL.

The project explores how an AR-based Urban Digital Twin can provide a shared spatial environment in which multiple users can visualise geospatial data and interact with spatial objects in real time.

The prototype integrates:

- **Google ARCore** for spatial alignment between devices
- **Cesium for Unity** for geospatial visualisation
- **Ubiq** for real-time multi-user networking
- **Unity** for AR interaction and application development

The current implementation uses Queen Elizabeth Olympic Park, London, as the geospatial context.

## Features

### AR Spatial Alignment

Uses **Google ARCore Cloud Anchors** to establish a shared spatial reference between multiple devices.

Users can either:

- Host a new Cloud Anchor
- Resolve an existing Cloud Anchor using its ID

This allows virtual content to be positioned consistently within the shared physical environment.

### Geospatial Visualisation

Uses **Cesium for Unity** and **Cesium ion** to stream geospatial 3D content.

The prototype includes:

- Google Photorealistic 3D Tiles
- Real-world geographic coordinates
- Georeferenced 3D terrain and buildings
- Spatial Point-of-Interest (POI) visualisation

### Multi-User Networking

Uses the **Ubiq networking framework** to synchronise user interactions across devices.

The system supports real-time synchronisation of:

- User identity
- Object creation
- Object position
- Object rotation
- Object state

Object transformations are synchronised using coordinates relative to the shared AR anchor.

### Interactive Spatial Markers

Users can place and manipulate numbered spatial markers within the AR environment.

Markers support:

- Placement
- Movement
- Rotation
- Scaling
- Deletion

Visual states are used to communicate interaction and ownership between users. Each user's objects are assigned a corresponding user colour, while selected and actively manipulated objects are visually distinguished.

### Geospatial Interaction Logging

The prototype records spatial interaction events for subsequent analysis.

Logged information includes:

- Timestamp
- Object identity
- User identity
- Longitude
- Latitude
- Elevation
- Interaction state

## System Architecture

The prototype consists of four main components:

```text
                 ┌───────────────────────┐
                 │   Google ARCore       │
                 │   Cloud Anchors       │
                 └───────────┬───────────┘
                             │
                             ▼
                 ┌───────────────────────┐
                 │   Shared AR Space     │
                 │   Spatial Alignment   │
                 └───────────┬───────────┘
                             │
              ┌──────────────┴──────────────┐
              │                             │
              ▼                             ▼
   ┌────────────────────┐        ┌────────────────────┐
   │ Cesium for Unity   │        │ Ubiq Networking    │
   │                    │        │                    │
   │ 3D Tiles           │        │ Multi-user sync    │
   │ Terrain            │        │ Object states      │
   │ Buildings          │        │ User identity      │
   └─────────┬──────────┘        └─────────┬──────────┘
             │                             │
             └──────────────┬──────────────┘
                            ▼
                 ┌───────────────────────┐
                 │  AR Interaction       │
                 │  & Spatial Markers    │
                 └───────────┬───────────┘
                             │
                             ▼
                 ┌───────────────────────┐
                 │   Interaction Logs    │
                 │   + Geospatial Data   │
                 └───────────────────────┘
```

## Technology Stack

| Component | Technology |
|---|---|
| Game / AR Engine | Unity 6000.0.46f1 |
| AR Framework | AR Foundation |
| Spatial Tracking | Google ARCore |
| Spatial Synchronisation | ARCore Cloud Anchors |
| Geospatial Visualisation | Cesium for Unity |
| 3D Geospatial Data | Google Photorealistic 3D Tiles |
| Multi-User Networking | Ubiq |
| Platform | Android |
| Device Testing | Google Pixel 6a / Pixel 7 |
| Programming | C# |

The prototype was developed using Unity `6000.0.46f1` and deployed on Android devices supporting ARCore.

## Installation

### 1. Clone the repository

```bash
git clone https://github.com/PeiChiTsai/CollabCityAR-Multi-User-Interactive-Urban-Digital-Twin.git
```

### 2. Open the project in Unity

Open the repository using:

```text
Unity 6000.0.46f1
```

### 3. Install / configure dependencies

The project requires:

- AR Foundation
- Google ARCore
- ARCore Extensions
- Cesium for Unity
- Ubiq

### 4. Configure external services

The prototype requires authentication/configuration for external services, including:

- **Google ARCore API / Cloud Anchors**
- **Cesium ion**

API credentials and access tokens should be configured locally and should **not** be committed to the repository.

### 5. Build for Android

Build and deploy the project to an ARCore-compatible Android device.

The original prototype was tested on Google Pixel 6a and Pixel 7 devices running Android 12+. Stable internet connectivity is required for the networking workflow.

## Usage

### Host a Shared AR Session

1. Launch the application.
2. Select **Host a New Anchor**.
3. Detect a suitable surface.
4. Place the anchor.
5. Scan the surrounding environment until sufficient visual features are detected.
6. Upload the anchor to ARCore.
7. Share the generated Cloud Anchor ID with other devices.

### Join an Existing Session

1. Launch the application.
2. Select **Resolve an Existing Cloud Anchor**.
3. Enter the Cloud Anchor ID.
4. Scan the environment.
5. Allow the system to resolve the shared anchor.
6. Enter the shared multi-user session.

Once synchronised, users can interact with the same geospatial environment and shared spatial objects.

## Spatial Synchronisation

The prototype uses a three-stage synchronisation workflow:

```text
ARCore Cloud Anchor
        ↓
Shared Spatial Reference
        ↓
Anchor-Relative Coordinates
        ↓
Ubiq Network Synchronisation
        ↓
Consistent Object State
```

Object positions and rotations are converted into an anchor-relative coordinate system before being transmitted through Ubiq. Receiving devices reconstruct the corresponding world-space transform using their locally resolved Cloud Anchor.

This approach separates **physical spatial alignment** from **networked interaction synchronisation**.

## Geospatial Data

The 3D environment is visualised using **Cesium for Unity**.

The prototype uses:

- Cesium ion
- Google Photorealistic 3D Tiles
- Cesium Georeference
- Geographic coordinates in WGS84

The study environment is centred on **Queen Elizabeth Olympic Park, London**, with additional georeferenced POI layers for contextual spatial interaction.

## Project Context

This prototype was developed for the following research project:

**Collaborative Geospatial Decision-Making Using XR-Integrated Urban Digital Twins**

The implementation focuses on the technical development of a shared AR environment for geospatial visualisation and multi-user spatial interaction.

For the research methodology, behavioural analysis, user evaluation, and findings, please refer to the associated dissertation.

## Dissertation

**Pei-Chi Tsai (2025)**  
*Collaborative Geospatial Decision-Making Using XR-Integrated Urban Digital Twins: System usability evaluation and behavioural analysis in multi-user AR environments.*

MRes Dissertation  
Centre for Advanced Spatial Analysis (CASA), UCL

## Related Paper

**Comparing XR-Integrated Urban Digital Twins and 2D Platforms for Collaborative Spatial Decision-Making**

Submitted to *Smart Cities*.

## Citation

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

## Acknowledgements

Developed at the **Centre for Advanced Spatial Analysis (CASA), UCL**.

Supervisors:

- Dr. Valerio Signorelli
- Prof. Andy Hudson-Smith
