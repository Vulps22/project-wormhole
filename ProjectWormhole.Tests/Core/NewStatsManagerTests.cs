using System;
using System.IO;
using Xunit;
using ProjectWormhole.Core;

namespace ProjectWormhole.Tests.Core
{
    public class NewStatsManagerTests
    {
        [Fact]
        public void NewStats_InitialState_HasDefaultValues()
        {
            // Arrange
            var mockFileSystem = new MockStatsFileSystem();
            var statsManager = new StatsManager(mockFileSystem, "test_stats.txt");

            // Act
            var stats = statsManager.GetStats();

            // Assert
            Assert.Equal(0, stats.HighestScore);
            Assert.Equal(0, stats.HighestLevel);
            Assert.Equal(0, stats.TotalTimePlayed);
            Assert.Equal(0, stats.TotalMissilesAvoided);
            Assert.Equal(0.0, stats.HighestAverageDangerMultiplier);
            Assert.Equal(0, stats.TotalHits);
        }

        [Fact]
        public void RecordGameCompleted_UpdatesHighestScoreAndLevel()
        {
            // Arrange
            var mockFileSystem = new MockStatsFileSystem();
            var statsManager = new StatsManager(mockFileSystem, "test_stats.txt");

            // Act
            statsManager.RecordGameCompleted(1000, 5, 120);

            // Assert
            var stats = statsManager.GetStats();
            Assert.Equal(1000, stats.HighestScore);
            Assert.Equal(5, stats.HighestLevel);
            Assert.Equal(120, stats.TotalTimePlayed);
        }

        [Fact]
        public void RecordDangerMultiplier_TracksAverageCorrectly()
        {
            // Arrange
            var mockFileSystem = new MockStatsFileSystem();
            var statsManager = new StatsManager(mockFileSystem, "test_stats.txt");

            // Act
            statsManager.RecordDangerMultiplier(2.0);
            statsManager.RecordDangerMultiplier(4.0);
            statsManager.RecordDangerMultiplier(3.0);

            // Assert
            double averageDanger = statsManager.GetAverageDangerMultiplier();
            Assert.Equal(3.0, averageDanger, 1); // Within 1 decimal place
        }

        [Fact]
        public void RecordMissileAvoided_IncrementsCounter()
        {
            // Arrange
            var mockFileSystem = new MockStatsFileSystem();
            var statsManager = new StatsManager(mockFileSystem, "test_stats.txt");

            // Act
            statsManager.RecordMissileAvoided();
            statsManager.RecordMissileAvoided();
            statsManager.RecordMissileAvoided();

            // Assert
            var stats = statsManager.GetStats();
            Assert.Equal(3, stats.TotalMissilesAvoided);
        }

        [Fact]
        public void RecordHitAndTimeAlive_CalculatesAverageTimeBetweenHits()
        {
            // Arrange
            var mockFileSystem = new MockStatsFileSystem();
            var statsManager = new StatsManager(mockFileSystem, "test_stats.txt");

            // Act
            statsManager.RecordTimeAlive(60); // 60 seconds alive
            statsManager.RecordHit(); // First hit
            statsManager.RecordTimeAlive(120); // 120 more seconds alive
            statsManager.RecordHit(); // Second hit

            // Assert
            double averageTimeBetweenHits = statsManager.GetAverageTimeBetweenHits();
            Assert.Equal(90.0, averageTimeBetweenHits, 1); // (60 + 120) / 2 = 90
        }

        [Fact]
        public void StaticMethods_WorkCorrectly()
        {
            // Act
            StatsManager.RecordDangerMultiplierStatic(5.0);
            StatsManager.RecordMissileAvoidedStatic();

            // Assert
            var stats = StatsManager.GetStatsStatic();
            Assert.True(stats.TotalMissilesAvoided >= 1);
        }
    }

    // Mock file system for testing (reusing from previous implementation)
    public class MockStatsFileSystem : IFileSystem
    {
        private string? _content;
        private bool _directoryCreated;

        public bool FileExists(string path) => _content != null;

        public string ReadAllText(string path) => _content ?? "";

        public void WriteAllText(string path, string content)
        {
            _content = content;
        }

        public void CreateDirectory(string path)
        {
            _directoryCreated = true;
        }

        public string GetCurrentDirectory() => Environment.CurrentDirectory;
    }
}
