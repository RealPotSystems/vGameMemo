using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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
            CaptureScreen(point);
            this.DialogResult = true;
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

        private void CaptureScreen(Point point)
        {
            var start = PointToScreen(_position);
            var end = PointToScreen(point);

            var x = start.X < end.X ? (int)start.X : (int)end.X;
            var y = start.Y < end.Y ? (int)start.Y : (int)end.Y;
            var width = (int)Math.Abs(end.X - start.X);
            var height = (int)Math.Abs(end.Y - start.Y);
            if (width == 0 || height == 0)
            {
                return;
            }

            this.Image = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var graph = System.Drawing.Graphics.FromImage(this.Image);
            graph.CopyFromScreen(new System.Drawing.Point(x, y), new System.Drawing.Point(), this.Image.Size);
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
    }
}
