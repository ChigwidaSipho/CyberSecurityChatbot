using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace CyberSecurityChatbot
{
    public partial class ChatbotGUI : UserControl
    {
        private readonly Chatbot _bot = new Chatbot();

        private static readonly SolidColorBrush BrushBotBubble = new SolidColorBrush(Color.FromRgb(0x1A, 0x27, 0x40));
        private static readonly SolidColorBrush BrushUserBubble = new SolidColorBrush(Color.FromRgb(0x0E, 0x3A, 0x2F));
        private static readonly SolidColorBrush BrushCyan = new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0xFF));
        private static readonly SolidColorBrush BrushGreen = new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x88));

        public ChatbotGUI()
        {
            InitializeComponent();
            Loaded += (s, e) => ShowWelcome();
        }

        private void ShowWelcome()
        {
            PlayGreeting("ChatBotGreeting.wav");
            AppendBotMessage(
                "  ██████╗██╗   ██╗██████╗ ███████╗██████╗ \n" +
                "  ██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗\n" +
                "  ██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝\n" +
                "  ██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗\n" +
                "  ╚██████╗   ██║   ██████╔╝███████╗██║  ██║\n" +
                "   ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝\n\n" +
                "👋 Welcome to the Cybersecurity Awareness System!\n\n" +
                "Ask me about:\n" +
                "• Password safety\n" +
                "• Phishing scams\n" +
                "• Safe browsing\n" +
                "• Privacy protection\n\n" +
                "Try typing: \"What is phishing?\""
            );
        }

        private void PlayGreeting(string fileName)
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                var player = new System.Media.SoundPlayer(path);
                player.Play();
            }
            catch { }
        }

        private void SendMessage()
        {
            string text = InputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            AppendUserMessage(text);
            InputBox.Clear();
            InputBox.Focus();

            string response = _bot.GetResponse(text);
            AppendBotMessage(response);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) => SendMessage();

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage();
        }

        private void Chip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                InputBox.Text = btn.Tag.ToString();
                SendMessage();
            }
        }

        private void AppendBotMessage(string message) => AddMessageRow(message, BrushBotBubble, BrushCyan, HorizontalAlignment.Left, true);
        private void AppendUserMessage(string message) => AddMessageRow(message, BrushUserBubble, BrushGreen, HorizontalAlignment.Right, false);

        private void AddMessageRow(string message, Brush background, Brush labelColor,
            HorizontalAlignment align, bool isBot)
        {
            var row = new Grid
            {
                Margin = new Thickness(0, 4, 0, 4),
                Opacity = 0
            };

            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition());

            var bubble = BuildBubble(message, background, labelColor, align, isBot);
            Grid.SetColumn(bubble, isBot ? 0 : 1);
            row.Children.Add(bubble);

            MessagePanel.Children.Add(row);

            Dispatcher.InvokeAsync(() =>
            {
                ChatScroll.UpdateLayout();
                ChatScroll.ScrollToEnd();
            });

            var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            row.BeginAnimation(OpacityProperty, anim);
        }

        private Border BuildBubble(string message, Brush background,
            Brush labelColor, HorizontalAlignment align, bool isBot)
        {
            var stack = new StackPanel();

            stack.Children.Add(new TextBlock
            {
                Text = isBot ? "🛡 CyberBot" : "👤 You",
                Foreground = labelColor,
                FontSize = 10,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(0, 0, 0, 4)
            });

            stack.Children.Add(new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                FontSize = 13,
                FontFamily = new FontFamily("Consolas"),
                TextWrapping = TextWrapping.Wrap
            });

            return new Border
            {
                Background = background,
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12),
                Margin = new Thickness(5),
                MaxWidth = 520,
                HorizontalAlignment = align,
                Child = stack,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 10,
                    Opacity = 0.25,
                    ShadowDepth = 1
                }
            };
        }
    }
}