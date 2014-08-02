using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace vGameMemo
{
    internal static class DpiChange
    {
        public static Point GetDpiScaleFactor(this Visual visual)
        {
            var source = PresentationSource.FromVisual(visual);
            if (source != null && source.CompositionTarget != null)
            {
                return new Point(
                    source.CompositionTarget.TransformToDevice.M11,
                    source.CompositionTarget.TransformToDevice.M22);
            }
            return new Point(1.0, 1.0);
        }
    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private KeyboardHandlerMulti kbh;
        private bool _captured = false;
        MemoWindow _mw = new MemoWindow();
        public MainWindow()
        {
            InitializeComponent();
            this.Left = 0;
            this.Top = 50;
            // ホットキーの設定
            kbh = new KeyboardHandlerMulti(this);
            kbh.Regist(ModifierKeys.Shift | ModifierKeys.Control, Key.C, new EventHandler(HotKeyPush));
        }

        private void HotKeyPush(object sender, EventArgs e)
        {
            ScreenCapture();
        }

        private void ScreenCapture()
        {
            TrimScreen ts = new TrimScreen();
            ts.Owner = this;
            this.Hide();
            _mw.Hide();
            bool? result = ts.ShowDialog();
            if (result == true)
            {
                _mw.Memo.Strokes.Clear();

                var dpiScaleFactor = DpiChange.GetDpiScaleFactor(this);

                var deviceWidth = dpiScaleFactor.X;
                var deviceHeight = dpiScaleFactor.Y;

                System.Drawing.Bitmap bmp = ts.Image;
                IntPtr hBitmap = bmp.GetHbitmap();
                var bitmapSource =
                    System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                var imageBrush = new ImageBrush(bitmapSource);
                DeleteObject(hBitmap);
                bmp.Dispose();

                _mw.Owner = this;
                _mw.Memo.Background = imageBrush;
                _mw.Memo.Width = bitmapSource.Width / deviceWidth;
                _mw.Memo.Height = bitmapSource.Height / deviceHeight;
                _mw.Width = bitmapSource.Width / deviceWidth + 6;
                _mw.Height = bitmapSource.Height / deviceHeight + 6;
                _mw.Left = this.Width;
                _mw.Top = this.Top;
                _mw.Show();
            }
            else
            {
                _mw.Show();
            }
            this.Show();
            ts.Dispose();
            this._captured = true;
        }

        private void LayoutRoot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ( !_mw.IsVisible && _captured)
            {
                _mw.Show();
            }
            else
            {
                _mw.Hide();
            }
        }
    }
}
