using System;
using System.Drawing;
using System.Windows.Forms;
using Xunit;
using ProjectWormhole.Core;

namespace ProjectWormhole.Tests.Core
{
    // Mock form wrapper for testing
    public class MockFormWrapper : IFormWrapper
    {
        public FormWindowState WindowState { get; set; }
        public FormBorderStyle FormBorderStyle { get; set; }
        public Size Size { get; set; }
        public FormStartPosition StartPosition { get; set; }
        public Point Location { get; set; }
        public Size ClientSize { get; set; } = new Size(1920, 1080);
    }

    public class SettingsTests
    {
        [Fact]
        public void Settings_Constructor_SetsDefaultValues()
        {
            // Act
            var settings = new Settings();
            
            // Assert
            Assert.Equal(WindowMode.Windowed, settings.WindowMode);
            Assert.Equal(new Size(800, 600), settings.Resolution);
            Assert.Equal(1.0f, settings.MasterVolume);
            Assert.Equal(0.4f, settings.MusicVolume);
            Assert.Equal(0.8f, settings.SfxVolume);
        }
        
        [Fact]
        public void WindowMode_CanBeSetAndRetrieved()
        {
            // Arrange
            var settings = new Settings();
            
            // Act & Assert
            settings.WindowMode = WindowMode.FullScreen;
            Assert.Equal(WindowMode.FullScreen, settings.WindowMode);
            
            settings.WindowMode = WindowMode.FullScreenWindowed;
            Assert.Equal(WindowMode.FullScreenWindowed, settings.WindowMode);
        }
        
        [Fact]
        public void Resolution_CanBeSetAndRetrieved()
        {
            // Arrange
            var settings = new Settings();
            var newResolution = new Size(1920, 1080);
            
            // Act
            settings.Resolution = newResolution;
            
            // Assert
            Assert.Equal(newResolution, settings.Resolution);
        }
        
        [Fact]
        public void Resolution_WithInvalidValues_ThrowsException()
        {
            // Arrange
            var settings = new Settings();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => settings.Resolution = new Size(0, 600));
            Assert.Throws<ArgumentException>(() => settings.Resolution = new Size(800, 0));
            Assert.Throws<ArgumentException>(() => settings.Resolution = new Size(-800, 600));
            Assert.Throws<ArgumentException>(() => settings.Resolution = new Size(800, -600));
        }
        
        [Theory]
        [InlineData(0.0f)]
        [InlineData(0.5f)]
        [InlineData(1.0f)]
        public void MasterVolume_WithValidValues_SetsCorrectly(float volume)
        {
            // Arrange
            var settings = new Settings();
            
            // Act
            settings.MasterVolume = volume;
            
            // Assert
            Assert.Equal(volume, settings.MasterVolume);
        }
        
        [Theory]
        [InlineData(-0.1f, 0.0f)]
        [InlineData(1.1f, 1.0f)]
        [InlineData(2.0f, 1.0f)]
        [InlineData(-5.0f, 0.0f)]
        public void MasterVolume_WithInvalidValues_ClampsToValidRange(float input, float expected)
        {
            // Arrange
            var settings = new Settings();
            
            // Act
            settings.MasterVolume = input;
            
            // Assert
            Assert.Equal(expected, settings.MasterVolume);
        }
        
        [Theory]
        [InlineData(0.0f)]
        [InlineData(0.4f)]
        [InlineData(1.0f)]
        public void MusicVolume_WithValidValues_SetsCorrectly(float volume)
        {
            // Arrange
            var settings = new Settings();
            
            // Act
            settings.MusicVolume = volume;
            
            // Assert
            Assert.Equal(volume, settings.MusicVolume);
        }
        
        [Theory]
        [InlineData(-0.1f, 0.0f)]
        [InlineData(1.1f, 1.0f)]
        public void MusicVolume_WithInvalidValues_ClampsToValidRange(float input, float expected)
        {
            // Arrange
            var settings = new Settings();
            
            // Act
            settings.MusicVolume = input;
            
            // Assert
            Assert.Equal(expected, settings.MusicVolume);
        }
        
        [Theory]
        [InlineData(0.0f)]
        [InlineData(0.8f)]
        [InlineData(1.0f)]
        public void SfxVolume_WithValidValues_SetsCorrectly(float volume)
        {
            // Arrange
            var settings = new Settings();
            
            // Act
            settings.SfxVolume = volume;
            
            // Assert
            Assert.Equal(volume, settings.SfxVolume);
        }
        
        [Theory]
        [InlineData(-0.1f, 0.0f)]
        [InlineData(1.1f, 1.0f)]
        public void SfxVolume_WithInvalidValues_ClampsToValidRange(float input, float expected)
        {
            // Arrange
            var settings = new Settings();
            
            // Act
            settings.SfxVolume = input;
            
            // Assert
            Assert.Equal(expected, settings.SfxVolume);
        }
        
