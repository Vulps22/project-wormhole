using System;
using System.Drawing;
using System.Windows.Forms;

namespace WormholeGame.Core
{
    public class MainMenu : Menu
    {
        private MenuManager menuManager;
        
        private Rectangle playButton;
        private Rectangle settingsButton;
        private Rectangle creditsButton;
        private Rectangle quitButton;
        private bool isPlayButtonHovered;
        private bool isSettingsButtonHovered;
        private bool isCreditsButtonHovered;
        private bool isQuitButtonHovered;
        
        // Menu constants
        private const int BUTTON_WIDTH = 200;
        private const int BUTTON_HEIGHT = 40; // Smaller since just text
        private const int BUTTON_SPACING = 40; // Closer together
        
        public MainMenu(MenuManager manager)
        {
            this.menuManager = manager;
            IsVisible = true;
            
            // Center the buttons (now 4 buttons)
            int buttonX = (Settings.Instance.Resolution.Width - BUTTON_WIDTH) / 2;
            int startY = (Settings.Instance.Resolution.Height - (BUTTON_HEIGHT * 4 + BUTTON_SPACING * 3)) / 2;
            
            playButton = new Rectangle(buttonX, startY, BUTTON_WIDTH, BUTTON_HEIGHT);
            settingsButton = new Rectangle(buttonX, startY + BUTTON_HEIGHT + BUTTON_SPACING, BUTTON_WIDTH, BUTTON_HEIGHT);
            creditsButton = new Rectangle(buttonX, startY + (BUTTON_HEIGHT + BUTTON_SPACING) * 2, BUTTON_WIDTH, BUTTON_HEIGHT);
            quitButton = new Rectangle(buttonX, startY + (BUTTON_HEIGHT + BUTTON_SPACING) * 3, BUTTON_WIDTH, BUTTON_HEIGHT);
        }
        
        public override void Update()
        {
            // Menu update logic (if needed)
        }
        
        public override void RecalculateLayout()
        {
            // Recalculate button positions for new resolution (now 4 buttons)
            int buttonX = (Settings.Instance.Resolution.Width - BUTTON_WIDTH) / 2;
            int startY = (Settings.Instance.Resolution.Height - (BUTTON_HEIGHT * 4 + BUTTON_SPACING * 3)) / 2;
            
            playButton = new Rectangle(buttonX, startY, BUTTON_WIDTH, BUTTON_HEIGHT);
            settingsButton = new Rectangle(buttonX, startY + BUTTON_HEIGHT + BUTTON_SPACING, BUTTON_WIDTH, BUTTON_HEIGHT);
            creditsButton = new Rectangle(buttonX, startY + (BUTTON_HEIGHT + BUTTON_SPACING) * 2, BUTTON_WIDTH, BUTTON_HEIGHT);
            quitButton = new Rectangle(buttonX, startY + (BUTTON_HEIGHT + BUTTON_SPACING) * 3, BUTTON_WIDTH, BUTTON_HEIGHT);
        }
        
        public override void HandleMouseMove(int mouseX, int mouseY)
        {
            isPlayButtonHovered = playButton.Contains(mouseX, mouseY);
            isSettingsButtonHovered = settingsButton.Contains(mouseX, mouseY);
            isCreditsButtonHovered = creditsButton.Contains(mouseX, mouseY);
            isQuitButtonHovered = quitButton.Contains(mouseX, mouseY);
        }
        
        public override void HandleMouseMove(int mouseX, int mouseY, Form form)
        {
            // Scale mouse coordinates back to game resolution
            var (scaleX, scaleY) = Settings.Instance.GetScalingFactors(form);
            int scaledX = (int)(mouseX / scaleX);
            int scaledY = (int)(mouseY / scaleY);
            
            HandleMouseMove(scaledX, scaledY);
        }
        
        public override bool HandleMouseClick(int mouseX, int mouseY)
        {
            if (playButton.Contains(mouseX, mouseY))
            {
                // Play button clicked - start the game
                menuManager.StartGame();
                return true;
            }
            else if (settingsButton.Contains(mouseX, mouseY))
            {
                // Settings button clicked
                menuManager.ShowSettingsMenu();
                return true;
            }
            else if (creditsButton.Contains(mouseX, mouseY))
            {
                // Credits button clicked
                menuManager.ShowCreditsMenu();
                return true;
            }
            else if (quitButton.Contains(mouseX, mouseY))
            {
                // Quit button clicked
                menuManager.QuitGame();
                return true;
            }
            return false; // No button clicked
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
            // MainMenu doesn't need mouse down handling
        }

        public override void HandleMouseDown(int mouseX, int mouseY, Form form)
        {
            // MainMenu doesn't need mouse down handling
        }

        public override void HandleMouseUp(int mouseX, int mouseY, Form form)
        {
            // MainMenu doesn't need mouse up handling
        }
        
        public override void Render(Graphics graphics)
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
            
            // Draw CREDITS button
            RenderButton(graphics, creditsButton, isCreditsButtonHovered, "CREDITS");
            
            // Draw QUIT button
            RenderButton(graphics, quitButton, isQuitButtonHovered, "QUIT");
            
            // Draw title
            using (Font titleFont = new Font("Arial", 48, FontStyle.Bold))
            using (Brush titleBrush = new SolidBrush(Color.White))
            {
                string title = "VORTEX EVADER";
                SizeF titleSize = graphics.MeasureString(title, titleFont);
                float titleX = (Settings.Instance.Resolution.Width - titleSize.Width) / 2;
                float titleY = playButton.Y - 120;
                graphics.DrawString(title, titleFont, titleBrush, titleX, titleY);
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
        }        private void RenderButton(Graphics graphics, Rectangle button, bool isHovered, string text)
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
