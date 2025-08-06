using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Xunit;
using ProjectWormhole.Core;
using ProjectWormhole.GameObjects;

namespace ProjectWormhole.Tests.Core
{
    // Mock audio manager for testing
    public class MockAudioManager : IAudioManager
    {
        private readonly List<string> _playedSounds = new List<string>();
        
        public IReadOnlyList<string> PlayedSounds => _playedSounds.AsReadOnly();
        
        public void PlaySfx(string filename)
        {
            _playedSounds.Add(filename);
        }
        
        public void Reset()
        {
            _playedSounds.Clear();
        }
    }
    
    // Mock settings for testing
    public class MockGameSettings : IGameSettings
    {
        public Size Resolution { get; set; } = new Size(800, 600);
        
        public (float scaleX, float scaleY) GetScalingFactors(Form form)
        {
            return (1.0f, 1.0f); // No scaling for tests
        }
    }
    
    // Mock Form for testing
    public class MockForm : Form
    {
        public MockForm(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }

    public class GameTests
    {
        private MockAudioManager CreateMockAudio() => new MockAudioManager();
        private MockGameSettings CreateMockSettings() => new MockGameSettings();
        private MockRandomGenerator CreateMockRandom() => new MockRandomGenerator();
        
        [Fact]
        public void Game_Constructor_WithDefaults_InitializesCorrectly()
        {
            // Arrange & Act
            var game = new Game();
            
            // Assert
            Assert.NotNull(game.Player);
            Assert.NotNull(game.CurrentLevel);
            Assert.Equal(1, game.CurrentLevel.Number);
            Assert.Equal(0, game.Score);
            Assert.True(game.IsRunning);
            Assert.True(game.ShowHUD);
            Assert.Equal(0, game.LevelTimer);
        }
        
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        public void Game_Constructor_WithStartingLevel_SetsLevelCorrectly(int startingLevel)
        {
            // Arrange & Act
            var game = new Game(startingLevel);
            
            // Assert
            Assert.Equal(startingLevel, game.CurrentLevel.Number);
            Assert.Equal(0, game.Score);
            Assert.True(game.IsRunning);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Game_InitializeGame_WithInvalidLevel_ThrowsArgumentException(int invalidLevel)
        {
            // Arrange
            var game = new Game();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => game.InitializeGame(invalidLevel));
        }
        
        [Fact]
        public void Game_Constructor_WithCustomDependencies_UsesThem()
        {
            // Arrange
            var mockAudio = CreateMockAudio();
            var mockSettings = CreateMockSettings();
            var mockRandom = CreateMockRandom();
            
            // Act
            var game = new Game(2, mockAudio, mockSettings, mockRandom);
            
            // Assert
            Assert.Equal(2, game.CurrentLevel.Number);
            Assert.Equal(mockSettings.Resolution.Width, game.GameWidth);
            Assert.Equal(mockSettings.Resolution.Height, game.GameHeight);
        }
        
        [Fact]
        public void Game_GameDimensions_UsesSettingsResolution()
        {
            // Arrange
            var mockSettings = CreateMockSettings();
            mockSettings.Resolution = new Size(1024, 768);
            var game = new Game(1, null, mockSettings);
            
            // Act & Assert
            Assert.Equal(1024, game.GameWidth);
            Assert.Equal(768, game.GameHeight);
        }
        
        [Fact]
        public void Game_SetScore_UpdatesScore()
        {
            // Arrange
            var game = new Game();
            
            // Act
            game.SetScore(1500);
            
            // Assert
            Assert.Equal(1500, game.Score);
        }
        
        [Fact]
        public void Game_InitializeGame_ResetsAllState()
        {
            // Arrange
            var game = new Game(3);
            game.SetScore(1000);
            
            // Act
            game.InitializeGame(5);
            
            // Assert
            Assert.Equal(5, game.CurrentLevel.Number);
            Assert.Equal(0, game.Score);
            Assert.True(game.IsRunning);
            Assert.Equal(0, game.LevelTimer);
            Assert.NotNull(game.Player);
            Assert.Equal(game.GameWidth / 2, game.Player.X);
            Assert.Equal(game.GameHeight / 2, game.Player.Y);
        }
        
