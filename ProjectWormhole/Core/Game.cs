using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ProjectWormhole.GameObjects;

namespace ProjectWormhole.Core
{
    // Interface for audio management to enable testing
    public interface IAudioManager
    {
        void PlaySfx(string filename);
    }
    
    // Wrapper for the actual AudioManager
    public class AudioManagerWrapper : IAudioManager
    {
        public void PlaySfx(string filename)
        {
            AudioManager.Instance.PlaySfx(filename);
        }
    }
    
    // Interface for settings access to enable testing
    public interface IGameSettings
    {
        Size Resolution { get; }
        (float scaleX, float scaleY) GetScalingFactors(Form form);
    }
    
    // Wrapper for the actual Settings
    public class GameSettingsWrapper : IGameSettings
    {
        public Size Resolution => Settings.Instance.Resolution;
        public (float scaleX, float scaleY) GetScalingFactors(Form form) => Settings.Instance.GetScalingFactors(form);
    }

    public class Game
    {
        public Player Player { get; private set; } = null!;
        public Level CurrentLevel { get; private set; } = null!;
        public int Score { get; private set; }
        public bool IsRunning { get; private set; }
        public bool ShowHUD { get; set; } = true; // Whether to show HUD during rendering
        
        private readonly IAudioManager _audioManager;
        private readonly IGameSettings _settings;
        private readonly IRandomGenerator _random;
        private int _levelTimer;
        
        // Statistics tracking
        private DateTime _gameStartTime;
        private DateTime _lastHitTime;
        private bool _statsRecorded; // To ensure we only record stats once per game
        private List<double> _dangerMultiplierHistory = new List<double>();
        private int _totalMissilesSpawned;
        private int _missilesAtGameStart;
        private int _frameCounter = 0; // To reduce stats recording frequency
        
        // Expose private fields for testing
        public int LevelTimer => _levelTimer;

        // Game world dimensions - now dynamic based on settings
        public int GameWidth => _settings.Resolution.Width;
        public int GameHeight => _settings.Resolution.Height;

        // Legacy constants for backward compatibility
        public const int GAME_WIDTH = 800;
        public const int GAME_HEIGHT = 600;

        // Method to set score (for reinitialization)
        public void SetScore(int score)
        {
            Score = score;
        }

        public Game(int startingLevel = 1, IAudioManager? audioManager = null, IGameSettings? settings = null, IRandomGenerator? randomGenerator = null)
        {
            _audioManager = audioManager ?? new AudioManagerWrapper();
            _settings = settings ?? new GameSettingsWrapper();
            _random = randomGenerator ?? new SystemRandomGenerator();
            InitializeGame(startingLevel);
        }

        public void InitializeGame(int startingLevel = 1)
        {
            if (startingLevel <= 0)
                throw new ArgumentException("Starting level must be positive", nameof(startingLevel));
                
            Score = 0;
            IsRunning = true;
            _levelTimer = 0;

            // Initialize statistics tracking
            _gameStartTime = DateTime.Now;
            _lastHitTime = DateTime.Now;
            _statsRecorded = false;
            _dangerMultiplierHistory.Clear();
            _totalMissilesSpawned = 0;
            _frameCounter = 0;

            Player = new Player(GameWidth / 2, GameHeight / 2);
            CurrentLevel = new Level(startingLevel, _random);
            
            // Record initial missile count
            _missilesAtGameStart = CurrentLevel.Missiles.Count;
        }

