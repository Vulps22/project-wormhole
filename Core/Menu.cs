using System;
using System.Drawing;
using System.Windows.Forms;

namespace WormholeGame.Core
{
    public class Menu
    {
        public bool IsVisible { get; private set; }
        
        private Rectangle playButton;
        private Rectangle settingsButton;
        private bool isPlayButtonHovered;
        private bool isSettingsButtonHovered;
        
        // Menu constants
        private const int BUTTON_WIDTH = 200;
        private const int BUTTON_HEIGHT = 40; // Smaller since just text
        private const int BUTTON_SPACING = 40; // Closer together
        
        public Menu()
        {
            IsVisible = true;
            
            // Center the buttons
            int buttonX = (Settings.Instance.Resolution.Width - BUTTON_WIDTH) / 2;
            int startY = (Settings.Instance.Resolution.Height - (BUTTON_HEIGHT * 2 + BUTTON_SPACING)) / 2;
            
            playButton = new Rectangle(buttonX, startY, BUTTON_WIDTH, BUTTON_HEIGHT);
            settingsButton = new Rectangle(buttonX, startY + BUTTON_HEIGHT + BUTTON_SPACING, BUTTON_WIDTH, BUTTON_HEIGHT);
        }
        
        public void Update()
        {
            // Menu update logic (if needed)
        }
        
        public void RecalculateLayout()
        {
            // Recalculate button positions for new resolution
            int buttonX = (Settings.Instance.Resolution.Width - BUTTON_WIDTH) / 2;
            int startY = (Settings.Instance.Resolution.Height - (BUTTON_HEIGHT * 2 + BUTTON_SPACING)) / 2;
            
            playButton = new Rectangle(buttonX, startY, BUTTON_WIDTH, BUTTON_HEIGHT);
            settingsButton = new Rectangle(buttonX, startY + BUTTON_HEIGHT + BUTTON_SPACING, BUTTON_WIDTH, BUTTON_HEIGHT);
        }
        
        public void HandleMouseMove(int mouseX, int mouseY)
        {
            isPlayButtonHovered = playButton.Contains(mouseX, mouseY);
            isSettingsButtonHovered = settingsButton.Contains(mouseX, mouseY);
        }
        
        public string HandleMouseClick(int mouseX, int mouseY)
        {
            if (playButton.Contains(mouseX, mouseY))
            {
                // Play button clicked
                IsVisible = false;
                return "play"; // Signal to start the game
            }
            else if (settingsButton.Contains(mouseX, mouseY))
            {
                // Settings button clicked
                return "settings"; // Signal to show settings
            }
            return ""; // No button clicked
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
                graphics.FillRectangle(overlay, 0, 0, Settings.Instance.Resolution.Width, Settings.Instance.Resolution.Height);
            }
            
            // Draw PLAY button
            RenderButton(graphics, playButton, isPlayButtonHovered, "PLAY");
            
            // Draw SETTINGS button
            RenderButton(graphics, settingsButton, isSettingsButtonHovered, "SETTINGS");
            
            // Draw title
            using (Font titleFont = new Font("Arial", 48, FontStyle.Bold))
            using (Brush titleBrush = new SolidBrush(Color.White))
            {
                string title = "WORMHOLE";
                SizeF titleSize = graphics.MeasureString(title, titleFont);
                float titleX = (Settings.Instance.Resolution.Width - titleSize.Width) / 2;
                float titleY = playButton.Y - 120;
                graphics.DrawString(title, titleFont, titleBrush, titleX, titleY);
            }
        }
        
        private void RenderButton(Graphics graphics, Rectangle button, bool isHovered, string text)
        {
            // Modern text-based button - no backgrounds, just clean text
            Color textColor = isHovered ? Color.Gray : Color.White;
            FontStyle fontStyle = text == "PLAY" ? FontStyle.Bold : FontStyle.Regular;
            int fontSize = text == "PLAY" ? 36 : 18;
            
            using (Font buttonFont = new Font("Arial", fontSize, fontStyle))
            using (Brush textBrush = new SolidBrush(textColor))
            {
                SizeF textSize = graphics.MeasureString(text, buttonFont);
                float textX = button.X + (button.Width - textSize.Width) / 2;
                float textY = button.Y + (button.Height - textSize.Height) / 2;
                graphics.DrawString(text, buttonFont, textBrush, textX, textY);
            }
        }
    }
}
