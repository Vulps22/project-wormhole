using System;
using System.Drawing;
using System.Windows.Forms;

namespace WormholeGame.Core
{
    public enum WindowMode
    {
        Windowed,
        FullScreen,
        FullScreenWindowed
    }

    public class Settings
    {
        public WindowMode WindowMode { get; set; } = WindowMode.Windowed;
        public Size Resolution { get; set; } = new Size(800, 600);
        
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
        
        private Settings() { }
        
        public void ApplyToForm(Form form)
        {
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
        
        public Size GetActualRenderSize(Form form)
        {
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
        
        public (float scaleX, float scaleY) GetScalingFactors(Form form)
        {
            var actualSize = GetActualRenderSize(form);
            float scaleX = (float)actualSize.Width / Resolution.Width;
            float scaleY = (float)actualSize.Height / Resolution.Height;
            return (scaleX, scaleY);
        }
    }
}
