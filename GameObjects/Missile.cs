using System;
using System.Drawing;

namespace WormholeGame.GameObjects
{
    public class Missile
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Size { get; private set; }
        
        private double velocityX;
        private double velocityY;
        private double posX;
        private double posY;
        private int bounces;
        
        // Missile constants
        public const int DEFAULT_SIZE = 15;
        public const int DEFAULT_SPEED = 6;
        public const int MAX_BOUNCES = 8;
        
        public Missile(int x, int y, int size, double velX, double velY)
        {
            posX = X = x;
            posY = Y = y;
            Size = size;
            velocityX = velX;
            velocityY = velY;
            bounces = 0;
        }
        
        public Missile(int x, int y, double velX, double velY) 
            : this(x, y, DEFAULT_SIZE, velX, velY)
        {
        }
        
        public void Update(int gameWidth, int gameHeight)
        {
            posX += velocityX;
            posY += velocityY;
            
            // Bounce off walls
            if (posX <= Size/2 || posX >= gameWidth - Size/2)
            {
                velocityX = -velocityX;
                bounces++;
            }
            if (posY <= Size/2 || posY >= gameHeight - Size/2)
            {
                velocityY = -velocityY;
                bounces++;
            }
            
            // Keep in bounds
            posX = Math.Max(Size/2, Math.Min(gameWidth - Size/2, posX));
            posY = Math.Max(Size/2, Math.Min(gameHeight - Size/2, posY));
            
            X = (int)posX;
            Y = (int)posY;
        }
        
        public bool IsExpired()
        {
            return bounces >= MAX_BOUNCES;
        }
        
        public void Render(Graphics graphics)
        {
            // Draw missile as red square
            using (Brush brush = new SolidBrush(Color.Red))
            {
                graphics.FillRectangle(brush, 
                    X - Size/2, Y - Size/2, 
                    Size, Size);
            }
        }
    }
}
