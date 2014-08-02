using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices; // DllImportをするために必要
using System.Threading; // イベントの別スレッド処理に必要
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop; // Window Handleの取得等に必要
 

namespace vGameMemo
{
    class KeyboardHandlerMulti
    {
        // HotKey Message ID
        private const int WM_HOTKEY = 0x0312;
        // ウィンドウハンドラ
        private IntPtr _windowHandle;

        // ホットキーIDとイベントで対になるディクショナリ
        private Dictionary<int, EventHandler> _hotkeyEvents;

        // 戻り値：成功 = 0以外、失敗 = 0（既に他が登録済み)
        [DllImport("user32.dll")]
        private static extern int RegisterHotKey(IntPtr hWnd, int id, int MOD_KEY, int VK);

        // 戻り値：成功 = 0以外、失敗 = 0
        [DllImport("user32.dll")]
        private static extern int UnregisterHotKey(IntPtr hWnd, int id);

        // 初期化
        public KeyboardHandlerMulti(Window window)
        {
            // WindowのHandleを取得
            WindowInteropHelper _host = new WindowInteropHelper(window);
            this._windowHandle = _host.Handle;

            // ホットキーのイベントハンドラを設定
            ComponentDispatcher.ThreadPreprocessMessage
                += ComponentDispatcher_ThreadPreprocessMessage;

            // イベントディクショナリを初期化
            _hotkeyEvents = new Dictionary<int, System.EventHandler>();
        }

        // HotKeyの動作を設定する
        public void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            // ホットキーを表すメッセージであるか否か
            if (msg.message != WM_HOTKEY) return;

            // 自分が登録したホットキーか否か
            var hotkeyID = msg.wParam.ToInt32();
            if (!this._hotkeyEvents.Any((x) => x.Key == hotkeyID)) return;

            // 両方を満たす場合は登録してあるホットキーのイベントを実行
            new ThreadStart(
                () => _hotkeyEvents[hotkeyID](this, EventArgs.Empty)
            ).Invoke();
        }

        // HotKeyの登録
        private int i = 0x0000;
        public void Regist(ModifierKeys modkey, Key trigger, EventHandler eh)
        {
            // 引数をintにキャスト
            var imod = modkey.ToInt32();
            var itrg = KeyInterop.VirtualKeyFromKey(trigger);

            // HotKey登録時に指定するIDを決定する
            while ((++i < 0xc000) && RegisterHotKey(this._windowHandle, i, imod, itrg) == 0) ;
            // 0xc000～0xffff はDLL用なので使用不可能
            // 0x0000～0xbfff はIDとして使用可能

            if (i < 0xc000)
            {
                this._hotkeyEvents.Add(i, eh);
            }
        }


        // HotKeyの全開放
        public void Unregist()
        {
            foreach (var hotkeyid in this._hotkeyEvents.Keys)
            {
                UnregisterHotKey(this._windowHandle, hotkeyid);
            }
        }
    }

    static class Extention
    {
        public static Int32 ToInt32(this ModifierKeys m)
        {
            return (Int32)m;
        }
    }
}
