using System;
using System.IO;

namespace ProjectWormhole.Core
{
    public class GameStats
    {
        // Core stats
        public int HighestScore { get; set; } = 0;
        public int HighestLevel { get; set; } = 0;
        public int TotalTimePlayed { get; set; } = 0; // in seconds
        
        // Danger multiplier tracking
        public double TotalDangerMultiplierSum { get; set; } = 0;
        public int DangerMultiplierSamples { get; set; } = 0;
        public double HighestAverageDangerMultiplier { get; set; } = 0;
        public int TotalTimeAtAverageDanger { get; set; } = 0; // in seconds
        
        // Hit tracking
        public int TotalHits { get; set; } = 0;
        public int TotalTimeAlive { get; set; } = 0; // in seconds (time when not hit)
        
        // Missile avoidance
        public int TotalMissilesAvoided { get; set; } = 0;
        
        // Installation date
        public DateTime FirstPlayDate { get; set; } = DateTime.Now;
    }

    public class StatsManager
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _statsFilePath;
        private GameStats _stats = null!;

        public StatsManager(IFileSystem? fileSystem = null, string? customPath = null)
        {
            _fileSystem = fileSystem ?? new FileSystemWrapper();
            
            if (customPath != null)
            {
                _statsFilePath = customPath;
            }
            else
            {
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "vulps22",
                    "project-wormhole"
                );
                _statsFilePath = Path.Combine(appDataPath, "stats.txt");
            }
            
