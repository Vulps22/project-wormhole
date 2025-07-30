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

## Multiplayer

### Online Competitive Mode

Players compete to see who can survive the longest in the vortex.

#### Power-Ups (Dropped by Wormholes)

- **Bumper-Cart Mode:** If a player bumps into the other, it pushes the opponent away.
- **Bombs:** Explode on use, dealing 5 damage and destroying nearby missiles.
- **Shield:** Missiles bounce off the player. Shields can stack up to 3, creating a faint blue ring around the player.
- **Blink:** Instantly swap positions with the opponent player.

Players can pick up power-ups by moving onto them.