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
        
        // Dropdown state
        private bool windowModeDropdownOpen = false;
        private bool resolutionDropdownOpen = false;
        private List<Rectangle> windowModeOptions = new List<Rectangle>();
        private List<Rectangle> resolutionOptions = new List<Rectangle>();
        private int hoveredWindowModeOption = -1;
        private int hoveredResolutionOption = -1;
        
        // Pending settings (not applied until user clicks Apply)
        private WindowMode pendingWindowMode;
        private int pendingResolutionIndex;

        private Form Window { get; set; } = null!; // Reference to the main form

        public SettingsMenu(MenuManager manager, Form window)
        {
            this.menuManager = manager;
            this.Window = window;
            IsVisible = false;

            // Initialize pending settings with current values
            pendingWindowMode = Settings.Instance.WindowMode;
            var currentRes = Settings.Instance.Resolution;
            pendingResolutionIndex = Array.FindIndex(Settings.AvailableResolutions,
                r => r.Width == currentRes.Width && r.Height == currentRes.Height);
            if (pendingResolutionIndex == -1) pendingResolutionIndex = 0;

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
            
            SetupDropdownOptions();
        }
        
        private void SetupDropdownOptions()
        {
            windowModeOptions.Clear();
            resolutionOptions.Clear();
            
            int optionWidth = 180;
            int optionHeight = 25;
            int dropdownX = windowModeButton.Right + 20; // To the right of the main button
            
            // Window mode dropdown options
            var modes = Enum.GetValues<WindowMode>();
            for (int i = 0; i < modes.Length; i++)
            {
                Rectangle optionRect = new Rectangle(
                    dropdownX, 
                    windowModeButton.Y + (i * optionHeight), 
                    optionWidth, 
                    optionHeight);
                windowModeOptions.Add(optionRect);
            }
            
            // Resolution dropdown options
            for (int i = 0; i < Settings.AvailableResolutions.Length; i++)
            {
                Rectangle optionRect = new Rectangle(
                    dropdownX, 
                    resolutionButton.Y + (i * optionHeight), 
                    optionWidth, 
                    optionHeight);
                resolutionOptions.Add(optionRect);
            }
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
            
            // Handle dropdown option hovers
            hoveredWindowModeOption = -1;
            hoveredResolutionOption = -1;
            
            if (windowModeDropdownOpen)
            {
                for (int i = 0; i < windowModeOptions.Count; i++)
                {
                    if (windowModeOptions[i].Contains(mouseX, mouseY))
                    {
                        hoveredWindowModeOption = i;
                        break;
                    }
                }
            }
            
            if (resolutionDropdownOpen)
            {
                for (int i = 0; i < resolutionOptions.Count; i++)
                {
                    if (resolutionOptions[i].Contains(mouseX, mouseY))
                    {
                        hoveredResolutionOption = i;
                        break;
                    }
                }
            }
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
            // Check dropdown option clicks first
            if (windowModeDropdownOpen)
            {
                for (int i = 0; i < windowModeOptions.Count; i++)
                {
                    if (windowModeOptions[i].Contains(mouseX, mouseY))
                    {
                        var modes = Enum.GetValues<WindowMode>();
                        pendingWindowMode = modes[i];
                        windowModeDropdownOpen = false;
                        return false;
                    }
                }
                // Click outside dropdown closes it
                windowModeDropdownOpen = false;
            }
            
            if (resolutionDropdownOpen)
            {
                for (int i = 0; i < resolutionOptions.Count; i++)
                {
                    if (resolutionOptions[i].Contains(mouseX, mouseY))
                    {
                        pendingResolutionIndex = i;
                        resolutionDropdownOpen = false;
                        return false;
                    }
                }
                // Click outside dropdown closes it
                resolutionDropdownOpen = false;
            }
            
            // Handle main button clicks
            if (windowModeButton.Contains(mouseX, mouseY))
            {
                windowModeDropdownOpen = !windowModeDropdownOpen;
                resolutionDropdownOpen = false; // Close other dropdown
                return false;
            }
            else if (resolutionButton.Contains(mouseX, mouseY))
            {
                resolutionDropdownOpen = !resolutionDropdownOpen;
                windowModeDropdownOpen = false; // Close other dropdown
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

        private void ApplySettings()
        {
            // Apply pending settings to actual settings
            Settings.Instance.WindowMode = pendingWindowMode;
            Settings.Instance.Resolution = Settings.AvailableResolutions[pendingResolutionIndex];
            
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
            string pendingWindowModeText = GetWindowModeText(pendingWindowMode);
            RenderButton(graphics, windowModeButton, isWindowModeHovered,
                $"Window Mode: {pendingWindowModeText}");

            // Resolution button
            string pendingResolutionText = GetResolutionText(Settings.AvailableResolutions[pendingResolutionIndex]);
            RenderButton(graphics, resolutionButton, isResolutionHovered,
                $"Resolution: {pendingResolutionText}");

            // Apply button
            RenderButton(graphics, applyButton, isApplyHovered, "APPLY");

            // Back button
            RenderButton(graphics, backButton, isBackHovered, "BACK");
            
            // Render dropdowns if open
            if (windowModeDropdownOpen)
            {
                RenderWindowModeDropdown(graphics);
            }
            
            if (resolutionDropdownOpen)
            {
                RenderResolutionDropdown(graphics);
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
        
        private string GetWindowModeText(WindowMode mode)
        {
            return mode switch
            {
                WindowMode.Windowed => "Windowed",
                WindowMode.FullScreen => "Full Screen",
                WindowMode.FullScreenWindowed => "Full Screen (Windowed)",
                _ => "Unknown"
            };
        }
        
        private string GetResolutionText(Size resolution)
        {
            return $"{resolution.Width}x{resolution.Height}";
        }
        
        private void RenderWindowModeDropdown(Graphics graphics)
        {
            var modes = Enum.GetValues<WindowMode>();
            
            for (int i = 0; i < modes.Length; i++)
            {
                bool isSelected = modes[i] == pendingWindowMode;
                bool isHovered = hoveredWindowModeOption == i;
                
                // Dropdown background
                Color bgColor = isSelected ? Color.FromArgb(100, Color.Blue) : 
                               isHovered ? Color.FromArgb(50, Color.White) : 
                               Color.FromArgb(180, Color.Black);
                               
                using (Brush bgBrush = new SolidBrush(bgColor))
                {
                    graphics.FillRectangle(bgBrush, windowModeOptions[i]);
                }
                
                // Dropdown border
                using (Pen borderPen = new Pen(Color.White, 1))
                {
                    graphics.DrawRectangle(borderPen, windowModeOptions[i]);
                }
                
                // Dropdown text
                Color textColor = isSelected ? Color.Yellow : Color.White;
                using (Font optionFont = new Font("Arial", 12))
                using (Brush textBrush = new SolidBrush(textColor))
                {
                    string text = GetWindowModeText(modes[i]);
                    graphics.DrawString(text, optionFont, textBrush, 
                        windowModeOptions[i].X + 5, windowModeOptions[i].Y + 2);
                }
            }
        }
        
        private void RenderResolutionDropdown(Graphics graphics)
        {
            for (int i = 0; i < Settings.AvailableResolutions.Length; i++)
            {
                bool isSelected = i == pendingResolutionIndex;
                bool isHovered = hoveredResolutionOption == i;
                
                // Dropdown background
                Color bgColor = isSelected ? Color.FromArgb(100, Color.Blue) : 
                               isHovered ? Color.FromArgb(50, Color.White) : 
                               Color.FromArgb(180, Color.Black);
                               
                using (Brush bgBrush = new SolidBrush(bgColor))
                {
                    graphics.FillRectangle(bgBrush, resolutionOptions[i]);
                }
                
                // Dropdown border
                using (Pen borderPen = new Pen(Color.White, 1))
                {
                    graphics.DrawRectangle(borderPen, resolutionOptions[i]);
                }
                
                // Dropdown text
                Color textColor = isSelected ? Color.Yellow : Color.White;
                using (Font optionFont = new Font("Arial", 12))
                using (Brush textBrush = new SolidBrush(textColor))
                {
                    string text = GetResolutionText(Settings.AvailableResolutions[i]);
                    graphics.DrawString(text, optionFont, textBrush, 
                        resolutionOptions[i].X + 5, resolutionOptions[i].Y + 2);
                }
            }
        }
    }
}
