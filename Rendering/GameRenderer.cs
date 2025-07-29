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
                graphics.DrawString("WASD to move", font, textBrush, 10, Game.GAME_HEIGHT - 30);
            }
        }
    }
}
