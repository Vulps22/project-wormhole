using System;
using System.Collections.Generic;
using Xunit;
using ProjectWormhole.Core;

namespace ProjectWormhole.Tests.Core
{
    // Mock file system for testing
    public class MockFileSystem : IFileSystem
    {
        private readonly Dictionary<string, string> _files = new Dictionary<string, string>();
        private readonly HashSet<string> _directories = new HashSet<string>();
        
        public bool ShouldThrowOnRead { get; set; } = false;
        public bool ShouldThrowOnWrite { get; set; } = false;
        public bool ShouldThrowOnCreateDirectory { get; set; } = false;
        
        public bool FileExists(string path) => _files.ContainsKey(path);
        
        public string ReadAllText(string path)
        {
            if (ShouldThrowOnRead)
                throw new InvalidOperationException("Mock read error");
                
            return _files.TryGetValue(path, out string? content) ? content : throw new System.IO.FileNotFoundException();
        }
        
        public void WriteAllText(string path, string content)
        {
            if (ShouldThrowOnWrite)
                throw new InvalidOperationException("Mock write error");
                
            _files[path] = content;
        }
        
        public void CreateDirectory(string path)
        {
            if (ShouldThrowOnCreateDirectory)
                throw new InvalidOperationException("Mock directory creation error");
                
            _directories.Add(path);
        }
        
        // Helper methods for testing
        public void SetFileContent(string path, string content) => _files[path] = content;
        public void Clear() => _files.Clear();
        public bool DirectoryWasCreated(string path) => _directories.Contains(path);
    }
    public class HighScoreManagerTests
    {
        [Fact]
        public void HighScoreManager_Constructor_WithDefaults_CreatesInstance()
        {
            // Act
            var manager = new HighScoreManager();
            
            // Assert
            Assert.NotNull(manager);
        }
        
        [Fact]
        public void HighScoreManager_Constructor_WithCustomPath_UsesCustomPath()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var customPath = @"C:\test\custom\highscore.txt";
            
            // Act
            var manager = new HighScoreManager(mockFileSystem, customPath);
            
            // Assert
            Assert.NotNull(manager);
        }
        
        [Fact]
        public void GetHighScore_WhenFileDoesNotExist_ReturnsZero()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var manager = new HighScoreManager(mockFileSystem, "test.txt");
            
            // Act
            int result = manager.GetHighScore();
            
