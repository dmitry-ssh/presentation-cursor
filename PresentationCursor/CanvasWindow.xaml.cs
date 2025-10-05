using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;

namespace PresentationCursor
{
    /// <summary>
    /// Interaction logic for CanvasWindow.xaml
    /// </summary>
    public partial class CanvasWindow : Window
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        [DllImport("Shcore.dll")]
        private static extern int GetDpiForMonitor(
            IntPtr hmonitor,
            int dpiType,
            out uint dpiX,
            out uint dpiY);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT { public int X; public int Y; }

        private readonly DispatcherTimer _timer;
        private readonly Queue<Ellipse> _trail = new();

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        
        public CanvasWindow()
        {
            InitializeComponent();

            Loaded += (_, __) =>
            {
                _timer.Start();
            };

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60fps
            };
            _timer.Tick += UpdateCursor;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            exStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT; // 👈 makes window fully click-through
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
        }
        
        private void UpdateCursor(object? sender, EventArgs e)
        {
            GetCursorPos(out POINT p);

            // Get DPI of the monitor where the cursor is
            var monitor = MonitorFromPoint(p, 2); // MONITOR_DEFAULTTONEAREST
            GetDpiForMonitor(monitor, 0, out uint dpiX, out uint dpiY); // MDT_EFFECTIVE_DPI

            double scaleX = dpiX / 96.0;
            double scaleY = dpiY / 96.0;

            // Convert screen coords (px) to WPF device-independent units
            double globalX = p.X / scaleX;
            double globalY = p.Y / scaleY;

            // Convert to window-relative coordinates
            var dpiScale = VisualTreeHelper.GetDpi(this);
            double windowLeft = this.Left * dpiScale.DpiScaleX;
            double windowTop = this.Top * dpiScale.DpiScaleY;
            
            double x = globalX - (windowLeft / scaleX);
            double y = globalY - (windowTop / scaleY);

            // Only draw if cursor is within this window's bounds
            if (x >= 0 && x <= this.Width && y >= 0 && y <= this.Height)
            {
                // Make elements visible
                CursorCircle.Visibility = Visibility.Visible;
                
                // Move the highlight circle
                Canvas.SetLeft(CursorCircle, x - CursorCircle.Width / 2);
                Canvas.SetTop(CursorCircle, y - CursorCircle.Height / 2);

                // Add a fading trail dot
                var dot = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Yellow,
                    Opacity = 0.6,
                    IsHitTestVisible = false
                };

                Canvas.SetLeft(dot, x - dot.Width / 2);
                Canvas.SetTop(dot, y - dot.Height / 2);
                OverlayCanvas.Children.Add(dot);
                _trail.Enqueue(dot);
            }
            else
            {
                // Hide cursor circle when not on this screen
                CursorCircle.Visibility = Visibility.Hidden;
            }

            // Animate trail fade/shrink (always process existing trail)
            foreach (var ellipse in _trail)
            {
                ellipse.Opacity -= 0.02;
                ellipse.Width *= 0.97;
                ellipse.Height *= 0.97;

                // Keep centered when shrinking
                double left = Canvas.GetLeft(ellipse) + (1 - 0.97) * ellipse.Width / 2;
                double top = Canvas.GetTop(ellipse) + (1 - 0.97) * ellipse.Height / 2;
                Canvas.SetLeft(ellipse, left);
                Canvas.SetTop(ellipse, top);
            }

            // Remove old dots
            while (_trail.Count > 0 && _trail.Peek().Opacity <= 0)
            {
                OverlayCanvas.Children.Remove(_trail.Dequeue());
            }
        }
    }
}
