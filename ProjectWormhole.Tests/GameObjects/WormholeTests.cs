using System;
using Xunit;
using ProjectWormhole.GameObjects;

namespace ProjectWormhole.Tests.GameObjects
{
    public class WormholeTests
    {
        [Fact]
        public void Wormhole_Constructor_WithAllParameters_SetsInitialValues()
        {
            // Arrange
            int x = 200, y = 150, size = 80;

            // Act
            var wormhole = new Wormhole(x, y, size);

            // Assert
            Assert.Equal(x, wormhole.X);
            Assert.Equal(y, wormhole.Y);
            Assert.Equal(size, wormhole.Size);
            Assert.Equal(0, wormhole.MissilesSpawned);
            Assert.False(wormhole.IsExpired());
            Assert.False(wormhole.ShouldExpire());
        }

        [Fact]
        public void Wormhole_Constructor_WithDefaults_UsesDefaultSize()
        {
            // Arrange
            int x = 100, y = 200;

            // Act
            var wormhole = new Wormhole(x, y);

            // Assert
            Assert.Equal(x, wormhole.X);
            Assert.Equal(y, wormhole.Y);
            Assert.Equal(Wormhole.DEFAULT_SIZE, wormhole.Size);
            Assert.Equal(0, wormhole.MissilesSpawned);
        }

        [Fact]
        public void Wormhole_Update_IncrementsTimer()
        {
            // Arrange
            var wormhole = new Wormhole(100, 100);

            // Act - Multiple updates to verify timer progression
            for (int i = 0; i < 5; i++)
            {
                wormhole.Update();
            }

            // Assert - We can't directly test the timer, but can test side effects
            Assert.False(wormhole.ShouldSpawnMissile()); // Should not spawn missile yet
        }

        [Fact]
        public void Wormhole_ShouldSpawnMissile_ReturnsTrueAfterInterval()
        {
            // Arrange
            var wormhole = new Wormhole(100, 100);

            // Act - Update for enough frames to trigger missile spawn
            for (int i = 0; i <= Wormhole.MISSILE_SPAWN_INTERVAL; i++)
            {
                wormhole.Update();
            }

            // Assert
            Assert.True(wormhole.ShouldSpawnMissile());
            Assert.Equal(1, wormhole.MissilesSpawned);
        }

        [Fact]
        public void Wormhole_ShouldSpawnMissile_ResetTimerAfterSpawning()
        {
            // Arrange
            var wormhole = new Wormhole(100, 100);

            // Act - Trigger first missile spawn
            for (int i = 0; i <= Wormhole.MISSILE_SPAWN_INTERVAL; i++)
            {
                wormhole.Update();
            }
            wormhole.ShouldSpawnMissile(); // This should reset the timer

            // Assert - Should not spawn another missile immediately
            Assert.False(wormhole.ShouldSpawnMissile());
            Assert.Equal(1, wormhole.MissilesSpawned);
        }

        [Fact]
        public void Wormhole_ShouldSpawnMissile_StopsAfterMaxMissiles()
        {
            // Arrange
            var wormhole = new Wormhole(100, 100);

            // Act - Spawn all possible missiles
            for (int missile = 0; missile < Wormhole.MAX_MISSILES_PER_WORMHOLE; missile++)
            {
                // Update enough to trigger spawn
                for (int i = 0; i <= Wormhole.MISSILE_SPAWN_INTERVAL; i++)
                {
                    wormhole.Update();
                }
                wormhole.ShouldSpawnMissile();
            }

            // Assert - Should not spawn more missiles
            for (int i = 0; i <= Wormhole.MISSILE_SPAWN_INTERVAL; i++)
            {
                wormhole.Update();
            }
            Assert.False(wormhole.ShouldSpawnMissile());
            Assert.Equal(Wormhole.MAX_MISSILES_PER_WORMHOLE, wormhole.MissilesSpawned);
        }