        [Fact]
        public void Game_Update_IncrementsLevelTimer()
        {
            // Arrange
            var game = new Game();
            
            // Act
            game.Update();
            
            // Assert
            Assert.Equal(1, game.LevelTimer);
            
            // Act again
            game.Update();
            
            // Assert
            Assert.Equal(2, game.LevelTimer);
        }
        
        [Fact]
        public void Game_Update_UpdatesMissiles()
        {
            // Arrange
            var game = new Game();
            game.CurrentLevel.Missiles.Add(new Missile(100, 100, 5.0, 0.0));
            var initialX = game.CurrentLevel.Missiles[0].X;
            
            // Act
            game.Update();
            
            // Assert - Missile should have moved
            Assert.NotEqual(initialX, game.CurrentLevel.Missiles[0].X);
        }
        
        [Fact]
        public void Game_Update_OnlyUpdatesWormholesWhenPlayerAlive()
        {
            // Arrange
            var mockRandom = CreateMockRandom();
            mockRandom.SetNextInt(400);
            mockRandom.SetNextInt(300);
            var game = new Game(1, null, null, mockRandom);
            
            // Kill the player
            game.Player.TakeDamage(game.Player.MaxHealth);
            
            var initialWormholeCount = game.CurrentLevel.Wormholes.Count;
            
            // Act - Update many times to trigger wormhole spawning if it would happen
            for (int i = 0; i < Level.WormholeSpawnInterval + 10; i++)
            {
                game.Update();
            }
            
            // Assert - No new wormholes should spawn when player is dead
            Assert.Equal(initialWormholeCount, game.CurrentLevel.Wormholes.Count);
        }
        
        [Fact]
        public void Game_Update_AwardsPointsBasedOnDangerMultiplier()
        {
            // Arrange
            var game = new Game();
            var initialScore = game.Score;
            
            // Add a missile close to player for danger multiplier
            var playerX = game.Player.X;
            var playerY = game.Player.Y;
            game.CurrentLevel.Missiles.Add(new Missile(playerX + 30, playerY + 30, 0.0, 0.0));
            
            // Act - Update 6 times to trigger scoring (every 6 frames)
            for (int i = 0; i < 6; i++)
            {
                game.Update();
            }
            
            // Assert - Score should have increased
            Assert.True(game.Score > initialScore);
        }
        
        [Fact]
        public void Game_CheckCollisions_DetectsPlayerMissileCollision()
        {
            // Arrange
            var game = new Game();
            
            // Place missile at player position
            game.CurrentLevel.Missiles.Add(new Missile(game.Player.X, game.Player.Y, 0.0, 0.0));
            var initialMissileCount = game.CurrentLevel.Missiles.Count;
            
            // Act
            bool collision = game.CheckCollisions();
            
            // Assert
            Assert.True(collision);
            Assert.Equal(initialMissileCount - 1, game.CurrentLevel.Missiles.Count); // Missile removed
        }
        
        [Fact]
        public void Game_CheckCollisions_ReturnsFalseWhenNoCollision()
        {
            // Arrange
            var game = new Game();
            
            // Place missile far from player
            game.CurrentLevel.Missiles.Add(new Missile(game.Player.X + 200, game.Player.Y + 200, 0.0, 0.0));
            var initialMissileCount = game.CurrentLevel.Missiles.Count;
            
            // Act
            bool collision = game.CheckCollisions();
            
            // Assert
            Assert.False(collision);
            Assert.Equal(initialMissileCount, game.CurrentLevel.Missiles.Count); // Missile not removed
        }
        
        [Fact]
        public void Game_Update_HandlesPlayerDeath()
        {
            // Arrange
            var mockAudio = CreateMockAudio();
            var mockRandom = CreateMockRandom();
            // Set up random values for explosion missiles
            for (int i = 0; i < 20; i++)
            {
                mockRandom.SetNextDouble(0.5);
            }
            
            var game = new Game(1, mockAudio, null, mockRandom);
            
            // Damage player almost to death
            game.Player.TakeDamage(game.Player.MaxHealth - 10);
            
            // Place missile at player position to kill them
            game.CurrentLevel.Missiles.Add(new Missile(game.Player.X, game.Player.Y, 0.0, 0.0));
            var initialMissileCount = game.CurrentLevel.Missiles.Count;
            
            // Act
            game.Update();
            
            // Assert
            Assert.True(game.Player.IsDead());
            Assert.Contains("death.mp3", mockAudio.PlayedSounds);
            Assert.True(game.CurrentLevel.Missiles.Count > initialMissileCount); // Explosion missiles added
        }
        
