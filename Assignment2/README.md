# CS 426 Assignment 4 – Physics, Textures, and Lights
**Name:** Syed Daanish Ismail  
**Course:** CS 426  
**Assignment:** Assignment 4

## Overview
This project extends my previous Assignment 2 level by adding new physics-based interactions, a textured billboard, and additional lighting. The overall design of the level is a sci-fi/computer-themed environment with interactive gameplay elements that affect how the player moves through the space.

## Overall Design
The level is designed around a compact obstacle-course style layout inside a sci-fi themed environment. The player navigates through the scene while interacting with different gameplay elements. The design goal was to make the environment feel more interactive and visually clear by using colored surfaces and lights to communicate gameplay meaning.

The red and blue surfaces are used as special movement zones. Their matching colored lights help the player quickly recognize the function of each area. The billboard adds environmental texture and supports the visual theme of the level.

## Physics Constructs Implemented

### 1. Collision-Based Moving Ball Obstacle
One physics construct is a moving ball obstacle placed in the level. The ball moves back and forth, and when the player collides with it, the collision pushes the player backward. This adds a physical gameplay hazard and satisfies the collision-based physics requirement.

### 2. Slippery Surface Zone
A blue slippery surface was added to the level. When the player enters this zone, movement becomes more slippery and less controlled, causing the player to slide more than normal. This creates a different physical response and changes how the player navigates the level.

### 3. Grip Surface Zone
A red grip surface was also added. When the player enters this zone, movement becomes more controlled and traction increases. Compared to the slippery surface, this zone allows tighter movement and faster stopping.

## Billboard
A textured billboard was added to the level as part of the environment design. The billboard helps make the scene feel more visually complete and supports the sci-fi/computer-themed setting by adding an extra textured visual element beyond the base level geometry.

## Lights
Two additional lights were added to the level beyond Assignment 2:

- **Red point light:** placed above the red grip zone
- **Blue point light:** placed above the blue slippery zone

These lights help reinforce the purpose of the surfaces through color and improve the visual readability of the level. At least one of these lights is stationary and colored according to the level design, satisfying the assignment requirement.

## How the Features Fit the Design
All of the added features were chosen to work together as part of the same overall level concept. The moving ball adds a gameplay obstacle, the slippery and grip zones add different movement behaviors, the billboard improves the environmental presentation, and the colored lights visually connect to the special floor surfaces. Together, these elements make the level more interactive, readable, and polished.

## Files Included
This repository includes:
- Unity project files
- Assignment 4 source code and assets
- Output folder with the built runnable version of the project