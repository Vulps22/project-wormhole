using System;
using System.Drawing;

namespace WormholeGame.GameObjects
{
    public class Player
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Size { get; private set; }
        
        // Player constants
        public const int DEFAULT_SIZE = 20;
        public const int DEFAULT_SPEED = 5;
        
        public Player(int x, int y, int size = DEFAULT_SIZE)
        {
            X = x;
            Y = y;
            Size = size;
        }
        
        public void Move(int deltaX, int deltaY, int gameWidth, int gameHeight)
        {
            X = Math.Max(Size/2, Math.Min(gameWidth - Size/2, X + deltaX));
            Y = Math.Max(Size/2, Math.Min(gameHeight - Size/2, Y + deltaY));
        }
        
        public void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public void Render(Graphics graphics)
        {
            // Draw player as white square
            using (Brush brush = new SolidBrush(Color.White))
            {
                graphics.FillRectangle(brush, 
                    X - Size/2, Y - Size/2, 
                    Size, Size);
            }
        }
    }
}
