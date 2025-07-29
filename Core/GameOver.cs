using System;
using System.Drawing;

namespace WormholeGame.Core
{
    public class GameOver
    {
        public bool IsVisible { get; private set; }
        
        private Rectangle restartButton;
        private bool isRestartHovered;
        private int finalLevel;
        private int finalScore;
        
        public GameOver()
        {
            IsVisible = false;
            
            // Position restart button in center of screen
            int buttonWidth = 120;
            int buttonHeight = 40;
            restartButton = new Rectangle(
                (Game.GAME_WIDTH - buttonWidth) / 2,
                (Game.GAME_HEIGHT / 2) + 60,
                buttonWidth,
                buttonHeight
            );
        }
        
        public void Show(int level, int score)
        {
            IsVisible = true;
            finalLevel = level;
            finalScore = score;
        }
        
        public void Hide()
        {
            IsVisible = false;
        }
        
        public void Update()
        {
            // Nothing to update for now
        }
        
        public void HandleMouseMove(int mouseX, int mouseY)
        {
            isRestartHovered = restartButton.Contains(mouseX, mouseY);
        }
        
        public bool HandleMouseClick(int mouseX, int mouseY)
        {
            if (restartButton.Contains(mouseX, mouseY))
            {
                Hide();
                return true; // Restart button clicked
            }
            return false;
        }
        
        public void Render(Graphics graphics)
        {
            if (!IsVisible) return;
            
            // Semi-transparent overlay
            using (Brush overlayBrush = new SolidBrush(Color.FromArgb(180, Color.Black)))
            {
                graphics.FillRectangle(overlayBrush, 0, 0, Game.GAME_WIDTH, Game.GAME_HEIGHT);
            }
            
            // Game Over title
            using (Font titleFont = new Font("Arial", 32, FontStyle.Bold))
            using (Brush titleBrush = new SolidBrush(Color.Red))
            {
                string title = "GAME OVER";
                SizeF titleSize = graphics.MeasureString(title, titleFont);
                float titleX = (Game.GAME_WIDTH - titleSize.Width) / 2;
                float titleY = Game.GAME_HEIGHT / 2 - 80;
                graphics.DrawString(title, titleFont, titleBrush, titleX, titleY);
            }
            
            // Final stats
            using (Font statsFont = new Font("Arial", 16))
            using (Brush statsBrush = new SolidBrush(Color.White))
            {
                string stats = $"Final Level: {finalLevel}\nFinal Score: {finalScore}";
                SizeF statsSize = graphics.MeasureString(stats, statsFont);
                float statsX = (Game.GAME_WIDTH - statsSize.Width) / 2;
                float statsY = Game.GAME_HEIGHT / 2 - 20;
                graphics.DrawString(stats, statsFont, statsBrush, statsX, statsY);
            }
            
            // Restart button
            Color buttonColor = isRestartHovered ? Color.LightGray : Color.Gray;
            using (Brush buttonBrush = new SolidBrush(buttonColor))
            using (Pen buttonPen = new Pen(Color.White, 2))
            using (Font buttonFont = new Font("Arial", 12, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(Color.Black))
            {
                graphics.FillRectangle(buttonBrush, restartButton);
                graphics.DrawRectangle(buttonPen, restartButton);
                
                string buttonText = "RESTART";
                SizeF textSize = graphics.MeasureString(buttonText, buttonFont);
                float textX = restartButton.X + (restartButton.Width - textSize.Width) / 2;
                float textY = restartButton.Y + (restartButton.Height - textSize.Height) / 2;
                graphics.DrawString(buttonText, buttonFont, textBrush, textX, textY);
            }
        }
    }
}
