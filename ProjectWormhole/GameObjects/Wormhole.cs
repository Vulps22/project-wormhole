using System;
using System.Drawing;

namespace ProjectWormhole.GameObjects
{
    public class Wormhole
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Size { get; private set; }
        public int MissilesSpawned => missilesSpawned; // Expose for logic calculations
        
        private int missileSpawnTimer;
        private int missilesSpawned;
        private Random random;
        private bool shouldExpire;
        private int originalSize;
        
        // Wormhole constants - each object knows its own properties!
        public const int DEFAULT_SIZE = 100;
        public const int MISSILE_SPAWN_INTERVAL = 60; // Every second
        public const int MAX_MISSILES_PER_WORMHOLE = 4;
        
        public Wormhole(int x, int y, int size = DEFAULT_SIZE)
        {
            X = x;
            Y = y;
            Size = size;
            originalSize = size;
            missileSpawnTimer = 0;
            missilesSpawned = 0;
            shouldExpire = false;
            random = new Random();
        }
        
        public void Update()
        {
            missileSpawnTimer++;
            
            // If we should expire, start shrinking
            if (shouldExpire)
            {
                Size = Math.Max(0, Size - 2); // Shrink by 2 pixels per frame
            }
        }
        
        public bool ShouldExpire()
        {
            // Expire when we've spawned all our missiles
            return missilesSpawned >= MAX_MISSILES_PER_WORMHOLE;
        }
        
        public void StartExpiring()
        {
            shouldExpire = true;
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
            return Size <= 0; // Only truly expired when size reaches 0
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
