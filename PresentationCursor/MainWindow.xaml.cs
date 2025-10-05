using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Media;

namespace PresentationCursor
{
    public partial class MainWindow : Window
    {
        readonly List<CanvasWindow> canvasWindows = new();
        
        public MainWindow()
        {
            InitializeComponent();
            Closing += OnClosing;
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            foreach (var canvasWindow in canvasWindows)
            {
                canvasWindow.Close();
            }
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            // Close any existing canvas windows
            foreach (var existingWindow in canvasWindows)
            {
                existingWindow.Close();
            }
            canvasWindows.Clear();
            
            int screenIndex = 0;
            // Create a canvas window for each screen
            foreach (Screen screen in Screen.AllScreens)
            {
                Debug.WriteLine($"Screen {screenIndex}: Bounds = {screen.Bounds}, Primary = {screen.Primary}");
                CanvasWindow window = new CanvasWindow();
                
                // Convert screen coordinates to WPF coordinates
                // WPF uses device-independent units, so we need to account for DPI
                var dpiScale = VisualTreeHelper.GetDpi(this);
                double scaleX = dpiScale.DpiScaleX;
                double scaleY = dpiScale.DpiScaleY;
                
                // Position the window to cover the entire screen
                window.Left = screen.Bounds.Left / scaleX;
                window.Top = screen.Bounds.Top / scaleY;
                window.Width = screen.Bounds.Width / scaleX;
                window.Height = screen.Bounds.Height / scaleY;
                
                Debug.WriteLine($"Setting window {screenIndex} position: Left={window.Left}, Top={window.Top}, Width={window.Width}, Height={window.Height}");
                
                // Make the window fullscreen and topmost
                window.Topmost = true;
                window.WindowStyle = WindowStyle.None;
                window.ResizeMode = ResizeMode.NoResize;
                
                // Force the window to show on the correct screen
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                
                window.Show();
                canvasWindows.Add(window);
                
                screenIndex++;
            }
        }
    }
}
