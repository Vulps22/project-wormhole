using System;
using System.Drawing;
using WormholeGame.GameObjects;

namespace WormholeGame.Core
{
    public class Game
    {
        public Player Player { get; private set; } = null!;
        public Level CurrentLevel { get; private set; } = null!;
        public int Score { get; private set; }
        public bool IsRunning { get; private set; }
        public bool ShowHUD { get; set; } = true; // Whether to show HUD during rendering
        private int levelTimer;

        // Game world dimensions - now dynamic based on settings
        public int GameWidth => Settings.Instance.Resolution.Width;
        public int GameHeight => Settings.Instance.Resolution.Height;

        // Legacy constants for backward compatibility
        public const int GAME_WIDTH = 800;
        public const int GAME_HEIGHT = 600;

        // Method to set score (for reinitialization)
        public void SetScore(int score)
        {
            Score = score;
        }

        public Game(int startingLevel = 1)
        {
            InitializeGame(startingLevel);
        }

        public void InitializeGame(int startingLevel = 1)
        {
            Score = 0;
            IsRunning = true;
            levelTimer = 0;

            Player = new Player(GameWidth / 2, GameHeight / 2);
            CurrentLevel = new Level(startingLevel);

        }

        public void Update()
        {
            // Always update missiles (for explosion effects even after death)
            CurrentLevel.UpdateMissiles(GameWidth, GameHeight);

            // Only update game logic if player is alive
            if (!Player.IsDead())
            {
                // Update wormholes and spawn new missiles
                CurrentLevel.UpdateWormholes(GameWidth, GameHeight);

                // Check collisions and handle damage
                if (CheckCollisions())
                {
                    Player.TakeDamage(10);

                    // If player just died, spawn explosion missiles
                    if (Player.IsDead())
                    {
                        SpawnExplosionMissiles();
                        AudioManager.Instance.PlaySfx("death.mp3");
                    }
                }

                if (CurrentLevel.IsLevelComplete(levelTimer % 60 == 0))
                {
                    AdvanceToNextLevel();
                }

                // Award survival points smoothly with danger multiplier!
                if (levelTimer % 6 == 0)
                {
                    int multiplier = CalculateDangerMultiplier();
                    Score += multiplier;

                    if (multiplier > 1)
                    {
                    }
                }

                // Output debug info (less frequently)
                if (levelTimer % 60 == 0) // Every second
                {
                }
            }

            levelTimer++;
        }

        public void MovePlayer(int deltaX, int deltaY)
        {
            Player.Move(deltaX, deltaY, GameWidth, GameHeight);
        }

        public void Render(Graphics graphics)
        {
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
            // Get scaling factors
            var (scaleX, scaleY) = Settings.Instance.GetScalingFactors(form);

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
            Random random = new Random();
            int explosionMissiles = 10;

            for (int i = 0; i < explosionMissiles; i++)
            {
                // Calculate random direction (angle in radians)
                double angle = random.NextDouble() * 2 * Math.PI;

                // Random speed between 3 and 8 for variety
                double speed = 3 + random.NextDouble() * 5;

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

        private void AdvanceToNextLevel()
        {
            CurrentLevel.Reset();
            CurrentLevel = new Level(CurrentLevel.Number + 1);
            levelTimer = 0;

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
