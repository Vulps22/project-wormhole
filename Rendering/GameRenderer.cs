using System.Drawing;
using WormholeGame.Core;
using WormholeGame.GameObjects;

namespace WormholeGame.Rendering
{
    public class GameRenderer
    {
        public void Render(Graphics graphics, GameState gameState)
        {
            // Clear screen
            graphics.Clear(Color.Black);
            
            // Draw player (now renders itself)
            gameState.Player.Render(graphics);
            
            // Draw wormholes (now render themselves)
            foreach (var wormhole in gameState.Wormholes)
            {
                wormhole.Render(graphics);
            }
            
            // Draw missiles (now render themselves)
            foreach (var missile in gameState.Missiles)
            {
                missile.Render(graphics);
            }
            
            // Draw UI
            using (Brush textBrush = new SolidBrush(Color.White))
            using (Font font = new Font("Arial", 12))
            {
                graphics.DrawString($"Level: {gameState.Level}", font, textBrush, 10, 10);
                graphics.DrawString($"Missiles: {gameState.Missiles.Count}/{gameState.Level * 3}", 
                    font, textBrush, 10, 30);
                graphics.DrawString("WASD to move", font, textBrush, 10, GameState.GAME_HEIGHT - 30);
            }
        }
    }
}
