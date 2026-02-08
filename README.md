# 🏀 Basket Challenge
A physics-based mobile basketball prototype developed in Unity.

This project was originally developed as part of a technical evaluation to deliver in approximately two weeks of work.

## 🎮 Overview
Basket Challenge is a simple arcade experience where the player launches a ball toward a hoop using touch input.

The project implements the core gameplay loop with a clear, component-based architecture.

## 📱 Features

- Physics-driven shooting mechanic
- Touch input based interaction
- Collision detection based scoring system
- Real-time score UI feedback
- Simple AI opponent with RNG-driven logic

## 🛠 Technical Approach

- **Component-driven gameplay design**  
  Gameplay responsibilities are separated into independent MonoBehaviour components to preserve separation of concerns.

- **Event-driven interaction**  
  Gameplay entities communicate through triggers and controlled references to reduce system coupling.

- **Prefab-based entity workflow**  
  Core gameplay objects are built as reusable prefabs to simplify iteration and maintain consistency.

- **Physics-driven mechanics**  
  Shooting and scoring rely on Unity’s physics system rather than geometry/animation-driven logic.

- **Centralized gameplay state management**  
  Scoring logic is handled through a dedicated manager to maintain consistent game state control.

- **Data-driven approach**  
  Where feasible, logic and data are decoupled using ScriptableObjects (e.g., game configs, AI parameters, game results).

### Requirements

The project was developed using Unity 2022 LTS version.

## Development Notes

This project was developed under a limited time frame, primarily outside working hours.
Below are a few contextual notes and design choices made during development:

- Assets were intentionally kept simple with a fun prototype look, to focus on gameplay systems and architecture.

- Some particle textures and shaders were manually created, and sound effects were edited (trimmed, equalized, normalized) to match the intended game feel.

- LINQ was occasionally used for initialization or readability, but never in performance-critical paths.
  
- The build has been tested with success on a OnePlus 8T device with Android 14.

- The physics-based shooting system may still need minor tuning or adjustments to ensure consistent backboard shot behavior across all shooting positions.

- I made my best effort to catch and fix all visible or critical bugs🐛 :]
