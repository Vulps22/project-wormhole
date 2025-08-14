using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProjectWormhole.Core
{
    public class StatsMenu : Menu
    {
        private MenuManager menuManager;
        private Rectangle backButton;
        private bool isBackButtonHovered;
        
        // Menu constants
        private const int BUTTON_WIDTH = 100;
        private const int BUTTON_HEIGHT = 40;
        
        public StatsMenu(MenuManager manager)
        {
            this.menuManager = manager;
            IsVisible = true;
            
            // Position back button at bottom center
            int buttonX = (Settings.Instance.Resolution.Width - BUTTON_WIDTH) / 2;
            int buttonY = Settings.Instance.Resolution.Height - 100;
            backButton = new Rectangle(buttonX, buttonY, BUTTON_WIDTH, BUTTON_HEIGHT);
        }
        
        public override void Update()
        {
            // Stats menu doesn't need regular updates
        }
        
        public override void RecalculateLayout()
        {
            // Recalculate button position for new resolution
            int buttonX = (Settings.Instance.Resolution.Width - BUTTON_WIDTH) / 2;
            int buttonY = Settings.Instance.Resolution.Height - 100;
            backButton = new Rectangle(buttonX, buttonY, BUTTON_WIDTH, BUTTON_HEIGHT);
        }
        
        public override void HandleMouseMove(int mouseX, int mouseY)
        {
            isBackButtonHovered = backButton.Contains(mouseX, mouseY);
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
            if (backButton.Contains(mouseX, mouseY))
            {
                menuManager.ShowMainMenu();
                return true;
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

        public override void HandleMouseDown(int mouseX, int mouseY)
        {
            // StatsMenu doesn't need mouse down handling
        }

        public override void HandleMouseDown(int mouseX, int mouseY, Form form)
        {
            // StatsMenu doesn't need mouse down handling
        }

        public override void HandleMouseUp(int mouseX, int mouseY, Form form)
        {
            // StatsMenu doesn't need mouse up handling
        }
        
        public override void Render(Graphics graphics)
        {
            if (!IsVisible) return;

            // Semi-transparent overlay
            using (Brush overlay = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
            {
                graphics.FillRectangle(overlay, 0, 0, Settings.Instance.Resolution.Width, Settings.Instance.Resolution.Height);
            }
            
            // Draw title
            using (Font titleFont = new Font("Arial", 36, FontStyle.Bold))
            using (Brush titleBrush = new SolidBrush(Color.White))
            {
                string title = "STATISTICS";
                SizeF titleSize = graphics.MeasureString(title, titleFont);
                float titleX = (Settings.Instance.Resolution.Width - titleSize.Width) / 2;
                float titleY = 50;
                graphics.DrawString(title, titleFont, titleBrush, titleX, titleY);
            }
            
            // Get stats
            var stats = StatsManager.GetStatsStatic();
            var statsManager = StatsManager.Instance;
            
            // Draw statistics
            using (Font statsFont = new Font("Arial", 16))
            using (Brush statsBrush = new SolidBrush(Color.White))
            using (Brush labelBrush = new SolidBrush(Color.LightGray))
            {
                float startY = 150;
                float lineHeight = 35;
                float labelX = Settings.Instance.Resolution.Width / 2 - 250;
                float valueX = Settings.Instance.Resolution.Width / 2 + 50;
                
                // Highest Score
                int highScore = HighScoreManager.GetHighScoreStatic();
                graphics.DrawString("Highest Score:", statsFont, labelBrush, labelX, startY);
                graphics.DrawString(highScore.ToString("N0"), statsFont, statsBrush, valueX, startY);
                
                // Highest Level
                graphics.DrawString("Highest Level:", statsFont, labelBrush, labelX, startY + lineHeight);
                graphics.DrawString(stats.HighestLevel.ToString("N0"), statsFont, statsBrush, valueX, startY + lineHeight);
                
                // Total time played
                var totalPlayTime = statsManager.GetTotalPlayTime();
                graphics.DrawString("Total Time Played:", statsFont, labelBrush, labelX, startY + lineHeight * 2);
                string playTimeText = $"{totalPlayTime.Hours:D2}h {totalPlayTime.Minutes:D2}m {totalPlayTime.Seconds:D2}s";
                graphics.DrawString(playTimeText, statsFont, statsBrush, valueX, startY + lineHeight * 2);
                
                // Average danger multiplier
                double avgDanger = statsManager.GetAverageDangerMultiplier();
                graphics.DrawString("Average Danger Multiplier:", statsFont, labelBrush, labelX, startY + lineHeight * 3);
                graphics.DrawString(avgDanger.ToString("F2") + "x", statsFont, statsBrush, valueX, startY + lineHeight * 3);
                
                // Highest average danger multiplier
                graphics.DrawString("Highest Avg Danger Multiplier:", statsFont, labelBrush, labelX, startY + lineHeight * 4);
                graphics.DrawString(stats.HighestAverageDangerMultiplier.ToString("F2") + "x", statsFont, statsBrush, valueX, startY + lineHeight * 4);
                
                // Average time at average danger
                var avgTimeAtDanger = statsManager.GetAverageTimeAtAverageDanger();
                graphics.DrawString("Avg Time at Avg Danger:", statsFont, labelBrush, labelX, startY + lineHeight * 5);
                string avgTimeText = $"{avgTimeAtDanger.Minutes:D2}m {avgTimeAtDanger.Seconds:D2}s";
                graphics.DrawString(avgTimeText, statsFont, statsBrush, valueX, startY + lineHeight * 5);
                
                // Total time at average danger
                var totalTimeAtDanger = statsManager.GetTotalTimeAtAverageDanger();
                graphics.DrawString("Total Time at Avg Danger:", statsFont, labelBrush, labelX, startY + lineHeight * 6);
                string totalTimeText = $"{totalTimeAtDanger.Hours:D2}h {totalTimeAtDanger.Minutes:D2}m {totalTimeAtDanger.Seconds:D2}s";
                graphics.DrawString(totalTimeText, statsFont, statsBrush, valueX, startY + lineHeight * 6);
                
                // Average time between hits
                double avgTimeBetweenHits = statsManager.GetAverageTimeBetweenHits();
                graphics.DrawString("Avg Time Between Hits:", statsFont, labelBrush, labelX, startY + lineHeight * 7);
                graphics.DrawString(avgTimeBetweenHits.ToString("F1") + "s", statsFont, statsBrush, valueX, startY + lineHeight * 7);
                
                // Total missiles avoided
                graphics.DrawString("Total Missiles Avoided:", statsFont, labelBrush, labelX, startY + lineHeight * 8);
                graphics.DrawString(stats.TotalMissilesAvoided.ToString("N0"), statsFont, statsBrush, valueX, startY + lineHeight * 8);
            }
            
            // Draw back button
            RenderButton(graphics, backButton, isBackButtonHovered, "BACK");
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
            // Modern text-based button - no backgrounds, just clean text
            Color textColor = isHovered ? Color.Gray : Color.White;
            
            using (Font buttonFont = new Font("Arial", 18, FontStyle.Regular))
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