            LoadStats();
        }

        // Static instance for backward compatibility
        public static StatsManager Instance { get; } = new StatsManager();

        public GameStats GetStats() => _stats;

        public void RecordGameCompleted(int finalScore, int finalLevel, int playTimeSeconds)
        {
            _stats.HighestScore = Math.Max(_stats.HighestScore, finalScore);
            _stats.HighestLevel = Math.Max(_stats.HighestLevel, finalLevel);
            _stats.TotalTimePlayed += playTimeSeconds;
            SaveStats();
        }

        public void RecordDangerMultiplier(double dangerMultiplier)
        {
            _stats.TotalDangerMultiplierSum += dangerMultiplier;
            _stats.DangerMultiplierSamples++;
            // Don't save immediately - save will be called by the game at appropriate intervals
        }

        public void RecordGameSession(double averageDangerMultiplier, int timeAtAverageDanger)
        {
            _stats.HighestAverageDangerMultiplier = Math.Max(_stats.HighestAverageDangerMultiplier, averageDangerMultiplier);
            _stats.TotalTimeAtAverageDanger += timeAtAverageDanger;
            // Don't save immediately - will be saved when game ends
        }

        public void RecordHit()
        {
            _stats.TotalHits++;
            // Don't save immediately - will be saved when game ends
        }

        public void RecordTimeAlive(int seconds)
        {
            _stats.TotalTimeAlive += seconds;
            // Don't save immediately - save will be called by the game at appropriate intervals
        }

        public void RecordMissileAvoided()
        {
            _stats.TotalMissilesAvoided++;
            // Don't save immediately - save will be called by the game at appropriate intervals
        }

        public void ForceSave()
        {
            SaveStats();
        }

        public double GetAverageDangerMultiplier()
        {
            if (_stats.DangerMultiplierSamples == 0) return 1.0;
            return _stats.TotalDangerMultiplierSum / _stats.DangerMultiplierSamples;
        }

        public double GetAverageTimeBetweenHits()
        {
            if (_stats.TotalHits == 0) return 0;
            return (double)_stats.TotalTimeAlive / _stats.TotalHits;
        }

        public TimeSpan GetTotalPlayTime()
        {
            return TimeSpan.FromSeconds(_stats.TotalTimePlayed);
        }

        public TimeSpan GetAverageTimeAtAverageDanger()
        {
            if (_stats.DangerMultiplierSamples == 0) return TimeSpan.Zero;
            return TimeSpan.FromSeconds(_stats.TotalTimeAtAverageDanger / _stats.DangerMultiplierSamples);
        }

        public TimeSpan GetTotalTimeAtAverageDanger()
        {
            return TimeSpan.FromSeconds(_stats.TotalTimeAtAverageDanger);
        }

        private void LoadStats()
        {
            try
            {
                if (_fileSystem.FileExists(_statsFilePath))
                {
                    string content = _fileSystem.ReadAllText(_statsFilePath);
                    var lines = content.Split('\n');
                    
                    _stats = new GameStats();
                    
                    foreach (var line in lines)
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim();
                            var value = parts[1].Trim();
                            
                            switch (key)
                            {
                                case "HighestScore":
                                    int.TryParse(value, out var highestScore);
                                    _stats.HighestScore = highestScore;
                                    break;
                                case "HighestLevel":
                                    int.TryParse(value, out var highestLevel);
                                    _stats.HighestLevel = highestLevel;
                                    break;
                                case "TotalTimePlayed":
                                    int.TryParse(value, out var timePlayed);
                                    _stats.TotalTimePlayed = timePlayed;
                                    break;
                                case "TotalDangerMultiplierSum":
                                    double.TryParse(value, out var dangerSum);
                                    _stats.TotalDangerMultiplierSum = dangerSum;
                                    break;
                                case "DangerMultiplierSamples":
                                    int.TryParse(value, out var dangerSamples);
                                    _stats.DangerMultiplierSamples = dangerSamples;
                                    break;
                                case "HighestAverageDangerMultiplier":
                                    double.TryParse(value, out var highestAvgDanger);
                                    _stats.HighestAverageDangerMultiplier = highestAvgDanger;
                                    break;
                                case "TotalTimeAtAverageDanger":
                                    int.TryParse(value, out var timeAtAvgDanger);
                                    _stats.TotalTimeAtAverageDanger = timeAtAvgDanger;
                                    break;
                                case "TotalHits":
                                    int.TryParse(value, out var totalHits);
                                    _stats.TotalHits = totalHits;
                                    break;
                                case "TotalTimeAlive":
                                    int.TryParse(value, out var timeAlive);
                                    _stats.TotalTimeAlive = timeAlive;
                                    break;
                                case "TotalMissilesAvoided":
                                    int.TryParse(value, out var missilesAvoided);
                                    _stats.TotalMissilesAvoided = missilesAvoided;
                                    break;
                                case "FirstPlayDate":
                                    DateTime.TryParse(value, out var firstPlay);
                                    _stats.FirstPlayDate = firstPlay;
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    _stats = new GameStats();
                }
            }
            catch (Exception)
            {
                _stats = new GameStats();
            }
        }

        private void SaveStats()
        {
            try
            {
                var directory = Path.GetDirectoryName(_statsFilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    _fileSystem.CreateDirectory(directory);
                }

                var content = $"HighestScore={_stats.HighestScore}\n" +
                             $"HighestLevel={_stats.HighestLevel}\n" +
                             $"TotalTimePlayed={_stats.TotalTimePlayed}\n" +
                             $"TotalDangerMultiplierSum={_stats.TotalDangerMultiplierSum:F6}\n" +
                             $"DangerMultiplierSamples={_stats.DangerMultiplierSamples}\n" +
                             $"HighestAverageDangerMultiplier={_stats.HighestAverageDangerMultiplier:F6}\n" +
                             $"TotalTimeAtAverageDanger={_stats.TotalTimeAtAverageDanger}\n" +
                             $"TotalHits={_stats.TotalHits}\n" +
                             $"TotalTimeAlive={_stats.TotalTimeAlive}\n" +
                             $"TotalMissilesAvoided={_stats.TotalMissilesAvoided}\n" +
                             $"FirstPlayDate={_stats.FirstPlayDate:yyyy-MM-dd HH:mm:ss}\n";

                _fileSystem.WriteAllText(_statsFilePath, content);
            }
            catch (Exception)
            {
                // Silently fail if we can't save stats
            }
        }

        // Static methods for backward compatibility
        public static GameStats GetStatsStatic() => Instance.GetStats();
        public static void RecordGameCompletedStatic(int finalScore, int finalLevel, int playTimeSeconds) => 
            Instance.RecordGameCompleted(finalScore, finalLevel, playTimeSeconds);
        public static void RecordDangerMultiplierStatic(double dangerMultiplier) => Instance.RecordDangerMultiplier(dangerMultiplier);
        public static void RecordGameSessionStatic(double averageDangerMultiplier, int timeAtAverageDanger) => 
            Instance.RecordGameSession(averageDangerMultiplier, timeAtAverageDanger);
        public static void RecordHitStatic() => Instance.RecordHit();
        public static void RecordTimeAliveStatic(int seconds) => Instance.RecordTimeAlive(seconds);
        public static void RecordMissileAvoidedStatic() => Instance.RecordMissileAvoided();
        public static void ForceSaveStatic() => Instance.ForceSave();
    }
}
