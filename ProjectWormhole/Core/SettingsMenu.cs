using System;
using System.Drawing;
using System.Reflection.Metadata;

namespace ProjectWormhole.Core
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
        private Rectangle masterVolumeSlider;
        private Rectangle musicVolumeSlider;
        private Rectangle sfxVolumeSlider;
        private Rectangle applyButton;

        private bool isBackHovered;
        private bool isWindowModeHovered;
        private bool isResolutionHovered;
        private bool isMasterVolumeHovered;
        private bool isMusicVolumeHovered;
        private bool isSfxVolumeHovered;
        private bool isApplyHovered;
        
        // Volume slider state
        private bool isDraggingMasterVolume = false;
        private bool isDraggingMusicVolume = false;
        private bool isDraggingSfxVolume = false;
        
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
        private float pendingMasterVolume;
        private float pendingMusicVolume;
        private float pendingSfxVolume;

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
            
            // Initialize pending volume settings
            pendingMasterVolume = Settings.Instance.MasterVolume;
            pendingMusicVolume = Settings.Instance.MusicVolume;
            pendingSfxVolume = Settings.Instance.SfxVolume;

            SetupButtons();
        }

        private void SetupButtons()
        {
            int buttonWidth = 200;
            int buttonHeight = 30;
            int centerX = (Settings.Instance.Resolution.Width - buttonWidth) / 2;
            int startY = 150; // Start higher to make room for volume sliders
            int spacing = 45; // More spacing between elements

            windowModeButton = new Rectangle(centerX, startY, buttonWidth, buttonHeight);
            resolutionButton = new Rectangle(centerX, startY + spacing, buttonWidth, buttonHeight);
            
            // Volume sliders - more spacing and better positioning
            int sliderWidth = 200;
            int sliderHeight = 20;
            int sliderX = centerX; // Align with buttons
            int volumeStartY = startY + spacing * 2 + 20; // More space after resolution
            
            masterVolumeSlider = new Rectangle(sliderX, volumeStartY, sliderWidth, sliderHeight);
            musicVolumeSlider = new Rectangle(sliderX, volumeStartY + 40, sliderWidth, sliderHeight);
            sfxVolumeSlider = new Rectangle(sliderX, volumeStartY + 80, sliderWidth, sliderHeight);
            
            applyButton = new Rectangle(centerX, volumeStartY + 140, buttonWidth, buttonHeight);
            backButton = new Rectangle(centerX, volumeStartY + 180, buttonWidth, buttonHeight);
            
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
            isMasterVolumeHovered = masterVolumeSlider.Contains(mouseX, mouseY);
            isMusicVolumeHovered = musicVolumeSlider.Contains(mouseX, mouseY);
            isSfxVolumeHovered = sfxVolumeSlider.Contains(mouseX, mouseY);
            isApplyHovered = applyButton.Contains(mouseX, mouseY);
            isBackHovered = backButton.Contains(mouseX, mouseY);

            //Console.WriteLine($"[DEBUG] HandleMouseMove {isDraggingMasterVolume}");

            // Handle volume slider dragging - allow dragging even outside slider bounds
            if (isDraggingMasterVolume)
            {
                Console.WriteLine($"[DEBUG] HandleMouseMove Called! Mouse dragged to: {mouseX}, {mouseY}");
                pendingMasterVolume = CalculateVolumeFromMouseX(mouseX, masterVolumeSlider);
                AudioManager.Instance.UpdateMusicVolume(); // Preview volume change
            }
            else if (isDraggingMusicVolume)
            {
                pendingMusicVolume = CalculateVolumeFromMouseX(mouseX, musicVolumeSlider);
                AudioManager.Instance.UpdateMusicVolume(); // Preview volume change
            }
            else if (isDraggingSfxVolume)
            {
                pendingSfxVolume = CalculateVolumeFromMouseX(mouseX, sfxVolumeSlider);
                // Could play a preview SFX here
            }
            
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

        public override void HandleMouseDown(int mouseX, int mouseY, Form form)
        {
            HandleMouseDown(mouseX, mouseY);
        }


        public override void HandleMouseDown(int mouseX, int mouseY)
        {
            Console.WriteLine($"[DEBUG] HandleMouseDown Called! Mouse down at: {mouseX}, {mouseY}");
            // Check volume slider clicks first - allow immediate volume setting and start dragging
            if (masterVolumeSlider.Contains(mouseX, mouseY))
            {
                Console.WriteLine($"[DEBUG] Master volume slider down at: {mouseX}, {mouseY}");
                isDraggingMasterVolume = true;
                pendingMasterVolume = CalculateVolumeFromMouseX(mouseX, masterVolumeSlider);
                AudioManager.Instance.UpdateMusicVolume(); // Immediate preview
                return;
            }
            else if (musicVolumeSlider.Contains(mouseX, mouseY))
            {
                isDraggingMusicVolume = true;
                pendingMusicVolume = CalculateVolumeFromMouseX(mouseX, musicVolumeSlider);
                AudioManager.Instance.UpdateMusicVolume(); // Immediate preview
                return;
            }
            else if (sfxVolumeSlider.Contains(mouseX, mouseY))
            {
                isDraggingSfxVolume = true;
                pendingSfxVolume = CalculateVolumeFromMouseX(mouseX, sfxVolumeSlider);
                // Could play immediate SFX preview here
                return;
            }
        }

        private float CalculateVolumeFromMouseX(int mouseX, Rectangle sliderRect)
        {
            // Calculate relative position within slider (0 to 1)
            float relativeX = (float)(mouseX - sliderRect.X) / sliderRect.Width;

            // Clamp to valid range
            relativeX = Math.Max(0, Math.Min(1, relativeX));

            return relativeX;
        }
        
        public override bool HandleMouseClick(int mouseX, int mouseY, Form form)
        {
            // Scale mouse coordinates back to game resolution
            var (scaleX, scaleY) = Settings.Instance.GetScalingFactors(form);
            int scaledX = (int)(mouseX / scaleX);
            int scaledY = (int)(mouseY / scaleY);
            
            return HandleMouseClick(scaledX, scaledY);
        }

        public override void HandleMouseUp(int mouseX, int mouseY, Form form)
        {
            // Scale mouse coordinates back to game resolution
            var (scaleX, scaleY) = Settings.Instance.GetScalingFactors(form);
            int scaledX = (int)(mouseX / scaleX);
            int scaledY = (int)(mouseY / scaleY);
            
            // Stop all volume dragging
            isDraggingMasterVolume = false;
            isDraggingMusicVolume = false;
            isDraggingSfxVolume = false;
        }

        private void ApplySettings()
        {
            // Apply pending settings to actual settings
            Settings.Instance.WindowMode = pendingWindowMode;
            Settings.Instance.Resolution = Settings.AvailableResolutions[pendingResolutionIndex];
            Settings.Instance.MasterVolume = pendingMasterVolume;
            Settings.Instance.MusicVolume = pendingMusicVolume;
            Settings.Instance.SfxVolume = pendingSfxVolume;
            
            // Update audio manager with new volumes
            AudioManager.Instance.UpdateMusicVolume();
            
            Settings.Instance.ApplyToForm(Window);
            IsDirty = true; // Mark settings as dirty so Form1 can reinitialize

            // Immediately recalculate our own layout since resolution might have changed
            SetupButtons();

            // This will be handled by Form1 when it detects settings change
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
            
            // Volume sliders
            RenderVolumeSlider(graphics, masterVolumeSlider, pendingMasterVolume, "Master Volume");
            RenderVolumeSlider(graphics, musicVolumeSlider, pendingMusicVolume, "Music Volume");
            RenderVolumeSlider(graphics, sfxVolumeSlider, pendingSfxVolume, "SFX Volume");
            
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

        private void RenderVolumeSlider(Graphics graphics, Rectangle sliderRect, float volume, string label)
        {
            // Draw label above the slider with more spacing
            using (Font labelFont = new Font("Arial", 12))
            using (Brush labelBrush = new SolidBrush(Color.White))
            {
                graphics.DrawString(label, labelFont, labelBrush, 
                    sliderRect.X, sliderRect.Y - 20);
            }

            // Draw slider track
            using (Brush trackBrush = new SolidBrush(Color.Gray))
            {
                graphics.FillRectangle(trackBrush, sliderRect);
            }

            // Draw slider border
            using (Pen borderPen = new Pen(Color.White, 2))
            {
                graphics.DrawRectangle(borderPen, sliderRect);
            }

            // Draw volume fill
            int fillWidth = (int)(sliderRect.Width * volume);
            if (fillWidth > 0)
            {
                Rectangle fillRect = new Rectangle(sliderRect.X, sliderRect.Y, fillWidth, sliderRect.Height);
                using (Brush fillBrush = new SolidBrush(Color.LightBlue))
                {
                    graphics.FillRectangle(fillBrush, fillRect);
                }
            }

            // Draw slider handle/thumb at current volume position
            int handleX = sliderRect.X + (int)(sliderRect.Width * volume);
            int handleY = sliderRect.Y - 2; // Slightly taller than track
            int handleWidth = 6;
            int handleHeight = sliderRect.Height + 4;
            
            Rectangle handleRect = new Rectangle(handleX - handleWidth/2, handleY, handleWidth, handleHeight);
            
            // Draw handle shadow for depth
            Rectangle shadowRect = new Rectangle(handleRect.X + 1, handleRect.Y + 1, handleRect.Width, handleRect.Height);
            using (Brush shadowBrush = new SolidBrush(Color.FromArgb(128, Color.Black)))
            {
                graphics.FillRectangle(shadowBrush, shadowRect);
            }
            
            // Draw main handle
            using (Brush handleBrush = new SolidBrush(Color.White))
            {
                graphics.FillRectangle(handleBrush, handleRect);
            }
            
            // Draw handle border
            using (Pen handleBorderPen = new Pen(Color.Black, 1))
            {
                graphics.DrawRectangle(handleBorderPen, handleRect);
            }

            // Draw volume percentage to the right of the slider
            using (Font percentFont = new Font("Arial", 11))
            using (Brush percentBrush = new SolidBrush(Color.White))
            {
                string percentText = $"{(int)(volume * 100)}%";
                graphics.DrawString(percentText, percentFont, percentBrush,
                    sliderRect.X + sliderRect.Width + 15, 
                    sliderRect.Y + 2);
            }
        }
    }
}