        [Fact]
        public void Wormhole_ShouldExpire_ReturnsTrueAfterMaxMissiles()
        {
            // Arrange
            var wormhole = new Wormhole(100, 100);

            // Act - Spawn all missiles
            for (int missile = 0; missile < Wormhole.MAX_MISSILES_PER_WORMHOLE; missile++)
            {
                for (int i = 0; i <= Wormhole.MISSILE_SPAWN_INTERVAL; i++)
                {
                    wormhole.Update();
                }
                wormhole.ShouldSpawnMissile();
            }

            // Assert
            Assert.True(wormhole.ShouldExpire());
        }

        [Fact]
        public void Wormhole_StartExpiring_BeginsShrinking()
        {
            // Arrange
            var wormhole = new Wormhole(100, 100);
            int originalSize = wormhole.Size;

            // Act
            wormhole.StartExpiring();
            wormhole.Update();

            // Assert
            Assert.True(wormhole.Size < originalSize);
        }

        [Fact]
        public void Wormhole_Update_ShrinksWhenExpiring()
        {
            // Arrange
            var wormhole = new Wormhole(100, 100);
            wormhole.StartExpiring();
            int sizeAfterFirstUpdate = wormhole.Size;

            // Act
            wormhole.Update();
            int sizeAfterSecondUpdate = wormhole.Size;

            // Assert
            Assert.True(sizeAfterSecondUpdate < sizeAfterFirstUpdate);
        }

        [Fact]
        public void Wormhole_IsExpired_ReturnsTrueWhenSizeReachesZero()
        {
            // Arrange
            var wormhole = new Wormhole(100, 100, 10); // Small size for faster testing
            wormhole.StartExpiring();

            // Act - Update until size reaches 0
            while (wormhole.Size > 0)
            {
                wormhole.Update();
            }

            // Assert
            Assert.True(wormhole.IsExpired());
            Assert.Equal(0, wormhole.Size);
        }

        [Fact]
        public void Wormhole_IsExpired_ReturnsFalseWhenSizeAboveZero()
        {
            // Arrange
            var wormhole = new Wormhole(100, 100);

            // Act & Assert - Initially not expired
            Assert.False(wormhole.IsExpired());

            // Act & Assert - Still not expired when expiring starts but size > 0
            wormhole.StartExpiring();
            wormhole.Update();
            if (wormhole.Size > 0)
            {
                Assert.False(wormhole.IsExpired());
            }
        }

        [Fact]
        public void Wormhole_Constants_HaveExpectedValues()
        {
            // Assert
            Assert.Equal(100, Wormhole.DEFAULT_SIZE);
            Assert.Equal(60, Wormhole.MISSILE_SPAWN_INTERVAL);
            Assert.Equal(4, Wormhole.MAX_MISSILES_PER_WORMHOLE);
        }

        [Fact]
        public void Wormhole_MissileSpawning_FollowsCorrectSequence()
        {
            // Arrange
            var wormhole = new Wormhole(100, 100);

            // Act & Assert - Test complete missile spawning sequence
            for (int expectedMissiles = 1; expectedMissiles <= Wormhole.MAX_MISSILES_PER_WORMHOLE; expectedMissiles++)
            {
                // Update to trigger spawn timing
                for (int i = 0; i <= Wormhole.MISSILE_SPAWN_INTERVAL; i++)
                {
                    wormhole.Update();
                }

                // Should be ready to spawn
                Assert.True(wormhole.ShouldSpawnMissile());
                Assert.Equal(expectedMissiles, wormhole.MissilesSpawned);

                // Should not spawn again immediately
                Assert.False(wormhole.ShouldSpawnMissile());
            }

            // Should not spawn more missiles
            Assert.True(wormhole.ShouldExpire());
        }

        [Theory]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(150)]
        public void Wormhole_ShrinkingSpeed_IsConsistent(int initialSize)
        {
            // Arrange
            var wormhole = new Wormhole(100, 100, initialSize);
            wormhole.StartExpiring();

            // Act
            int sizeBeforeUpdate = wormhole.Size;
            wormhole.Update();
            int sizeAfterUpdate = wormhole.Size;

            // Assert - Should shrink by exactly 2 pixels per update
            int expectedShrinkage = 2;
            int actualShrinkage = sizeBeforeUpdate - sizeAfterUpdate;
            Assert.Equal(expectedShrinkage, actualShrinkage);
        }
    }
}