        [Fact]
        public void ApplyToForm_WithWindowed_ConfiguresFormCorrectly()
        {
            // Arrange
            var settings = new Settings();
            var mockForm = new MockFormWrapper();
            settings.WindowMode = WindowMode.Windowed;
            settings.Resolution = new Size(1024, 768);
            
            // Act
            settings.ApplyToForm(mockForm);
            
            // Assert
            Assert.Equal(FormWindowState.Normal, mockForm.WindowState);
            Assert.Equal(FormBorderStyle.FixedSingle, mockForm.FormBorderStyle);
            Assert.Equal(new Size(1040, 807), mockForm.Size); // 1024+16, 768+39
            Assert.Equal(FormStartPosition.CenterScreen, mockForm.StartPosition);
        }
        
        [Fact]
        public void ApplyToForm_WithFullScreen_ConfiguresFormCorrectly()
        {
            // Arrange
            var settings = new Settings();
            var mockForm = new MockFormWrapper();
            settings.WindowMode = WindowMode.FullScreen;
            
            // Act
            settings.ApplyToForm(mockForm);
            
            // Assert
            Assert.Equal(FormWindowState.Maximized, mockForm.WindowState);
            Assert.Equal(FormBorderStyle.None, mockForm.FormBorderStyle);
        }
        
        [Fact]
        public void ApplyToForm_WithFullScreenWindowed_ConfiguresFormCorrectly()
        {
            // Arrange
            var settings = new Settings();
            var mockForm = new MockFormWrapper();
            settings.WindowMode = WindowMode.FullScreenWindowed;
            
            // Act
            settings.ApplyToForm(mockForm);
            
            // Assert
            Assert.Equal(FormWindowState.Normal, mockForm.WindowState);
            Assert.Equal(FormBorderStyle.None, mockForm.FormBorderStyle);
            // Size should be either the primary screen size or the fallback
            Assert.True(mockForm.Size.Width > 0 && mockForm.Size.Height > 0);
            Assert.Equal(new Point(0, 0), mockForm.Location);
        }
        
