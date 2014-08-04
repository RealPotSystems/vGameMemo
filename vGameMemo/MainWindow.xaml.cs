using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Interop;

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

        private KeyboardHandlerMulti kbh = null;
        private bool _captured = false;
        private bool _ts = false;
        private MemoWindow _mw = new MemoWindow();
        public MainWindow()
        {
            InitializeComponent();
            this.Left = 0;
            this.Top = 56;
            // ホットキーの設定
            kbh = new KeyboardHandlerMulti(this);
            
            kbh.Regist(ModifierKeys.None, Key.F9, new EventHandler(HotKeyPush_Caputure));
            kbh.Regist(ModifierKeys.Shift | ModifierKeys.Control | ModifierKeys.Alt, Key.C, new EventHandler(HotKeyPush_Caputure));
            kbh.Regist(ModifierKeys.Shift | ModifierKeys.Control | ModifierKeys.Alt, Key.Q, new EventHandler(HotKeyPush_Quit));
        }

        private void HotKeyPush_Caputure(object sender, EventArgs e)
        {
            if (!_ts)
            {
                _ts = true;
                ScreenCapture();
            }
        }

        private void HotKeyPush_Quit(object sender, EventArgs e)
        {
            this.Close();
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
                _mw.Memo.Width = bitmapSource.Width / deviceWidth;
                _mw.Memo.Height = bitmapSource.Height / deviceHeight;
                _mw.Width = bitmapSource.Width / deviceWidth + 6;
                _mw.Height = bitmapSource.Height / deviceHeight + 6;
                _mw.Left = this.Width;
                _mw.Top = this.Top;
                _mw.Memo.Background = imageBrush;
                _mw.Show();
                this._captured = true;
            }
            else
            {
                if ( this._captured )
                {
                    _mw.Show();
                }
            }
            this.Show();
            ts.Close();
            ts.Dispose();

            _ts = false;
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mw.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if ( kbh != null )
            {
                kbh.Unregist();
            }
        }
    }
}