        public void Update()
        {
            // Always update missiles (for explosion effects even after death)
            CurrentLevel.UpdateMissiles(GameWidth, GameHeight);

            // Only update game logic if player is alive
            if (!Player.IsDead())
            {
                // Track danger multiplier locally (don't save to file until game ends)
                int dangerMultiplier = CalculateDangerMultiplier();
                _dangerMultiplierHistory.Add(dangerMultiplier);
                
                // Only record stats in memory every 60 frames (once per second) AND only when actually playing
                _frameCounter++;
                if (_frameCounter % 60 == 0 && IsRunning && ShowHUD) // ShowHUD indicates we're in gameplay, not menu
                {
                    StatsManager.RecordDangerMultiplierStatic(dangerMultiplier);
                    StatsManager.RecordTimeAliveStatic(60); // 60 frames = ~1 second
                }

                // Update wormholes and spawn new missiles
                int missilesBefore = CurrentLevel.Missiles.Count;
                
                CurrentLevel.UpdateWormholes(GameWidth, GameHeight);
                
                // Track missiles spawned for avoidance calculation
                int newMissiles = CurrentLevel.Missiles.Count - missilesBefore;
                _totalMissilesSpawned += newMissiles;

                // Check collisions and handle damage
                if (CheckCollisions())
                {
                    Player.TakeDamage(10);
                    // Only record hit if we're actually playing (not in menu)
                    if (IsRunning && ShowHUD)
                    {
                        StatsManager.RecordHitStatic(); // Record hit but don't save yet
                    }
                    _lastHitTime = DateTime.Now;

                    // If player just died, spawn explosion missiles
                    if (Player.IsDead())
                    {
                        SpawnExplosionMissiles();
                        _audioManager.PlaySfx("death.mp3");
                        RecordGameStats(); // Record stats when game ends and SAVE to file
                    }
                }

                if (CurrentLevel.IsLevelComplete(_levelTimer % 60 == 0))
                {
                    AdvanceToNextLevel();
                }

                // Award survival points smoothly with danger multiplier!
                if (_levelTimer % 6 == 0)
                {
                    int multiplier = CalculateDangerMultiplier();
                    Score += multiplier;
                }

                // Output debug info (less frequently)
                if (_levelTimer % 60 == 0) // Every second
                {
                    // Debug output could be added here if needed
                }
            }

            _levelTimer++;
        }

        public void MovePlayer(int deltaX, int deltaY)
        {
            if (Player == null)
                throw new InvalidOperationException("Game not initialized");
                
            Player.Move(deltaX, deltaY, GameWidth, GameHeight);
        }

        public void Render(Graphics graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));
                
            // Clear screen
            graphics.Clear(Color.Black);

            // Render player
            if (!Player.IsDead())
            {
                Player.Render(graphics);
            }

            // Render wormholes
            foreach (var wormhole in CurrentLevel.Wormholes)
            {
                wormhole.Render(graphics);
            }

            // Render missiles
            foreach (var missile in CurrentLevel.Missiles)
            {
                missile.Render(graphics);
            }

