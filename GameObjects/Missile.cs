using System;
using System.Collections.Generic;
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
        private Queue<Point> trail;
        
        // Missile constants
        public const int DEFAULT_SIZE = 15;
        public const int DEFAULT_SPEED = 6;
        public const int MAX_BOUNCES = 8;
        public const int TRAIL_LENGTH = 20; // Number of trail points
        
        public Missile(int x, int y, int size, double velX, double velY)
        {
            posX = X = x;
            posY = Y = y;
            Size = size;
            velocityX = velX;
            velocityY = velY;
            bounces = 0;
            trail = new Queue<Point>();
            
            // Initialize trail with starting position
            trail.Enqueue(new Point(x, y));
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
            
            // Add current position to trail
            trail.Enqueue(new Point(X, Y));
            
            // Keep trail at max length
            while (trail.Count > TRAIL_LENGTH)
            {
                trail.Dequeue();
            }
        }
        
        public bool IsExpired()
        {
            return bounces >= MAX_BOUNCES;
        }
        
        public void Render(Graphics graphics)
        {
            // Draw trail first (behind the missile)
            if (trail.Count > 1)
            {
                Point[] trailPoints = trail.ToArray();
                
                // Draw trail segments with fading alpha
                for (int i = 0; i < trailPoints.Length - 1; i++)
                {
                    // Calculate alpha based on position in trail (newer = more opaque)
                    int alpha = (int)(255 * ((float)(i + 1) / trailPoints.Length) * 0.6); // Max 60% opacity
                    
                    using (Pen trailPen = new Pen(Color.FromArgb(alpha, Color.Red), 15))
                    {
                        graphics.DrawLine(trailPen, trailPoints[i], trailPoints[i + 1]);
                    }
                }
            }
            
            // Draw missile as red square (on top of trail)
            using (Brush brush = new SolidBrush(Color.Red))
            {
                graphics.FillRectangle(brush, 
                    X - Size/2, Y - Size/2, 
                    Size, Size);
            }
        }
    }
}
