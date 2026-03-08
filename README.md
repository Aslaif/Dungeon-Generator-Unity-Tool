# Unity Dungeon Generator

Procedural dungeon generator tool for Unity with multithreaded blueprint generation.  
Object instantiation runs on the main thread due to Unity’s threading limitations.

---

## Preview

![Dungeon Generation](images/dungeon_generation.gif)

![Generated Dungeon](images/dungeon_example.png)
<img width="913" height="500" alt="Bild 3" src="https://github.com/user-attachments/assets/064a3c8d-ae29-44bf-934a-2722ea09f0e4" />

---

## Overview

This project is a dungeon generator tool created for the Unity Editor.  
The generator creates procedural dungeon layouts based on configurable parameters such as map size, room count and dungeon levels.

The blueprint generation can run in multiple threads.  
Due to Unity's threading restrictions, the final instantiation of objects is executed on the main thread.

---

## How to Use

The dungeon generator can be opened inside the Unity Editor: Window/Dungeon Generator

The tool consists of two main tabs used for configuring and generating dungeons.

---

## Tab 1 – Generation Settings

The first tab controls the procedural generation settings.

Options include:

- **Random Seed**  
  Defines the seed used for generation.  
  If the seed is set to `0`, the generator uses `Time.deltaTime` to create a random seed.

- **Multithreading Toggle**  
  Enables multithreaded dungeon blueprint generation.

- **Map Width / Height**  
  Defines the overall size of the generated dungeon layout.

- **Dungeon Levels**  
  Determines how many layers or floors the dungeon will contain.

- **Minimum Room Count**  
  Defines the minimum number of rooms that should appear in the generated dungeon.

---

## Tab 2 – Prefab Configuration

The second tab is used to configure the prefabs used for generation.

Users can assign different prefabs to lists that are used during dungeon generation.

Options include:

- Room prefab selection
- Element width and height configuration
- Assigning different prefabs for room generation

These prefabs are then used as building blocks when the dungeon is generated.

---

## Example Generation

![Generation Example](images/generation_example.gif)

---

## Technologies

- Unity
- C#
- Multithreading
- Procedural generation

---

## Project Goal

The goal of this project was to experiment with procedural level generation and multithreaded processing inside the Unity Editor.
