using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace vGameMemo
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private KeyboardHandlerMulti kbh;
        private bool _closed = true;
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
                //_mw.Memo.Width = bitmapSource.PixelWidth;
                //_mw.Memo.Height = bitmapSource.PixelHeight;
                //_mw.Width = bitmapSource.Width + 6;
                //_mw.Height = bitmapSource.Height + 6;
                _mw.Memo.Width = bitmapSource.Width;
                _mw.Memo.Height = bitmapSource.Height;
                _mw.Width = bitmapSource.Width + 6;
                _mw.Height = bitmapSource.Height + 6;
                _mw.Left = 0;
                _mw.Top = this.Top;
                _mw.Show();
                this.Left = _mw.Width;
            }
            else
            {
                _mw.Show();
            }
            this.Show();
            ts.Dispose();
            this._captured = true;
            this._closed = false;
        }

        private void LayoutRoot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_closed && _captured)
            {
                _mw.Show();
                this.Left = _mw.Width;
                _closed = false;
            }
            else
            {
                _mw.Hide();
                this.Left = 0;
                _closed = true;
            }
        }
    }
}
