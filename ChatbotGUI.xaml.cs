using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace CyberSecurityChatbot
{
    /// <summary>
    /// Chat tab — renders message bubbles and routes user input to the shared Chatbot engine.
    /// SetBot() must be called by MainWindow after InitializeComponent() to inject the shared bot.
    /// </summary>
    public partial class ChatbotGUI : UserControl
    {
        // ===== BRUSHES =====
        private static readonly SolidColorBrush BrushBotBubble  = new SolidColorBrush(Color.FromRgb(0x1A, 0x27, 0x40));
        private static readonly SolidColorBrush BrushUserBubble = new SolidColorBrush(Color.FromRgb(0x0E, 0x3A, 0x2F));
        private static readonly SolidColorBrush BrushCyan       = new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0xFF));
        private static readonly SolidColorBrush BrushGreen      = new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x88));

        private Chatbot _bot;

        // ===== PARAMETERLESS CONSTRUCTOR (required by XAML designer) =====
        public ChatbotGUI()
        {
            InitializeComponent();
        }

        // ===== INJECTION =====
        /// <summary>Called by MainWindow to inject the shared Chatbot instance.</summary>
        public void SetBot(Chatbot bot)
        {
            _bot = bot;
            // Wire Loaded here so we don't show welcome before bot is ready
            Loaded += (s, e) => ShowWelcome();
        }

        // ===== WELCOME =====
        private void ShowWelcome()
        {
            PlayGreeting("ChatBotGreeting.wav");

            AppendBotMessage(
                "╔══════════════════════════════════════╗\n" +
                "║        🤖  Y.O.U  CYBER BOT  🤖     ║\n" +
                "╚══════════════════════════════════════╝\n\n" +
                "🛡 Welcome to your cybersecurity assistant.\n\n" +
                "I can help you with:\n" +
                "• Password safety      • Phishing & scams\n" +
                "• Privacy protection   • Safe browsing\n" +
                "• Malware awareness    • VPN & 2FA\n\n" +
                "💬 Try: \"What is phishing?\"  or  \"Give me a password tip\"\n" +
                "📋 Say: \"Add task\" to manage your security tasks\n" +
                "🎮 Say: \"Start quiz\" to test your knowledge\n\n" +
                "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                "⚡ Status: ONLINE      🔒 Protection: ACTIVE\n" +
                "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
            );
        }

        private void PlayGreeting(string fileName)
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                if (System.IO.File.Exists(path))
                    new System.Media.SoundPlayer(path).Play();
            }
            catch { /* Audio is non-critical */ }
        }

        // ===== SEND =====
        private void SendMessage()
        {
            if (_bot == null) return;

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

        // ===== MESSAGE RENDERING =====
        private void AppendBotMessage(string msg)  => AddMessageRow(msg, BrushBotBubble,  BrushCyan,  HorizontalAlignment.Left,  isBot: true);
        private void AppendUserMessage(string msg) => AddMessageRow(msg, BrushUserBubble, BrushGreen, HorizontalAlignment.Right, isBot: false);

        private void AddMessageRow(string message, Brush background, Brush labelColor,
                                   HorizontalAlignment align, bool isBot)
        {
            var row = new Grid { Margin = new Thickness(0, 4, 0, 4), Opacity = 0 };
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

            row.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200)));
        }

        private Border BuildBubble(string message, Brush background, Brush labelColor,
                                   HorizontalAlignment align, bool isBot)
        {
            var stack = new StackPanel();

            stack.Children.Add(new TextBlock
            {
                Text       = isBot ? "🛡 CyberBot" : "👤 You",
                Foreground = labelColor,
                FontSize   = 10,
                FontFamily = new FontFamily("Consolas"),
                Margin     = new Thickness(0, 0, 0, 4)
            });

            stack.Children.Add(new TextBlock
            {
                Text         = message,
                Foreground   = Brushes.White,
                FontSize     = 13,
                FontFamily   = new FontFamily("Consolas"),
                TextWrapping = TextWrapping.Wrap
            });

            return new Border
            {
                Background          = background,
                CornerRadius        = new CornerRadius(12),
                Padding             = new Thickness(12),
                Margin              = new Thickness(5),
                MaxWidth            = 520,
                HorizontalAlignment = align,
                Child               = stack,
                Effect              = new DropShadowEffect
                {
                    Color       = Colors.Black,
                    BlurRadius  = 10,
                    Opacity     = 0.25,
                    ShadowDepth = 1
                }
            };
        }
    }
}
