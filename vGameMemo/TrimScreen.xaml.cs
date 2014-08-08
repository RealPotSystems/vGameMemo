using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Interop;

namespace vGameMemo
{
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class TrimScreen : Window , IDisposable
    {
        internal System.Drawing.Bitmap Image { get; private set; }  
        private Point _position;
        private bool _trimEnable = false;
        const int WM_SYSKEYDOWN = 0x0104;
        const int VK_F4 = 0x73;

        public TrimScreen()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var screen = System.Windows.Forms.Screen.PrimaryScreen;
            this.Left = screen.Bounds.Left;
            this.Top = screen.Bounds.Top;
            this.Width = screen.Bounds.Width;
            this.Height = screen.Bounds.Height;
            this.ScreenArea.Geometry1 = new RectangleGeometry(new Rect(0, 0, screen.Bounds.Width, screen.Bounds.Height));
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if ((msg == WM_SYSKEYDOWN) &&
                (wParam.ToInt32() == VK_F4))
            {
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void DrawingPath_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var path = sender as Path;
            if (path == null)
            {
                return;
            }
            var point = e.GetPosition(path);
            _position = point;
            _trimEnable = true;
            this.Cursor = Cursors.Cross;
            path.CaptureMouse();
        }

        private void DrawingPath_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var path = sender as Path;
            if (path == null)
            {
                return;
            }
            var point = e.GetPosition(path);
            this.Cursor = Cursors.Arrow;
            path.ReleaseMouseCapture();
            this.DialogResult = CaptureScreen(point);
            if (_trimEnable)
            {
                this.Close();
            }
            _trimEnable = false;
        }

        private void DrawingPath_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_trimEnable)
            {
                return;
            }
            var path = sender as Path;
            if (path == null)
            {
                return;
            }
            var point = e.GetPosition(path);
            {
                // 矩形の描画
                var x = _position.X < point.X ? _position.X : point.X;
                var y = _position.Y < point.Y ? _position.Y : point.Y;
                var width = Math.Abs(point.X - _position.X);
                var height = Math.Abs(point.Y - _position.Y);
                this.ScreenArea.Geometry2 = new RectangleGeometry(new Rect(x, y, width, height));
            }
        }

        private bool CaptureScreen(Point point)
        {
            var start = PointToScreen(_position);
            var end = PointToScreen(point);

            var x = start.X < end.X ? (int)start.X : (int)end.X;
            var y = start.Y < end.Y ? (int)start.Y : (int)end.Y;
            var width = (int)Math.Abs(end.X - start.X);
            var height = (int)Math.Abs(end.Y - start.Y);
            if (width == 0 || height == 0)
            {
                return false;
            }

            this.Image = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var graph = System.Drawing.Graphics.FromImage(this.Image);
            graph.CopyFromScreen(new System.Drawing.Point(x, y), new System.Drawing.Point(), this.Image.Size);
            return true;
        }

        public void Dispose()
        {
            if (this.Image != null)
            {
                this.Image.Dispose();
            }
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;  // Cancel
            this.Close();
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x8000000;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE, GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }
    }
}
