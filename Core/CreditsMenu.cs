using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace WormholeGame.Core
{
    public class CreditSection
    {
        public string Title { get; set; } = string.Empty;
        public List<string> Credits { get; set; } = new List<string>();
    }

    public class CreditsMenu
    {
        public bool IsVisible { get; private set; }
        private Rectangle backButton;
        private bool isBackHovered;
        private List<CreditSection> creditSections = new List<CreditSection>();
        private int scrollOffset = 0;
        private const int LINE_HEIGHT = 25;
        private const int SECTION_SPACING = 40;

        public CreditsMenu()
        {
            IsVisible = false;
            LoadCredits();
            SetupButtons();
        }

        private void SetupButtons()
        {
            int buttonWidth = 100;
            int buttonHeight = 30;
            int centerX = (Settings.Instance.Resolution.Width - buttonWidth) / 2;
            int bottomY = Settings.Instance.Resolution.Height - 80;

            backButton = new Rectangle(centerX, bottomY, buttonWidth, buttonHeight);
        }

        private void LoadCredits()
        {
            creditSections.Clear();
            string creditPath = Path.Combine(Directory.GetCurrentDirectory(), "credit");
            
            if (!Directory.Exists(creditPath))
            {
                Console.WriteLine("Credits folder not found!");
                return;
            }

            var sectionFolders = Directory.GetDirectories(creditPath);
            
            foreach (var folder in sectionFolders)
            {
                var section = new CreditSection();
                
                // Read title.txt for section title
                string titleFile = Path.Combine(folder, "title.txt");
                if (File.Exists(titleFile))
                {
                    section.Title = File.ReadAllText(titleFile).Trim();
                }
                else
                {
                    section.Title = Path.GetFileName(folder); // Fallback to folder name
                }

                // Read all other .txt files for credits
                var creditFiles = Directory.GetFiles(folder, "*.txt")
                    .Where(f => !Path.GetFileName(f).Equals("title.txt", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f);

                foreach (var file in creditFiles)
                {
                    try
                    {
                        string content = File.ReadAllText(file).Trim();
                        if (!string.IsNullOrEmpty(content))
                        {
                            section.Credits.Add(content);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading credit file {file}: {ex.Message}");
                    }
                }

                if (section.Credits.Count > 0)
                {
                    creditSections.Add(section);
                }
            }

            Console.WriteLine($"Loaded {creditSections.Count} credit sections");
        }

        public void Show()
        {
            IsVisible = true;
            scrollOffset = 0; // Reset scroll when showing
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public void Update()
        {
            // Handle scrolling if needed
        }

        public void HandleMouseMove(int mouseX, int mouseY)
        {
            isBackHovered = backButton.Contains(mouseX, mouseY);
        }

        public void HandleMouseMove(int mouseX, int mouseY, Form form)
        {
            // Scale mouse coordinates back to game resolution
            var (scaleX, scaleY) = Settings.Instance.GetScalingFactors(form);
            int scaledX = (int)(mouseX / scaleX);
            int scaledY = (int)(mouseY / scaleY);
            
            HandleMouseMove(scaledX, scaledY);
        }

        public bool HandleMouseClick(int mouseX, int mouseY)
        {
            if (backButton.Contains(mouseX, mouseY))
            {
                Hide();
                return true; // Back to settings menu
            }

            return false;
        }

        public bool HandleMouseClick(int mouseX, int mouseY, Form form)
        {
            // Scale mouse coordinates back to game resolution
            var (scaleX, scaleY) = Settings.Instance.GetScalingFactors(form);
            int scaledX = (int)(mouseX / scaleX);
            int scaledY = (int)(mouseY / scaleY);
            
            return HandleMouseClick(scaledX, scaledY);
        }

        public void RecalculateLayout()
        {
            SetupButtons();
        }

        public void Render(Graphics graphics)
        {
            if (!IsVisible) return;

            // Semi-transparent overlay
            using (Brush overlayBrush = new SolidBrush(Color.FromArgb(180, Color.Black)))
            {
                graphics.FillRectangle(overlayBrush, 0, 0, Settings.Instance.Resolution.Width, Settings.Instance.Resolution.Height);
            }

            // Credits title
            using (Font titleFont = new Font("Arial", 24, FontStyle.Bold))
            using (Brush titleBrush = new SolidBrush(Color.White))
            {
                string title = "CREDITS";
                SizeF titleSize = graphics.MeasureString(title, titleFont);
                float titleX = (Settings.Instance.Resolution.Width - titleSize.Width) / 2;
                graphics.DrawString(title, titleFont, titleBrush, titleX, 40);
            }

            // Render credit sections
            int currentY = 100 - scrollOffset;
            
            foreach (var section in creditSections)
            {
                // Section title
                using (Font sectionFont = new Font("Arial", 18, FontStyle.Bold))
                using (Brush sectionBrush = new SolidBrush(Color.Cyan))
                {
                    graphics.DrawString(section.Title, sectionFont, sectionBrush, 50, currentY);
                    currentY += 30;
                }

                // Section credits
                using (Font creditFont = new Font("Arial", 12))
                using (Brush creditBrush = new SolidBrush(Color.White))
                {
                    foreach (var credit in section.Credits)
                    {
                        // Handle multi-line credits
                        string[] lines = credit.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                        {
                            graphics.DrawString(line.Trim(), creditFont, creditBrush, 70, currentY);
                            currentY += LINE_HEIGHT;
                        }
                        currentY += 10; // Small space between credit entries
                    }
                }

                currentY += SECTION_SPACING; // Space between sections
            }

            // Back button
            RenderButton(graphics, backButton, isBackHovered, "BACK");
        }

        public void Render(Graphics graphics, Form form)
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
            
            using (Font buttonFont = new Font("Arial", 16, FontStyle.Bold))
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