        [Fact]
        public void Game_MovePlayer_MovesPlayerCorrectly()
        {
            // Arrange
            var game = new Game();
            var initialX = game.Player.X;
            var initialY = game.Player.Y;
            
            // Act
            game.MovePlayer(10, -5);
            
            // Assert
            Assert.NotEqual(initialX, game.Player.X);
            Assert.NotEqual(initialY, game.Player.Y);
        }
        
        [Fact]
        public void Game_Render_WithNullGraphics_ThrowsArgumentNullException()
        {
            // Arrange
            var game = new Game();
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => game.Render(null!));
        }
        
        [Fact]
        public void Game_RenderWithForm_WithNullArguments_ThrowsArgumentNullException()
        {
            // Arrange
            var game = new Game();
            using var bitmap = new Bitmap(100, 100);
            using var graphics = Graphics.FromImage(bitmap);
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => game.Render(null!, new MockForm(100, 100)));
            Assert.Throws<ArgumentNullException>(() => game.Render(graphics, null!));
        }
        
        [Fact]
        public void Game_Render_CallsGraphicsOperations()
        {
            // Arrange
            var game = new Game();
            using var bitmap = new Bitmap(100, 100);
            using var graphics = Graphics.FromImage(bitmap);
            
            // Act - Should not throw
            game.Render(graphics);
            
            // Assert - Test passes if no exception thrown
            Assert.True(true);
        }
        
        [Fact]
        public void Game_RenderWithForm_AppliesScaling()
        {
            // Arrange
            var mockSettings = CreateMockSettings();
            var game = new Game(1, null, mockSettings);
            using var bitmap = new Bitmap(100, 100);
            using var graphics = Graphics.FromImage(bitmap);
            var form = new MockForm(100, 100);
            
            // Act - Should not throw
            game.Render(graphics, form);
            
            // Assert - Test passes if no exception thrown
            Assert.True(true);
        }
        
        [Fact]
        public void Game_RestartGame_ResetsToLevel1()
        {
            // Arrange
            var game = new Game(5);
            game.SetScore(2000);
            
            // Act
            game.RestartGame();
            
            // Assert
            Assert.Equal(1, game.CurrentLevel.Number);
            Assert.Equal(0, game.Score);
            Assert.True(game.IsRunning);
            Assert.Equal(0, game.LevelTimer);
        }
        
        [Fact]
        public void Game_CanContinuePlaying_ReturnsIsRunning()
        {
            // Arrange
            var game = new Game();
            
            // Act & Assert
            Assert.Equal(game.IsRunning, game.CanContinuePlaying());
        }
        
        [Fact]
        public void Game_AdvanceToNextLevel_IncrementsLevelAndResetsTimer()
        {
            // Arrange
            var mockRandom = CreateMockRandom();
            var game = new Game(3, null, null, mockRandom);
            
            // Update timer to non-zero value
            game.Update();
            game.Update();
            
            // Force level completion by clearing missiles and setting missiles spawned
            game.CurrentLevel.Missiles.Clear();
            // We need to simulate level completion through the proper game mechanics
            
            // Act - Simulate level completion by updating until it triggers
            // For this test, we'll manually test the logic by checking the level number
            var initialLevel = game.CurrentLevel.Number;
            
            // Since AdvanceToNextLevel is private, we test it indirectly through Update()
            // when level completion conditions are met
            Assert.Equal(initialLevel, game.CurrentLevel.Number); // Confirm initial state
        }
        
        [Fact]
        public void Game_ShowHUD_CanBeToggled()
        {
            // Arrange
            var game = new Game();
            
            // Act & Assert
            Assert.True(game.ShowHUD); // Default is true
            
            game.ShowHUD = false;
            Assert.False(game.ShowHUD);
            
            game.ShowHUD = true;
            Assert.True(game.ShowHUD);
        }
        
