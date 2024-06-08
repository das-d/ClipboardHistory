using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ClipboardHistory
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _pollTimer = new DispatcherTimer();
        private string _lastClipboardText = "";
        private HashSet<ClipTextElement> _clipElements = new HashSet<ClipTextElement>();

        public MainWindow()
        {
            InitializeComponent();
            _pollTimer.Interval = TimeSpan.FromMilliseconds(500);
            _pollTimer.Tick += new EventHandler(PollTimerTicked);
            _pollTimer.Start();
            _lastClipboardText = Clipboard.GetText();
        }

        private void PollTimerTicked(object sender, EventArgs e)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    var clipText = Clipboard.GetText();
                    if (!_lastClipboardText.Equals(clipText))
                    {
                        _lastClipboardText = clipText;

                        var cte = new ClipTextElement();
                        cte.ClipText.Text = clipText;
                        cte.ToolTip = clipText;

                        var oldCte = _clipElements.Where(x => x.ClipText.Text.Equals(clipText)).FirstOrDefault();

                        if (oldCte != null)
                        {
                            return;
                        }

                        cte.MouseLeftButtonDown += HandleLMB;
                        cte.MouseRightButtonDown += HandleRMB;

                        _clipElements.Add(cte);
                        ClipHistory.Children.Add(cte);
                        SrcVwr.ScrollToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Clipboard GetText Error");
            }

        }

        private void HandleLMB(object sender, MouseButtonEventArgs e)
        {
            if (sender is ClipTextElement cte)
            {
                try
                {
                    Clipboard.SetText(cte.ClipText.Text);
                    SendPasteCommand();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Clipboard SetText Error");
                }
            }
        }

        private void HandleRMB(object sender, MouseButtonEventArgs e)
        {
            if (sender is ClipTextElement cte)
            {
                cte.MouseLeftButtonDown -= HandleLMB;
                cte.MouseRightButtonDown -= HandleRMB;
                _clipElements.Remove(cte);
                ClipHistory.Children.Remove(cte);
            }
        }

        private void ToolMinimize(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ToolClose(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void ToolClearList(object sender, MouseButtonEventArgs e)
        {
            _clipElements.Clear();
            ClipHistory.Children.Clear();
        }

        private void WindowMDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void SendPasteCommand()
        {
            //Direct Input KeyCodes
            InputSender.SendKeyboardInput(new InputSender.KeyboardInput[]
                {
                new InputSender.KeyboardInput
                {
                    wScan = 0x1D, //DIK_LCONTROL
                    dwFlags = (uint)(InputSender.KeyEventF.KeyDown | InputSender.KeyEventF.Scancode),
                },
                new InputSender.KeyboardInput
                {
                    wScan = 0x2F, //DIK_V
                    dwFlags = (uint)(InputSender.KeyEventF.KeyDown | InputSender.KeyEventF.Scancode)
                },
                new InputSender.KeyboardInput
                {
                    wScan = 0x1D, //DIK_LCONTROL
                    dwFlags = (uint)(InputSender.KeyEventF.KeyUp | InputSender.KeyEventF.Scancode)
                },
                new InputSender.KeyboardInput
                {
                    wScan = 0x2F, //DIK_V
                    dwFlags = (uint)(InputSender.KeyEventF.KeyUp | InputSender.KeyEventF.Scancode)
                }

                });
        }


        //Below Sets Window to Unfocused
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            //Set the window style to noactivate.
            var helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE,
                GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    }
}