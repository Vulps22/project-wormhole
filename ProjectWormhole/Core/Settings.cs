using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProjectWormhole.Core
{
    public enum WindowMode
    {
        Windowed,
        FullScreen,
        FullScreenWindowed
    }

    // Interface for form operations to enable testing
    public interface IFormWrapper
    {
        FormWindowState WindowState { get; set; }
        FormBorderStyle FormBorderStyle { get; set; }
        Size Size { get; set; }
        FormStartPosition StartPosition { get; set; }
        Point Location { get; set; }
        Size ClientSize { get; }
    }
    
    // Wrapper for actual Windows Form
    public class WindowsFormWrapper : IFormWrapper
    {
        private readonly Form _form;
        
        public WindowsFormWrapper(Form form)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
        }
        
        public FormWindowState WindowState 
        { 
            get => _form.WindowState; 
            set => _form.WindowState = value; 
        }
        
        public FormBorderStyle FormBorderStyle 
        { 
            get => _form.FormBorderStyle; 
            set => _form.FormBorderStyle = value; 
        }
        
        public Size Size 
        { 
            get => _form.Size; 
            set => _form.Size = value; 
        }
        
        public FormStartPosition StartPosition 
        { 
            get => _form.StartPosition; 
            set => _form.StartPosition = value; 
        }
        
        public Point Location 
        { 
            get => _form.Location; 
            set => _form.Location = value; 
        }
        
        public Size ClientSize => _form.ClientSize;
    }

    public class Settings
    {
        private WindowMode _windowMode = WindowMode.Windowed;
        private Size _resolution = new Size(800, 600);
        private float _masterVolume = 1.0f;
        private float _musicVolume = 0.4f;
        private float _sfxVolume = 0.8f;
        
        public WindowMode WindowMode 
        { 
            get => _windowMode;
            set => _windowMode = value;
        }
        
        public Size Resolution 
        { 
            get => _resolution;
            set 
            {
                if (value.Width <= 0 || value.Height <= 0)
                    throw new ArgumentException("Resolution must have positive width and height");
                _resolution = value;
            }
        }
        
        // Audio settings with validation
        public float MasterVolume 
        { 
            get => _masterVolume;
            set => _masterVolume = Math.Clamp(value, 0.0f, 1.0f);
        }
        
        public float MusicVolume 
        { 
            get => _musicVolume;
            set => _musicVolume = Math.Clamp(value, 0.0f, 1.0f);
        }
        
        public float SfxVolume 
        { 
            get => _sfxVolume;
            set => _sfxVolume = Math.Clamp(value, 0.0f, 1.0f);
        }
        
        // Available resolutions
        public static readonly Size[] AvailableResolutions = new Size[]
        {
            new Size(800, 600),
            new Size(1024, 768),
            new Size(1280, 720),
            new Size(1366, 768),
            new Size(1920, 1080),
            new Size(2560, 1440)
        };
        
        public static Settings Instance { get; private set; } = new Settings();
        
        // Allow creation of new instances for testing
        public Settings() { }
        
        // Convenience method for working with Windows Forms directly
        public void ApplyToForm(Form form)
        {
            ApplyToForm(new WindowsFormWrapper(form));
        }
        
        // Main method that works with the interface for testability
        public void ApplyToForm(IFormWrapper form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            
            switch (WindowMode)
            {
                case WindowMode.Windowed:
                    form.WindowState = FormWindowState.Normal;
                    form.FormBorderStyle = FormBorderStyle.FixedSingle;
                    form.Size = new Size(Resolution.Width + 16, Resolution.Height + 39); // Account for borders
                    form.StartPosition = FormStartPosition.CenterScreen;
                    break;
                    
                case WindowMode.FullScreen:
                    form.WindowState = FormWindowState.Maximized;
                    form.FormBorderStyle = FormBorderStyle.None;
                    break;
                    
                case WindowMode.FullScreenWindowed:
                    form.WindowState = FormWindowState.Normal;
                    form.FormBorderStyle = FormBorderStyle.None;
                    form.Size = Screen.PrimaryScreen?.Bounds.Size ?? new Size(1920, 1080);
                    form.Location = new Point(0, 0);
                    break;
            }
        }
        
        public string GetWindowModeText()
        {
            return WindowMode switch
            {
                WindowMode.Windowed => "Windowed",
                WindowMode.FullScreen => "Full Screen",
                WindowMode.FullScreenWindowed => "Full Screen (Windowed)",
                _ => "Unknown"
            };
        }
        
        public string GetResolutionText()
        {
            return $"{Resolution.Width}x{Resolution.Height}";
        }
        
        // Overload for Windows Forms convenience
        public Size GetActualRenderSize(Form form)
        {
            return GetActualRenderSize(new WindowsFormWrapper(form));
        }
        
        // Main method that works with interface for testability
        public Size GetActualRenderSize(IFormWrapper form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            
            switch (WindowMode)
            {
                case WindowMode.Windowed:
                    return Resolution;
                    
                case WindowMode.FullScreen:
                case WindowMode.FullScreenWindowed:
                    return form.ClientSize;
                    
                default:
                    return Resolution;
            }
        }
        
        // Overload for Windows Forms convenience
        public (float scaleX, float scaleY) GetScalingFactors(Form form)
        {
            return GetScalingFactors(new WindowsFormWrapper(form));
        }
        
        // Main method that works with interface for testability
        public (float scaleX, float scaleY) GetScalingFactors(IFormWrapper form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            
            var actualSize = GetActualRenderSize(form);
            float scaleX = (float)actualSize.Width / Resolution.Width;
            float scaleY = (float)actualSize.Height / Resolution.Height;
            return (scaleX, scaleY);
        }
        
        // Validation methods
        public bool IsValidResolution(Size resolution)
        {
            return resolution.Width > 0 && resolution.Height > 0;
        }
        
        public bool IsValidVolume(float volume)
        {
            return volume >= 0.0f && volume <= 1.0f;
        }
    }
}
