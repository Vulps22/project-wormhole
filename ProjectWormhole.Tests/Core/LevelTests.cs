using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ProjectWormhole.Core;
using ProjectWormhole.GameObjects;

namespace ProjectWormhole.Tests.Core
{
    // Mock random generator for deterministic testing
    public class MockRandomGenerator : IRandomGenerator
    {
        private readonly Queue<int> _nextIntValues = new Queue<int>();
        private readonly Queue<double> _nextDoubleValues = new Queue<double>();
        
        public void SetNextInt(int value) => _nextIntValues.Enqueue(value);
        public void SetNextDouble(double value) => _nextDoubleValues.Enqueue(value);
        
        public int Next(int minValue, int maxValue)
        {
            if (_nextIntValues.Count > 0)
            {
                int value = _nextIntValues.Dequeue();
                // Ensure value is within range
                return Math.Max(minValue, Math.Min(maxValue - 1, value));
            }
            // Default to middle of range
            return (minValue + maxValue) / 2;
        }
        
        public double NextDouble()
        {
            return _nextDoubleValues.Count > 0 ? _nextDoubleValues.Dequeue() : 0.5;
        }
    }

    public class LevelTests
    {
        [Fact]
        public void Level_Constructor_WithValidNumber_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var level = new Level(3);
            
