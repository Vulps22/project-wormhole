using System;
using System.Collections.Generic;
using System.Drawing;
using WormholeGame.GameObjects;

namespace WormholeGame.Core
{
    public class GameState
    {
        public Player Player { get; private set; } = null!;
        public List<Wormhole> Wormholes { get; private set; } = null!;
        public List<Missile> Missiles { get; private set; } = null!;
        public int Level { get; private set; }
        public int Score { get; private set; }
        public bool IsRunning { get; private set; }
        
        private Random random;
        private int wormholeSpawnTimer;
        private int levelTimer;

        private int wormholesSpawned;
        private int missilesSpawned;
        
        // Game constants (only game-level stuff here)
        public const int GAME_WIDTH = 800;
        public const int GAME_HEIGHT = 600;
        
        public GameState()
        {
            random = new Random();
            InitializeGame();
        }
        
        public void InitializeGame()
        {
            Level = 1;
            Score = 0;
            IsRunning = true;
            wormholeSpawnTimer = 0;
            levelTimer = 0;
            
            Player = new Player(GAME_WIDTH / 2, GAME_HEIGHT / 2);
            Wormholes = new List<Wormhole>();
            Missiles = new List<Missile>();

            wormholesSpawned = 0;
            missilesSpawned = 0;
        }
        
        public void UpdateGame()
        {
            if (!IsRunning) return;
            // Spawn wormholes periodically (but only up to current level)
            wormholeSpawnTimer++;
            if (wormholeSpawnTimer > 120 && wormholesSpawned < Level) // Every 2 seconds at 60 FPS, max = level
            {
                SpawnWormhole();
                wormholeSpawnTimer = 0;
                wormholesSpawned++;
            }
            
            // Update wormholes and spawn missiles
            for (int i = Wormholes.Count - 1; i >= 0; i--)
            {
                Wormholes[i].Update();
                if (Wormholes[i].ShouldSpawnMissile())
                {
                    SpawnMissile(Wormholes[i]);
                    missilesSpawned++;
                }
                if (Wormholes[i].IsExpired())
                {
                    Wormholes.RemoveAt(i);
                }
            }
            
            // Update missiles
            for (int i = Missiles.Count - 1; i >= 0; i--)
            {
                Missiles[i].Update(GAME_WIDTH, GAME_HEIGHT);
                if (Missiles[i].IsExpired())
                {
                    Missiles.RemoveAt(i);
                }
            }
            
            // Level progression
            levelTimer++;
            if (levelTimer > 1800) // 30 seconds per level
            {
                Level++;
                levelTimer = 0;
            }
        }
        
        public void MovePlayer(int deltaX, int deltaY)
        {
            Player.Move(deltaX, deltaY, GAME_WIDTH, GAME_HEIGHT);
        }
        
        public bool CheckCollisions()
        {
            Rectangle playerRect = new Rectangle(Player.X - Player.Size/2, Player.Y - Player.Size/2, 
                                               Player.Size, Player.Size);
            
            foreach (var missile in Missiles)
            {
                Rectangle missileRect = new Rectangle(missile.X - missile.Size/2, missile.Y - missile.Size/2,
                                                    missile.Size, missile.Size);
                
                if (playerRect.IntersectsWith(missileRect))
                {
                    return true; // Collision detected
                }
            }
            return false;
        }
        
        public void GameOver()
        {
            IsRunning = false;
        }
        
        public void RestartGame()
        {
            InitializeGame();
        }
        
        private void SpawnWormhole()
        {
            int x = random.Next(Wormhole.DEFAULT_SIZE, GAME_WIDTH - Wormhole.DEFAULT_SIZE);
            int y = random.Next(Wormhole.DEFAULT_SIZE, GAME_HEIGHT - Wormhole.DEFAULT_SIZE);
            Wormholes.Add(new Wormhole(x, y));
        }
        
        private bool canLevelSpawnMissile()
        {
            return missilesSpawned < Level * 3; // Level * 3 missiles max
        }

        private void SpawnMissile(Wormhole wormhole)
        {
            if (!canLevelSpawnMissile()) return; // Level * 3 missiles max
            
            // Random direction
            double angle = random.NextDouble() * 2 * Math.PI;
            double velX = Math.Cos(angle) * Missile.DEFAULT_SPEED;
            double velY = Math.Sin(angle) * Missile.DEFAULT_SPEED;
            
            Missiles.Add(new Missile(wormhole.X, wormhole.Y, velX, velY));
        }
    }
}
