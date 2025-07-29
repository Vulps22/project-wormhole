using System;
using System.Drawing;
using System.Windows.Forms;

namespace WormholeGame.Core
{
    public class Menu
    {
        public bool IsVisible { get; private set; }
        
        private Rectangle playButton;
        private bool isPlayButtonHovered;
        
        // Menu constants
        private const int BUTTON_WIDTH = 200;
        private const int BUTTON_HEIGHT = 60;
        
        public Menu()
        {
            IsVisible = true;
            
            // Center the play button
            int buttonX = (Game.GAME_WIDTH - BUTTON_WIDTH) / 2;
            int buttonY = (Game.GAME_HEIGHT - BUTTON_HEIGHT) / 2;
            playButton = new Rectangle(buttonX, buttonY, BUTTON_WIDTH, BUTTON_HEIGHT);
        }
        
        public void Update()
        {
            // Menu update logic (if needed)
        }
        
        public void HandleMouseMove(int mouseX, int mouseY)
        {
            isPlayButtonHovered = playButton.Contains(mouseX, mouseY);
        }
        
        public bool HandleMouseClick(int mouseX, int mouseY)
        {
            if (playButton.Contains(mouseX, mouseY))
            {
                // Play button clicked
                IsVisible = false;
                return true; // Signal to start the game
            }
            return false;
        }
        
        public void Show()
        {
            IsVisible = true;
        }
        
        public void Hide()
        {
            IsVisible = false;
        }
        
        public void Render(Graphics graphics)
        {
            if (!IsVisible) return;
            
            // Semi-transparent overlay
            using (Brush overlay = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
            {
                graphics.FillRectangle(overlay, 0, 0, Game.GAME_WIDTH, Game.GAME_HEIGHT);
            }
            
            // Draw PLAY button
            Color buttonColor = isPlayButtonHovered ? Color.LightGray : Color.White;
            using (Brush buttonBrush = new SolidBrush(buttonColor))
            using (Brush borderBrush = new SolidBrush(Color.Black))
            using (Font buttonFont = new Font("Arial", 24, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(Color.Black))
            {
                // Draw button border
                graphics.FillRectangle(borderBrush, playButton.X - 2, playButton.Y - 2, 
                    playButton.Width + 4, playButton.Height + 4);
                
                // Draw button
                graphics.FillRectangle(buttonBrush, playButton);
                
                // Draw button text
                string buttonText = "PLAY";
                SizeF textSize = graphics.MeasureString(buttonText, buttonFont);
                float textX = playButton.X + (playButton.Width - textSize.Width) / 2;
                float textY = playButton.Y + (playButton.Height - textSize.Height) / 2;
                graphics.DrawString(buttonText, buttonFont, textBrush, textX, textY);
            }
            
            // Draw title
            using (Font titleFont = new Font("Arial", 48, FontStyle.Bold))
            using (Brush titleBrush = new SolidBrush(Color.White))
            {
                string title = "WORMHOLE";
                SizeF titleSize = graphics.MeasureString(title, titleFont);
                float titleX = (Game.GAME_WIDTH - titleSize.Width) / 2;
                float titleY = playButton.Y - 120;
                graphics.DrawString(title, titleFont, titleBrush, titleX, titleY);
            }
        }
    }
}
