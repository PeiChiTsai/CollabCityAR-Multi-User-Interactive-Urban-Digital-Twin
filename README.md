Please be aware that this README was edited after 22 Aug 2025. Except README, all other files remain the same as it was uploaded on 22 Aug 2025. Should you need to access the old version, please click on [this link](https://github.com/PeiChiTsai/CollabCityAR-Multi-User-Interactive-Urban-Digital-Twin/tree/5367423fbb5a75e67a8d76d6cff6255ca7704854).

# CollabCity AR - Multi-User Interactive Urban Digital Twin

## 🔹 Overview

This repository contains the prototype developed for Pei-Chi Tsai's MRes Dissertation

**“ Collaborative Geospatial Decision-Making Using XR-Integrated Urban Digital Twins** – System usability evaluation and behavioural analysis in multi-user AR environments”

and the paper:

**“Comparing XR-Integrated Urban Digital Twins and 2D Platforms for Collaborative Spatial Decision-Making”** (submitted to Smart Cities).

The prototype demonstrates how a multi-user Augmented Reality Urban Digital Twin (XR-UDT) can support collaborative spatial reasoning by synchronising geospatial data and user interactions across handheld devices.

## 🔹 Features

AR Spatial Alignment – Multi-device consistency using Google ARCore Cloud Anchors.

Geospatial Data Integration – 3D tiles of Queen Elizabeth Olympic Park via Cesium ion.

Multi-User Networking – Real-time peer-to-peer communication using the Ubiq framework.

Interaction Tools – Place, move, and delete route markers; colour-coded by user ID.

System Logs – Automatic logging of all marker events with geospatial coordinates and timestamps.

## 🔹 System Architecture

## 🔹 Installation

1. Clone this repository:
```
git clone https://github.com/PeiChiTsai/CollabCityAR-Multi-User-Interactive-Urban-Digital-Twin.git
```
2. Open in Unity (tested with Unity 2022.3.x LTS).

3. Install dependencies:

    AR Foundation + ARCore Extensions

    Cesium for Unity

    Ubiq Framework

5. Build to Android devices (tested on Pixel series).

## 🔹 Usage

Start the session on one device (host).

Other devices join the same anchor session.

Users can place and edit route markers collaboratively.

Logs are saved automatically in /Logs/ as JSON.

