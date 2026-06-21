using System.Windows;

namespace CyberSecurityChatbot
{
    /// <summary>
    /// Shell window — owns the shared Chatbot instance and wires all tabs together.
    /// Handles cross-tab navigation triggered by NLP commands in the chat.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Chatbot _bot;

        public MainWindow()
        {
            InitializeComponent();

            // One Chatbot instance shared across all tabs so they all write to the same log
            _bot = new Chatbot();

            // Inject bot into chat tab
            ChatTab.SetBot(_bot);

            // Inject shared bot log into task, quiz, and log tabs
            TaskTab.SetBot(_bot);
            QuizTab.SetBot(_bot);
            LogTab.SetLog(_bot.Log);

            // Listen for NLP navigation requests from the chat
            _bot.NavigateRequested += OnNavigateRequested;
        }

        // Called when Chatbot detects "go to tasks", "start quiz", etc.
        private void OnNavigateRequested(string destination)
        {
            Dispatcher.Invoke(() =>
            {
                switch (destination)
                {
                    case "tasks":
                        MainTabs.SelectedItem = MainTabs.Items[1];
                        break;
                    case "quiz":
                        MainTabs.SelectedItem = MainTabs.Items[2];
                        QuizTab.StartQuiz();
                        break;
                    case "log":
                        MainTabs.SelectedItem = MainTabs.Items[3];
                        LogTab.Refresh();
                        break;
                }
            });
        }
    }
}
