using System;
using System.Drawing;

namespace WormholeGame.Core
{
    public enum SettingsMenuAction
    {
        None,
        BackToMainMenu
    }

    public class SettingsMenu : Menu
    {
        private MenuManager menuManager;
        public bool IsDirty { get; private set; } = false; // Track if settings have changed

        private Rectangle backButton;
        private Rectangle windowModeButton;
        private Rectangle resolutionButton;
        private Rectangle applyButton;

        private bool isBackHovered;
        private bool isWindowModeHovered;
        private bool isResolutionHovered;
        private bool isApplyHovered;
        private int currentResolutionIndex;

        private Form Window { get; set; } = null!; // Reference to the main form

        public SettingsMenu(MenuManager manager, Form window)
        {
            this.menuManager = manager;
            this.Window = window;
            IsVisible = false;

            // Find current resolution index
            var currentRes = Settings.Instance.Resolution;
            currentResolutionIndex = Array.FindIndex(Settings.AvailableResolutions,
                r => r.Width == currentRes.Width && r.Height == currentRes.Height);
            if (currentResolutionIndex == -1) currentResolutionIndex = 0;

            SetupButtons();
        }

        private void SetupButtons()
        {
            int buttonWidth = 200;
            int buttonHeight = 30; // Smaller for text-only buttons
            int centerX = (Settings.Instance.Resolution.Width - buttonWidth) / 2;
            int startY = 200;
            int spacing = 40; // Closer together

            windowModeButton = new Rectangle(centerX, startY, buttonWidth, buttonHeight);
            resolutionButton = new Rectangle(centerX, startY + spacing, buttonWidth, buttonHeight);
            applyButton = new Rectangle(centerX, startY + spacing * 2, buttonWidth, buttonHeight);
            backButton = new Rectangle(centerX, startY + spacing * 3 + 20, buttonWidth, buttonHeight);
        }

        public override void Update()
        {
            // Nothing to update for now
        }

        public void ClearDirtyFlag()
        {
            IsDirty = false;
        }
        
        public override void RecalculateLayout()
        {
            SetupButtons();
        }

        public override void HandleMouseMove(int mouseX, int mouseY)
        {
            isWindowModeHovered = windowModeButton.Contains(mouseX, mouseY);
            isResolutionHovered = resolutionButton.Contains(mouseX, mouseY);
            isApplyHovered = applyButton.Contains(mouseX, mouseY);
            isBackHovered = backButton.Contains(mouseX, mouseY);
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
            if (windowModeButton.Contains(mouseX, mouseY))
            {
                CycleWindowMode();
                return false;
            }
            else if (resolutionButton.Contains(mouseX, mouseY))
            {
                CycleResolution();
                return false;
            }
            else if (applyButton.Contains(mouseX, mouseY))
            {
                ApplySettings();
                return false;
            }
            else if (backButton.Contains(mouseX, mouseY))
            {
                menuManager.ShowMainMenu();
                return true; // Back to main menu
            }

            return false;
        }
        
        public override bool HandleMouseClick(int mouseX, int mouseY, Form form)
        {
            // Scale mouse coordinates back to game resolution
            var (scaleX, scaleY) = Settings.Instance.GetScalingFactors(form);
            int scaledX = (int)(mouseX / scaleX);
            int scaledY = (int)(mouseY / scaleY);
            
            return HandleMouseClick(scaledX, scaledY);
        }

        private void CycleWindowMode()
        {
            var modes = Enum.GetValues<WindowMode>();
            int currentIndex = Array.IndexOf(modes, Settings.Instance.WindowMode);
            currentIndex = (currentIndex + 1) % modes.Length;
            Settings.Instance.WindowMode = modes[currentIndex];
        }

        private void CycleResolution()
        {
            currentResolutionIndex = (currentResolutionIndex + 1) % Settings.AvailableResolutions.Length;
            Settings.Instance.Resolution = Settings.AvailableResolutions[currentResolutionIndex];
        }

        private void ApplySettings()
        {
            Settings.Instance.ApplyToForm(Window);
            IsDirty = true; // Mark settings as dirty so Form1 can reinitialize

            // Immediately recalculate our own layout since resolution might have changed
            SetupButtons();

            // This will be handled by Form1 when it detects settings change
            Console.WriteLine("Settings applied!");
        }

        public override void Render(Graphics graphics)
        {
            if (!IsVisible) return;

            // Semi-transparent overlay
            using (Brush overlayBrush = new SolidBrush(Color.FromArgb(180, Color.Black)))
            {
                graphics.FillRectangle(overlayBrush, 0, 0, Settings.Instance.Resolution.Width, Settings.Instance.Resolution.Height);
            }

            // Settings title
            using (Font titleFont = new Font("Arial", 24, FontStyle.Bold))
            using (Brush titleBrush = new SolidBrush(Color.White))
            {
                string title = "SETTINGS";
                SizeF titleSize = graphics.MeasureString(title, titleFont);
                float titleX = (Settings.Instance.Resolution.Width - titleSize.Width) / 2;
                graphics.DrawString(title, titleFont, titleBrush, titleX, 120);
            }

            // Window Mode button
            RenderButton(graphics, windowModeButton, isWindowModeHovered,
                $"Window Mode: {Settings.Instance.GetWindowModeText()}");

            // Resolution button
            RenderButton(graphics, resolutionButton, isResolutionHovered,
                $"Resolution: {Settings.Instance.GetResolutionText()}");

            // Apply button
            RenderButton(graphics, applyButton, isApplyHovered, "APPLY");

            // Back button
            RenderButton(graphics, backButton, isBackHovered, "BACK");
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

        private void RenderButton(Graphics graphics, Rectangle button, bool isHovered, string text)
        {
            // Modern text-based button - clean and minimal
            Color textColor = isHovered ? Color.Gray : Color.White;
            int fontSize = text == "BACK" ? 16 : 14;
            FontStyle fontStyle = text == "BACK" ? FontStyle.Bold : FontStyle.Regular;

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
