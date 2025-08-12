# HSOMP Holographic Visualizer

This repository is part of the **HSOMP (Hologram Stability on Moving Platform)** project.  
It contains a **Unity + MRTK** application for the HoloLens 2 that receives position and rotation data from an external camera (via UDP) and uses it to **stabilize holograms**, even when the **SLAM** and **IMU** sensors fail.


## 📌 Problem
On moving platforms (e.g., vehicles, tanks), HoloLens 2 **IMU sensors** may produce drift errors.  
If SLAM tracking is also degraded, holograms will drift and lose alignment.  
By receiving **external tracking data** from a camera that detects a QR code on the headset, this system can maintain stable hologram placement.

## 🚀 Features
- **Unity MRTK-based HoloLens app**  
- Receives **UDP position and rotation** data from an external tracker  
- Places holograms in the correct location and orientation  
- Works even if **SLAM** and **IMU** fail


## 📦 Technologies
- **HoloLens 2**
- **Unity**
- **Mixed Reality Toolkit (MRTK)**
- **UDP Networking**


## 🔧 Installation
```bash
git clone https://github.com/xr-internship-team/hsomp-holographic-visualizer.git
```
1. Open the project in **Unity**.  
2. Install the **Mixed Reality Toolkit (MRTK)** package.  
3. Configure the UDP receiver with the correct IP and port.  
4. Build and deploy the application to HoloLens 2.

## ▶ Usage
1. Start the **outside marker tracking** system from the companion repository:  
   [hsomp-outside-marker-tracking](https://github.com/xr-internship-team/hsomp-outside-marker-tracking)  
2. Deploy and run the **holographic visualizer** on HoloLens 2.  
3. When the HoloLens receives external tracking data, holograms will remain **stable** even if SLAM/IMU tracking degrades.

<p align="center">
  <img src="./docs/demo_video.gif" alt="Demo Video" />
</p>

## 📜 License
This project is licensed under the terms specified in the repository.
