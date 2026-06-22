using System;
using System.Threading.Tasks;
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
        private static readonly SolidColorBrush BrushBotBubble = new SolidColorBrush(Color.FromRgb(0x1A, 0x27, 0x40));
        private static readonly SolidColorBrush BrushUserBubble = new SolidColorBrush(Color.FromRgb(0x0E, 0x3A, 0x2F));
        private static readonly SolidColorBrush BrushCyan = new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0xFF));
        private static readonly SolidColorBrush BrushGreen = new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x88));

        private Chatbot _bot;
        private bool _hasShownWelcome = false;
        private readonly Random _rand = new Random();

        public ChatbotGUI()
        {
            InitializeComponent();
        }

        public void SetBot(Chatbot bot)
        {
            _bot = bot;
            Loaded += (s, e) => ShowWelcome();
        }

        private void ShowWelcome()
        {
            // Guard against double-firing if SetBot/Loaded ever runs more than once
            // (e.g. tab re-shown, control reused).
            if (_hasShownWelcome) return;
            _hasShownWelcome = true;

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
            catch { }
        }

        // ================= SEND FLOW =================
        // Fire-and-forget async void is intentional here: this is a UI event handler
        // (button click / key press), which is the one place async void is the
        // correct pattern in WPF. Exceptions inside are caught so a bad response
        // can't silently crash the chat.
        private async void SendMessage()
        {
            if (_bot == null) return;

            string text = InputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            AppendUserMessage(text);
            InputBox.Clear();
            SetInputEnabled(false);

            var typingBubble = AppendTypingIndicator();

            try
            {
                // Small randomized delay so replies don't feel instant/robotic.
                // Scales slightly with message length to mimic "reading" time.
                int delayMs = 350 + _rand.Next(250, 650) + Math.Min(text.Length * 8, 600);
                await Task.Delay(delayMs);

                string response = _bot.GetResponse(text);

                RemoveMessageRow(typingBubble);
                AppendBotMessage(response);
            }
            catch (Exception)
            {
                RemoveMessageRow(typingBubble);
                AppendBotMessage("Hmm, something glitched on my end. Try that again?");
            }
            finally
            {
                SetInputEnabled(true);
                InputBox.Focus();
            }
        }

        private void SetInputEnabled(bool enabled)
        {
            InputBox.IsEnabled = enabled;
            SendButton.IsEnabled = enabled;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) => SendMessage();

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage();
        }

        private void Chip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null && InputBox.IsEnabled)
            {
                InputBox.Text = btn.Tag.ToString();
                SendMessage();
            }
        }

        // ================= MESSAGE RENDERING =================
        private void AppendBotMessage(string msg) =>
            AddMessageRow(msg, BrushBotBubble, BrushCyan, HorizontalAlignment.Left, true);

        private void AppendUserMessage(string msg) =>
            AddMessageRow(msg, BrushUserBubble, BrushGreen, HorizontalAlignment.Right, false);

        private Grid AddMessageRow(string message, Brush background, Brush labelColor,
                                   HorizontalAlignment align, bool isBot)
        {
            var row = new Grid { Margin = new Thickness(0, 4, 0, 4), Opacity = 0 };
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition());

            var bubble = BuildBubble(message, background, labelColor, align, isBot);
            Grid.SetColumn(bubble, isBot ? 0 : 1);
            row.Children.Add(bubble);

            MessagePanel.Children.Add(row);
            ScrollToEnd();

            row.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200)));

            return row;
        }

        // ================= TYPING INDICATOR =================
        // A lightweight "bot is typing..." bubble shown while the response is
        // being "composed". Removed once the real reply lands.
        private Grid AppendTypingIndicator()
        {
            var row = new Grid { Margin = new Thickness(0, 4, 0, 4), Opacity = 0 };
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition());

            var dots = new TextBlock
            {
                Text = "● ● ●",
                Foreground = BrushCyan,
                FontSize = 13,
                FontFamily = new FontFamily("Consolas")
            };

            // Subtle pulse animation so the dots don't sit dead-still.
            dots.BeginAnimation(OpacityProperty, new DoubleAnimation
            {
                From = 0.3,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(550),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            });

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = "🛡 CyberBot",
                Foreground = BrushCyan,
                FontSize = 10,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(0, 0, 0, 4)
            });
            stack.Children.Add(dots);

            var bubble = new Border
            {
                Background = BrushBotBubble,
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12),
                Margin = new Thickness(5),
                MaxWidth = 520,
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = stack,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 10,
                    Opacity = 0.25,
                    ShadowDepth = 1
                }
            };

            Grid.SetColumn(bubble, 0);
            row.Children.Add(bubble);

            MessagePanel.Children.Add(row);
            ScrollToEnd();

            row.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150)));

            return row;
        }

        private void RemoveMessageRow(Grid row)
        {
            if (row == null || !MessagePanel.Children.Contains(row)) return;

            // Fade out, then remove — avoids an abrupt pop when the typing
            // bubble is swapped for the real message.
            var fadeOut = new DoubleAnimation(row.Opacity, 0, TimeSpan.FromMilliseconds(120));
            fadeOut.Completed += (s, e) => MessagePanel.Children.Remove(row);
            row.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void ScrollToEnd()
        {
            Dispatcher.InvokeAsync(() =>
            {
                ChatScroll.UpdateLayout();
                ChatScroll.ScrollToEnd();
            });
        }

        private Border BuildBubble(string message, Brush background, Brush labelColor,
                                   HorizontalAlignment align, bool isBot)
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