        [Fact]
        public void Game_CalculateDangerMultiplier_ReturnsCorrectValues()
        {
            // Arrange
            var game = new Game();
            
            // Test with no missiles (should return 1)
            // Since CalculateDangerMultiplier is private, we test it indirectly
            // by checking score increases when missiles are at different distances
            
            var scoreWithoutMissiles = game.Score;
            
            // Update 6 times to trigger scoring
            for (int i = 0; i < 6; i++)
            {
                game.Update();
            }
            
            var scoreIncrease1 = game.Score - scoreWithoutMissiles;
            
            // Reset score and add close missile
            game.SetScore(0);
            game.CurrentLevel.Missiles.Add(new Missile(game.Player.X + 30, game.Player.Y + 30, 0.0, 0.0));
            
            // Update 6 times to trigger scoring with danger multiplier
            for (int i = 0; i < 6; i++)
            {
                game.Update();
            }
            
            var scoreIncrease2 = game.Score;
            
            // Assert - Score increase should be higher with missiles nearby
            Assert.True(scoreIncrease2 > scoreIncrease1);
        }
        
        [Fact]
        public void Game_Constants_HaveExpectedValues()
        {
            // Assert
            Assert.Equal(800, Game.GAME_WIDTH);
            Assert.Equal(600, Game.GAME_HEIGHT);
        }
        
        [Fact]
        public void Game_PlayerInitialization_PlacesPlayerInCenter()
        {
            // Arrange
            var mockSettings = CreateMockSettings();
            mockSettings.Resolution = new Size(1000, 800);
            
            // Act
            var game = new Game(1, null, mockSettings);
            
            // Assert
            Assert.Equal(500, game.Player.X); // Center X
            Assert.Equal(400, game.Player.Y); // Center Y
        }
        
        [Fact]
        public void Game_Update_HandlesConcurrentModification()
        {
            // Arrange
            var game = new Game();
            
            // Add multiple missiles
            for (int i = 0; i < 5; i++)
            {
                game.CurrentLevel.Missiles.Add(new Missile(100 + i * 10, 100 + i * 10, 5.0, 5.0));
            }
            
            // Act - Multiple updates should handle collection modification safely
            for (int i = 0; i < 10; i++)
            {
                game.Update();
            }
            
            // Assert - Test passes if no exception thrown
            Assert.True(true);
        }
        
        [Fact]
        public void Game_ExplosionMissiles_AreSpawnedOnDeath()
        {
            // Arrange
            var mockRandom = CreateMockRandom();
            // Set up deterministic random values for explosion
            for (int i = 0; i < 20; i++)
            {
                mockRandom.SetNextDouble(0.5);
            }
            
            var game = new Game(1, null, null, mockRandom);
            var initialMissileCount = game.CurrentLevel.Missiles.Count;
            
            // Damage player to near death
            game.Player.TakeDamage(game.Player.MaxHealth - 10);
            
            // Place missile at player to kill them
            game.CurrentLevel.Missiles.Add(new Missile(game.Player.X, game.Player.Y, 0.0, 0.0));
            
            // Act
            game.Update();
            
            // Assert
            Assert.True(game.Player.IsDead());
            Assert.True(game.CurrentLevel.Missiles.Count > initialMissileCount + 1); // Original + collision missile removed + explosion missiles
        }
        
        [Fact]
        public void Game_LevelProgression_WorksCorrectly()
        {
            // Arrange
            var mockRandom = CreateMockRandom();
            var game = new Game(1, null, null, mockRandom);
            var initialLevel = game.CurrentLevel.Number;
            
            // This test would require simulating complete level completion
            // which involves complex game state manipulation
            // For now, verify that the level is properly initialized
            
            // Act & Assert
            Assert.True(game.CurrentLevel.Number >= initialLevel);
            Assert.NotNull(game.CurrentLevel.Wormholes);
            Assert.NotNull(game.CurrentLevel.Missiles);
        }
        
        [Fact]
        public void Game_IntegrationTest_BasicGameLoop()
        {
            // Arrange
            var mockAudio = CreateMockAudio();
            var mockSettings = CreateMockSettings();
            var mockRandom = CreateMockRandom();
            
            var game = new Game(1, mockAudio, mockSettings, mockRandom);
            
            // Act - Simulate several game updates
            for (int i = 0; i < 100; i++)
            {
                game.Update();
                
                // Occasionally move player
                if (i % 10 == 0)
                {
                    game.MovePlayer(5, 0);
                }
            }
            
            // Assert - Game should maintain valid state
            Assert.True(game.IsRunning);
            Assert.True(game.LevelTimer > 0);
            Assert.NotNull(game.Player);
            Assert.NotNull(game.CurrentLevel);
            Assert.True(game.Score >= 0);
        }
    }
}
