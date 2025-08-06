using System;
using System.IO;

namespace WormholeGame.Core
{
    public static class HighScoreManager
    {
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "vulps22",
            "vortex-evader"
        );
        
        private static readonly string HighScoreFilePath = Path.Combine(AppDataPath, "highscore.txt");
        
        public static int GetHighScore()
        {
            try
            {
                if (File.Exists(HighScoreFilePath))
                {
                    string content = File.ReadAllText(HighScoreFilePath).Trim();
                    if (int.TryParse(content, out int score))
                    {
                        return score;
                    }
                }
            }
            catch (Exception)
            {
                // If there's any error reading the file, return 0
            }
            
            return 0;
        }
        
        public static void SaveHighScore(int score)
        {
            try
            {
                // Create directory if it doesn't exist
                Directory.CreateDirectory(AppDataPath);
                
                // Only save if it's actually a new high score
                if (score > GetHighScore())
                {
                    File.WriteAllText(HighScoreFilePath, score.ToString());
                }
            }
            catch (Exception)
            {
                // Silently fail if we can't save the high score
                // The game should continue working even if high score saving fails
            }
        }
        
        public static bool IsNewHighScore(int score)
        {
            return score > GetHighScore();
        }
    }
}
