using System;
using System.Collections.Generic;
using ProjectWormhole.GameObjects;

namespace ProjectWormhole.Core
{
    // Interface for random number generation to enable testing
    public interface IRandomGenerator
    {
        int Next(int minValue, int maxValue);
        double NextDouble();
    }
    
    // Wrapper for System.Random
    public class SystemRandomGenerator : IRandomGenerator
    {
        private readonly Random _random;
        
        public SystemRandomGenerator(Random? random = null)
        {
            _random = random ?? new Random();
        }
        
        public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);
        public double NextDouble() => _random.NextDouble();
    }

    public class Level
    {
        public int Number { get; private set; }
        public List<Wormhole> Wormholes { get; private set; }
        public List<Missile> Missiles { get; private set; }
        
        private readonly IRandomGenerator _random;
        private int _wormholeSpawnTimer;
        private int _wormholesSpawned;
        private int _missilesSpawned;
        
        // Level configuration - make constants for better testability
        public int MaxMissiles => Number * 3;
        public int MaxWormholes => (MaxMissiles + 4) / 5; // Ceiling division: each wormhole handles up to 5 missiles
        public const int MissilesPerWormhole = 5;
        public const int WormholeSpawnInterval = 120; // 2 seconds at 60 FPS
        
        // Expose private fields for testing
        public int WormholeSpawnTimer => _wormholeSpawnTimer;
        public int WormholesSpawned => _wormholesSpawned;
        public int MissilesSpawned => _missilesSpawned;
        
        public Level(int number, IRandomGenerator? randomGenerator = null)
        {
            if (number <= 0)
                throw new ArgumentException("Level number must be positive", nameof(number));
                
            Number = number;
            _random = randomGenerator ?? new SystemRandomGenerator();
            Wormholes = new List<Wormhole>();
            Missiles = new List<Missile>();
            _wormholeSpawnTimer = 0;
            _wormholesSpawned = 0;
            _missilesSpawned = 0;
        }
        
        public void Update(int gameWidth, int gameHeight)
        {
            if (gameWidth <= 0 || gameHeight <= 0)
                throw new ArgumentException("Game dimensions must be positive");
                
            // Spawn wormholes only when needed (when current wormholes can't handle remaining missiles)
            _wormholeSpawnTimer++;
            if (_wormholeSpawnTimer > WormholeSpawnInterval && ShouldSpawnNewWormhole())
            {
                SpawnWormhole(gameWidth, gameHeight);
                _wormholeSpawnTimer = 0;
                _wormholesSpawned++;
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
                    _missilesSpawned++;
                }
                
                if (Wormholes[i].IsExpired())
                {
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
        
        public void UpdateMissiles(int gameWidth, int gameHeight)
        {
            // Update missiles only (for post-death explosion effects)
            for (int i = Missiles.Count - 1; i >= 0; i--)
            {
                Missiles[i].Update(gameWidth, gameHeight);
                if (Missiles[i].IsExpired())
                {
                    Missiles.RemoveAt(i);
                }
            }
        }
        
        public void UpdateWormholes(int gameWidth, int gameHeight)
        {
            if (gameWidth <= 0 || gameHeight <= 0)
                throw new ArgumentException("Game dimensions must be positive");
                
            // Spawn wormholes only when needed (when current wormholes can't handle remaining missiles)
            _wormholeSpawnTimer++;
            if (_wormholeSpawnTimer > WormholeSpawnInterval && ShouldSpawnNewWormhole())
            {
                SpawnWormhole(gameWidth, gameHeight);
                _wormholeSpawnTimer = 0;
                _wormholesSpawned++;
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
                    _missilesSpawned++;
                }
                
                if (Wormholes[i].IsExpired())
                {
                    Wormholes.RemoveAt(i);
                }
            }
        }
        
        public bool CanSpawnWormhole()
        {
            return _wormholesSpawned < MaxWormholes;
        }
        
        public bool ShouldSpawnNewWormhole()
        {
            // Only spawn a new wormhole if:
            // 1. We haven't reached the max wormholes for this level
            // 2. Current wormholes can't handle all remaining missiles (each can spawn 5 max)
            if (!CanSpawnWormhole()) return false;
            
            int remainingMissiles = MaxMissiles - _missilesSpawned;
            int currentWormholeCapacity = Wormholes.Count * MissilesPerWormhole;
            
            // Get missiles already spawned by current wormholes
            int missilesFromCurrentWormholes = 0;
            foreach (var wormhole in Wormholes)
            {
                missilesFromCurrentWormholes += wormhole.MissilesSpawned;
            }
            
            int availableCapacity = currentWormholeCapacity - missilesFromCurrentWormholes;
            
            return remainingMissiles > availableCapacity;
        }
        
        public bool CanSpawnMissile()
        {
            return _missilesSpawned < MaxMissiles;
        }
        
        public bool IsLevelComplete(bool shouldPrintDebugInfo = false)
        {
            // Only complete when ALL missiles for this level have been spawned AND cleared
            // Don't advance just because wormholes disappeared early
            bool allMissilesSpawned = _missilesSpawned >= MaxMissiles;
            bool allMissilesGone = Missiles.Count == 0;
            if (shouldPrintDebugInfo)
            {
                // Debug info could be logged here if needed
            }
            return allMissilesSpawned && allMissilesGone;
        }

        private void SpawnWormhole(int gameWidth, int gameHeight)
        {
            if (gameWidth <= 0 || gameHeight <= 0)
                throw new ArgumentException("Game dimensions must be positive");
                
            int x = _random.Next(Wormhole.DEFAULT_SIZE, gameWidth - Wormhole.DEFAULT_SIZE);
            int y = _random.Next(Wormhole.DEFAULT_SIZE, gameHeight - Wormhole.DEFAULT_SIZE);
            Wormholes.Add(new Wormhole(x, y));
        }
        
        private void SpawnMissile(Wormhole wormhole)
        {
            if (wormhole == null)
                throw new ArgumentNullException(nameof(wormhole));
                
            // Random direction
            double angle = _random.NextDouble() * 2 * Math.PI;
            double velX = Math.Cos(angle) * Missile.DEFAULT_SPEED;
            double velY = Math.Sin(angle) * Missile.DEFAULT_SPEED;
            
            Missiles.Add(new Missile(wormhole.X, wormhole.Y, velX, velY));
        }
        
        public void Reset()
        {
            Wormholes.Clear();
            Missiles.Clear();
            _wormholeSpawnTimer = 0;
            _wormholesSpawned = 0;
            _missilesSpawned = 0;
        }
    }
}