        [Fact]
        public void ApplyToForm_WithNullForm_ThrowsArgumentNullException()
        {
            // Arrange
            var settings = new Settings();
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => settings.ApplyToForm((IFormWrapper)null!));
        }
        
        [Theory]
        [InlineData(WindowMode.Windowed, "Windowed")]
        [InlineData(WindowMode.FullScreen, "Full Screen")]
        [InlineData(WindowMode.FullScreenWindowed, "Full Screen (Windowed)")]
        public void GetWindowModeText_ReturnsCorrectText(WindowMode mode, string expected)
        {
            // Arrange
            var settings = new Settings();
            settings.WindowMode = mode;
            
            // Act
            string result = settings.GetWindowModeText();
            
            // Assert
            Assert.Equal(expected, result);
        }
        
        [Theory]
        [InlineData(800, 600, "800x600")]
        [InlineData(1920, 1080, "1920x1080")]
        [InlineData(2560, 1440, "2560x1440")]
        public void GetResolutionText_ReturnsCorrectFormat(int width, int height, string expected)
        {
            // Arrange
            var settings = new Settings();
            settings.Resolution = new Size(width, height);
            
            // Act
            string result = settings.GetResolutionText();
            
            // Assert
            Assert.Equal(expected, result);
        }
        
        [Fact]
        public void GetActualRenderSize_WithWindowed_ReturnsResolution()
        {
            // Arrange
            var settings = new Settings();
            var mockForm = new MockFormWrapper();
            settings.WindowMode = WindowMode.Windowed;
            settings.Resolution = new Size(1024, 768);
            
            // Act
            Size result = settings.GetActualRenderSize(mockForm);
            
            // Assert
            Assert.Equal(new Size(1024, 768), result);
        }
        
        [Fact]
        public void GetActualRenderSize_WithFullScreen_ReturnsClientSize()
        {
            // Arrange
            var settings = new Settings();
            var mockForm = new MockFormWrapper();
            mockForm.ClientSize = new Size(1600, 900);
            settings.WindowMode = WindowMode.FullScreen;
            
            // Act
            Size result = settings.GetActualRenderSize(mockForm);
            
            // Assert
            Assert.Equal(new Size(1600, 900), result);
        }
        
        [Fact]
        public void GetActualRenderSize_WithFullScreenWindowed_ReturnsClientSize()
        {
            // Arrange
            var settings = new Settings();
            var mockForm = new MockFormWrapper();
            mockForm.ClientSize = new Size(2560, 1440);
            settings.WindowMode = WindowMode.FullScreenWindowed;
            
            // Act
            Size result = settings.GetActualRenderSize(mockForm);
            
            // Assert
            Assert.Equal(new Size(2560, 1440), result);
        }
        
        [Fact]
        public void GetActualRenderSize_WithNullForm_ThrowsArgumentNullException()
        {
            // Arrange
            var settings = new Settings();
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => settings.GetActualRenderSize((IFormWrapper)null!));
        }
        
        [Fact]
        public void GetScalingFactors_CalculatesCorrectly()
        {
            // Arrange
            var settings = new Settings();
            var mockForm = new MockFormWrapper();
            settings.Resolution = new Size(800, 600);
            mockForm.ClientSize = new Size(1600, 1200);
            settings.WindowMode = WindowMode.FullScreen;
            
            // Act
            var (scaleX, scaleY) = settings.GetScalingFactors(mockForm);
            
            // Assert
            Assert.Equal(2.0f, scaleX);
            Assert.Equal(2.0f, scaleY);
        }
        
        [Fact]
        public void GetScalingFactors_WithWindowed_UsesResolution()
        {
            // Arrange
            var settings = new Settings();
            var mockForm = new MockFormWrapper();
            settings.Resolution = new Size(1024, 768);
            settings.WindowMode = WindowMode.Windowed;
            
            // Act
            var (scaleX, scaleY) = settings.GetScalingFactors(mockForm);
            
            // Assert
            Assert.Equal(1.0f, scaleX);
            Assert.Equal(1.0f, scaleY);
        }
        
        [Fact]
        public void GetScalingFactors_WithNullForm_ThrowsArgumentNullException()
        {
            // Arrange
            var settings = new Settings();
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => settings.GetScalingFactors((IFormWrapper)null!));
        }
        
        [Theory]
        [InlineData(800, 600, true)]
        [InlineData(1920, 1080, true)]
        [InlineData(0, 600, false)]
        [InlineData(800, 0, false)]
        [InlineData(-800, 600, false)]
        [InlineData(800, -600, false)]
        public void IsValidResolution_ReturnsCorrectResult(int width, int height, bool expected)
        {
            // Arrange
            var settings = new Settings();
            
            // Act
            bool result = settings.IsValidResolution(new Size(width, height));
            
            // Assert
            Assert.Equal(expected, result);
        }
        
        [Theory]
        [InlineData(0.0f, true)]
        [InlineData(0.5f, true)]
        [InlineData(1.0f, true)]
        [InlineData(-0.1f, false)]
        [InlineData(1.1f, false)]
        public void IsValidVolume_ReturnsCorrectResult(float volume, bool expected)
        {
            // Arrange
            var settings = new Settings();
            
            // Act
            bool result = settings.IsValidVolume(volume);
            
            // Assert
            Assert.Equal(expected, result);
        }
        
        [Fact]
        public void AvailableResolutions_ContainsExpectedValues()
        {
            // Assert
            Assert.Contains(new Size(800, 600), Settings.AvailableResolutions);
            Assert.Contains(new Size(1024, 768), Settings.AvailableResolutions);
            Assert.Contains(new Size(1280, 720), Settings.AvailableResolutions);
            Assert.Contains(new Size(1366, 768), Settings.AvailableResolutions);
            Assert.Contains(new Size(1920, 1080), Settings.AvailableResolutions);
            Assert.Contains(new Size(2560, 1440), Settings.AvailableResolutions);
            Assert.Equal(6, Settings.AvailableResolutions.Length);
        }
        
        [Fact]
        public void StaticInstance_IsNotNull()
        {
            // Act & Assert
            Assert.NotNull(Settings.Instance);
        }
        
        [Fact]
        public void StaticInstance_IsSingleton()
        {
            // Act
            var instance1 = Settings.Instance;
            var instance2 = Settings.Instance;
            
            // Assert
            Assert.Same(instance1, instance2);
        }
        
        [Fact]
        public void Settings_MultipleInstances_WorkIndependently()
        {
            // Arrange
            var settings1 = new Settings();
            var settings2 = new Settings();
            
            // Act
            settings1.WindowMode = WindowMode.FullScreen;
            settings1.Resolution = new Size(1920, 1080);
            settings2.WindowMode = WindowMode.Windowed;
            settings2.Resolution = new Size(800, 600);
            
            // Assert
            Assert.Equal(WindowMode.FullScreen, settings1.WindowMode);
            Assert.Equal(WindowMode.Windowed, settings2.WindowMode);
            Assert.Equal(new Size(1920, 1080), settings1.Resolution);
            Assert.Equal(new Size(800, 600), settings2.Resolution);
        }
        
        [Fact]
        public void Settings_CompleteConfiguration_WorksTogether()
        {
            // Arrange
            var settings = new Settings();
            var mockForm = new MockFormWrapper();
            
            // Act - Configure all settings
            settings.WindowMode = WindowMode.FullScreen;
            settings.Resolution = new Size(1280, 720);
            settings.MasterVolume = 0.8f;
            settings.MusicVolume = 0.6f;
            settings.SfxVolume = 0.9f;
            
            settings.ApplyToForm(mockForm);
            
            // Assert - Verify all settings work together
            Assert.Equal(WindowMode.FullScreen, settings.WindowMode);
            Assert.Equal("Full Screen", settings.GetWindowModeText());
            Assert.Equal("1280x720", settings.GetResolutionText());
            Assert.Equal(0.8f, settings.MasterVolume);
            Assert.Equal(0.6f, settings.MusicVolume);
            Assert.Equal(0.9f, settings.SfxVolume);
            Assert.Equal(FormWindowState.Maximized, mockForm.WindowState);
            Assert.Equal(FormBorderStyle.None, mockForm.FormBorderStyle);
        }
    }
}
