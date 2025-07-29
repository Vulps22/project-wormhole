# Wormhole Game

A 2D arcade-style game built from scratch in C# using Windows Forms (no game engine).

## Game Concept

- **Player**: A ship controlled with WASD keys
- **Enemies**: Red missile squares that emerge from wormholes
- **Mechanics**: 
  - Missiles bounce off screen edges multiple times before dying
  - Each level spawns 3×level missiles
  - Each wormhole produces up to 5 missiles maximum
  - Wormholes and missile directions are randomized

## How to Run

```bash
dotnet run
```

## Game Rules

- Move your ship with WASD to avoid the red missiles
- Survive as long as possible to progress through levels
- Missiles increase with each level (3*level)
- Good luck!
