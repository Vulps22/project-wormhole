using System;
using System.Drawing;

namespace WormholeGame.GameObjects
{
    public class Wormhole
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Size { get; private set; }
        
        private int lifeTime;
        private int missileSpawnTimer;
        private int missilesSpawned;
        private Random random;
        
        // Wormhole constants - each object knows its own properties!
        public const int DEFAULT_SIZE = 100;
        public const int LIFETIME_FRAMES = 300; // 5 seconds at 60 FPS
        public const int MISSILE_SPAWN_INTERVAL = 60; // Every second
        public const int MAX_MISSILES_PER_WORMHOLE = 5;
        
        public Wormhole(int x, int y, int size = DEFAULT_SIZE)
        {
            X = x;
            Y = y;
            Size = size;
            lifeTime = LIFETIME_FRAMES;
            missileSpawnTimer = 0;
            missilesSpawned = 0;
            random = new Random();
        }
        
        public void Update()
        {
            lifeTime--;
            missileSpawnTimer++;
        }
        
        public bool ShouldSpawnMissile()
        {
            if (missilesSpawned >= MAX_MISSILES_PER_WORMHOLE) return false;
            if (missileSpawnTimer > MISSILE_SPAWN_INTERVAL)
            {
                missileSpawnTimer = 0;
                missilesSpawned++;
                return true;
            }
            return false;
        }
        
        public bool IsExpired()
        {
            return lifeTime <= 0;
        }
        
        public void Render(Graphics graphics)
        {
            int centerX = X - Size/2;
            int centerY = Y - Size/2;
            
            // Draw blue outer ring
            using (Brush outerBrush = new SolidBrush(Color.Blue))
            {
                graphics.FillEllipse(outerBrush, 
                    centerX, centerY, Size, Size);
            }
            
            // Draw white inner circle (60% of outer size)
            int innerSize = (int)(Size * 0.6f);
            int innerOffset = (Size - innerSize) / 2;
            using (Brush innerBrush = new SolidBrush(Color.White))
            {
                graphics.FillEllipse(innerBrush, 
                    centerX + innerOffset, centerY + innerOffset,
                    innerSize, innerSize);
            }
        }
    }
}