            if (ShowHUD)
            {
                RenderHUD(graphics);
            }
        }

        public void Render(Graphics graphics, Form form)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));
            if (form == null)
                throw new ArgumentNullException(nameof(form));
                
            // Get scaling factors
            var (scaleX, scaleY) = _settings.GetScalingFactors(form);

            // Save the original transform
            var originalTransform = graphics.Transform;

            // Apply scaling transform
            graphics.ScaleTransform(scaleX, scaleY);

            // Render normally (everything will be scaled)
            Render(graphics);

            // Restore original transform
            graphics.Transform = originalTransform;
        }

        private void RenderHUD(Graphics graphics)
        {
            using (Brush textBrush = new SolidBrush(Color.White))
            using (Font font = new Font("Arial", 12))
            {
                graphics.DrawString($"Level: {CurrentLevel.Number}", font, textBrush, 10, 10);
                graphics.DrawString($"Score: {Score:N0}", font, textBrush, 150, 10);
                graphics.DrawString($"Missiles: {CurrentLevel.Missiles.Count}/{CurrentLevel.MaxMissiles}",
                    font, textBrush, 10, 30);
                graphics.DrawString($"Wormholes: {CurrentLevel.Wormholes.Count}/{CurrentLevel.MaxWormholes}",
                    font, textBrush, 10, 50);

                // Draw health bar
                RenderHealthBar(graphics);

                graphics.DrawString("WASD to move", font, textBrush, 10, GameHeight - 30);

                // Draw danger multiplier in bottom right
                int multiplier = CalculateDangerMultiplier();
                string multiplierText = $"Danger Multiplier: x{multiplier}";
                SizeF textSize = graphics.MeasureString(multiplierText, font);
                graphics.DrawString(multiplierText, font, textBrush,
                    GameWidth - textSize.Width - 10, GameHeight - 30);
            }
        }

        private void RenderHealthBar(Graphics graphics)
        {
            int barWidth = 200;
            int barHeight = 20;
            int barX = 10;
            int barY = 70;

            // Background
            using (Brush backgroundBrush = new SolidBrush(Color.DarkRed))
            {
                graphics.FillRectangle(backgroundBrush, barX, barY, barWidth, barHeight);
            }

            // Health fill
            float healthPercentage = (float)Player.Health / Player.MaxHealth;
            int healthWidth = (int)(barWidth * healthPercentage);

            Color healthColor = healthPercentage > 0.6f ? Color.Green :
                               healthPercentage > 0.3f ? Color.Yellow : Color.Red;

            using (Brush healthBrush = new SolidBrush(healthColor))
            {
                graphics.FillRectangle(healthBrush, barX, barY, healthWidth, barHeight);
            }

            // Border
            using (Pen borderPen = new Pen(Color.White, 2))
            {
                graphics.DrawRectangle(borderPen, barX, barY, barWidth, barHeight);
            }

            // Health text
            using (Font font = new Font("Arial", 10))
            using (Brush textBrush = new SolidBrush(Color.White))
            {
                string healthText = $"Health: {Player.Health}/{Player.MaxHealth}";
                graphics.DrawString(healthText, font, textBrush, barX + 5, barY + 2);
            }
        }

        public bool CheckCollisions()
        {
            Rectangle playerRect = new Rectangle(
                Player.X - Player.Size / 2,
                Player.Y - Player.Size / 2,
                Player.Size,
                Player.Size);

            // Check collisions in reverse order so we can safely remove items
            for (int i = CurrentLevel.Missiles.Count - 1; i >= 0; i--)
            {
                var missile = CurrentLevel.Missiles[i];
                Rectangle missileRect = new Rectangle(
                    missile.X - missile.Size / 2,
                    missile.Y - missile.Size / 2,
                    missile.Size,
                    missile.Size);

                if (playerRect.IntersectsWith(missileRect))
                {
                    // Remove the missile that hit the player
                    CurrentLevel.Missiles.RemoveAt(i);
                    return true; // Collision detected
                }
            }
            return false;
        }

        private void SpawnExplosionMissiles()
        {
            const int explosionMissiles = 10;

            for (int i = 0; i < explosionMissiles; i++)
            {
                // Calculate random direction (angle in radians)
                double angle = _random.NextDouble() * 2 * Math.PI;

                // Random speed between 3 and 8 for variety
                double speed = 3 + _random.NextDouble() * 5;

                // Calculate velocity components
                double velX = Math.Cos(angle) * speed;
                double velY = Math.Sin(angle) * speed;

                // Create missile at player position
                Missile explosionMissile = new Missile(Player.X, Player.Y, velX, velY);
                CurrentLevel.Missiles.Add(explosionMissile);
            }
        }

        public void RestartGame()
        {
            InitializeGame(1);
        }

        public void ExitToMainMenu()
        {
            // If player is exiting mid-game, record stats for the current session
            if (!_statsRecorded && IsRunning && !Player.IsDead())
            {
                RecordGameStats();
            }
        }

        private void AdvanceToNextLevel()
        {
            CurrentLevel.Reset();
            CurrentLevel = new Level(CurrentLevel.Number + 1, _random);
            _levelTimer = 0;
        }

        private void RecordGameStats()
        {
            if (_statsRecorded) return; // Only record once per game
            
            int playTimeSeconds = (int)(DateTime.Now - _gameStartTime).TotalSeconds;
            
            // Calculate average danger multiplier for this game
            double averageDangerMultiplier = _dangerMultiplierHistory.Count > 0 
                ? _dangerMultiplierHistory.Average() 
                : 1.0;
            
            // Calculate missiles avoided (total spawned minus hits)
            int missilesAvoided = Math.Max(0, _totalMissilesSpawned - 1); // -1 for the hit that killed us
            for (int i = 0; i < missilesAvoided; i++)
            {
                StatsManager.RecordMissileAvoidedStatic();
            }
            
            // Record game session data
            StatsManager.RecordGameSessionStatic(averageDangerMultiplier, playTimeSeconds);
            StatsManager.RecordGameCompletedStatic(Score, CurrentLevel.Number, playTimeSeconds);
            
            // Force save to file now that game is over
            StatsManager.ForceSaveStatic();
            
            _statsRecorded = true;
        }

        public bool CanContinuePlaying()
        {
            return IsRunning;
        }

        private int CalculateDangerMultiplier()
        {
            if (CurrentLevel.Missiles.Count == 0) return 1; // No danger = base points

            int dangerousProximities = 0;
            const int CLOSE_RANGE = 60;     // Very close - 3x multiplier zone
            const int MEDIUM_RANGE = 120;   // Medium close - 2x multiplier zone

            foreach (var missile in CurrentLevel.Missiles)
            {
                double distance = Math.Sqrt(
                    Math.Pow(Player.X - missile.X, 2) +
                    Math.Pow(Player.Y - missile.Y, 2));

                if (distance <= CLOSE_RANGE)
                {
                    dangerousProximities += 3; // Very dangerous!
                }
                else if (distance <= MEDIUM_RANGE)
                {
                    dangerousProximities += 1; // Somewhat dangerous
                }
            }

            // Cap the multiplier at reasonable levels
            return Math.Min(1 + dangerousProximities, 10);
        }
    }
}
