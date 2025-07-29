using System;
using System.Collections.Generic;
using WormholeGame.GameObjects;

namespace WormholeGame.Core
{
    public class Level
    {
        public int Number { get; private set; }
        public List<Wormhole> Wormholes { get; private set; }
        public List<Missile> Missiles { get; private set; }
        
        private Random random;
        private int wormholeSpawnTimer;
        private int wormholesSpawned;
        private int missilesSpawned;
        
        // Level configuration
        public int MaxMissiles => Number * 3;
        public int MaxWormholes => (MaxMissiles + 4) / 5; // Ceiling division: each wormhole handles up to 5 missiles
        public int MissilesPerWormhole => 5;
        public int WormholeSpawnInterval => 120; // 2 seconds at 60 FPS
        
        public Level(int number)
        {
            Number = number;
            random = new Random();
            Wormholes = new List<Wormhole>();
            Missiles = new List<Missile>();
            wormholeSpawnTimer = 0;
            wormholesSpawned = 0;
            missilesSpawned = 0;
        }
        
        public void Update(int gameWidth, int gameHeight)
        {
            // Spawn wormholes only when needed (when current wormholes can't handle remaining missiles)
            wormholeSpawnTimer++;
            if (wormholeSpawnTimer > WormholeSpawnInterval && ShouldSpawnNewWormhole())
            {
                SpawnWormhole(gameWidth, gameHeight);
                wormholeSpawnTimer = 0;
                wormholesSpawned++;
            }
            
            // Update wormholes and spawn missiles
            for (int i = Wormholes.Count - 1; i >= 0; i--)
            {
                Wormholes[i].Update();
                
                // Check if wormhole should start expiring
                if (Wormholes[i].ShouldExpire())
                {
                    Wormholes[i].StartExpiring();
                }
                
                if (Wormholes[i].ShouldSpawnMissile() && CanSpawnMissile())
                {
                    SpawnMissile(Wormholes[i]);
                    missilesSpawned++;
                    Console.WriteLine($"Level {Number}: Total missiles spawned: {missilesSpawned}/{MaxMissiles}");
                }
                
                if (Wormholes[i].IsExpired())
                {
                    Console.WriteLine($"Level {Number}: Wormhole expired after spawning {Wormholes[i].MissilesSpawned} missiles");
                    Wormholes.RemoveAt(i);
                }
            }
            
            // Update missiles
            for (int i = Missiles.Count - 1; i >= 0; i--)
            {
                Missiles[i].Update(gameWidth, gameHeight);
                if (Missiles[i].IsExpired())
                {
                    Missiles.RemoveAt(i);
                }
            }
        }
        
        public bool CanSpawnWormhole()
        {
            return wormholesSpawned < MaxWormholes;
        }
        
        public bool ShouldSpawnNewWormhole()
        {
            // Only spawn a new wormhole if:
            // 1. We haven't reached the max wormholes for this level
            // 2. Current wormholes can't handle all remaining missiles (each can spawn 5 max)
            if (!CanSpawnWormhole()) return false;
            
            int remainingMissiles = MaxMissiles - missilesSpawned;
            int currentWormholeCapacity = Wormholes.Count * MissilesPerWormhole;
            
            // Get missiles already spawned by current wormholes
            int missilesFromCurrentWormholes = 0;
            foreach (var wormhole in Wormholes)
            {
                missilesFromCurrentWormholes += wormhole.MissilesSpawned;
            }
            
            int availableCapacity = currentWormholeCapacity - missilesFromCurrentWormholes;
            
            Console.WriteLine($"Level {Number}: Remaining missiles: {remainingMissiles}, Available capacity: {availableCapacity}");
            
            return remainingMissiles > availableCapacity;
        }
        
        public bool CanSpawnMissile()
        {
            return missilesSpawned < MaxMissiles;
        }
        
        public bool IsLevelComplete(bool shouldPrintDebugInfo = false)
        {
            // Only complete when ALL missiles for this level have been spawned AND cleared
            // Don't advance just because wormholes disappeared early
            bool allMissilesSpawned = missilesSpawned >= MaxMissiles;
            bool allMissilesGone = Missiles.Count == 0;
            if (shouldPrintDebugInfo)
            {
                Console.WriteLine($"Level {Number} complete check: Missiles spawned: {missilesSpawned}, Missiles left: {Missiles.Count}");
                Console.WriteLine($"Level {Number} complete: {allMissilesSpawned && allMissilesGone}");
            }
            return allMissilesSpawned && allMissilesGone;
        }

        private void SpawnWormhole(int gameWidth, int gameHeight)
        {
            int x = random.Next(Wormhole.DEFAULT_SIZE, gameWidth - Wormhole.DEFAULT_SIZE);
            int y = random.Next(Wormhole.DEFAULT_SIZE, gameHeight - Wormhole.DEFAULT_SIZE);
            Wormholes.Add(new Wormhole(x, y));
            
            Console.WriteLine($"Level {Number}: Spawned wormhole #{wormholesSpawned + 1}/{MaxWormholes} (can handle {MissilesPerWormhole} missiles)");
        }
        
        private void SpawnMissile(Wormhole wormhole)
        {
            // Random direction
            double angle = random.NextDouble() * 2 * Math.PI;
            double velX = Math.Cos(angle) * Missile.DEFAULT_SPEED;
            double velY = Math.Sin(angle) * Missile.DEFAULT_SPEED;
            
            Missiles.Add(new Missile(wormhole.X, wormhole.Y, velX, velY));
            
            Console.WriteLine($"Level {Number}: Spawned missile #{missilesSpawned + 1}/{MaxMissiles}");
        }
        
        public void Reset()
        {
            Wormholes.Clear();
            Missiles.Clear();
            wormholeSpawnTimer = 0;
            wormholesSpawned = 0;
            missilesSpawned = 0;
        }
    }
}
