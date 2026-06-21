using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CyberSecurityChatbot
{
    public partial class ActivityLogControl : UserControl
    {
        private ActivityLog _log;
        private bool _showAll = false;

        public ActivityLogControl()      
        {
            InitializeComponent();
        }

        public void SetLog(ActivityLog log)
        {   
            _log = log;
            Refresh();
        }

        public void Refresh()
        {
            if (_log == null || LogPanel == null) return;

            var entries = _showAll
                ? _log.GetAll()
                : _log.GetRecent(10);

            LogPanel.Children.Clear();

            foreach (var entry in entries)
            {
                LogPanel.Children.Add(new TextBlock
                {
                    Text = entry.ToString(),
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 3, 0, 3),
                    TextWrapping = TextWrapping.Wrap
                });
            }

            var count = _log.Count;

            LogCountLabel.Text = _showAll
                ? $"Showing all {count} actions"
                : $"Showing last 10 of {count} actions";

            FooterLabel.Text = _showAll
                ? $"Total actions recorded: {count}"
                : $"Preview mode active — click Show More to expand";

            ShowMoreBtn.Content = _showAll
                ? "Show Less ▲"
                : "Show More ▼";
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void ShowMore_Click(object sender, RoutedEventArgs e)
        {
            _showAll = !_showAll;
            Refresh();
        }
    }
}