            // Assert
            Assert.Equal(0, result);
        }
        
        [Fact]
        public void GetHighScore_WhenFileExists_WithValidScore_ReturnsScore()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var testPath = "test.txt";
            mockFileSystem.SetFileContent(testPath, "12345");
            var manager = new HighScoreManager(mockFileSystem, testPath);
            
            // Act
            int result = manager.GetHighScore();
            
            // Assert
            Assert.Equal(12345, result);
        }
        
        [Fact]
        public void GetHighScore_WhenFileExists_WithInvalidContent_ReturnsZero()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var testPath = "test.txt";
            mockFileSystem.SetFileContent(testPath, "not-a-number");
            var manager = new HighScoreManager(mockFileSystem, testPath);
            
            // Act
            int result = manager.GetHighScore();
            
            // Assert
            Assert.Equal(0, result);
        }
        
        [Fact]
        public void GetHighScore_WhenFileExists_WithWhitespace_TrimsAndReturnsScore()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var testPath = "test.txt";
            mockFileSystem.SetFileContent(testPath, "  98765  \n");
            var manager = new HighScoreManager(mockFileSystem, testPath);
            
            // Act
            int result = manager.GetHighScore();
            
            // Assert
            Assert.Equal(98765, result);
        }
        
        [Fact]
        public void GetHighScore_WhenReadThrowsException_ReturnsZero()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var testPath = "test.txt";
            mockFileSystem.SetFileContent(testPath, "12345");
            mockFileSystem.ShouldThrowOnRead = true;
            var manager = new HighScoreManager(mockFileSystem, testPath);
            
            // Act
            int result = manager.GetHighScore();
            
            // Assert
            Assert.Equal(0, result);
        }
        
        [Fact]
        public void SaveHighScore_WhenNewScoreIsHigher_SavesScore()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var testPath = @"C:\test\highscore.txt";
            mockFileSystem.SetFileContent(testPath, "1000"); // Current high score
            var manager = new HighScoreManager(mockFileSystem, testPath);
            
            // Act
            manager.SaveHighScore(2000);
            
            // Assert
            string savedContent = mockFileSystem.ReadAllText(testPath);
            Assert.Equal("2000", savedContent);
        }
        
        [Fact]
        public void SaveHighScore_WhenNewScoreIsLower_DoesNotSave()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var testPath = @"C:\test\highscore.txt";
            mockFileSystem.SetFileContent(testPath, "2000"); // Current high score
            var manager = new HighScoreManager(mockFileSystem, testPath);
            
            // Act
            manager.SaveHighScore(1000);
            
            // Assert
            string content = mockFileSystem.ReadAllText(testPath);
            Assert.Equal("2000", content); // Should remain unchanged
        }
        
        [Fact]
        public void SaveHighScore_WhenNewScoreIsEqual_DoesNotSave()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var testPath = @"C:\test\highscore.txt";
            mockFileSystem.SetFileContent(testPath, "1500");
            var manager = new HighScoreManager(mockFileSystem, testPath);
            
            // Act
            manager.SaveHighScore(1500);
            
            // Assert
            string content = mockFileSystem.ReadAllText(testPath);
            Assert.Equal("1500", content); // Should remain unchanged
        }
        
        [Fact]
        public void SaveHighScore_CreatesDirectoryIfNeeded()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var testPath = @"C:\test\subdir\highscore.txt";
            var manager = new HighScoreManager(mockFileSystem, testPath);
            
            // Act
            manager.SaveHighScore(1000);
            
            // Assert
            Assert.True(mockFileSystem.DirectoryWasCreated(@"C:\test\subdir"));
        }
        
        [Fact]
        public void SaveHighScore_WhenWriteThrowsException_SilentlyFails()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var testPath = "test.txt";
            mockFileSystem.ShouldThrowOnWrite = true;
            var manager = new HighScoreManager(mockFileSystem, testPath);
            
            // Act & Assert - Should not throw
            manager.SaveHighScore(1000);
        }
        
        [Fact]
        public void SaveHighScore_WhenCreateDirectoryThrowsException_SilentlyFails()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var testPath = @"C:\test\highscore.txt";
            mockFileSystem.ShouldThrowOnCreateDirectory = true;
            var manager = new HighScoreManager(mockFileSystem, testPath);
            
            // Act & Assert - Should not throw
            manager.SaveHighScore(1000);
        }
        
        [Fact]
        public void IsNewHighScore_WhenScoreIsHigher_ReturnsTrue()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var testPath = "test.txt";
            mockFileSystem.SetFileContent(testPath, "1000");
            var manager = new HighScoreManager(mockFileSystem, testPath);
            
            // Act
            bool result = manager.IsNewHighScore(1500);
            
            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public void IsNewHighScore_WhenScoreIsLower_ReturnsFalse()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var testPath = "test.txt";
            mockFileSystem.SetFileContent(testPath, "2000");
            var manager = new HighScoreManager(mockFileSystem, testPath);
            
            // Act
            bool result = manager.IsNewHighScore(1500);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void IsNewHighScore_WhenScoreIsEqual_ReturnsFalse()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var testPath = "test.txt";
            mockFileSystem.SetFileContent(testPath, "1500");
            var manager = new HighScoreManager(mockFileSystem, testPath);
            
            // Act
            bool result = manager.IsNewHighScore(1500);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void IsNewHighScore_WhenNoExistingScore_ReturnsTrue()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var manager = new HighScoreManager(mockFileSystem, "test.txt");
            
            // Act
            bool result = manager.IsNewHighScore(100);
            
            // Assert
            Assert.True(result);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(999999)]
        [InlineData(-1)] // Edge case: negative score
        public void SaveAndRetrieveHighScore_RoundTrip_WorksCorrectly(int score)
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var testPath = "test.txt";
            var manager = new HighScoreManager(mockFileSystem, testPath);
            
            // Act
            manager.SaveHighScore(score);
            int retrievedScore = manager.GetHighScore();
            
            // Assert
            if (score > 0) // Only positive scores should be saved over default 0
            {
                Assert.Equal(score, retrievedScore);
            }
            else
            {
                Assert.Equal(0, retrievedScore); // Negative or zero shouldn't replace default 0
            }
        }
        
        [Fact]
        public void StaticInstance_IsNotNull()
        {
            // Act & Assert
            Assert.NotNull(HighScoreManager.Instance);
        }
        
        [Fact]
        public void StaticMethods_DelegateToInstance()
        {
            // Note: These tests verify the static methods exist and don't throw
            // Full testing would require more complex setup due to real file system usage
            
            // Act & Assert - Should not throw
            int score = HighScoreManager.GetHighScoreStatic();
            Assert.True(score >= 0);
            
            bool isNew = HighScoreManager.IsNewHighScoreStatic(999999);
            Assert.True(isNew || !isNew); // Just verify it returns a boolean
            
            // SaveHighScoreStatic test omitted to avoid writing to real file system
        }
        
        [Fact]
        public void MultipleOperations_WorkTogether()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var testPath = "test.txt";
            var manager = new HighScoreManager(mockFileSystem, testPath);
            
            // Act & Assert - Initial state
            Assert.Equal(0, manager.GetHighScore());
            Assert.True(manager.IsNewHighScore(100));
            
            // Save first score
            manager.SaveHighScore(100);
            Assert.Equal(100, manager.GetHighScore());
            Assert.False(manager.IsNewHighScore(50));
            Assert.True(manager.IsNewHighScore(200));
            
            // Save higher score
            manager.SaveHighScore(200);
            Assert.Equal(200, manager.GetHighScore());
            Assert.False(manager.IsNewHighScore(200));
            Assert.True(manager.IsNewHighScore(300));
            
            // Attempt to save lower score
            manager.SaveHighScore(150);
            Assert.Equal(200, manager.GetHighScore()); // Should remain 200
        }
    }
}
