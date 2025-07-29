using System;
using System.Drawing;
using WormholeGame.GameObjects;

namespace WormholeGame.Core
{
    public class Game
    {
        public Player Player { get; private set; } = null!;
        public Level CurrentLevel { get; private set; } = null!;
        public int Score { get; private set; }
        public bool IsRunning { get; private set; }
        
        private int levelTimer;
        
        // Game world constants
        public const int GAME_WIDTH = 800;
        public const int GAME_HEIGHT = 600;
        
        public bool GameJustEnded { get; private set; }
        
        public Game()
        {
            InitializeGame();
        }
        
        public void InitializeGame()
        {
            Score = 0;
            IsRunning = true;
            GameJustEnded = false;
            levelTimer = 0;
            
            Player = new Player(GAME_WIDTH / 2, GAME_HEIGHT / 2);
            CurrentLevel = new Level(1);
            
            Console.WriteLine("🎮 Game Started! Welcome to Wormhole!");
        }
        
        public void Update()
        {
            if (!IsRunning) return;
            
            if(Player.Health <= 0)
            {
                GameOver();
                return;
            }

            // Update current level
            CurrentLevel.Update(GAME_WIDTH, GAME_HEIGHT);
            
            // Check collisions and handle damage
            if (CheckCollisions())
            {
                Player.TakeDamage(10);
                Console.WriteLine($"💥 Player hit! Health: {Player.Health}");
                
                // Check if player died from this hit
                if (Player.Health <= 0)
                {
                    GameOver();
                    return;
                }
            }

            if (CurrentLevel.IsLevelComplete(levelTimer % 60 == 0))
            {
                AdvanceToNextLevel();
            }
            
            // Output debug info (less frequently)
            if (levelTimer % 60 == 0) // Every second
            {
                Console.WriteLine($"🎯 Level {CurrentLevel.Number} | " +
                                $"Wormholes: {CurrentLevel.Wormholes.Count} | " +
                                $"Missiles: {CurrentLevel.Missiles.Count} | " +
                                $"Time: {levelTimer / 60}s");
            }
            levelTimer++;
        }
        
        public void MovePlayer(int deltaX, int deltaY)
        {
            Player.Move(deltaX, deltaY, GAME_WIDTH, GAME_HEIGHT);
        }
        
        public bool CheckCollisions()
        {
            Rectangle playerRect = new Rectangle(
                Player.X - Player.Size/2, 
                Player.Y - Player.Size/2, 
                Player.Size, 
                Player.Size);
            
            // Check collisions in reverse order so we can safely remove items
            for (int i = CurrentLevel.Missiles.Count - 1; i >= 0; i--)
            {
                var missile = CurrentLevel.Missiles[i];
                Rectangle missileRect = new Rectangle(
                    missile.X - missile.Size/2, 
                    missile.Y - missile.Size/2,
                    missile.Size, 
                    missile.Size);
                
                if (playerRect.IntersectsWith(missileRect))
                {
                    // Remove the missile that hit the player
                    CurrentLevel.Missiles.RemoveAt(i);
                    Console.WriteLine($"💥 Missile destroyed on impact with player!");
                    return true; // Collision detected
                }
            }
            return false;
        }
        
        public void GameOver()
        {
            IsRunning = false;
            GameJustEnded = true;
            Console.WriteLine($"💀 GAME OVER! Final Level: {CurrentLevel.Number}, Score: {Score}");
        }
        
        public void AcknowledgeGameOver()
        {
            GameJustEnded = false;
        }
        
        public void RestartGame()
        {
            InitializeGame();
        }
        
        private void AdvanceToNextLevel()
        {
            CurrentLevel.Reset();
            CurrentLevel = new Level(CurrentLevel.Number + 1);
            levelTimer = 0;
            
            Console.WriteLine($"🎉 LEVEL UP! Welcome to Level {CurrentLevel.Number}!");
            Console.WriteLine($"   Max Missiles: {CurrentLevel.MaxMissiles}");
            Console.WriteLine($"   Max Wormholes: {CurrentLevel.MaxWormholes} (each spawns up to {CurrentLevel.MissilesPerWormhole} missiles)");
        }
        
        public bool CanContinuePlaying()
        {
            return IsRunning;
        }
    }
}
