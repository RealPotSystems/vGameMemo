using System.Windows;
using System.Windows.Media;
using System.Windows.Ink;

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
    }
}
