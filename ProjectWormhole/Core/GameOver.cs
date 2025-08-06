using System;
using System.Drawing;

namespace ProjectWormhole.Core
{
    public class GameOverMenu : Menu
    {
        private MenuManager menuManager;
        private Rectangle restartButton;
        private Rectangle mainMenuButton;
        private bool isRestartHovered;
        private bool isMainMenuHovered;
        private int finalLevel;
        private int finalScore;
        
        public GameOverMenu(MenuManager manager, int level, int score)
        {
            this.menuManager = manager;
            this.finalLevel = level;
            this.finalScore = score;
            
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            // Two buttons side by side
            int buttonWidth = 120;
            int buttonHeight = 30;
            int buttonSpacing = 40;
            int totalWidth = (buttonWidth * 2) + buttonSpacing;
            int startX = (Settings.Instance.Resolution.Width - totalWidth) / 2;
            int buttonY = (Settings.Instance.Resolution.Height / 2) + 60;
            
            restartButton = new Rectangle(startX, buttonY, buttonWidth, buttonHeight);
            mainMenuButton = new Rectangle(startX + buttonWidth + buttonSpacing, buttonY, buttonWidth, buttonHeight);
        }
        
        public override void Update()
        {
            // Nothing to update for now
        }
        
        public override void RecalculateLayout()
        {
            SetupButtons();
        }
        
        public override void HandleMouseMove(int mouseX, int mouseY)
        {
            isRestartHovered = restartButton.Contains(mouseX, mouseY);
            isMainMenuHovered = mainMenuButton.Contains(mouseX, mouseY);
        }
        
        public override bool HandleMouseClick(int mouseX, int mouseY)
        {
            if (restartButton.Contains(mouseX, mouseY))
            {
                menuManager.StartGame();
                return true; // Restart button clicked
            }
            else if (mainMenuButton.Contains(mouseX, mouseY))
            {
                menuManager.ShowMainMenu();
                return true; // Main menu button clicked
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
            
            // Show "NEW HIGH SCORE!" if applicable
            if (HighScoreManager.IsNewHighScoreStatic(finalScore))
            {
                using (Font highScoreFont = new Font("Arial", 20, FontStyle.Bold))
                using (Brush highScoreBrush = new SolidBrush(Color.Gold))
                {
                    string newHighScoreText = "NEW HIGH SCORE!";
                    SizeF newHighScoreSize = graphics.MeasureString(newHighScoreText, highScoreFont);
                    float newHighScoreX = (Settings.Instance.Resolution.Width - newHighScoreSize.Width) / 2;
                    float newHighScoreY = Settings.Instance.Resolution.Height / 2 + 20;
                    graphics.DrawString(newHighScoreText, highScoreFont, highScoreBrush, newHighScoreX, newHighScoreY);
                }
            }
            
            // Restart button - modern text style
            Color restartTextColor = isRestartHovered ? Color.Gray : Color.White;
            using (Font buttonFont = new Font("Arial", 16, FontStyle.Bold))
            using (Brush restartTextBrush = new SolidBrush(restartTextColor))
            {
                string restartText = "RESTART";
                SizeF restartTextSize = graphics.MeasureString(restartText, buttonFont);
                float restartTextX = restartButton.X + (restartButton.Width - restartTextSize.Width) / 2;
                float restartTextY = restartButton.Y + (restartButton.Height - restartTextSize.Height) / 2;
                graphics.DrawString(restartText, buttonFont, restartTextBrush, restartTextX, restartTextY);
            }
            
            // Main Menu button - modern text style
            Color mainMenuTextColor = isMainMenuHovered ? Color.Gray : Color.White;
            using (Font buttonFont = new Font("Arial", 16, FontStyle.Bold))
            using (Brush mainMenuTextBrush = new SolidBrush(mainMenuTextColor))
            {
                string mainMenuText = "MAIN MENU";
                SizeF mainMenuTextSize = graphics.MeasureString(mainMenuText, buttonFont);
                float mainMenuTextX = mainMenuButton.X + (mainMenuButton.Width - mainMenuTextSize.Width) / 2;
                float mainMenuTextY = mainMenuButton.Y + (mainMenuButton.Height - mainMenuTextSize.Height) / 2;
                graphics.DrawString(mainMenuText, buttonFont, mainMenuTextBrush, mainMenuTextX, mainMenuTextY);
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

        public override void HandleMouseDown(int mouseX, int mouseY)
        {
            // GameOver doesn't need mouse down handling
        }

        public override void HandleMouseDown(int mouseX, int mouseY, Form form)
        {
            // GameOver doesn't need mouse down handling
        }

        public override void HandleMouseUp(int mouseX, int mouseY, Form form)
        {
            // GameOver doesn't need mouse up handling
        }
    }
}