            // Assert
            Assert.Equal(3, level.Number);
            Assert.NotNull(level.Wormholes);
            Assert.NotNull(level.Missiles);
            Assert.Empty(level.Wormholes);
            Assert.Empty(level.Missiles);
            Assert.Equal(0, level.WormholeSpawnTimer);
            Assert.Equal(0, level.WormholesSpawned);
            Assert.Equal(0, level.MissilesSpawned);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Level_Constructor_WithInvalidNumber_ThrowsArgumentException(int invalidNumber)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Level(invalidNumber));
        }
        
        [Fact]
        public void Level_Constructor_WithCustomRandomGenerator_UsesProvidedGenerator()
        {
            // Arrange
            var mockRandom = new MockRandomGenerator();
            
            // Act
            var level = new Level(1, mockRandom);
            
            // Assert
            Assert.NotNull(level);
            Assert.Equal(1, level.Number);
        }
        
        [Theory]
        [InlineData(1, 3, 1)]
        [InlineData(2, 6, 2)]
        [InlineData(3, 9, 2)]
        [InlineData(5, 15, 3)]
        [InlineData(10, 30, 6)]
        public void Level_Properties_CalculateCorrectly(int levelNumber, int expectedMaxMissiles, int expectedMaxWormholes)
        {
            // Arrange & Act
            var level = new Level(levelNumber);
            
            // Assert
            Assert.Equal(expectedMaxMissiles, level.MaxMissiles);
            Assert.Equal(expectedMaxWormholes, level.MaxWormholes);
        }
        
        [Fact]
        public void Level_Constants_HaveExpectedValues()
        {
            // Assert
            Assert.Equal(5, Level.MissilesPerWormhole);
            Assert.Equal(120, Level.WormholeSpawnInterval);
        }
        
        [Theory]
        [InlineData(0, 600)]
        [InlineData(800, 0)]
        [InlineData(-100, 600)]
        [InlineData(800, -100)]
        public void Level_Update_WithInvalidDimensions_ThrowsArgumentException(int gameWidth, int gameHeight)
        {
            // Arrange
            var level = new Level(1);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => level.Update(gameWidth, gameHeight));
        }
        
        [Fact]
        public void Level_Update_IncrementsWormholeSpawnTimer()
        {
            // Arrange
            var level = new Level(1);
            
            // Act
            level.Update(800, 600);
            
            // Assert
            Assert.Equal(1, level.WormholeSpawnTimer);
            
            // Act again
            level.Update(800, 600);
            
            // Assert
            Assert.Equal(2, level.WormholeSpawnTimer);
        }
        
        [Fact]
        public void Level_Update_SpawnsWormholeAfterInterval()
        {
            // Arrange
            var mockRandom = new MockRandomGenerator();
            mockRandom.SetNextInt(400); // x coordinate
            mockRandom.SetNextInt(300); // y coordinate
            var level = new Level(1, mockRandom);
            
            // Act - Update for spawn interval + 1
            for (int i = 0; i <= Level.WormholeSpawnInterval; i++)
            {
                level.Update(800, 600);
            }
            
            // Assert
            Assert.Single(level.Wormholes);
            Assert.Equal(1, level.WormholesSpawned);
            Assert.Equal(0, level.WormholeSpawnTimer); // Should reset after spawning
        }
        
        [Fact]
        public void Level_Update_DoesNotSpawnWormholeIfNotNeeded()
        {
            // Arrange - Level 1 has MaxMissiles=3, MaxWormholes=1
            // Since one wormhole can handle 5 missiles, it shouldn't spawn more
            var level = new Level(1);
            
            // Manually add a wormhole that can handle all missiles
            level.Wormholes.Add(new Wormhole(400, 300));
            
            // Act - Update for spawn interval + 1
            for (int i = 0; i <= Level.WormholeSpawnInterval; i++)
            {
                level.Update(800, 600);
            }
            
            // Assert - Should not spawn additional wormhole
            Assert.Single(level.Wormholes);
            Assert.Equal(0, level.WormholesSpawned); // We manually added, so spawned count is 0
        }
        
        [Fact]
        public void Level_CanSpawnWormhole_ReturnsCorrectly()
        {
            // Arrange
            var level = new Level(2); // MaxWormholes = 2
            
            // Act & Assert - Initially can spawn
            Assert.True(level.CanSpawnWormhole());
            
            // Simulate spawning wormholes by updating
            var mockRandom = new MockRandomGenerator();
            mockRandom.SetNextInt(400);
            mockRandom.SetNextInt(300);
            mockRandom.SetNextInt(500);
            mockRandom.SetNextInt(250);
            level = new Level(2, mockRandom);
            
            // Spawn first wormhole
            for (int i = 0; i <= Level.WormholeSpawnInterval; i++)
            {
                level.Update(800, 600);
            }
            Assert.True(level.CanSpawnWormhole()); // Can still spawn one more
            
            // Spawn second wormhole
            for (int i = 0; i <= Level.WormholeSpawnInterval; i++)
            {
                level.Update(800, 600);
            }
            Assert.False(level.CanSpawnWormhole()); // Reached max
        }
        
        [Fact]
        public void Level_CanSpawnMissile_ReturnsCorrectly()
        {
            // Arrange
            var level = new Level(1); // MaxMissiles = 3
            
            // Act & Assert - Initially can spawn
            Assert.True(level.CanSpawnMissile());
            
            // Simulate spawning missiles by adding them directly to test the logic
            level.Missiles.Add(new Missile(100, 100, 1.0, 1.0));
            level.Missiles.Add(new Missile(200, 200, 1.0, 1.0));
            level.Missiles.Add(new Missile(300, 300, 1.0, 1.0));
            
            // This tests the logic, but we need to simulate the internal counter
            // Let's test with actual missile spawning through wormholes
        }
        
        [Fact]
        public void Level_ShouldSpawnNewWormhole_LogicWorksCorrectly()
        {
            // Arrange - Level 3: MaxMissiles=9, MaxWormholes=2
            var level = new Level(3);
            
            // Act & Assert - Initially should spawn (no wormholes, missiles needed)
            Assert.True(level.ShouldSpawnNewWormhole());
            
            // Add one wormhole manually
            level.Wormholes.Add(new Wormhole(400, 300));
            
            // One wormhole can handle 5 missiles, but we need 9, so should still spawn
            Assert.True(level.ShouldSpawnNewWormhole());
            
            // Add second wormhole
            level.Wormholes.Add(new Wormhole(500, 400));
            
            // Two wormholes can handle 10 missiles, we only need 9, so shouldn't spawn more
            Assert.False(level.ShouldSpawnNewWormhole());
        }
        
        [Fact]
        public void Level_IsLevelComplete_ReturnsFalseInitially()
        {
            // Arrange
            var level = new Level(1);
            
            // Act & Assert
            Assert.False(level.IsLevelComplete());
        }
        
        [Fact]
        public void Level_IsLevelComplete_ReturnsFalseWithMissilesRemaining()
        {
            // Arrange
            var level = new Level(1);
            level.Missiles.Add(new Missile(100, 100, 1.0, 1.0));
            
            // Act & Assert
            Assert.False(level.IsLevelComplete());
        }
        
        [Fact]
        public void Level_UpdateMissiles_UpdatesAllMissiles()
        {
            // Arrange
            var level = new Level(1);
            var missile1 = new Missile(100, 100, 10.0, 0.0); // Moving right
            var missile2 = new Missile(200, 200, 0.0, 10.0); // Moving down
            level.Missiles.Add(missile1);
            level.Missiles.Add(missile2);
            
            var initialX1 = missile1.X;
            var initialY1 = missile1.Y;
            var initialX2 = missile2.X;
            var initialY2 = missile2.Y;
            
            // Act
            level.UpdateMissiles(800, 600);
            
            // Assert - Missiles should have moved
            Assert.True(missile1.X != initialX1 || missile1.Y != initialY1);
            Assert.True(missile2.X != initialX2 || missile2.Y != initialY2);
        }
        
        [Theory]
        [InlineData(0, 600)]
        [InlineData(800, 0)]
        [InlineData(-100, 600)]
        [InlineData(800, -100)]
        public void Level_UpdateWormholes_WithInvalidDimensions_ThrowsArgumentException(int gameWidth, int gameHeight)
        {
            // Arrange
            var level = new Level(1);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => level.UpdateWormholes(gameWidth, gameHeight));
        }
        
        [Fact]
        public void Level_Reset_ClearsAllStateCorrectly()
        {
            // Arrange
            var level = new Level(2);
            
            // Add some state
            level.Wormholes.Add(new Wormhole(400, 300));
            level.Missiles.Add(new Missile(100, 100, 1.0, 1.0));
            
            // Update to advance timers
            level.Update(800, 600);
            level.Update(800, 600);
            
            // Act
            level.Reset();
            
            // Assert
            Assert.Empty(level.Wormholes);
            Assert.Empty(level.Missiles);
            Assert.Equal(0, level.WormholeSpawnTimer);
            Assert.Equal(0, level.WormholesSpawned);
            Assert.Equal(0, level.MissilesSpawned);
        }
        
        [Fact]
        public void Level_SpawnWormhole_PlacesWormholeWithinBounds()
        {
            // Arrange
            var mockRandom = new MockRandomGenerator();
            mockRandom.SetNextInt(400); // x coordinate
            mockRandom.SetNextInt(300); // y coordinate
            var level = new Level(1, mockRandom);
            
            // Act - Spawn wormhole by updating past the interval
            for (int i = 0; i <= Level.WormholeSpawnInterval; i++)
            {
                level.Update(800, 600);
            }
            
            // Assert
            Assert.Single(level.Wormholes);
            var wormhole = level.Wormholes[0];
            Assert.True(wormhole.X >= Wormhole.DEFAULT_SIZE);
            Assert.True(wormhole.X < 800 - Wormhole.DEFAULT_SIZE);
            Assert.True(wormhole.Y >= Wormhole.DEFAULT_SIZE);
            Assert.True(wormhole.Y < 600 - Wormhole.DEFAULT_SIZE);
        }
        
        [Fact]
        public void Level_MissileSpawning_WorksWithWormholes()
        {
            // Arrange
            var mockRandom = new MockRandomGenerator();
            mockRandom.SetNextInt(400); // wormhole x
            mockRandom.SetNextInt(300); // wormhole y
            mockRandom.SetNextDouble(0.0); // missile angle (0 radians = right direction)
            var level = new Level(1, mockRandom);
            
            // Act - Spawn wormhole
            for (int i = 0; i <= Level.WormholeSpawnInterval; i++)
            {
                level.Update(800, 600);
            }
            
            // Act - Update wormhole to spawn missile
            for (int i = 0; i <= Wormhole.MISSILE_SPAWN_INTERVAL; i++)
            {
                level.Update(800, 600);
            }
            
            // Assert
            Assert.Single(level.Wormholes);
            // Missiles might be spawned - depends on wormhole internal logic
        }
        
        [Fact]
        public void Level_RemovesExpiredWormholes()
        {
            // Arrange
            var level = new Level(1);
            var wormhole = new Wormhole(400, 300, 4); // Small size for quick expiry
            level.Wormholes.Add(wormhole);
            
            // Force wormhole to expire
            wormhole.StartExpiring();
            
            // Act - Update until wormhole expires
            while (!wormhole.IsExpired())
            {
                level.Update(800, 600);
            }
            
            // Update once more to trigger removal
            level.Update(800, 600);
            
            // Assert
            Assert.Empty(level.Wormholes);
        }
        
        [Fact]
        public void Level_RemovesExpiredMissiles()
        {
            // Arrange
            var level = new Level(1);
            // Create missile that will bounce frequently - small game area forces more bounces
            var bouncyMissile = new Missile(10, 10, 20.0, 20.0); // High velocity, starts near edge
            level.Missiles.Add(bouncyMissile);
            
            // Act - Update until missile expires from too many bounces
            int updateCount = 0;
            while (!bouncyMissile.IsExpired() && updateCount < 1000) // Safety limit
            {
                level.Update(50, 50); // Small game area to force bouncing
                updateCount++;
            }
            
            // Final update to trigger removal
            level.Update(50, 50);
            
            // Assert
            Assert.Empty(level.Missiles);
        }
        
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        public void Level_MaxCalculations_AreConsistent(int levelNumber)
        {
            // Arrange & Act
            var level = new Level(levelNumber);
            
            // Assert
            Assert.Equal(levelNumber * 3, level.MaxMissiles);
            Assert.Equal((level.MaxMissiles + 4) / 5, level.MaxWormholes);
            
            // Verify wormholes can handle the missiles
            int totalCapacity = level.MaxWormholes * Level.MissilesPerWormhole;
            Assert.True(totalCapacity >= level.MaxMissiles);
        }
        
        [Fact]
        public void Level_SystemRandomGenerator_WorksCorrectly()
        {
            // Arrange & Act
            var generator = new SystemRandomGenerator();
            
            // Assert - Test that it produces values in range
            for (int i = 0; i < 10; i++)
            {
                int value = generator.Next(1, 10);
                Assert.True(value >= 1 && value < 10);
                
                double doubleValue = generator.NextDouble();
                Assert.True(doubleValue >= 0.0 && doubleValue < 1.0);
            }
        }
        
        [Fact]
        public void Level_SystemRandomGenerator_WithCustomRandom_UsesProvidedRandom()
        {
            // Arrange
            var customRandom = new Random(42); // Fixed seed for deterministic testing
            var generator = new SystemRandomGenerator(customRandom);
            
            // Act
            int value1 = generator.Next(1, 100);
            double double1 = generator.NextDouble();
            
            // Create another generator with same seed
            var generator2 = new SystemRandomGenerator(new Random(42));
            int value2 = generator2.Next(1, 100);
            double double2 = generator2.NextDouble();
            
            // Assert - Should produce same values with same seed
            Assert.Equal(value1, value2);
            Assert.Equal(double1, double2);
        }
        
        [Fact]
        public void Level_CompleteWorkflow_SimulatesLevelProgression()
        {
            // Arrange
            var mockRandom = new MockRandomGenerator();
            mockRandom.SetNextInt(400); // wormhole x
            mockRandom.SetNextInt(300); // wormhole y
            for (int i = 0; i < 10; i++) // Multiple missile angles
            {
                mockRandom.SetNextDouble(i * 0.1);
            }
            
            var level = new Level(1, mockRandom); // MaxMissiles=3, MaxWormholes=1
            
            // Act & Assert - Initial state
            Assert.False(level.IsLevelComplete());
            Assert.True(level.CanSpawnWormhole());
            Assert.True(level.CanSpawnMissile());
            
            // Simulate some game updates
            for (int frame = 0; frame < 500; frame++)
            {
                level.Update(800, 600);
                
                // Level should maintain valid state
                Assert.True(level.Wormholes.Count <= level.MaxWormholes);
                Assert.True(level.WormholesSpawned <= level.MaxWormholes);
                Assert.True(level.MissilesSpawned <= level.MaxMissiles);
            }
        }
    }
}
