using System;
using System.IO;

namespace ProjectWormhole.Core
{
    public interface IFileSystem
    {
        bool FileExists(string path);
        string ReadAllText(string path);
        void WriteAllText(string path, string content);
        void CreateDirectory(string path);
    }
    
    public class FileSystemWrapper : IFileSystem
    {
        public bool FileExists(string path) => File.Exists(path);
        public string ReadAllText(string path) => File.ReadAllText(path);
        public void WriteAllText(string path, string content) => File.WriteAllText(path, content);
        public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    }
    
    public class HighScoreManager
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _highScoreFilePath;
        
        public HighScoreManager(IFileSystem fileSystem = null, string customPath = null)
        {
            _fileSystem = fileSystem ?? new FileSystemWrapper();
            
            if (customPath != null)
            {
                _highScoreFilePath = customPath;
            }
            else
            {
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "vulps22",
                    "project-wormhole" // Updated from vortex-evader to match project name
                );
                _highScoreFilePath = Path.Combine(appDataPath, "highscore.txt");
            }
        }
        
        // Static instance for backward compatibility
        public static HighScoreManager Instance { get; } = new HighScoreManager();
        
        public int GetHighScore()
        {
            try
            {
                if (_fileSystem.FileExists(_highScoreFilePath))
                {
                    string content = _fileSystem.ReadAllText(_highScoreFilePath).Trim();
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
        
        public void SaveHighScore(int score)
        {
            try
            {
                // Create directory if it doesn't exist
                var directory = Path.GetDirectoryName(_highScoreFilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    _fileSystem.CreateDirectory(directory);
                }
                
                // Only save if it's actually a new high score
                if (score > GetHighScore())
                {
                    _fileSystem.WriteAllText(_highScoreFilePath, score.ToString());
                }
            }
            catch (Exception)
            {
                // Silently fail if we can't save the high score
                // The game should continue working even if high score saving fails
            }
        }
        
        public bool IsNewHighScore(int score)
        {
            return score > GetHighScore();
        }
        
        // Static methods for backward compatibility
        public static int GetHighScoreStatic() => Instance.GetHighScore();
        public static void SaveHighScoreStatic(int score) => Instance.SaveHighScore(score);
        public static bool IsNewHighScoreStatic(int score) => Instance.IsNewHighScore(score);
    }
}
