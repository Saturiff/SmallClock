using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace Word1
{
    public partial class MainWindow : Window
    {
        #region 計時主邏輯

        private Timer MainTimer;
        public MainWindow()
        {
            InitializeComponent();
            System.Windows.Media.CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);

            MainTimer = new Timer { Interval = 10 };
            MainTimer.Tick += new EventHandler(TimerTick);
            MainTimer.Start();
        }

        private bool show_button = false;
        void TimerTick(object sender, EventArgs e)
        {
            TimeLable.Content =
                ((DateTime.Now.Hour) < 10 ? '0' + Convert.ToString(DateTime.Now.Hour) : Convert.ToString(DateTime.Now.Hour))
                + ':' + (DateTime.Now.Minute < 10 ? '0' + Convert.ToString(DateTime.Now.Minute) : Convert.ToString(DateTime.Now.Minute))
                + ':' + (DateTime.Now.Second < 10 ? '0' + Convert.ToString(DateTime.Now.Second) : Convert.ToString(DateTime.Now.Second));
            if (show_button)
            {
                Min_button.Opacity = 1;
                Close_button.Opacity = 1;
            }
            else
            {
                Min_button.Opacity = 0;
                Close_button.Opacity = 0;
            }
        }

        #endregion

        #region 綁定鍵盤觸發可拖曳視窗事件
        // 來源 reina42689/ArkServerQuery/.../ARKWatchDog/MainWindow.xaml.cs
        // Hook全域鍵盤，在該視窗重新渲染時執行
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            bool isKeyDown = ((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.Down) > 0) || ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.Down) > 0);
            bool isManipulatable = (((Keyboard.GetKeyStates(Key.OemTilde) & KeyStates.None) == 0) || ((Keyboard.GetKeyStates(Key.OemQuotes) & KeyStates.None) == 0)) && canManipulateWindow;
            if (isKeyDown)
            {
                ToggleManipulateWindow(KeyStates.Down);
            }
            else if (isManipulatable)
            {
                ToggleManipulateWindow(KeyStates.None);
            }
        }

        // 初始化時將目前視窗參數儲存
        private IntPtr hwnd;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetOriStyle(hwnd);
        }

        private bool canManipulateWindow = false;

        private KeyStates gKeyStates = KeyStates.None;

        private void ToggleManipulateWindow(KeyStates inKeyStates)
        {
            /* None -> Down, Down -> None : 改變可操縱視窗狀態並保存目前狀態，視窗可移動時將停止伺服器訪問以增進使用者體驗
             * None -> None, Down -> Down : 不做任何事
             */
            if (inKeyStates != gKeyStates) // 狀態改變則致能
            {
                canManipulateWindow = (canManipulateWindow) ? false : true;
                WindowsServices.SetWindowExTransparent(hwnd);
                gKeyStates = inKeyStates;
                if (canManipulateWindow) MainTimer.Stop();
                else MainTimer.Start();
            }
        }

        #endregion

        #region 視窗物件

        private void ClickMin(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void ClickExit(object sender, RoutedEventArgs e)
        {
            Close();
            Environment.Exit(Environment.ExitCode);
        }
        private void ClickDrag(object sender, MouseButtonEventArgs e) => DragMove();

        private void ShowButton(object sender, System.Windows.Input.MouseEventArgs e) => show_button = true;

        private void HideButton(object sender, System.Windows.Input.MouseEventArgs e) => show_button = false;

        #endregion
    }
}
