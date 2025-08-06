using System.Drawing;
using Xunit;
using ProjectWormhole.GameObjects;

namespace ProjectWormhole.Tests.GameObjects
{
    public class PlayerTests
    {
        [Fact]
        public void Player_Constructor_SetsInitialValues()
        {
            // Arrange & Act
            var player = new Player(100, 200, 25);

            // Assert
            Assert.Equal(100, player.X);
            Assert.Equal(200, player.Y);
            Assert.Equal(25, player.Size);
            Assert.Equal(Player.MAX_HEALTH, player.Health);
            Assert.False(player.IsDead());
        }

        [Fact]
        public void Player_DefaultConstructor_UsesDefaultSize()
        {
            // Arrange & Act
            var player = new Player(50, 75);

            // Assert
            Assert.Equal(50, player.X);
            Assert.Equal(75, player.Y);
            Assert.Equal(Player.DEFAULT_SIZE, player.Size);
        }

        [Fact]
        public void Player_Move_UpdatesPosition()
        {
            // Arrange
            var player = new Player(100, 100);
            int gameWidth = 800;
            int gameHeight = 600;

            // Act
            player.Move(10, -5, gameWidth, gameHeight);

            // Assert
            Assert.Equal(110, player.X);
            Assert.Equal(95, player.Y);
        }

        [Fact]
        public void Player_Move_RespectsBoundaries()
        {
            // Arrange
            var player = new Player(10, 10, 20); // Size 20, so radius is 10
            int gameWidth = 800;
            int gameHeight = 600;

            // Act - try to move beyond left boundary
            player.Move(-20, 0, gameWidth, gameHeight);

            // Assert - should be clamped to boundary (size/2 = 10)
            Assert.Equal(10, player.X);
            Assert.Equal(10, player.Y);

            // Act - try to move beyond right boundary
            player.SetPosition(790, 100);
            player.Move(20, 0, gameWidth, gameHeight);

            // Assert - should be clamped to boundary (800 - 10 = 790)
            Assert.Equal(790, player.X);
        }

        [Fact]
        public void Player_TakeDamage_ReducesHealth()
        {
            // Arrange
            var player = new Player(100, 100);

            // Act
            player.TakeDamage(25);

            // Assert
            Assert.Equal(75, player.Health);
            Assert.False(player.IsDead());
        }

        [Fact]
        public void Player_TakeDamage_CannotGoBelowZero()
        {
            // Arrange
            var player = new Player(100, 100);

            // Act
            player.TakeDamage(150); // More than max health

            // Assert
            Assert.Equal(0, player.Health);
            Assert.True(player.IsDead());
        }

        [Fact]
        public void Player_SetPosition_UpdatesCoordinates()
        {
            // Arrange
            var player = new Player(100, 100);

            // Act
            player.SetPosition(250, 350);

            // Assert
            Assert.Equal(250, player.X);
            Assert.Equal(350, player.Y);
        }
    }
}
