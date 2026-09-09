using System.Windows;
using System.Windows.Controls;
using Linker.Ui.Components;

namespace Linker.Ui.Components
{
    public partial class ToastHostControl : UserControl
    {
        private System.Windows.Threading.DispatcherTimer _hideTimer;
        private System.Windows.Threading.DispatcherTimer _hideCompleteTimer;

        public ToastHostControl()
        {
            InitializeComponent();
            ToastService.Requested += OnToastRequested;
        }

        private void OnToastRequested(string message)
        {
            Dispatcher.Invoke(() =>
            {
                MessageText.Text = message;
                _hideCompleteTimer?.Stop();
                Bubble.Visibility = Visibility.Visible;
                Bubble.Opacity = 1;

                _hideTimer?.Stop();
                _hideTimer = new System.Windows.Threading.DispatcherTimer { Interval = System.TimeSpan.FromSeconds(2.2) };
                _hideTimer.Tick += (s, e) =>
                {
                    _hideTimer.Stop();
                    Bubble.Opacity = 0;
                    _hideCompleteTimer = new System.Windows.Threading.DispatcherTimer { Interval = System.TimeSpan.FromMilliseconds(200) };
                    _hideCompleteTimer.Tick += (s2, e2) =>
                    {
                        _hideCompleteTimer.Stop();
                        Bubble.Visibility = Visibility.Collapsed;
                    };
                    _hideCompleteTimer.Start();
                };
                _hideTimer.Start();
            });
        }
    }
}
