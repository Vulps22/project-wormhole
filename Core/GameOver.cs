using System;
using System.Drawing;

namespace WormholeGame.Core
{
    public class GameOverMenu : Menu
    {
        private MenuManager menuManager;
        private Rectangle restartButton;
        private bool isRestartHovered;
        private int finalLevel;
        private int finalScore;
        
        public GameOverMenu(MenuManager manager, int level, int score)
        {
            this.menuManager = manager;
            this.finalLevel = level;
            this.finalScore = score;
            
            // Position restart button in center of screen
            int buttonWidth = 120;
            int buttonHeight = 30; // Smaller for text-only
            restartButton = new Rectangle(
                (Settings.Instance.Resolution.Width - buttonWidth) / 2,
                (Settings.Instance.Resolution.Height / 2) + 60,
                buttonWidth,
                buttonHeight
            );
        }
        
        public override void Update()
        {
            // Nothing to update for now
        }
        
        public override void RecalculateLayout()
        {
            // Recalculate button position for new resolution
            int buttonWidth = 120;
            int buttonHeight = 30;
            restartButton = new Rectangle(
                (Settings.Instance.Resolution.Width - buttonWidth) / 2,
                (Settings.Instance.Resolution.Height / 2) + 60,
                buttonWidth,
                buttonHeight
            );
        }
        
        public override void HandleMouseMove(int mouseX, int mouseY)
        {
            isRestartHovered = restartButton.Contains(mouseX, mouseY);
        }
        
        public override bool HandleMouseClick(int mouseX, int mouseY)
        {
            if (restartButton.Contains(mouseX, mouseY))
            {
                menuManager.StartGame();
                return true; // Restart button clicked
            }
            return false;
        }
        
        public override void Render(Graphics graphics)
        {
            if (!IsVisible) return;
            
            // Semi-transparent overlay
            using (Brush overlayBrush = new SolidBrush(Color.FromArgb(180, Color.Black)))
            {
                graphics.FillRectangle(overlayBrush, 0, 0, Settings.Instance.Resolution.Width, Settings.Instance.Resolution.Height);
            }
            
            // Game Over title
            using (Font titleFont = new Font("Arial", 32, FontStyle.Bold))
            using (Brush titleBrush = new SolidBrush(Color.Red))
            {
                string title = "GAME OVER";
                SizeF titleSize = graphics.MeasureString(title, titleFont);
                float titleX = (Settings.Instance.Resolution.Width - titleSize.Width) / 2;
                float titleY = Settings.Instance.Resolution.Height / 2 - 80;
                graphics.DrawString(title, titleFont, titleBrush, titleX, titleY);
            }
            
            // Final stats
            using (Font statsFont = new Font("Arial", 16))
            using (Brush statsBrush = new SolidBrush(Color.White))
            {
                string stats = $"Final Level: {finalLevel}\nFinal Score: {finalScore}";
                SizeF statsSize = graphics.MeasureString(stats, statsFont);
                float statsX = (Settings.Instance.Resolution.Width - statsSize.Width) / 2;
                float statsY = Settings.Instance.Resolution.Height / 2 - 20;
                graphics.DrawString(stats, statsFont, statsBrush, statsX, statsY);
            }
            
            // Restart button - modern text style
            Color textColor = isRestartHovered ? Color.Gray : Color.White;
            using (Font buttonFont = new Font("Arial", 16, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(textColor))
            {
                string buttonText = "RESTART";
                SizeF textSize = graphics.MeasureString(buttonText, buttonFont);
                float textX = restartButton.X + (restartButton.Width - textSize.Width) / 2;
                float textY = restartButton.Y + (restartButton.Height - textSize.Height) / 2;
                graphics.DrawString(buttonText, buttonFont, textBrush, textX, textY);
            }
        }
        
        public override void Render(Graphics graphics, Form form)
        {
            if (!IsVisible) return;
            
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
        
        public override void HandleMouseMove(int mouseX, int mouseY, Form form)
        {
            // Scale mouse coordinates back to game resolution
            var (scaleX, scaleY) = Settings.Instance.GetScalingFactors(form);
            int scaledX = (int)(mouseX / scaleX);
            int scaledY = (int)(mouseY / scaleY);
            
            HandleMouseMove(scaledX, scaledY);
        }
        
        public override bool HandleMouseClick(int mouseX, int mouseY, Form form)
        {
            // Scale mouse coordinates back to game resolution
            var (scaleX, scaleY) = Settings.Instance.GetScalingFactors(form);
            int scaledX = (int)(mouseX / scaleX);
            int scaledY = (int)(mouseY / scaleY);
            
            return HandleMouseClick(scaledX, scaledY);
        }
    }
}
