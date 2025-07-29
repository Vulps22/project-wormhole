using System.Drawing;
using WormholeGame.Core;
using WormholeGame.GameObjects;

namespace WormholeGame.Rendering
{
    public class GameRenderer
    {
        public void Render(Graphics graphics, Game game)
        {
            // Clear screen
            graphics.Clear(Color.Black);
            
            // Draw player (now renders itself)
            game.Player.Render(graphics);
            
            // Draw wormholes (now render themselves)
            foreach (var wormhole in game.CurrentLevel.Wormholes)
            {
                wormhole.Render(graphics);
            }
            
            // Draw missiles (now render themselves)
            foreach (var missile in game.CurrentLevel.Missiles)
            {
                missile.Render(graphics);
            }
            
            // Draw UI
            using (Brush textBrush = new SolidBrush(Color.White))
            using (Font font = new Font("Arial", 12))
            {
                graphics.DrawString($"Level: {game.CurrentLevel.Number}", font, textBrush, 10, 10);
                graphics.DrawString($"Missiles: {game.CurrentLevel.Missiles.Count}/{game.CurrentLevel.MaxMissiles}", 
                    font, textBrush, 10, 30);
                graphics.DrawString($"Wormholes: {game.CurrentLevel.Wormholes.Count}/{game.CurrentLevel.MaxWormholes}", 
                    font, textBrush, 10, 50);
                
                // Draw health bar
                DrawHealthBar(graphics, game.Player.Health, game.Player.MaxHealth);
                
                graphics.DrawString("WASD to move", font, textBrush, 10, Game.GAME_HEIGHT - 30);
            }
        }
        
        private void DrawHealthBar(Graphics graphics, int currentHealth, int maxHealth)
        {
            const int barWidth = 200;
            const int barHeight = 20;
            const int barX = 10;
            const int barY = 75;
            
            // Calculate health percentage
            double healthPercent = (double)currentHealth / maxHealth;
            int fillWidth = (int)(barWidth * healthPercent);
            
            // Draw background (black border)
            using (Brush borderBrush = new SolidBrush(Color.White))
            {
                graphics.FillRectangle(borderBrush, barX - 2, barY - 2, barWidth + 4, barHeight + 4);
            }
            
            // Draw background (dark)
            using (Brush bgBrush = new SolidBrush(Color.DarkRed))
            {
                graphics.FillRectangle(bgBrush, barX, barY, barWidth, barHeight);
            }
            
            // Draw health fill
            if (fillWidth > 0)
            {
                Color healthColor = healthPercent > 0.6 ? Color.Green : 
                                   healthPercent > 0.3 ? Color.Yellow : Color.Red;
                using (Brush healthBrush = new SolidBrush(healthColor))
                {
                    graphics.FillRectangle(healthBrush, barX, barY, fillWidth, barHeight);
                }
            }
            
            // Draw health text
            using (Brush textBrush = new SolidBrush(Color.White))
            using (Font font = new Font("Arial", 10, FontStyle.Bold))
            {
                string healthText = $"Health: {currentHealth}/{maxHealth}";
                graphics.DrawString(healthText, font, textBrush, barX + barWidth + 10, barY + 2);
            }
        }
    }
}
