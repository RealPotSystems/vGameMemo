﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Ink;
using System.Windows.Interop;

namespace vGameMemo
{
    /// <summary>
    /// MemoWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MemoWindow : Window
    {
        private SolidColorBrush _brush = null;
        private DrawingAttributes _pen = null;
        int _pensize = 9;
        const int WM_SYSKEYDOWN = 0x0104;
        const int VK_F4 = 0x73;
        public MemoWindow()
        {
            InitializeComponent();

            this._brush = new SolidColorBrush();
            this._pen = new DrawingAttributes();
            this._pen.Width = _pensize;
            this._pen.Height = _pensize;
            this._pen.Color = Colors.Red;
            this._pen.FitToCurve = true;
            this._pen.IsHighlighter = true;
            this._pen.IgnorePressure = true;
            this.Memo.DefaultDrawingAttributes = this._pen;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
    }
}
